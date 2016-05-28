using System;
using System.ComponentModel;
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
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowsNavigationUI = false;

            DataContext = new MainWindowViewModel(Dispatcher);

            ((INotifyPropertyChanged)ViewModel).PropertyChanged += OnPropertyChanged;

            ViewModel.TransStateTo(Types.AppState.Loading);
        }

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedIndex":
                    var page = PageFromIndex((Types.PageIndex)ViewModel.SelectedIndex);
                    if (page != null)
                    {
                        Navigate(page);
                    }
                    break;
            }
        }

        public Page PageFromIndex(Types.PageIndex index)
        {
            switch (index)
            {
                case Types.PageIndex.MessageView:
                    return new MessageView()
                    {
                        DataContext = ViewModel.MessageViewViewModel
                    };
                case Types.PageIndex.SearchControl:
                    return new SearchControl()
                    {
                        DataContext = ViewModel.SearchControlViewModelOpt
                    };
                default:
                    return null;
            }
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
