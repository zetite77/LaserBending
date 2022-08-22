using System;
using System.Collections.Generic;
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

namespace LaserBendingMeasurementSystem.Page
{
    /// <summary>
    /// MenuHistoryInfo_PageHistory.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuHistoryInfo : UserControl
    {
        List<history> historys = null;
        public MenuHistoryInfo()
        {
            InitializeComponent();
            historys = new List<history>();
            historys.Add(new history() { recpy = "John", pass = "합", xy = "-", date = "2022.08.22 13:00:00" });
            historys.Add(new history() { recpy = "Emma", pass = "합", xy = "-", date = "2022.08.22 13:04:00" });
            historys.Add(new history() { recpy = "Sophia", pass = "불", xy = "x:2,y:3", date = "2022.08.22 13:08:00" });
            his.ItemsSource = historys;
        }
        public class history
        {
            public string recpy { get; set; }
            public string pass { get; set; }
            public string xy { get; set; }
            public string date { get; set; }
        }
    }
    
}
