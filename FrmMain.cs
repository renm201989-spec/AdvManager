using AdvBillingSystem.ACM;
using AdvBillingSystem.DBB;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvBillingSystem
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();

            comboBox1.Items.Add("CASE-1");
            comboBox1.Items.Add("CASE-2");
            comboBox1.Items.Add("CASE-3");
            comboBox1.Items.Add("CASE-4");
            comboBox1.Items.Add("CASE-5");
            comboBox1.Items.Add("CASE-6");
            comboBox1.Items.Add("CASE-7");
            comboBox1.Items.Add("CASE-8");
            comboBox1.Items.Add("CASE-9");
            comboBox1.Items.Add("CASE-10");
            comboBox1.Items.Add("CASE-11");
            comboBox1.Items.Add("CASE-12");
            comboBox1.Items.Add("CASE-13");
            comboBox1.Items.Add("CASE-14");
            comboBox1.Items.Add("CASE-15");
            comboBox1.Items.Add("CASE-16");
            comboBox1.Items.Add("CASE-17");
            comboBox1.Items.Add("CASE-18");
            comboBox1.Items.Add("CASE-19");
            comboBox1.Items.Add("CASE-20");

            // Optional: select first item by default
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            LoadClients();
            AddGridButtons();


        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var client = new Client
                {
                    CaseNumber = textBox2.Text,
                    CaseType = comboBox1.Text,
                    Name = textBox1.Text,
                    Address = richTextBox1.Text,
                    Mobile = textBox3.Text,
                    Email = textBox4.Text,
                    Remarks = richTextBox2.Text,
                    VisitedDt = dateTimePicker1.Value.ToString("yyyy-MM-dd"),
                    TotalAmount = decimal.TryParse(textBox5.Text, out var ta) ? ta : 0,
                    TotalFees = decimal.TryParse(textBox6.Text, out var tf) ? tf : 0,
                    OtherFees = decimal.TryParse(textBox7.Text, out var of) ? of : 0,
                    TotalPaid = decimal.TryParse(textBox8.Text, out var tp) ? tp : 0,
                    Balance = decimal.TryParse(textBox8.Text, out var b) ? b : 0, // or compute: TotalAmount - TotalPaid
                    ActiveUser = 1
                };

                var db = new BillingDbContext();
                if (button1.Tag != null) // Update
                {
                    client.ClientId = (int)button1.Tag;
                    db.UpdateClient(client);
                    button1.Tag = null;
                    button1.Text = "Save";
                }
                else // Insert
                {
                    db.InsertClient(client);
                }


                MessageBox.Show("Client saved successfully!");
                LoadClients();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadClients()
        {
            var db = new BillingDbContext();
            var clients = db.GetAllClients();
            dataGridView1.DataSource = clients;
            if (dataGridView1.Columns.Contains("ClientId"))
                dataGridView1.Columns["ClientId"].Visible = false;
        }

        private void ClearForm()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            comboBox1.SelectedIndex = -1;
            richTextBox1.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            richTextBox2.Text = "";
            dateTimePicker1.Value = DateTime.Now;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void AddGridButtons()
        {
            if (!dataGridView1.Columns.Contains("Edit"))
            {
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn();
                editBtn.HeaderText = "Edit";
                editBtn.Text = "Edit";
                editBtn.Name = "Edit";
                editBtn.UseColumnTextForButtonValue = true;
                dataGridView1.Columns.Add(editBtn);
            }

            if (!dataGridView1.Columns.Contains("Delete"))
            {
                DataGridViewButtonColumn delBtn = new DataGridViewButtonColumn();
                delBtn.HeaderText = "Delete";
                delBtn.Text = "Delete";
                delBtn.Name = "Delete";
                delBtn.UseColumnTextForButtonValue = true;
                dataGridView1.Columns.Add(delBtn);
            }

            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex == -1) return; // Header row

            var client = dataGridView1.Rows[e.RowIndex].DataBoundItem as Client;
            if (client == null) return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                // Fill form with client details
                textBox1.Text = client.Name;
                textBox2.Text = client.CaseNumber;
                comboBox1.Text = client.CaseType;
                richTextBox1.Text = client.Address;
                textBox3.Text = client.Mobile;
                textBox4.Text = client.Email;
                richTextBox2.Text = client.Remarks;
                dateTimePicker1.Value = Convert.ToDateTime(client.VisitedDt);
                textBox5.Text = client.TotalAmount?.ToString();
                textBox6.Text = client.TotalFees?.ToString();
                textBox7.Text = client.OtherFees?.ToString();
                textBox8.Text = client.TotalPaid?.ToString();
                textBox8.Text = client.Balance?.ToString();

                // Save client id in Tag for update
                button1.Tag = client.ClientId;
                button1.Text = "Update";
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (MessageBox.Show($"Are you sure to delete '{client.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var db = new BillingDbContext();
                    db.DeleteClient(client.ClientId);
                    LoadClients();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SearchClients(textBox9.Text.Trim());
        }

        private void SearchClients(string searchText)
        {
            var db = new BillingDbContext();
            var allClients = db.GetAllClients();

            // Filter by Name or CaseNumber
            var filtered = allClients
    .Where(c => (!string.IsNullOrEmpty(c.Name) && c.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
             || (!string.IsNullOrEmpty(c.CaseNumber) && c.CaseNumber.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0))
    .ToList();


            dataGridView1.DataSource = filtered;

            // Hide ClientId column if exists
            if (dataGridView1.Columns.Contains("ClientId"))
                dataGridView1.Columns["ClientId"].Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PaymentFrm paymentForm = new PaymentFrm();
            
            paymentForm.ShowDialog();
        }
    }
}
