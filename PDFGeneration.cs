using AdvBillingSystem.ACM;
using AdvBillingSystem.DBB;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvBillingSystem.SRV
{
    public class PDFGeneration
    {

        // Public entrypoint: returns path to generated PDF
        public  string GenerateInvoicePdf(int clientId, string caseType ,int paymentId)
        {
            var db = new BillingDbContext();
            decimal totalSubAmount = 0;
            // Fetch client & payments
            var client = db.GetClientByIdAndCase(clientId, caseType);
            if (client == null)
                throw new InvalidOperationException("Client / case not found.");

            var payments = db.GetPaymentsByClientAndCasePayment(clientId, caseType , paymentId);

            var totalPaid = payments.Sum(p => p.AmountPaid);
           // var balance = client.TotalAmount - totalPaid;

            // Create invoice number
            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}";
            var invoiceDate = DateTime.Now;

            // File path - put in Documents or app folder
            var fileName = $"Invoice_{invoiceNumber}.pdf";
            var fileNameMGN = $"MGN/0000{DateTime.Now.Millisecond}/{DateTime.Now.Year}.pdf";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(documentsPath, fileName);

            // Use QuestPDF to create the document
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("MEMORANDUM OF FEES").FontSize(18).Bold().Underline();
                                col.Item().Text("Makil, Manu & Murali").Bold();
                                col.Item().Text("Advocates & Solicitors");
                                col.Item().Text("Cochin | Chennai");
                                col.Item().Text("Mobile:+91 8075667251").SemiBold().Underline(); 
                                col.Item().Text("Email: makilmanumurali@gmail.com").SemiBold().Underline();
                            });

                            row.ConstantItem(200).Column(col =>
                            {
                                col.Item().Text($"Invoice #{fileNameMGN}").FontSize(10).SemiBold();
                                col.Item().Text($"Date: {invoiceDate:dd-MMM-yyyy}");
                               // col.Item().Text($"Case Type: {client.CaseNumber}");
                            });
                        });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(6);

                        // Client info
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"To:").SemiBold();
                                c.Item().Text(client.Name);
                                if (!string.IsNullOrWhiteSpace(client.Address)) c.Item().Text(client.Address);
                                if (!string.IsNullOrWhiteSpace(client.Mobile)) c.Item().Text($"Phone: {client.Mobile}");
                                if (!string.IsNullOrWhiteSpace(client.Email)) c.Item().Text($"Email: {client.Email}");
                            });

                            r.ConstantItem(200).Column(c =>
                            {
                                c.Item().Text($"Case Number: {client.CaseNumber}");
                               // c.Item().Text($"Total Fees: {client.TotalAmount:C}");
                            });
                        });

                        col.Item().LineHorizontal(1);

                        // Payments table header
                        //  col.Item().Text("Payment Details").Bold();

                        col.Item().PaddingVertical(10).Text("Payment Summary").FontSize(13).Bold().Underline();

                        col.Item().Table(table =>
                        {
                            // Define columns: Date, Remarks, Amount
                            table.ColumnsDefinition(columns =>
                            {
                               // columns.RelativeColumn(2);  // Date
                                columns.RelativeColumn(7);  // Description / Remarks
                                columns.RelativeColumn(3);  // Amount
                            });

                            // Header Row
                            table.Header(header =>
                            {
                             //   header.Cell().Element(HeaderCellStyle).Text("Date").SemiBold();
                              //  header.Cell().Element(HeaderCellStyle).Text("").SemiBold();
                              //  header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount (₹)").SemiBold();
                            });

                            // Data Rows
                            decimal totalAmount = 0;
                            
                            foreach (var payment in payments)
                            {
                              //  table.Cell().Element(CellStyle).Text(payment.PaymentDate.ToString("dd-MM-yyyy"));
                                table.Cell().Border(1).Element(CellStyle).Text("");
                                table.Cell().Border(1).Element(CellStyle).AlignRight().Text(payment.CourtFees?.ToString("0.00"));
                                table.Cell().Border(1).Element(CellStyle).Text("Clerical expenses, process fees and e-filing charges");
                                table.Cell().Border(1).Element(CellStyle).AlignRight().Text(payment.ClericalFees?.ToString("0.00"));
                                table.Cell().Border(1).Element(CellStyle).Text("Professional fee towards");
                                table.Cell().Border(1).Element(CellStyle).AlignRight().Text(payment.AmountPaid.ToString("0.00"));
                                totalAmount += payment.AmountPaid;

                                totalSubAmount += (payment.AmountPaid  )
                   + (payment.CourtFees ?? 0)
                   + (payment.ClericalFees ?? 0);

                                table.Cell().Border(1).Element(CellStyle).Text("Total ");
                                table.Cell().Border(1).Element(CellStyle).AlignRight().Text(totalSubAmount.ToString("0.00"));

                               
                            }

                            // If no payments
                            if (!payments.Any())
                            {
                                table.Cell().ColumnSpan(3).Element(CellStyle).Text("No payment records available.");
                            }

                            // Total Row (Bold)
                            //table.Cell().ColumnSpan(2).Element(TotalCellStyle).Text("Total Paid:");
                           // table.Cell().Element(TotalCellStyle).AlignRight().Text(totalAmount.ToString("0.00"));

                            IContainer CellStyle(IContainer containerver)
                            {
                                return containerver.PaddingVertical(4).PaddingHorizontal(4);
                            }

                            IContainer HeaderCellStyle(IContainer containerver)
                            {
                                return containerver
                                    .DefaultTextStyle(x => x.FontSize(11))
                                    .PaddingVertical(5)
                                   // .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Darken2);
                            }
                        });

                       // col.Item().LineHorizontal(1);

                        // Totals
                        col.Item().AlignRight().Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                               // r.RelativeItem().Border(1).Text("Total:");
                              //  r.ConstantItem(200).Border(1).AlignRight().Text(totalSubAmount.ToString("0.00"));
                            });
                            c.Item().Row(r =>
                            {

                               // r.RelativeItem().Text("Total Paid:");
                               // r.ConstantItem(200).AlignRight().Text(totalPaid.ToString("0.00"));
                            });
                            c.Item().Row(r =>
                            {
                               // r.RelativeItem().Text("Balance:");
                              //  r.ConstantItem(200).AlignRight().Text(balance?.ToString("0.00")).Bold();
                            });
                        });

                        string totalInWords = NumberToWordsConverter.ConvertAmountToWords(totalSubAmount);

                        col.Item().Text($"({totalInWords})");

                        col.Item().Text("(This is an electronically generated invoice, hence does not require a signature.)").FontColor(Colors.Grey.Medium);
                        col.Item().Text("Adv.Manu Nair G.");
                        col.Item().LineHorizontal(1);
                        col.Item().Text("P.S.Please quote the number and date of this memo while sending the payment.");
                        col.Item().Text("PAN No. FMMPM2716J").SemiBold();
                         

                        //  col.Item().LineHorizontal(1);

                        // Bank details / footer block
                        col.Item().Text("Bank Account Details of Adv.Manu Nair G").Bold().Underline();

                        col.Item().Text("Name of Bank     : South Indian Bank");
                        col.Item().Text("A/c No                : 0344053000000587");
                        col.Item().Text("IFSC Code           : SIBL0000344");
                        col.Item().Text("Branch                : Kottayam Collectorate");
                        col.Item().Text("UPI ID                 : 9400802430@ybl");
                    });

                    //page.Footer()
                    //    .AlignCenter()
                    //    .Text(x => x.Span("This is a computer-generated invoice , hence does not require a signature.").SemiBold().Underline());
                });
            });

            // generate PDF
            doc.GeneratePdf(path);

            return path;
        }
    }
}
