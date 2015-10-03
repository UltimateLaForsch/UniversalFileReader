using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniversalFileReader ;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void button1_Click(object sender, EventArgs e)
        {
            testClass();
        }

        public void testClass()
        {
            BasicReader br = new BasicReader();
            br.inputFile = @"C:\Users\Patrick\Desktop\KoStkurz.csv";//   Konten.sql";
            br.inputCode = 437;
            br.hasHeaders = true;
            string gesamtText = br.loadFile();
            listBox1.Items.Insert(0, gesamtText);
            listBox1.Refresh();
            string[] gespliteterText = br.splitIntoLines(gesamtText);
            foreach(string t in gespliteterText)
            {
                listBox1.Items.Insert(0, t);
                listBox1.Refresh();
            }
            int anzahlZeilen=br.noLines;
            string[,] array = br.splitIntoColumns(gespliteterText);
            int anzahlSpalten = br.noColumns;
            for(int schleife=0; schleife < anzahlZeilen; schleife++)
            {
                for(int schleife2=0; schleife2 < anzahlSpalten; schleife2++)
                {
                    listBox1.Items.Insert(0, array[schleife, schleife2]);
                }
            }
            listBox1.Items.Insert(0, "Anzahl DataSets: " + anzahlZeilen);
            listBox1.Items.Insert(0, "Anzahl Spalten: " + anzahlSpalten);
            for (int schleife = 0; schleife < br.noColumns;schleife++)
                listBox1.Items.Insert(0, "Headers: " + br.headers[schleife]);
            listBox1.Refresh();

            string[] dbColumns = { "kosttest", "nametest" };
            int[] mapping = { 1, 0};   // -1 for ignore
            string[] dbColTypes =  { "s", "s" };  // s for string
            string tableName = "ufrtest";
            string dbname = "fibudw";
            br.dataIntoDB(BasicReader.databaseType.mySQL, getConnection(), dbname, dbColumns, dbColTypes, tableName, mapping);
        }





        public string getConnection()
        {
            string compName = Environment.MachineName;
            string connectionString = @"server=localhost;userid=admin;password=" + password() + ";database=fibuDW";
            return connectionString;
        }

        private string password()
        {
            string compName = Environment.MachineName;
            string pw = "merkel";
            return pw;
        }

    }
}
