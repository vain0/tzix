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
using VainZero.Tzix.Desktop.Core;
using static VainZero.Tzix.Desktop.Core.DispatcherTypes;

namespace VainZero.Tzix.Desktop
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
            
            DataContext = new MainWindowViewModel(new DotNetDispatcher(Dispatcher));

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
                case "PageIndex":
                    var page = PageFromIndex(ViewModel.PageIndex);
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
                    var ctx = ViewModel.SearchControlViewModelOpt;
                    return new SearchControl()
                    {
                        DataContext = (ctx == null ? null : ctx.Value)
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
