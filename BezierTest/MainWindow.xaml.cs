using System.Linq;
using System.Windows;

namespace BezierTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var points = bezierEditor1.Sample(20).ToList();
        }
    }
}
