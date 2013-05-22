/**********************************************************************
 * Created by : Nenad Samardzic
 * Dates      : 05/02/2013 - 05/22/2013
 * Description: The application represents C# stock/trading visualization assignment.
 * Idea       : Visual Studio C# WPF project utilizes a SciChart visualization component
 *              in order to displays stock market data and specific technical indicators.
 *              It utilizes OHLC Data Series to visualize prices and simple line series
 *              to visualize volumes data.
 * Parameters : Application uses external data sources - .csv files.
 **********************************************************************/

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Utility;

namespace TradingVisualizationAssignment
{
    public partial class MainWindow : UserControl
    {
        private string sLocation;
        private Boolean inMillions = true;

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

        //Defines the way of volume chart presentation
        private void cbChecked(object sender, RoutedEventArgs e)
        {
            inMillions = ((CheckBox)sender).IsChecked == true;
            if (cboStock.SelectedItem != null)
            {
                sciChart.DataSet = ReadStockData(cboStock.SelectedItem.ToString() + ".csv");
                sciChart.ZoomExtents();
            }
        }

        //Return relevant data set
        private DataSeriesSet<DateTime, double> ReadStockData(String sFileName)
        {
            //local variables and structures
            string sFile;
            int nRow = 0;
            string sStockFile = sLocation + "\\" + sFileName;

            DateTime xAxis;
            double dOpen, dHigh, dLow, dClose;
            int nVolume;
            List<Tuple<DateTime, double, double, double, double, int>> listStock = new List<Tuple<DateTime, double, double, double, double, int>>();

            //Data structure to return
            var dataSeriesSet = new DataSeriesSet<DateTime, double>();
            var series = dataSeriesSet.AddSeries<OhlcDataSeries<DateTime, double>>();
            var seriesLine = dataSeriesSet.AddSeries();
            seriesLine.SeriesName = "Volume";

            //reading stock file
            using (StreamReader sr = new StreamReader(sStockFile))
                sFile = sr.ReadToEnd();
            //parsing the file
            string[] rows = sFile.Split(('\n'));
            foreach (string row in rows)
            {
                if (row == "") continue;
                string[] columns = row.Split((','));
                if (nRow++ == 0) //(Sort of) Check of the data file structure
                {
                    if (!columns[0].Substring(0, 4).Equals("DATE", StringComparison.InvariantCultureIgnoreCase) ||
                    !columns[1].Substring(0, 4).Equals("OPEN", StringComparison.InvariantCultureIgnoreCase) ||
                    !columns[2].Substring(0, 4).Equals("HIGH", StringComparison.InvariantCultureIgnoreCase) ||
                    !columns[3].Substring(0, 3).Equals("LOW", StringComparison.InvariantCultureIgnoreCase) ||
                    !columns[4].Substring(0, 5).Equals("CLOSE", StringComparison.InvariantCultureIgnoreCase) ||
                    !columns[5].Substring(0, 6).Equals("VOLUME", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageBoxResult result = MessageBox.Show("Your source data file doesn't have the required structure!\nPlease, re-check your file.\nAborting!", "Data structure error!");
                        dataSeriesSet = null;
                        break;
                    }
                    else continue;
                }
                try //Check of the source data types
                {
                    xAxis = DateTime.Parse(columns[0]);
                    dOpen = Convert.ToDouble(columns[1]);
                    dHigh = Convert.ToDouble(columns[2]);
                    dLow = Convert.ToDouble(columns[3]);
                    dClose = Convert.ToDouble(columns[4]);
                    nVolume = Convert.ToInt32(columns[5]);

                    //Add the row to the list
                    listStock.Add(new Tuple<DateTime, double, double, double, double, int>(xAxis, dOpen, dHigh, dLow, dClose, nVolume));
                }
                catch
                {
                    MessageBoxResult result = MessageBox.Show("Your source file contains some data which is not of the required data type!\nPlease, re-check your file.\nAborting!", "Data type error!");
                    dataSeriesSet = null;
                    break;
                }
            }
            listStock.Sort(Comparer<Tuple<DateTime, double, double, double, double, int>>.Default); //sort and set as source
            foreach(Tuple<DateTime, double, double, double, double, int> newLine in listStock)
            {
                series.Append(newLine.Item1, newLine.Item2, newLine.Item3, newLine.Item4, newLine.Item5);
                if (inMillions)
                {
                    seriesLine.Append(newLine.Item1, newLine.Item6 / 1000000);
                }
                else
                {
                    seriesLine.Append(newLine.Item1, newLine.Item6);
                }
            }
            return dataSeriesSet;
        }
    }
}