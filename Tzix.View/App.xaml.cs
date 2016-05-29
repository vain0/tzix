using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Tzix.View
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static void Abort(string message)
        {
            MessageBox.Show(
                message,
                ResourceAssembly.GetName().Name,
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
            Current.Shutdown();
        }
    }
}
