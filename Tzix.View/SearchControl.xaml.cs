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
    /// SearchControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchControl : Page
    {
        public SearchControl()
        {
            InitializeComponent();

            _searchBox.Focus();
        }

        private void _foundList_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_foundList.SelectedIndex < 0 && !_foundList.Items.IsEmpty)
            {
                _foundList.SelectedIndex = 0;
            }
        }
    }
}
