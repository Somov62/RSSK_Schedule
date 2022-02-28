using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExcelParserLib;

namespace RSSK_Schedule
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExcelParser parser;
        public MainWindow()
        {
            InitializeComponent();
            parser = new ExcelParser($"{Environment.CurrentDirectory}\\Rasp21-22.xlsx");
            pageContainer.NavigationService.Navigate(new ZamenyPage());
            

        }

        //private void ShowRasp_Click(object sender, RoutedEventArgs e)
        //{
        //    if (groupPicker.SelectedIndex == -1) return;
        //    output.Text = parser.GetSchedule(groupPicker.Text, datePicker.SelectedDate.Value);
        //}
    }
}
