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
using Tzix.ViewModel;

namespace Tzix.View
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(Dispatcher);
            ViewModel.TransStateTo(Types.AppState.Loading);
        }

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }
        
        private void _mainWindow_Closed(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Save();
            }
        }
    }
}
