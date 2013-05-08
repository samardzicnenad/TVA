using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Utility;

namespace TradingVisualizationAssignment
{
    public partial class MainWindow : UserControl
    {
        public string sLocation;

        public MainWindow()
        {
            Boolean bFirst = true;
            string sSelectedItem = "";

            InitializeComponent();

            //Open folder dialog
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Please, select the folder which contains your stock market data files.";
            System.Windows.Forms.DialogResult drResult = fbd.ShowDialog();
            sLocation = fbd.SelectedPath;

            while (fbd.SelectedPath == "")
            {
                fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = "You have to choose the folder which contains your stock market data files before you continue!";
                drResult = fbd.ShowDialog();
                sLocation = fbd.SelectedPath;
            }

            //Loading all .csv files into the combo box
            foreach (string stock in Directory.EnumerateFiles(fbd.SelectedPath, "*.csv"))
            {
                string stockName = stock.Substring(stock.LastIndexOf("\\") + 1, stock.Length - (stock.LastIndexOf("\\") + 1) - 4);
                cboStock.Items.Add(stockName);
                //Load the first file - optional...can work without setting the SelectedItem...everything below may be omitted
                if (bFirst)
                {
                    sSelectedItem = stockName;
                    bFirst = false;
                }
            }
            cboStock.SelectedItem = sSelectedItem;
        }

        //Changing the stock gives different data
        private void cboStockChanged(object sender, SelectionChangedEventArgs e)
        {
            sciChart.DataSet = ReadStockData(cboStock.SelectedItem.ToString() + ".csv");
            sciChart.ZoomExtents();
        }

        //Return relevant data set
        private DataSeriesSet<DateTime, double> ReadStockData(String sFileName)
        {
            //Data structure to return
            var dataSeriesSet = new DataSeriesSet<DateTime, double>();
            var series = dataSeriesSet.AddSeries<OhlcDataSeries<DateTime, double>>();

            //local variables declarations
            string sFile;
            int nRow = 0;
            string sStockFile = sLocation + "\\" + sFileName;

            //reading stock file
            using (StreamReader sr = new StreamReader(sStockFile))
                sFile = sr.ReadToEnd();
            //parsing the file
            string[] rows = sFile.Split(('\n'));
            foreach (string row in rows)
            {
                if ((nRow++ == 0) || (row == "")) continue;
                string[] columns = row.Split((','));
                series.Append(DateTime.Parse(columns[0]), Convert.ToDouble(columns[1]), Convert.ToDouble(columns[2]),
                    Convert.ToDouble(columns[3]), Convert.ToDouble(columns[4]));
            }
            return dataSeriesSet;
        }
    }
}