using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projections_App_V1
{
    public partial class Projections_App_V1 : Form
    {
        public Projections_App_V1()
        {
            InitializeComponent();
           
            txtInitialBalance.TextChanged += (sender, e) => UpdateBalances();
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellEndEdit += (s, e) => dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            dataGridView1.CellClick += dataGridView1_CellClick;

           //Method for Dateblock edit
            void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
            {
                // Check if the edit is in the Input Date column
                if (e.ColumnIndex == dataGridView1.Columns["Input Date"].Index)
                {
                    // Cancel the edit operation
                    e.Cancel = true;
                }
            }



            //defining what to do on cell click in Input Date
            void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                if (e.ColumnIndex == dataGridView1.Columns["Input Date"].Index && e.RowIndex >= 0)
                {
                    // Initialize a new DateTimePicker control
                    DateTimePicker picker = new DateTimePicker();
                    // Set its format
                    picker.Format = DateTimePickerFormat.Custom;

                    // Place the DateTimePicker control over the cell
                    dataGridView1.Controls.Add(picker);
                    picker.Location = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                    picker.Width = dataGridView1.Columns[e.ColumnIndex].Width;
                    picker.Visible = true;

                    // Handle the CloseUp event to apply the selected date
                    picker.CloseUp += (s, args) =>
                    {
                        dataGridView1.CurrentCell.Value = picker.Value.ToShortDateString();
                        picker.Visible = false;
                        dataGridView1.Controls.Remove(picker); // Ensure the picker is removed after selection
                        picker.Dispose(); // Dispose the picker to clean up resources

                        ReorderRowsByDate();
                    };

                    // Handle the Leave event to remove the DateTimePicker from the DataGridView
                    picker.Leave += (s, args) => picker.Visible = false;
                }
            }

            //row and font setup below
            dataGridView1.RowTemplate.Height = 32; // Adjust the value as needed for row height
            dataGridView1.DefaultCellStyle.Font = new Font(dataGridView1.Font.FontFamily, 12, FontStyle.Regular);
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
            //Making the rows and columns for the data to sit in
            //Must be 3 rows if 3 columns 

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("StartingBalance", typeof(decimal));
            dataTable.Columns.Add("Charged Amnt", typeof(decimal));
            dataTable.Columns.Add("Remaining Bal.", typeof(decimal));
            dataTable.Columns.Add("Input Date", typeof(DateTime));
            foreach (DataRow row in dataTable.Rows)
            {
                row["Remaining Bal."] = DBNull.Value; // Sets the initial value to be blank
            }

            dataTable.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value);
            dataTable.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value);
            dataTable.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value);

            //Formatting numbers to currency

            void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
            {
                // Ensure the columns exist before attempting to format them
                if (dataGridView1.Columns.Contains("StartingBalance"))
                    dataGridView1.Columns["StartingBalance"].DefaultCellStyle.Format = "C2";
                if (dataGridView1.Columns.Contains("Charged Amnt"))
                    dataGridView1.Columns["Charged Amnt"].DefaultCellStyle.Format = "C2";
                if (dataGridView1.Columns.Contains("Remaining Bal."))
                    dataGridView1.Columns["Remaining Bal."].DefaultCellStyle.Format = "C2";
                    dataGridView1.Columns["Input Date"].DefaultCellStyle.Format = "d"; // Short date pattern

            }
            //adding datasource to datatable as defined above
            //Making remaining/starting balance blocks read only
            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns["Remaining Bal."].ReadOnly = true;
            dataGridView1.Columns["StartingBalance"].ReadOnly = true;

            //logic for data change in a cell



            //trying to get text box to update the starting balance here
           void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
            {
                // If the change is in the Charged Amnt column, update balances.
                if (e.ColumnIndex == dataGridView1.Columns["Charged Amnt"].Index)
                {
                    UpdateBalances();
                }
            }



           

                    
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Math implementation
            if (e.ColumnIndex == 0 || e.ColumnIndex == 1) // Assuming columns 0 and 1 are StartingBalance and Charged Amnt
            {
                var row = dataGridView1.Rows[e.RowIndex];
                if (int.TryParse(row.Cells["StartingBalance"].Value?.ToString(), out int startingBalance) &&
                    int.TryParse(row.Cells["Charged Amnt"].Value?.ToString(), out int chargeBlock))
                {
                    row.Cells["Remaining Bal."].Value = startingBalance - chargeBlock;
                }
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
        //Updating the balances automatically based on an initial value in the text box
        private void UpdateBalances()
        {
            decimal initialBalance;
            if (!decimal.TryParse(txtInitialBalance.Text, out initialBalance))
            {
                MessageBox.Show("Invalid entry. Please enter a valid number.");
                return;
            }

            decimal currentBalance = initialBalance;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                // Assuming the user inputs the charge amount
                decimal chargeAmount = 0m;
                if (decimal.TryParse((row.Cells["Charged Amnt"].Value ?? "0").ToString(), out chargeAmount))
                {
                    // Set the starting balance for the current row
                    if (row.Index == 0 || dataGridView1.Rows.Count == 1) // Check if it's the first row or there's only one row
                    {
                        row.Cells["StartingBalance"].Value = initialBalance;
                    }
                    else
                    {
                        row.Cells["StartingBalance"].Value = currentBalance; // Use the remaining balance from the previous operation as the starting balance
                    }

                    // Calculate the remaining balance for the current row
                    currentBalance -= chargeAmount;
                    row.Cells["Remaining Bal."].Value = currentBalance;
                }
            }
        }


        //Trying to save the data to csv so user can use it again
        //Need to add button so they can save there is no File Save option or header
        private void SaveDataTableToCsv(DataTable dataTable, string filePath)
        {
            StringBuilder csvContent = new StringBuilder();

            // Optional: Write column headers
            string[] columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            csvContent.AppendLine(string.Join(",", columnNames));

            // Write rows
            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString().Replace(",", string.Empty)); // Ensure no commas in fields
                csvContent.AppendLine(string.Join(",", fields));
            }

            // Save to file
            System.IO.File.WriteAllText(filePath, csvContent.ToString());
        }
        private DataTable LoadDataTableFromCsv(string filePath)
        {
            DataTable dataTable = new DataTable();

            // Initialize columns in your DataTable if necessary
            dataTable.Columns.Add("StartingBalance", typeof(decimal));
            dataTable.Columns.Add("Charged Amnt", typeof(decimal));
            dataTable.Columns.Add("Remaining Bal.", typeof(decimal));

            // Read the CSV file line by line
            string[] lines = System.IO.File.ReadAllLines(filePath);

            // Optional: Assume first line contains column names and skip it
            for (int i = 1; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(',');
                DataRow row = dataTable.NewRow();
                row["StartingBalance"] = int.Parse(fields[0]);
                row["Charged Amnt"] = int.Parse(fields[1]);
                row["Remaining Bal."] = int.Parse(fields[2]); // Adjust this based on your actual CSV structure and data types
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
       private void ReorderRowsByDate()
        {
            DataTable dataTable = dataGridView1.DataSource as DataTable;
            if (dataTable != null)
            {
                dataTable.DefaultView.Sort = "Input Date ASC"; // Sorts the rows in ascending order by the Input Date column
                dataGridView1.DataSource = dataTable.DefaultView.ToTable();
                UpdateBalances();
            }
        }
    }


   

}
