using AdvBillingSystem.ACM;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvBillingSystem.DBB
{
    public class BillingDbContext
    {
        private readonly string _connectionString = "Data Source=billing.db";

        public IDbConnection CreateConnection() => new SQLiteConnection(_connectionString);

        public void Initialize()
        {
            try
            {
                var conn = CreateConnection();
                conn.Open();

                conn.Execute(@"
                CREATE TABLE IF NOT EXISTS Clients (
                    ClientId      INTEGER PRIMARY KEY AUTOINCREMENT,
                    CaseNumber    TEXT,
                    CaseType      TEXT,
                    Name          TEXT NOT NULL,
                    Address       TEXT,
                    Mobile        TEXT,
                    Email         TEXT,
                    Remarks       TEXT,
                    VisitedDt     TEXT,
                    TotalAmount   REAL,
                    TotalFees     REAL DEFAULT 0,
                    OtherFees     REAL DEFAULT 0,
                    TotalPaid     REAL DEFAULT 0,
                    Balance       REAL DEFAULT 0,
                    ActiveUser    INTEGER DEFAULT 1
                );

CREATE TABLE IF NOT EXISTS Payments (
    PaymentId   INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId    INTEGER NOT NULL,
    CaseType    TEXT NOT NULL,
    PaymentDate TEXT NOT NULL,
    AmountPaid  REAL NOT NULL,
    CourtFees   REAL ,
    ClericalFees REAl,
    Remarks     TEXT,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);
            ");
            }
            catch (Exception ex) { throw ex; }
        }

        public void InsertClient(Client client)
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"
            INSERT INTO Clients 
            (CaseNumber, CaseType, Name, Address, Mobile, Email, Remarks, VisitedDt, 
             TotalAmount, TotalFees, OtherFees, TotalPaid, Balance, ActiveUser)
            VALUES 
            (@CaseNumber, @CaseType, @Name, @Address, @Mobile, @Email, @Remarks, @VisitedDt,
             @TotalAmount, @TotalFees, @OtherFees, @TotalPaid, @Balance, @ActiveUser);
        ";

            conn.Execute(sql, client);
        }


        public List<Client> GetAllClients()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string sql = "SELECT * FROM Clients order by Name";
            return conn.Query<Client>(sql).ToList();
        }

        public void UpdateClient(Client client)
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string sql = @"UPDATE Clients SET 
                    CaseNumber=@CaseNumber, CaseType=@CaseType, Name=@Name, Address=@Address, 
                    Mobile=@Mobile, Email=@Email, Remarks=@Remarks, VisitedDt=@VisitedDt, 
                    TotalAmount=@TotalAmount, TotalFees=@TotalFees, OtherFees=@OtherFees, 
                    TotalPaid=@TotalPaid, Balance=@Balance 
                   WHERE ClientId=@ClientId";
            conn.Execute(sql, client);
        }

        public void DeleteClient(int clientId)
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string sql = "DELETE FROM Clients WHERE ClientId=@ClientId";
            conn.Execute(sql, new { ClientId = clientId });
        }


        public decimal GetTotalAmountByCaseType(int clientId, string caseType)
        {
            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();
                string query = @"SELECT TotalAmount FROM Clients 
                         WHERE ClientId = @ClientId AND CaseType = @CaseType";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@CaseType", caseType);

                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public void InsertPayment(Payment payment)
        {
            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();

                string query = @"
            INSERT INTO Payments (ClientId, CaseType, PaymentDate, AmountPaid, CourtFees , ClericalFees ,Remarks)
            VALUES (@ClientId, @CaseType, @PaymentDate, @AmountPaid,@CourtFees ,@ClearicalFees , @Remarks )
        ";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", payment.ClientId);
                    cmd.Parameters.AddWithValue("@CaseType", payment.CaseType);
                    cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@AmountPaid", payment.AmountPaid);
                    cmd.Parameters.AddWithValue("@CourtFees", payment.CourtFees);
                    cmd.Parameters.AddWithValue("@ClearicalFees", payment.ClericalFees);

                    cmd.Parameters.AddWithValue("@Remarks", payment.Remarks ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public decimal GetTotalPaid(int clientId, string caseType)
        {
            decimal totalPaid = 0;

            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();

                string query = @"
            SELECT SUM(AmountPaid) 
            FROM Payments
            WHERE ClientId = @ClientId AND CaseType = @CaseType
        ";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@CaseType", caseType);

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        totalPaid = Convert.ToDecimal(result);
                    }
                }
            }

            return totalPaid;
        }

        public List<Payment> GetPaymentsByClientAndCase(int clientId, string caseType)
        {
            var payments = new List<Payment>();

            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();

                string query = @"
            SELECT PaymentId, ClientId, CaseType, PaymentDate, AmountPaid, Remarks , CourtFees, ClericalFees
            FROM Payments
            WHERE ClientId = @ClientId AND CaseType = @CaseType 
            ORDER BY PaymentDate ASC
        ";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@CaseType", caseType);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new Payment
                            {
                                PaymentId = reader["PaymentId"] != DBNull.Value ? Convert.ToInt32(reader["PaymentId"]) : 0,
                                ClientId = reader["ClientId"] != DBNull.Value ? Convert.ToInt32(reader["ClientId"]) : 0,
                                CaseType = reader["CaseType"]?.ToString(),
                                PaymentDate = reader["PaymentDate"] != DBNull.Value
                                              ? DateTime.Parse(reader["PaymentDate"].ToString())
                                              : DateTime.MinValue,
                                AmountPaid = reader["AmountPaid"] != DBNull.Value ? Convert.ToDecimal(reader["AmountPaid"]) : 0,
                                CourtFees = reader["CourtFees"] != DBNull.Value ? Convert.ToDecimal(reader["CourtFees"]) : 0,
                                ClericalFees = reader["ClericalFees"] != DBNull.Value ? Convert.ToDecimal(reader["ClericalFees"]) : 0,
                                Remarks = reader["Remarks"]?.ToString()
                            });
                        }
                    }
                }
            }

            return payments;
        }


        public List<Payment> GetPaymentsByClientAndCasePayment(int clientId, string caseType ,int paymentId)
        {
            var payments = new List<Payment>();

            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();

                string query = @"
            SELECT PaymentId, ClientId, CaseType, PaymentDate, AmountPaid, Remarks , CourtFees, ClericalFees
            FROM Payments
            WHERE ClientId = @ClientId AND CaseType = @CaseType  AND PaymentId = @PaymentId
            ORDER BY PaymentDate ASC
        ";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@CaseType", caseType);
                    cmd.Parameters.AddWithValue("@PaymentId", paymentId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new Payment
                            {
                                PaymentId = reader["PaymentId"] != DBNull.Value ? Convert.ToInt32(reader["PaymentId"]) : 0,
                                ClientId = reader["ClientId"] != DBNull.Value ? Convert.ToInt32(reader["ClientId"]) : 0,
                                CaseType = reader["CaseType"]?.ToString(),
                                PaymentDate = reader["PaymentDate"] != DBNull.Value
                                              ? DateTime.Parse(reader["PaymentDate"].ToString())
                                              : DateTime.MinValue,
                                AmountPaid = reader["AmountPaid"] != DBNull.Value ? Convert.ToDecimal(reader["AmountPaid"]) : 0,
                                CourtFees = reader["CourtFees"] != DBNull.Value ? Convert.ToDecimal(reader["CourtFees"]) : 0,
                                ClericalFees = reader["ClericalFees"] != DBNull.Value ? Convert.ToDecimal(reader["ClericalFees"]) : 0,
                                Remarks = reader["Remarks"]?.ToString()
                            });
                        }
                    }
                }
            }

            return payments;
        }

        public List<string> GetCaseTypesByClient(int clientId)
        {
            var list = new List<string>();

            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();
                string query = "SELECT DISTINCT CaseType FROM Clients WHERE ClientId = @ClientId";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader["CaseType"].ToString());
                        }
                    }
                }
            }

            return list;
        }

        public  Client GetClientByIdAndCase(int clientId, string caseType)
        {
             var conn = new SQLiteConnection("Data Source=billing.db");
            conn.Open();

            string sql = @"
            SELECT ClientId, CaseNumber, CaseType, Name, Address, Mobile, Email, IFNULL(TotalAmount, 0) AS TotalAmount
            FROM Clients
            WHERE ClientId = @ClientId AND CaseType = @CaseType
            LIMIT 1;
        ";

             var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClientId", clientId);
            cmd.Parameters.AddWithValue("@CaseType", caseType);

             var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                return new Client
                {
                    ClientId = Convert.ToInt32(rdr["ClientId"]),
                    CaseNumber = rdr["CaseNumber"]?.ToString(),
                    CaseType = rdr["CaseType"]?.ToString(),
                    Name = rdr["Name"]?.ToString(),
                    Address = rdr["Address"]?.ToString(),
                    Mobile = rdr["Mobile"]?.ToString(),
                    Email = rdr["Email"]?.ToString(),
                    TotalAmount = Convert.ToDecimal(rdr["TotalAmount"])
                };
            }

            return null;
        }

        public List<Client> GetAllClientsByName()
        {
            var list = new List<Client>();

            using (var conn = new SQLiteConnection("Data Source=billing.db"))
            {
                conn.Open();
                string query = "SELECT ClientId, Name FROM Clients ORDER BY Name";

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Client
                        {
                            ClientId = Convert.ToInt32(reader["ClientId"]),
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }

            return list;
        }

    }
}
