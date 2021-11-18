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
using System.Windows.Shapes;

namespace LodmodsGUI
{
    public class ConfigConstructorInfo
    {
        public string ConfigName { get; set; }
        public string ConfigType { get; set; }
        public List<string> RegionList { get; set; }
    }

    public partial class RegionSelector : Window
    {
        public RegionSelector()
        {
            InitializeComponent();
            base.DataContext = new ConfigConstructorInfo();
        }

        private void CancelRegionSelection(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetConfigName(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Create a MainConfig object, then open up form for customizing config
            Close();
        }
    }
}
