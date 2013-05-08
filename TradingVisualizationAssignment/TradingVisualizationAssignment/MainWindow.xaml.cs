using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Utility;

namespace TradingVisualizationAssignment
{
    public partial class MainWindow : Window
    //public partial class MainWindow : UserControl //DATETIME VARIANTA
    {
        //setting the values of global variables to the values that are "good enough"
        public DateTime minDate = DateTime.Parse("01/01/2200"), maxDate = DateTime.Parse("01/01/1900");
        public double minPrice = 10000000, maxPrice = 0;
        //Logger class
        private class OutputWindowLogger : ISciChartLoggerFacade
        {
            public void Log(string formatString, params object[] args)
            {
                Debug.WriteLine(formatString, args);
            }
        }

        public MainWindow()
        {
            // Setting a logger will log debug messages to the console, but will incur a HUGE performance hit, so use it wisely! 
            SciChartDebugLogger.Instance.SetLogger(new OutputWindowLogger());
            //SciChartDebugLogger.Instance.WriteLine("Hello World!");
            InitializeComponent();
            //Loaded += new RoutedEventHandler(MainWindowLoaded);
            Loaded += MainWindowLoaded;
        }

        private DataSeriesSet<double, double> ReadStockData(String sFileName)
        //private DataSeriesSet<DateTime, double> ReadStockData(String sFileName) //DATETIME VARIANTA
        {
            //Data structure to return
            //var dataset = new DataSeriesSet<DateTime, double>(); //DATETIME VARIANTA
            var dataset = new DataSeriesSet<double, double>();
            //var series = dataset.AddSeries<OhlcDataSeries<DateTime, double>>(); //OHLC DATETIME VARIANTA
            var series = dataset.AddSeries();
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
                sRow = sFile.Substring(0,nIndex);
                sFile = sFile.Substring(nIndex + 1);
                //parsing the header - no need to right now...could be done if needed
                if (nRow++ == 0) continue;
                //parsing row by row
                nCol = 1;
                while (sRow != "")
                {
                    nIndex = sRow.IndexOf(",");
                    if (nIndex == -1 && nCol == 6) //needs better approach probably
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
                            if (xAxis > maxDate) maxDate = xAxis;
                            if (xAxis < minDate) minDate = xAxis;
                            break;
                        case 2:
                            priceOpen = Convert.ToDouble(sEl);
                            //if (priceOpen > maxPrice) maxPrice = priceOpen;
                            //if (priceOpen < minPrice) minPrice = priceOpen;
                            break;
                        case 3:
                            priceHigh = Convert.ToDouble(sEl);
                            if (priceHigh > maxPrice) maxPrice = priceHigh;
                            if (priceHigh < minPrice) minPrice = priceHigh;
                            break;
                        case 4:
                            priceLow = Convert.ToDouble(sEl);
                            if (priceLow > maxPrice) maxPrice = priceLow;
                            if (priceLow < minPrice) minPrice = priceLow;
                            break;
                        case 5:
                            priceClose = Convert.ToDouble(sEl);
                            //if (priceClose > maxPrice) maxPrice = priceClose;
                            //if (priceClose < minPrice) minPrice = priceClose;
                            break;
                        case 6:
                            nVolume = Convert.ToInt32(sEl);
                            break;
                    }
                }
                //series.Append(xAxis, priceOpen, priceHigh, priceLow, priceClose); //OHLC DATETIME VARIANTA
            }
            for (int i = 0; i < 1000; i++)
            {
                series.Append(i, Math.Sin(2 * Math.PI * i / 1000));
            }
            return dataset;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            // GrowBy expands a gap around the data when zooming to extents
            sciChartSurface.YAxis.GrowBy = new DoubleRange(0.2, 0.2);
            // VisibleRange may be set explicitly, or use AutoRange to zoom to extents
            sciChartSurface.XAxis.VisibleRange = new DoubleRange(0,1000);
            sciChartSurface.YAxis.VisibleRange = new DoubleRange(-1.5, 1.5);
            //sciChartSurface.XAxis.VisibleRange = new DateRange(minDate, maxDate); //DATETIME VARIANTA
            //sciChartSurface.XAxis.VisibleRange = new DateRange(DateTime.Parse("01/01/2012"), DateTime.Parse("12/31/2012"));
            //sciChartSurface.YAxis.VisibleRange = new DoubleRange(555, 775);
            //sciChartSurface.YAxis.VisibleRange = new DoubleRange(minPrice, maxPrice); //DATETIME VARIANTA
            // Note: Can also be set in Xaml, inside the <SciChartSurface.DataSet> tag
            sciChartSurface.DataSet = ReadStockData("goog.csv");
        }
    }
}