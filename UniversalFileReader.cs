using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;

namespace UniversalFileReader
{
    public class BasicReader
    {
        //// Properties

        // Überschriften vorhanden?
        private bool _hasHeaders;

        public bool hasHeaders
        {
            get { return (_hasHeaders);  }
            set { _hasHeaders = value; }
        }

        private string[] _headers;

        public string[] headers
        {
            get { return (_headers); }
        }

        // Trennzeichen
        private string _delimiter;

        public string delimiter
        {
            get { return (_delimiter); }
            set { _delimiter = value; }
        }

        // Anzahl Zeilen
        private int _noLines;

        public int noLines
        {
            get { return (_noLines); }
        }

        // Anzahl Spalten
        private int _noColumns;

        public int noColumns
        {
            get { return (_noColumns); }
        }

        // Array
        private string[,] _dataArray;

        public string[,] dataArray
        {
            get { return _dataArray; }
        }

        // Input-File
        private string _inputFile;

        public string inputFile
        {
            get { return (_inputFile); }
            set { _inputFile = value; }
        }

        // Input-File  Codepage
        private int _inputCode;

        public int inputCode
        {
            get {  return (_inputCode); }
            set { _inputCode = value; }
        }

        // Output-File-Typ
        private string _fileExt;

        public string fileExt
        {
            get { return (_fileExt); }
            set { _fileExt = value; }
        }

        // enum
        public enum databaseType
        {
            mySQL,
            MSSQL,
            ODBC
        }

        
        ///// Methoden

        // Datei laden
        public string loadFile()
        {
            StreamReader sr = new StreamReader(_inputFile, Encoding.GetEncoding(_inputCode)); //, System.Text.Encoding.Default);
            string completeText = sr.ReadToEnd();
            sr.Close();
            return completeText;
            //byte[] bytes = Encoding.GetEncoding(437).GetBytes(completeText);
            //string completeTextUTF8 = Encoding.UTF8.GetString(bytes);
            //return completeTextUTF8;
        }

        public string[] splitIntoLines(string text)
        {
            string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            getHeaders(lines[0]);
            _noLines = lines.GetUpperBound(0);
            return lines;
        }

        private void getHeaders(string firstLine)
        {
            int headersCount = firstLine.Split(';').GetUpperBound(0);
            string[] tempCount = new string[headersCount];
            if (_hasHeaders == true)
            {
                _headers = firstLine.Split(';');
            }
            else
            {
                for (int schleife = 0; schleife < headersCount; schleife++)
                {
                    tempCount[schleife] = (schleife + 1).ToString();
                }
                _headers = tempCount;
            }

        }

        public string[,] splitIntoColumns(string[] lineArray)
        {
            string[] tempSplit;
            int anzahlRow=_noLines;
            int anzahlCol = lineArray[0].Split(';').GetUpperBound(0);
            _noColumns = anzahlCol;
            string [,] splittedData = new string[anzahlRow,anzahlCol]; 
            for(int schleife=0; schleife < anzahlRow; schleife++)
            {
                for(int schleife2=0; schleife2 < anzahlCol; schleife2++)
                {
                    tempSplit = lineArray[schleife].Split(';');
                    splittedData[schleife, schleife2] = tempSplit[schleife2];
                }
            }
            _dataArray = splittedData;
            return splittedData;
        }

        private double conversion(string readvalue)
        {

            if (readvalue == "") readvalue = "0.0";
            string compName = Environment.MachineName;
            if (compName.Substring(0, 2) == "JB")
            {

            }
            else
            {
                readvalue = readvalue.Replace(",", ".");
            }

            // nur wegen US-Einstellung zuhause
            //readvalue = readvalue.Replace(".", ",");

            double convValue = Convert.ToDouble(readvalue);
            return convValue;
        }

        public void dataIntoDB(databaseType d, string connection, string dbName, string[] dbColNames, string[] dbColTypes, string targetTable, int [] headerMapping)
        {
            
            // Fehler
            if(_dataArray == null)
            {
                Console.WriteLine("Universal File Reader> No data in array to write in database...");
                Console.Read();
            }
            switch (d)
            {
                case databaseType.mySQL:
                    // Verbindung ok?
                    MySqlConnection conn;
                    conn = new MySqlConnection();
                    conn.ConnectionString = connection;
                    try
                    {
                        conn.Open();
                        string strSQL = "DELETE FROM fibuDW.ufrtest";
                        MySqlCommand cmd3 = new MySqlCommand(strSQL, conn);
                        cmd3.ExecuteScalar();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("ufr> " + ex);
                    }
                    finally
                    {
                        //conn.Close();
                    }
                    // Daten anpassen
                    int endzeile = (_hasHeaders==true) ? noLines : noLines+1;
                    int startzeile = (_hasHeaders == true) ? 1 : 0;

                    for (int schleife = startzeile; schleife < endzeile; schleife++)
                    {
                        string query = "INSERT INTO " + dbName + "." + targetTable + " (" + dbColNames[0] + ") values (@element)";
                        MySqlCommand cmd = new MySqlCommand(query, conn);

                        switch (dbColTypes[0])
                        {
                            case "s":
                                cmd.Parameters.AddWithValue("@element", _dataArray[schleife, 0]);
                                break;
                            case "i":
                                cmd.Parameters.AddWithValue("@element", Convert.ToInt16(_dataArray[schleife, 0]));
                                break;
                            case "d":
                                cmd.Parameters.AddWithValue("@element", Convert.ToDouble(conversion(dataArray[schleife, 0])));
                                break;
                            case "b":
                                cmd.Parameters.AddWithValue("@element", Convert.ToBoolean(_dataArray[schleife, 0]));
                                break;
                        }
                        cmd.ExecuteScalar();

                        for (int schleife2 = 1; schleife2 < noColumns; schleife2++)
                        {
                            string query2 = "UPDATE " + dbName + "." + targetTable + " SET " + dbColNames[schleife2] + " = @elementz WHERE " + dbColNames[0] +
                                                    " = '" + _dataArray[schleife, 0] + "'";
                            Console.WriteLine(query2);
                            MySqlCommand cmd2 = new MySqlCommand(query2, conn);

                            switch (dbColTypes[schleife2])
                            {
                                case "s":
                                    cmd2.Parameters.AddWithValue("@elementz", _dataArray[schleife, schleife2]);
                                    break;
                                case "i":
                                    //cmd.Parameters.AddWithValue("@elementz", Convert.ToInt16(_dataArray[schleife, schleife2]));
                                    cmd2.Parameters.Add("@elementz", MySqlDbType.Int16).Value = Convert.ToInt16(_dataArray[schleife, schleife2]);
                                    break;
                                case "d":
                                    cmd2.Parameters.AddWithValue("@elementz", conversion(_dataArray[schleife, schleife2]));
                                    break;
                                case "b":
                                    cmd2.Parameters.AddWithValue("@elementz", Convert.ToBoolean(_dataArray[schleife, schleife2]));
                                    break;

                            }
                            cmd2.ExecuteScalar();

                        }
                    }
                    
                    
                    break;
                default:
                    Console.WriteLine("ufr> db-type not possible");
                    break;
            }

           

        }
    }

  
   
}
