using AdvBillingSystem.ACM;
using AdvBillingSystem.DBB;
using AdvBillingSystem.SRV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvBillingSystem
{
    public partial class PaymentFrm : Form
    {
        public PaymentFrm()
        {
         
            InitializeComponent();
            //System.Threading.Thread.Sleep(5000);
            LoadClients();

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }


        private void buttonPay_Click(object sender, EventArgs e)
        {
           
        }

        private void LoadPayments(int clientId, string caseType)
        {
            var db = new BillingDbContext();
            var payments = db.GetPaymentsByClientAndCase(clientId, caseType);

            dataGridView1.DataSource = payments;
            AddGenerateInvoiceButtonColumn();

            if (dataGridView1.Columns.Contains("PaymentId"))
                dataGridView1.Columns["PaymentId"].Visible = false;


        }

        private void AddGenerateInvoiceButtonColumn()
        {
            // Check if the column already exists
            if (!dataGridView1.Columns.Contains("btnGenerateInvoice"))
            {
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
                btnCol.Name = "btnGenerateInvoice";
                btnCol.HeaderText = "Invoice";
                btnCol.Text = "Generate";
                btnCol.UseColumnTextForButtonValue = true;
                btnCol.Width = 100;
                dataGridView1.Columns.Add(btnCol);
                this.dataGridView1.CellClick += DataGridView1_CellClick;
            }
         
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore header and invalid clicks
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // Check if our invoice button column was clicked
            if (dataGridView1.Columns[e.ColumnIndex].Name == "btnGenerateInvoice")
            {
                // Get ClientId and CaseType from the row
                int clientId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ClientId"].Value);
                string caseType = dataGridView1.Rows[e.RowIndex].Cells["CaseType"].Value.ToString();
                int paymentId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["PaymentId"].Value);

                try
                {
                    PDFGeneration pDF = new PDFGeneration();
                    // Generate invoice using your InvoiceService
                    string pdfPath = pDF.GenerateInvoicePdf(clientId, caseType, paymentId);

                    MessageBox.Show($"Invoice generated:\n{pdfPath}", "Invoice Created",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Open the PDF automatically
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pdfPath,
                        UseShellExecute = true
                    });

                    // (Optional) refresh Invoice History grid if you have one
                  //  LoadInvoiceHistory(clientId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating invoice: " + ex.Message);
                }
            }
        }

        //private decimal GetBalance(int clientId, string caseType)
        //{
        //    var db = new BillingDbContext();
        //    decimal totalPaid = db.GetTotalPaid(clientId, caseType);
        //    decimal totalAmount =db.GetTotalAmountByCaseType(clientId,caseType);
        //    return totalAmount - totalPaid;
        //}

        private void LoadClients()
        {
            var db = new BillingDbContext();
            var clients = db.GetAllClientsByName(); // returns List<Client>
            clients.Insert(0, new Client { ClientId = 0, Name = "-- Select Client --" });
            clientNm.DataSource = clients;
           // clients.Insert(0, new Client { ClientId = -1, Name = "-- Select Client --" });
            clientNm.DisplayMember = "Name"; // what user sees
            clientNm.ValueMember = "ClientId"; // hidden value
            clientNm.SelectedIndex = 0; // no selection initially
        }

        private void LoadCaseTypes(int clientId)
        {
            var db = new BillingDbContext();
            var caseTypes = db.GetCaseTypesByClient(clientId); // List<string>
            caseTypes.Insert(0, "-- Select Case --");
            Clientcase.DataSource = caseTypes;
            Clientcase.SelectedIndex = 0;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clientNm.SelectedIndex <= 0 || Clientcase.SelectedIndex <= 0) return;

            var db = new BillingDbContext();
            int clientId = Convert.ToInt32(clientNm.SelectedValue);
            string caseType = Clientcase.SelectedValue?.ToString();
            decimal totalAmount = db.GetTotalAmountByCaseType(clientId, caseType);
            textBox1.Text = Convert.ToString(totalAmount);
            var totalPaid = db.GetTotalPaid(clientId, caseType);
            var balance = totalAmount - totalPaid;
            textBox2.Text = Convert.ToString(balance);
            LoadPayments(clientId, caseType);
        }

        private void clientNm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clientNm.SelectedIndex <= 0) return;

            if (clientNm.SelectedValue != null)
            {
                LoadCaseTypes(Convert.ToInt32(clientNm.SelectedValue));
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (clientNm.SelectedItem == null || Clientcase.SelectedItem == null)
                return;

            var client = clientNm.SelectedItem as Client;
            string caseType = Clientcase.Text;

            if (!decimal.TryParse(textBox3.Text, out decimal paymentAmount) || paymentAmount <= 0)
            {
                MessageBox.Show("Enter a valid payment amount.");
                return;
            }

            // Calculate remaining balance for this client & case type
            var db = new BillingDbContext();
            decimal totalPaid = db.GetTotalPaid(client.ClientId, caseType);
            decimal totalAmount = db.GetTotalAmountByCaseType(client.ClientId, caseType); // Could be hardcoded or fetched
            decimal balance = totalAmount - totalPaid;

            if (paymentAmount > balance)
            {
                MessageBox.Show("Payment exceeds remaining balance!");
                return;
            }

            var payment = new Payment
            {
                ClientId = client.ClientId,
                CaseType = caseType,
                PaymentDate = DateTime.Now,
                AmountPaid = paymentAmount,
                Remarks = richTextBox1.Text,
                CourtFees = textBox5.Text != ""? Convert.ToDecimal(textBox5.Text):0,
                ClericalFees = textBox4.Text != ""? Convert.ToDecimal(textBox4.Text):0
            };

            db.InsertPayment(payment);

            // Update displayed balance
            textBox2.Text = (balance - paymentAmount).ToString("0.00");

            LoadPayments(client.ClientId, caseType);

            textBox3.Clear();
            richTextBox1.Clear();
        }
    }
}
