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

namespace RSSK_Schedule
{
    /// <summary>
    /// Логика взаимодействия для ZamenyPage.xaml
    /// </summary>
    public partial class ZamenyPage : Page
    {
        public ZamenyPage()
        {
            InitializeComponent();
            List<Entity> items = new List<Entity>()
            {
                new Entity() {Date = DateTime.Now},
                new Entity() {Date = DateTime.Now},
                new Entity() {Date = DateTime.Now},
                new Entity() {Date = DateTime.Now},
                new Entity() {Date = DateTime.Now}
            };
            daysView.ItemsSource = items;
        }
    }
}
