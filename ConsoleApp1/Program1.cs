using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program1
    {
        static void Main1(string[] args)
        {

            string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string LogFolder = @"C:\Log\";
            try
            {
                SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
                //Provide the folder path where excel files are present
                String FolderPath = @"C:\Users\amrita.dutta\Desktop\SW Cleansed Readings";
                String TableName = "";
                //Provide the schema for tables in which we want to load Excel files
                String SchemaName = "dbo";
                //Provide the Database Name in which table or view exists
                string DatabaseName = "SWRSSDb_1403";
                //Provide the SQL Server Name 
                string SQLServerName = "rssdbservertest.database.windows.net";

                var directory = new DirectoryInfo(FolderPath);
                FileInfo[] files = directory.GetFiles();

                //Declare and initilize variables
                string fileFullPath = "";

                foreach (FileInfo file in files)
                {
                    fileFullPath = FolderPath + "\\" + file.Name;

                    //Create Excel Connection
                    string ConStr;
                    string HDR;
                    HDR = "YES";
                    ConStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileFullPath
                        + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
                    OleDbConnection cnn = new OleDbConnection(ConStr);

                    //Remove All Numbers and other characters and leave alphabets for name
                    //System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z]");
                    TableName = file.Name.Replace("csv", "");

                    //Get Sheet Name
                    cnn.Open();
                    DataTable dtSheet = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    string sheetname;
                    sheetname = "Sheet1$";

                    //foreach (DataRow drSheet in dtSheet.Rows)
                    //{
                    //    if (drSheet["TABLE_NAME"].ToString().Contains("$"))
                    //    {
                    //        sheetname = drSheet["TABLE_NAME"].ToString();                        
                    //    }
                    //}

                    //Load the DataTable with Sheet Data so we can get the column header
                    OleDbCommand oconn = new OleDbCommand("select * from [" + sheetname + "]", cnn);
                    OleDbDataAdapter adp = new OleDbDataAdapter(oconn);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    cnn.Close();


                    connBuilder.DataSource = "rssdbservertest.database.windows.net";
                    connBuilder.UserID = "rssdbserver";
                    connBuilder.Password = "password@2";
                    connBuilder.InitialCatalog = "SWRSSDb_1403";
                    SqlConnection connection = new SqlConnection(connBuilder.ConnectionString);
                    //connection.Open();

                    string ExcelHeaderColumn = "";
                    string SQLQueryToGetMatchingColumn = "";
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (i != dt.Columns.Count - 1)
                            ExcelHeaderColumn += "'" + dt.Columns[i].ColumnName + "'" + ",";
                        else
                            ExcelHeaderColumn += "'" + dt.Columns[i].ColumnName + "'";
                    }

                    SQLQueryToGetMatchingColumn = "select STUFF((Select  ',['+Column_Name+']' from Information_schema.Columns where Table_Name='" +
                    "Dummy1" + "' and Table_SChema='" + SchemaName + "'" +
                                 "and Column_Name in (" + @ExcelHeaderColumn + ") for xml path('')),1,1,'') AS ColumnList";


                    //Get Matching Column List from SQL Server
                    string SQLColumnList = "";
                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = SQLQueryToGetMatchingColumn;
                    connection.Open();
                    SQLColumnList = Convert.ToString(cmd.ExecuteScalar());
                    connection.Close();


                    //Use Actual Matching Columns to get data from Excel Sheet
                    OleDbConnection cnn1 = new OleDbConnection(ConStr);
                    cnn1.Open();
                    OleDbCommand oconn1 = new OleDbCommand("select " + SQLColumnList
                        + " from [" + sheetname + "]", cnn1);
                    OleDbDataAdapter adp1 = new OleDbDataAdapter(oconn1);
                    DataTable dt1 = new DataTable();
                    adp1.Fill(dt1);
                    cnn1.Close();

                    //Delete the row if all values are nulll
                    int columnCount = dt1.Columns.Count;
                    for (int i = dt1.Rows.Count - 1; i >= 0; i--)
                    {
                        bool allNull = true;
                        for (int j = 0; j < columnCount; j++)
                        {
                            if (dt1.Rows[i][j] != DBNull.Value)
                            {
                                allNull = false;
                            }
                        }
                        if (allNull)
                        {
                            dt1.Rows[i].Delete();
                        }
                    }
                    dt1.AcceptChanges();


                    connection.Open();


                    using (SqlBulkCopy BC = new SqlBulkCopy(connection))
                    {
                        BC.DestinationTableName = "Dummy1";
                        foreach (var column in dt1.Columns)
                            BC.ColumnMappings.Add(column.ToString(), column.ToString());
                        BC.WriteToServer(dt);
                    }

                }

                   
                //using (SqlConnection connection = new SqlConnection(connBuilder.ConnectionString))
                //{
                //    connection.Open();
                //    StringBuilder strBuilder = new StringBuilder();
                //    strBuilder.Append("SELECT AssetName as ProductName ");
                //    strBuilder.Append("FROM Asset as p");
                //    string cmdText = strBuilder.ToString();
                //    using (SqlCommand sqlCmd = new SqlCommand(cmdText, connection))
                //    {
                //        using (SqlDataReader sqlReader = sqlCmd.ExecuteReader())
                //        {
                //            while (sqlReader.Read())
                //            {
                //                Console.WriteLine("{0}", sqlReader.GetString(0));
                //            }
                //        }
                //    }
                //}
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.InnerException);
            }
            Console.WriteLine("Process Complete");
        }
    }
}
