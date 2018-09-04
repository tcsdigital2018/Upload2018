using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //New changes one
            string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string LogFolder = @"C:\Log\";
            string filename1 = string.Empty;
            try
            {
                SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
                //Declare Variables and provide values
                string SourceFolderPath = @"C:\Users\amrita.dutta\Desktop\RevisedWaterLevel";  //Provide the Source Folder where files are present
                string FileExtension = ".csv"; //Provide the extension of files you need to load, can be .txt or .csv
                string FileDelimiter = ","; // provide the file delimiter such as comma or pipe
                string ArchiveFolder = @"C:\Users\amrita.dutta\Desktop\Archive"; //Provide the archive folder path where files will be moved
                string TableName = "dbo.SWCleansedReadings_RevisedV1"; //Provide the table name in which you would like to load the files.

                
                //Create Connection to SQL Server in which you like to load files
                connBuilder.DataSource = "test";
                connBuilder.UserID = "test";
                connBuilder.Password = "12@2";
                connBuilder.InitialCatalog = "test1";


                SqlConnection connection = new SqlConnection(connBuilder.ConnectionString);

                //Reading file names one by one
                string[] fileEntries = Directory.GetFiles(SourceFolderPath, "*" + FileExtension);
                foreach (string fileName in fileEntries)
                {

                    //Writing Data of File Into Table
                    int counter = 0;
                    string line;
                    string ColumnList = "";
                    filename1 = fileName;
                    System.IO.StreamReader SourceFile = new System.IO.StreamReader(fileName);
                    connection.Open();
                    while ((line = SourceFile.ReadLine()) != null)
                    {
                        if (counter == 0)
                        {
                            //By using Header Row, Build Column List
                            ColumnList = "[" + line.Replace(FileDelimiter, "],[") + "]";
                            string[] words = ColumnList.Split(',');
                            //if (words.Length == 8)
                            //{
                            //    ColumnList = "Reservoir_Engineer,Asset_Name,Reading_Date,Monitoring_Type,Monitoring_Name,Stored_Reading,CLEANSED,Comments";
                            //}
                            //else if (words.Length == 6)
                            //{
                            //    ColumnList = "Reservoir_Engineer,Asset_Name,Reading_Date,Monitoring_Type,Monitoring_Name,Stored_Reading";
                            //}
                            //else
                            //{
                            //    ColumnList = "Reservoir_Engineer,Asset_Name,Reading_Date,Monitoring_Type,Monitoring_Name,Stored_Reading,CLEANSED";
                            //}

                            ColumnList = "Asset_Name,Reading_Date,MonitoringType,MonitoringName,Collected_Feet,Collected_Inches";

                        }
                        else
                        {

                            //Build and Execute Insert Statement to insert record
                            string query = "Insert into " + TableName + " (" + ColumnList + ") ";
                            query += "VALUES('" + line.Replace(FileDelimiter, "','") + "')";

                            SqlCommand SQLCmd = new SqlCommand(query, connection);
                            SQLCmd.ExecuteNonQuery();
                        }

                        counter++;
                    }

                    SourceFile.Close();
                    connection.Close();
                    //move the file to archive folder after adding datetime to it
                    File.Move(fileName, ArchiveFolder + "\\" +
                        (fileName.Replace(SourceFolderPath, "")).Replace(FileExtension, "")
                        + "_" + datetime + FileExtension);
                    filename1 = string.Empty;
                }
            }
            catch (Exception exception)
            {
                // Create Log File for Errors
                //using (StreamWriter sw = File.CreateText(LogFolder
                //    + "\\" + "ErrorLog_" + datetime + ".log"))
                //{
                //    sw.WriteLine(exception.ToString());

                //}
              

            }



        }
    }
}
