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
        /*//Set for the purpose of scaling the charts - won't need them
        public DateTime minDate, maxDate;
        public double minPrice, maxPrice;*/

        public MainWindow()
        {
            InitializeComponent();

            //temporary data sources list
            string[] arr1 = new string[] {"goog.csv", "citi.csv", "msft.csv", "csco.csv"};
            foreach (string stock in arr1)
            {
                cboStock.Items.Add(stock);
            }
            //Start from the first one
            cboStock.SelectedItem = "goog.csv";
        }

        //Changing the stock gives different data
        private void cboStockChanged(object sender, SelectionChangedEventArgs e)
        {
            sciChart.DataSet = ReadStockData(cboStock.SelectedItem.ToString());
            sciChart.ZoomExtents();
        }

        //Return relevant data set
        private DataSeriesSet<DateTime, double> ReadStockData(String sFileName)
        {
            //Data structure to return
            var dataSeriesSet = new DataSeriesSet<DateTime, double>();
            var series = dataSeriesSet.AddSeries<OhlcDataSeries<DateTime, double>>();
            series.SeriesName = "Stock chart";

            //local variables declarations
            String sFile, sRow, sEl;
            int nRow = 0, nCol, nIndex, nVolume;
            double priceOpen = 0, priceHigh = 0, priceLow = 0, priceClose = 0;
            DateTime xAxis = DateTime.Parse("01/01/1900");
            String sStockFile = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + sFileName;

            //reading stock file
            using (StreamReader sr = new StreamReader(sStockFile))
                sFile = sr.ReadToEnd();
            //parsing the file
            while (sFile != "")
            {
                nIndex = sFile.IndexOf("\n");
                sRow = sFile.Substring(0, nIndex);
                sFile = sFile.Substring(nIndex + 1);
                //parsing the header - no need to right now...could be done if needed
                if (nRow++ == 0) continue;
                //parsing row by row
                nCol = 1;
                while (sRow != "")
                {
                    nIndex = sRow.IndexOf(",");
                    if (nIndex == -1 && nCol == 6) //needs better approach probably - won't waste time right now
                    {
                        sEl = sRow;
                        sRow = "";
                    }
                    else
                    {
                        sEl = sRow.Substring(0, nIndex);
                        sRow = sRow.Substring(nIndex + 1);
                    }
                    switch (nCol++)
                    {
                        case 1:
                            xAxis = DateTime.Parse(sEl);
                            /*if (xAxis > maxDate) maxDate = xAxis;
                            if (xAxis < minDate) minDate = xAxis;*/
                            break;
                        case 2:
                            priceOpen = Convert.ToDouble(sEl);
                            /*if (priceOpen > maxPrice) maxPrice = priceOpen;
                            if (priceOpen < minPrice) minPrice = priceOpen;*/
                            break;
                        case 3:
                            priceHigh = Convert.ToDouble(sEl);
                            /*if (priceHigh > maxPrice) maxPrice = priceHigh;
                            if (priceHigh < minPrice) minPrice = priceHigh;*/
                            break;
                        case 4:
                            priceLow = Convert.ToDouble(sEl);
                            /*if (priceLow > maxPrice) maxPrice = priceLow;
                            if (priceLow < minPrice) minPrice = priceLow;*/
                            break;
                        case 5:
                            priceClose = Convert.ToDouble(sEl);
                            /*if (priceClose > maxPrice) maxPrice = priceClose;
                            if (priceClose < minPrice) minPrice = priceClose;*/
                            break;
                        case 6:
                            nVolume = Convert.ToInt32(sEl); //Don't use it at the time
                            break;
                    }
                }
                series.Append(xAxis, priceOpen, priceHigh, priceLow, priceClose);
            }
            return dataSeriesSet;
        }
    }
}