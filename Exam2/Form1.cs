using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Exam2
{
    /// <summary>
    /// ->****READ ME****<-
    /// Pag mag himo mog button click event pa move under sa "your button event below" ayaw pud idelete 
    /// ang comment also please label with comments which is which. I follow ra unsa akong gi himo
    /// para dali ra i locate ang methods if ever ipa explain ta sa code.
    /// </summary>
    public partial class Form1 : Form
    {
        Connection conn = new Connection();
        StreamReader read;
        StreamWriter write;
        private const string pendingOrdersFile = "Pending Orders.txt";
        private const string completedOrdersFile = "Completed Orders.txt";
        private const string user = "Admin";
        private const string pass = "123";
        private bool isLoggedIn = false;
        private DataTable originalDataTable;


        public Form1()
        {
            InitializeComponent();
            tabControl1.Selecting += new TabControlCancelEventHandler(changeState);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'farmDBDataSet4.Crop_Table' table. You can move, or remove it, as needed.
            this.crop_TableTableAdapter1.Fill(this.farmDBDataSet4.Crop_Table);
            // TODO: This line of code loads data into the 'farmDBDataSet3.Transaction_History_Table' table. You can move, or remove it, as needed.
            this.transaction_History_TableTableAdapter.Fill(this.farmDBDataSet3.Transaction_History_Table);
            // TODO: This line of code loads data into the 'farmDBDataSet2.Sales_Table' table. You can move, or remove it, as needed.
            this.sales_TableTableAdapter.Fill(this.farmDBDataSet2.Sales_Table);
            // TODO: This line of code loads data into the 'farmDBDataSet1.Supply_Table2' table. You can move, or remove it, as needed.
            this.supply_Table2TableAdapter.Fill(this.farmDBDataSet1.Supply_Table2);

            loadListView();

            loadDashboard();
        }

        // Dashboard method
        private void loadListView()
        {
            LoadOrders(pendingOrdersFile, listView4);
            LoadOrders(completedOrdersFile, listView3);
        }
        private void loadDashboard()
        {
            // Show most recent dates
            updateRecentLabel("Crop_Table", label24);
            updateRecentLabel("Supply_Table2", label27);

            // Show crop stock information
            displayStock("Crop_Table", label25, "MAX");
            displayStock("Crop_Table", label26, "MIN");

            // Show supply stock information
            displayStock("Supply_Table2", label28, "MAX");
            displayStock("Supply_Table2", label29, "MIN");

            // Show Ready for sale and in need for resupply
            loadCritical("Crop_Table", listView1);
            loadCritical("Supply_Table2", listView2);
        }

        private void loadCritical(string table, ListView listView)
        {
            SqlConnection kon = conn.getCon();
            string column = "";
            string query = "";
            if (table.Equals("Crop_Table"))
            {
                column = "ProductName";
                query = $"SELECT {column} FROM {table} WHERE Quantity >= 50";
            }

            if (table.Equals("Supply_Table2"))
            {
                column = "SupplyName";
                query = $"SELECT {column} FROM {table} WHERE Quantity < 50";
            }

            

            using (kon)
            {
                kon.Open();

                using (SqlCommand cmd = new SqlCommand(query, kon))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader[column].ToString();
                            ListViewItem item = new ListViewItem(name);


                            
                            listView.Items.Add(item);
                            
                        }

                    }
                }
            }
        }

        // Login
        private void changeState(object sender, TabControlCancelEventArgs e)
        {
            if(!isLoggedIn && e.TabPageIndex != 0)
            {
                e.Cancel = true;
                MessageBox.Show("Please login to access other tabs.");
            }
        }

        private void login(object sender, EventArgs e)
        {
            string username = textBox15.Text;
            string password = textBox16.Text;

            if(username.Equals(user) && password.Equals(pass))
            {
                isLoggedIn = true;  
                MessageBox.Show("Login successful!");
                tabControl1.SelectedIndex = 1;
                textBox15.Clear();
                textBox16.Clear();
            } else
            {
                MessageBox.Show("Incorrect username or password");
            }
        }

        // Button Events
        // Search for sales and history table
        private void transactionSearch(object sender, EventArgs e)
        {
            string name = textBox19.Text;

            filterTable("Transaction History_Table", name);
            textBox19.Clear();
        }

        private void clearTransaction(object sender, EventArgs e)
        {
            resetTable("Transaction History_Table");
        }

        private void filterTable(object sender, EventArgs e)
        {
            string choice = comboBox1.SelectedItem.ToString();
            filter("Transaction History_Table", choice);
        }

        private void filter(string table, string type)
        {
            switch (table)
            {
                case "Sales_Table":
                    if (type.Equals("Ascending"))
                    {
                        salesTableBindingSource.Sort = "Quantity ASC";
                    } 
                    else
                    {
                        salesTableBindingSource.Sort = "Quantity DESC";
                    }
                    break;
                case "Transaction History_Table":
                    transactionHistoryTableBindingSource.Filter = "TransactionType = '" + type + "'";
                    break;
                default:
                    return;
            }
        }
        private void salesSearch(object sender, EventArgs e)
        {
            string name = textBox18.Text;

            filterTable("Sales_Table", name);
            textBox19.Clear();
        }

        private void clearSales(object sender, EventArgs e)
        {
            resetTable("Sales_Table");
        }

        private void clearFilter(object sender, EventArgs e)
        {
            resetTable("Sales_Table");
        }

        private void resetTable(string table)
        {
            switch (table)
            {
                case "Sales_Table":
                    salesTableBindingSource.Filter = string.Empty;
                    break;
                case "Transaction History_Table":
                    transactionHistoryTableBindingSource.Filter = string.Empty;
                    break;
            }
        }

        private void filterTable(string table, string name)
        {
            switch(table)
            {
                case "Sales_Table":
                    salesTableBindingSource.Filter = "ProductName = '"+name+"'";
                    if (salesTableBindingSource.Count == 0)
                    {
                        MessageBox.Show("Item does not exist in Sales.");
                        resetTable("Sales_Table");
                    }
                    break;
                case "Transaction History_Table":
                    transactionHistoryTableBindingSource.Filter = "Item = '" + name + "'";
                    if (transactionHistoryTableBindingSource.Count == 0)
                    {
                        MessageBox.Show("Item does not exist in Transaction History.");
                        resetTable("Transaction History_Table");
                    }
                    break;
            }
        }


        // Dashboard updates
        public void updateRecentLabel(string tableName, Label label)
        {
            SqlConnection kon = conn.getCon();
            string column = "";
            if (tableName.Equals("Crop_Table"))
            {
                column = "DateOfHarvest";
            }

            if (tableName.Equals("Supply_Table2"))
            {
                column = "SupplyDate";
            }

            string query = $"SELECT MAX({column}) FROM {tableName}";
            DateTime recentDate = DateTime.MinValue;

            using (kon)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(query, kon);
                    kon.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        recentDate = Convert.ToDateTime(result); 
                    }
                }
                catch (Exception ex)
                {
                    
                }

                label.Text = recentDate != DateTime.MinValue ? recentDate.ToString("d") : "No dates available";
            }

        }



        public void displayStock(string tableName, Label label, string level)
        {
            SqlConnection kon = conn.getCon();
            string name = "";

            if (tableName.Equals("Crop_Table"))
            {
                name = "ProductName";
            }

            if (tableName.Equals("Supply_Table2"))
            {
                name = "SupplyName";
            }
            string query = $"SELECT TOP 1 {name}, Quantity FROM {tableName} " +
                $"WHERE Quantity = (SELECT {level}(Quantity) FROM {tableName})";

            using (kon)
            {
                kon.Open();
                using (SqlCommand cmd = new SqlCommand(query, kon))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string productName = reader[name].ToString();
                        string quantity = reader["Quantity"].ToString();
                        label.Text = $"{productName}: {quantity}";
                    }
                    else
                    {
                        label.Text = "No data available.";
                    }
                }

            }
        }

        // Supply Button events
        // Update supply
        private void updateSupplyItem(object sender, EventArgs e)
        {
            SqlConnection kon = conn.getCon();
            string supplyIDText = textBox9.Text;  
            string supplyName = textBox13.Text.Trim();
            string category = textBox12.Text;
            string quantityInput = textBox11.Text;
            string supplyDate = textBox10.Text;
            string expense = textBox14.Text;    

            try
            {
                using (kon)
                {
                    kon.Open();

                    // Check if supply exists in the database
                    string checkSupplyQuery = "SELECT Quantity FROM Supply_Table2 WHERE SupplyID = @SupplyID";
                    bool exists = false;
                    int inputStock, currStock = getStock(supplyName, "Supply_Table2"), newStock = 0;
                    int id = int.Parse(supplyIDText);
                    DateTime parsedDate;


                    if (string.IsNullOrEmpty(supplyDate))
                    {
                        parsedDate = DateTime.Now;
                    }
                    else
                    {
                        if (!DateTime.TryParse(supplyDate, out parsedDate))
                        {
                            MessageBox.Show("Invalid date format. Please enter a valid date in the correct format.");
                            return;
                        }
                    }

                    using (SqlCommand checkCmd = new SqlCommand(checkSupplyQuery, kon))
                    {
                        checkCmd.Parameters.AddWithValue("@SupplyID", id);
                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                exists = true;
                                currStock = reader.GetInt32(0);  // Get current stock from database
                            }
                        }
                    }

                    // If the supply doesn't exist, show a message and return
                    if (!exists)
                    {
                        MessageBox.Show("SupplyID not found in inventory.");
                        return;
                    }

                    string updateQuery = buildSupplyUpdateQuery(supplyName, category, quantityInput);

                    if (string.IsNullOrEmpty(updateQuery))
                    {
                        MessageBox.Show("No fields to update. Please fill at least one field.");
                        return;
                    }

                    // Execute the update command
                    using (SqlCommand cmd = new SqlCommand(updateQuery, kon))
                    {
                        if (!string.IsNullOrEmpty(supplyName))
                        {
                            cmd.Parameters.AddWithValue("@SupplyName", supplyName);
                        }

                        if (!string.IsNullOrEmpty(category))
                        {
                            cmd.Parameters.AddWithValue("@Category", category);
                        }

                        if (!string.IsNullOrEmpty(quantityInput) && int.TryParse(quantityInput, out inputStock))
                        {
                            newStock = inputStock + currStock;  
                            cmd.Parameters.AddWithValue("@Quantity", newStock);
                        }

                        cmd.Parameters.AddWithValue("@SupplyDate", parsedDate);

                        cmd.Parameters.AddWithValue("@SupplyID", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Check if any rows were affected
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Supply updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("SupplyID not found in inventory.");
                        }

                        decimal parsedExpense = 0;
                        if (decimal.TryParse(expense, out parsedExpense))
                        {
                            string nameQuery = "SELECT SupplyName FROM Supply_Table2 WHERE SupplyID = @SupplyID";
                            string retrievedSupplyName = "";

                            using (SqlCommand nameCmd = new SqlCommand(nameQuery, kon))
                            {
                                nameCmd.Parameters.AddWithValue("@SupplyID", id);

                                using (SqlDataReader reader = nameCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        retrievedSupplyName = reader["SupplyName"].ToString();
                                    }
                                }
                            }

                            // Only proceed if SupplyName is retrieved
                            if (!string.IsNullOrEmpty(retrievedSupplyName))
                            {
                                // Insert the history record
                                insertHistory(retrievedSupplyName, int.Parse(quantityInput), "Expense", parsedExpense);
                            }

                            MessageBox.Show("Expense recorded successfully.");
                        }
                    }
                }


            

                clearSupplyText();

                // Reload the inventory
                this.supply_Table2TableAdapter.Fill(this.farmDBDataSet1.Supply_Table2);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        
        private string buildSupplyUpdateQuery(string supplyName, string category, string quantityText)
        {
            // Prepare the update query dynamically
            string query = "UPDATE Supply_Table2 SET ";
            List<string> updateFields = new List<string>();

            if (!string.IsNullOrEmpty(supplyName))
            {
                updateFields.Add("SupplyName = @SupplyName");
            }

            if (!string.IsNullOrEmpty(category))
            {
                updateFields.Add("Category = @Category");
            }

            if (!string.IsNullOrEmpty(quantityText))
            {
                updateFields.Add("Quantity = @Quantity");
            }

            updateFields.Add("SupplyDate = @SupplyDate");

            // If no fields are updated, show a message and return
            if (updateFields.Count == 0)
            {
                return null;
            }

            // Finalize the query by adding the conditions
            query += string.Join(", ", updateFields) + " WHERE SupplyID = @SupplyID";
            return query;
        }

        // Remove supply
        private void button5_Click(object sender, EventArgs e)
        {
            SqlConnection kon = conn.getCon();
            string supplyIDText = textBox9.Text; // Get the SupplyID from the textbox

            if (string.IsNullOrEmpty(supplyIDText))
            {
                MessageBox.Show("Please enter a SupplyID.");
                return;
            }

            if (!int.TryParse(supplyIDText, out int supplyID))
            {
                MessageBox.Show("Invalid SupplyID. Please enter a valid number.");
                return;
            }

            string query = "DELETE FROM Supply_Table2 WHERE SupplyID = @SupplyID";

            using (kon)
            {
                SqlCommand command = new SqlCommand(query, kon);
                command.Parameters.AddWithValue("@SupplyID", supplyID);

                try
                {
                    kon.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Supply entry removed successfully!");

                        // Clear the SupplyID textbox after deletion
                        textBox9.Clear();

                        // Refresh the inventory to show the updated list of supplies
                        this.supply_Table2TableAdapter.Fill(this.farmDBDataSet1.Supply_Table2);
                    }
                    else
                    {
                        MessageBox.Show("SupplyID not found in the inventory.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        // Add supply
        private void button6_Click(object sender, EventArgs e)
        {
            SqlConnection kon = conn.getCon();
            string supplyName = textBox13.Text;
            string category = textBox12.Text;
            string quantityText = textBox11.Text;
            string supplyDate = textBox10.Text;
            decimal expense = 0;
            DateTime formattedDate;

            if (!decimal.TryParse(textBox14.Text, out expense))
            {
                MessageBox.Show("Please enter expenses.");
                return;
            }
            if (!DateTime.TryParse(supplyDate, out formattedDate))
            {
                MessageBox.Show("Please enter a valid supply date.");
                return;
            }

            string query = "INSERT INTO Supply_Table2 (SupplyName, Category, Quantity, SupplyDate) " +
                           "VALUES (@SupplyName, @Category, @Quantity, @SupplyDate)";

            using (kon)
            {
                SqlCommand command = new SqlCommand(query, kon);
                command.Parameters.AddWithValue("@SupplyName", supplyName);
                command.Parameters.AddWithValue("@Category", category);

                if (!int.TryParse(quantityText, out int quantity))
                {
                    MessageBox.Show("Please enter a valid quantity.");
                    return;
                }
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@SupplyDate", supplyDate);

                try
                {
                    kon.Open();
                    command.ExecuteNonQuery();

                    MessageBox.Show("Entry added successfully to the database.");

                    insertHistory(supplyName, quantity, "Expense", expense);
                    clearSupplyText();
                    loadDashboard();

                    this.supply_Table2TableAdapter.Fill(this.farmDBDataSet1.Supply_Table2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }   
            }
        }

        private void clearSupplyText()
        {
            textBox14.Clear();
            textBox13.Clear();
            textBox12.Clear();
            textBox11.Clear();
            textBox10.Clear();
        }

        // Supply Filtering
        private void clearSupplyFilter(object sender, EventArgs e)
        {
            getOriginalTable();
            supplyTable2BindingSource.RemoveFilter();
        }

        private void updateCategory(object sender, EventArgs e)
        {
            string selectedCategory = comboBox3.SelectedItem.ToString();
            string viewMode = comboBox2.SelectedItem.ToString();
            getOriginalTable();
            Dictionary<int, string> monthMap = getMonthMap();
            
            if (!string.IsNullOrEmpty(selectedCategory) && viewMode.Equals("Category"))
            {
                supplyTable2BindingSource.Filter = "Category = '" + selectedCategory + "'";
            }

            if (monthMap.ContainsValue(selectedCategory))
            {
                int monthNumber = monthMap.First(m => m.Value.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase)).Key;
                var filteredRows = from row in originalDataTable.AsEnumerable()
                                   where row["SupplyDate"] != DBNull.Value
                                   let supplyDate = (DateTime)row["SupplyDate"]
                                   where supplyDate.Month == monthNumber
                                   select row;

                DataTable filteredTable = filteredRows.CopyToDataTable();
                supplyTable2BindingSource.DataSource = filteredTable;

            }
        }

        private void selectViewMode(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                populateCategory();
            }

            if (comboBox2.SelectedIndex == 1)
            {
                populateSupplyDate();
            }
        }

        private void populateSupplyDate()
        {
            SqlConnection kon = conn.getCon();
            string query = "SELECT DISTINCT MONTH(SupplyDate) AS MonthNumber FROM Supply_Table2";            Dictionary<int, string> monthMap = getMonthMap();
            using (kon)
            {
                try
                {
                    kon.Open();

                    using (SqlCommand cmd = new SqlCommand(query, kon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            comboBox3.Items.Clear();

                            while (reader.Read())
                            {
                                
                                string monthString = reader["MonthNumber"].ToString();
                                int monthNumber = Convert.ToInt32(monthString); 

                                if (monthMap.ContainsKey(monthNumber))
                                {
                                    comboBox3.Items.Add(monthMap[monthNumber]);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void getOriginalTable()
        {
            string query = "SELECT * FROM Supply_Table2";  

            using (SqlConnection kon = conn.getCon())
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, kon);
                originalDataTable = new DataTable();  
                adapter.Fill(originalDataTable);  
            }

            supplyTable2BindingSource.DataSource = originalDataTable;
        }

        private Dictionary<int, string> getMonthMap()
        {
            Dictionary<int, string> monthMap = new Dictionary<int, string>()
            {
                { 1, "January" },
                { 2, "February" },
                { 3, "March" },
                { 4, "April" },
                { 5, "May" },
                { 6, "June" },
                { 7, "July" },
                { 8, "August" },
                { 9, "September" },
                { 10, "October" },
                { 11, "November" },
                { 12, "December" }
            };

            return monthMap;
        }

        private void populateCategory()
        {
            SqlConnection kon = conn.getCon();
            string query = "SELECT DISTINCT Category FROM Supply_Table2";
            using (kon)
            {
                try
                {
                    kon.Open();

                    using (SqlCommand cmd = new SqlCommand(query, kon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            comboBox3.Items.Clear();

                            while (reader.Read())
                            {
                                string supplyName = reader["Category"].ToString();
                                comboBox3.Items.Add(supplyName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        // Crop button events
        // Add crop item
        private void button3_Click_1(object sender, EventArgs e)
        {
            string productName = textBox4.Text;
            string quantity = textBox5.Text;
            string pricePerUnit = textBox6.Text;
            DateTime dateOfHarvest = DateTime.Now;
            string expiryDate = textBox7.Text;

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(quantity) || string.IsNullOrEmpty(pricePerUnit) || string.IsNullOrEmpty(expiryDate))
            {
                MessageBox.Show("Please fill all the fields.");
                return;
            }

            try
            {
                // SQL connection and command
                SqlConnection kon = conn.getCon();
                using (kon)
                {
                    kon.Open();
                    string query = "INSERT INTO Crop_Table (ProductName, Quantity, SalesPricePerUnit,DateOfHarvest, ExpiryDate) " +
                                   "VALUES (@ProductName, @Quantity, @SalesPricePerUnit, @DateOfHarvest, @ExpiryDate)";

                    using (SqlCommand cmd = new SqlCommand(query, kon))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        cmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(quantity));
                        cmd.Parameters.AddWithValue("@SalesPricePerUnit", Convert.ToDecimal(pricePerUnit));
                        cmd.Parameters.AddWithValue("@DateOfHarvest", dateOfHarvest);
                        cmd.Parameters.AddWithValue("@ExpiryDate", DateTime.Parse(expiryDate));

                        cmd.ExecuteNonQuery();
                    }
                }

                // Clear the text boxes
                clearCropText();

                loadDashboard();

                this.crop_TableTableAdapter1.Fill(this.farmDBDataSet4.Crop_Table);

                MessageBox.Show("Crop added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Delete Crop item
        private void button4_Click(object sender, EventArgs e)
        {
            
            string productIDText = textBox8.Text;

            if (string.IsNullOrEmpty(productIDText))
            {
                MessageBox.Show("Please enter a ProductID.");
                return;
            }

            if (!int.TryParse(productIDText, out int productID))
            {
                MessageBox.Show("Invalid ProductID. Please enter a valid number.");
                return;
            }

            try
            {
                
                SqlConnection kon = conn.getCon();
                 using (kon)
                {
                    kon.Open();
                    string query = "DELETE FROM Crop_Table WHERE ProductID = @ProductID";

                    using (SqlCommand cmd = new SqlCommand(query, kon))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", productID);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Crop removed successfully!");

                            this.crop_TableTableAdapter.Fill(this.farmDBDataSet.Crop_Table);
                        }
                        else
                        {
                            MessageBox.Show("ProductID not found in inventory.");
                        }
                    }
                }

                loadDashboard();
                textBox8.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Update crop
        private void updateCrop_button(object sender, EventArgs e)
        {
            SqlConnection kon = conn.getCon();
            string productIDText = textBox8.Text;
            string productName = textBox4.Text;
            string quantity = textBox5.Text;
            string pricePerUnit = textBox6.Text;
            DateTime sqlDate = DateTime.Now;
            string expiryDate = textBox7.Text;

            if (!int.TryParse(productIDText, out int productID))
            {
                MessageBox.Show("Invalid ProductID. Please enter a valid number.");
                return;
            }

            try
            {
                using (kon)
                {
                    kon.Open();

                    string checkProductQuery = "SELECT Quantity FROM Crop_Table WHERE ProductID = @ProductID";
                    int inputStock = 0;
                    int currStock = getStock(productName, "Crop_Table");
                    int newStock = 0;
                    int id = int.Parse(productIDText);

                    using (SqlCommand checkCmd = new SqlCommand(checkProductQuery, kon))
                    {
                        checkCmd.Parameters.AddWithValue("@ProductID", productID);
                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currStock = reader.GetInt32(0);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(quantity) && int.TryParse(quantity, out inputStock))
                    {
                        newStock = inputStock + currStock; 
                    }

                    string updateQuery = buildCropUpdateQuery(productName, quantity, pricePerUnit, expiryDate);

                    updateCrop(kon, updateQuery, id, productName, quantity, newStock, pricePerUnit, sqlDate, expiryDate);
                }

                clearCropText();
                loadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Utility methods for update crop
        private void updateCrop(SqlConnection kon, string query, int id, string productName, string quantity, int newStock, string pricePerUnit, DateTime sqlDate, string expiryDate)
        {
            using (SqlCommand cmd = new SqlCommand(query, kon))
            {
                cmd.Parameters.AddWithValue("@ProductID", id);

                if (!string.IsNullOrEmpty(productName))
                {
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                }

                if (!string.IsNullOrEmpty(quantity))
                {
                    cmd.Parameters.AddWithValue("@Quantity", newStock);
                }

                if (!string.IsNullOrEmpty(pricePerUnit))
                {
                    if (decimal.TryParse(pricePerUnit, out decimal price))
                    {
                        cmd.Parameters.AddWithValue("@SalesPricePerUnit", price);
                    }
                }

                cmd.Parameters.AddWithValue("@DateOfHarvest", sqlDate);

                if (!string.IsNullOrEmpty(expiryDate))
                {
                    if (DateTime.TryParse(expiryDate, out DateTime expiry))
                    {
                        cmd.Parameters.AddWithValue("@ExpiryDate", expiry);
                    }
                }

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Crop updated successfully!");
                    this.crop_TableTableAdapter1.Fill(this.farmDBDataSet4.Crop_Table);
                }
                else
                {
                    MessageBox.Show("ProductID not found in inventory.");
                }
            }
        }

        private string buildCropUpdateQuery(string name, string quantity, string price, string expiry)
        {
            string query = "UPDATE Crop_Table SET ";
            List<string> updateFields = new List<string>();

            if (!string.IsNullOrEmpty(name))
            {
                updateFields.Add("ProductName = @ProductName");
            }

            if (!string.IsNullOrEmpty(quantity))
            {
                updateFields.Add("Quantity = @Quantity");
            }

            if (!string.IsNullOrEmpty(price))
            {
                updateFields.Add("SalesPricePerUnit = @SalesPricePerUnit");
            }

            updateFields.Add("DateOfHarvest = @DateOfHarvest");

            if (!string.IsNullOrEmpty(expiry))
            {
                updateFields.Add("ExpiryDate = @ExpiryDate");
            }

            if (updateFields.Count == 0)
            {
                MessageBox.Show("No fields to update. Please fill at least one field.");
                return null;
            }

            query += string.Join(", ", updateFields) + " WHERE ProductID = @ProductID";

            return query;
        }
        private void clearCropText()
        {
            textBox8.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();
        }

        // Crop Filtering
        private void searchCrop(object sender, EventArgs e)
        {
            string productName = textBox17.Text;
            searchName(productName);
        }
        private void clearSearch(object sender, EventArgs e)
        {
            cropTableBindingSource1.Filter = string.Empty;
        }
        private void filterCrop(object sender, EventArgs e)
        {
            int selection = comboBox4.SelectedIndex;

            filterCropsBy(selection);
        }
        private void filterCropsBy(int selection)
        {
            switch (selection)
            {
                case 0: 
                    cropTableBindingSource1.Sort = "Quantity ASC";
                    break;
                case 1: 
                    cropTableBindingSource1.Sort = "Quantity DESC";
                    break;
                case 2: 
                    cropTableBindingSource1.Sort = "SalesPricePerUnit ASC";
                    break;
                case 3: 
                    cropTableBindingSource1.Sort = "SalesPricePerUnit DESC";
                    break;
                default:
                    MessageBox.Show("Invalid selection.");
                    break;
            }
        }

        private void searchName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                cropTableBindingSource1.Filter = "ProductName = '" + name + "'";
            } else
            {
                MessageBox.Show("Search query empty! Please enter a name.");
                return;
            }
        }
        private void clearSort(object sender, EventArgs e)
        {
            cropTableBindingSource1.Sort = string.Empty;
        }

        // Order button events
        // Add order(no sql yet)
        private void button1_Click(object sender, EventArgs e)
        {
            string product = textBox1.Text;
            string quantity = textBox2.Text;
            DateTime currentDate = DateTime.Now;
            string sqlDate = currentDate.ToString("yyyy-MM-dd");

            ListViewItem orderDisplay = new ListViewItem(product);
            orderDisplay.SubItems.Add(quantity);
            orderDisplay.SubItems.Add(sqlDate);

            listView4.Items.Add(orderDisplay);
            SaveOrders(product, quantity, sqlDate, pendingOrdersFile);
            textBox1.Clear();
            textBox2.Clear();
        }

        // Sell order(no sql yet)
        private void button2_Click(object sender, EventArgs e)
        {
            string selectedProduct = textBox3.Text;
            DateTime currentDate = DateTime.Now;
            string sqlDate = currentDate.ToString("yyyy-MM-dd");
            decimal price = getPrice(selectedProduct);

            try
            {
                ListViewItem item = null;
                string product = null;
                int quantity = 0;
                

                foreach (ListViewItem items in listView4.Items)
                {
                    if(items.Text.Equals(selectedProduct, StringComparison.OrdinalIgnoreCase))
                    {
                        item = items;
                        product = items.Text;
                        if((int.TryParse(items.SubItems[1].Text, out quantity) && quantity > 0))
                        {
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Invalid quantity or quantity must be greater than zero.");
                            return;
                        }
                       
                    }
                }

                decimal sale = price * quantity;

                if (item == null)
                {
                    MessageBox.Show("Product not found!");
                } 
                else
                {
                  
                    insertSalesQuery(product, quantity, price);
                    insertHistory(product, quantity, "Sale", sale);
                    SaveOrders(product, quantity.ToString(), sqlDate, completedOrdersFile);
                    moveToFinished(product);

                    textBox3.Clear();
                }
            }
            catch(ArgumentOutOfRangeException ex)
            {
                MessageBox.Show($"Pending Orders empty!");
                textBox3.Clear();
            } 
        }

        // Insert to sales table
        public void insertSalesQuery(string productName, int quantity, decimal price)
        {
            SqlConnection kon = conn.getCon();

            string checkProductQuery = "SELECT Quantity, Revenue FROM Sales_Table WHERE ProductName = @ProductName";
            string insertSalesQuery = "INSERT INTO Sales_Table (ProductName, Quantity, Revenue) VALUES (@ProductName, @Quantity, @Revenue)";
            string updateStockQuery = "UPDATE Crop_Table SET Quantity = @NewStock WHERE ProductName = @ProductName";
            string updateSalesQuery = "UPDATE Sales_Table SET Quantity = @Quantity, Revenue = @Revenue WHERE ProductName = @ProductName";


            int currStock = getStock(productName, "Crop_Table");
            int newStock = currStock - quantity;


            if (newStock < 0)
            {
                MessageBox.Show($"Insufficient stock for Product: {productName}!");
                return;
            }

            decimal revenue = price * quantity;

            using (kon)
            {
                kon.Open();

                bool exists = false;
                int currQuantity = 0;
                decimal currRevenue = 0;

                using (SqlCommand checkCmd = new SqlCommand(checkProductQuery, kon))
                {
                    checkCmd.Parameters.AddWithValue("@ProductName", productName);
                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            exists = true;
                            currQuantity = reader.GetInt32(0);
                            currRevenue = Convert.ToDecimal(reader[1]);
                        }
                    }
                }

                if (exists)
                {
                    int updateQuantity = currQuantity + quantity;
                    decimal updateRevenue = currRevenue + revenue;

                    // Insert into Sales_Table
                    using (SqlCommand cmd = new SqlCommand(updateSalesQuery, kon))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        cmd.Parameters.AddWithValue("@Quantity", updateQuantity);
                        cmd.Parameters.AddWithValue("@Revenue", updateRevenue);
                        cmd.ExecuteNonQuery();
                    }

                } else
                {
                    // Insert into Sales_Table
                    using (SqlCommand cmd = new SqlCommand(insertSalesQuery, kon))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@Revenue", revenue);
                        cmd.ExecuteNonQuery();
                    }
                }
                

                // Update Crop_Table stock
                using (SqlCommand updateCmd = new SqlCommand(updateStockQuery, kon))
                {
                    updateCmd.Parameters.AddWithValue("@NewStock", newStock);
                    updateCmd.Parameters.AddWithValue("@ProductName", productName);
                    updateCmd.ExecuteNonQuery();
                }

                // Refresh the data in the tables
                this.crop_TableTableAdapter1.Fill(this.farmDBDataSet4.Crop_Table);
                this.sales_TableTableAdapter.Fill(this.farmDBDataSet2.Sales_Table);
            }
        }

        // Get Stock
        public int getStock(string productName, string table)
        {
            SqlConnection kon = conn.getCon();

            string column = "";
            if (table.Equals("Crop_Table"))
            {
                column = "ProductName";
            }

            if (table.Equals("Supply_Table2"))
            {
                column = "SupplyName";
            }

            string query = $"SELECT Quantity FROM {table} WHERE {column} = @ProductName";
            int stock = 0;

            using (kon)
            {
                kon.Open();

                using (SqlCommand cmd = new SqlCommand(query, kon))
                {
                    cmd.Parameters.AddWithValue("@ProductName", productName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                stock = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }

            return stock;
        }

        // Get sales price
        public decimal getPrice(string productName)
        {
            SqlConnection kon = conn.getCon();
            string query = "SELECT ProductID, ProductName, Quantity, SalesPricePerUnit, ExpiryDate FROM Crop_Table WHERE ProductName = @ProductName";
            decimal price = 0;

            using (kon)
            {
                kon.Open();

                using (SqlCommand cmd = new SqlCommand(query, kon))
                {
                    cmd.Parameters.AddWithValue("@ProductName", productName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(3))
                            {
                                price = reader.GetDecimal(3);
                            }
                        }
                    }
                }
            }

            return price;
        }

        // Insert to transaction history
        public void insertHistory(string item, int quantity, string type, decimal cash)
        {
            SqlConnection kon = conn.getCon();
            string query = "INSERT INTO [Transaction History_Table] (Item, Quantity, TransactionType, Cash, Date) VALUES (@Item, @Quantity, @TransactionType, @Cash, @Date)";
          
            
            using (kon)
            {
                kon.Open();

                using (SqlCommand cmd = new SqlCommand(query, kon))
                {
                    cmd.Parameters.AddWithValue("@Item", item);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@TransactionType", type);
                    cmd.Parameters.AddWithValue("@Cash", cash);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                
                this.transaction_History_TableTableAdapter.Fill(this.farmDBDataSet3.Transaction_History_Table);
            }

            
        }

        // Other ListView Methods for order
        private void moveToFinished(string product)
        {
            if (string.IsNullOrEmpty(product))
            {
                MessageBox.Show("Please enter a product.");
                return;
            }

            foreach (ListViewItem item in listView4.Items)
            {
                if (!item.Text.Equals(product, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ListViewItem completedOrder = new ListViewItem(item.Text);
                for (int i = 1; i < item.SubItems.Count; i++)
                {
                    completedOrder.SubItems.Add(item.SubItems[i].Text);
                }

                MessageBox.Show($"Order for '{product}' complete!");
                listView3.Items.Add(completedOrder);
                listView4.Items.Remove(item);
                RemoveFromFile(product, pendingOrdersFile);
                return;
            }

            MessageBox.Show($"Order for '{product}' not found!");
        }

        // File handling for orders
        private void LoadOrders(string file, ListView listview)
        {
            if (File.Exists(file))
            {
                using (read = new StreamReader(file))
                {
                    string line;
                    while ((line = read.ReadLine()) != null)
                    {
                        string[] columns = line.Split(',');
                        ListViewItem item = new ListViewItem(columns[0]);

                        for (int i = 1; i < columns.Length; i++)
                        {
                            item.SubItems.Add(columns[i]);
                        }

                        listview.Items.Add(item);
                    }
                }
            }
        }

        private void SaveOrders(string product, string quantity, string date, string file)
        {
            using (StreamWriter write = new StreamWriter(file, true))
            {
                write.WriteLine($"{product},{quantity},{date}");
            }
        }

        private void RemoveFromFile(string product, string file)
        {
            List<string> lines = new List<string>();

            using (read = new StreamReader(file))
            {
                string line;
                while ((line = read.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    if (!columns[0].Equals(product, StringComparison.OrdinalIgnoreCase))
                    {
                        lines.Add(line);
                    }
                }
            }

            using (write = new StreamWriter(file))
            {
                foreach (string remainingLine in lines)
                {
                    write.WriteLine(remainingLine);
                }
            }
        }

       
    }
}