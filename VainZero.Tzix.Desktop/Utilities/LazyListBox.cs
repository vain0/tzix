using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VainZero.Tzix.Desktop.Utilities
{
    public class LazyListBox : ListBox
    {
        public LazyListBox()
            : base()
        {
            AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(OnScrollChanged));
            ItemsSource = new ObservableCollection<object>();
        }

        private ObservableCollection<object> ObservableItemsSource
        {
            get
            {
                var itemsSource = ItemsSource;
                if (itemsSource == null) return null;
                return itemsSource as ObservableCollection<object>;
            }
        }

        public static readonly DependencyProperty LazyItemsSourceProperty =
            DependencyProperty.Register(
                "LazyItemsSource",
                typeof(IEnumerable<IEnumerable>),
                typeof(LazyListBox),
                new FrameworkPropertyMetadata()
                {
                    PropertyChangedCallback = OnLazyItemsSourcePropertyChanged,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                }
            );

        public IEnumerable<IEnumerable> LazyItemsSource
        {
            get { return (IEnumerable<IEnumerable>)GetValue(LazyItemsSourceProperty); }
            set { SetValue(LazyItemsSourceProperty, value); }
        }

        public static void OnLazyItemsSourcePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var self = sender as LazyListBox;
            if (self == null) return;
            self._clears = true;
            self._unloadedChunks = self.LazyItemsSource;
            var task = self.FetchNextItems();
        }

        private bool _clears = false;
        private IEnumerable<IEnumerable> _unloadedChunks;
        private CancellationTokenSource _addTaskCanceller;

        private async Task FetchNextItems()
        {
            try
            {
                if (_unloadedChunks == null || !_unloadedChunks.Any()) return;

                var source = ObservableItemsSource;
                if (source == null) return;

                _addTaskCanceller?.Cancel(throwOnFirstException: true);
                _addTaskCanceller = new CancellationTokenSource();
                var newItems = await Task.Factory.StartNew(() =>
                {
                    var nextChunk = _unloadedChunks.First();
                    return nextChunk.Cast<object>().ToArray();
                }, _addTaskCanceller.Token);
                if (_addTaskCanceller.IsCancellationRequested) return;

                _unloadedChunks = _unloadedChunks.Skip(1);
                Dispatcher.Invoke(() =>
                {
                    ClearIfNecessary();
                    foreach (var x in newItems)
                    {
                        source.Add(x);
                    }
                });
            }
            finally
            {
                ClearIfNecessary();
            }
        }

        private void ClearIfNecessary()
        {
            if (_clears)
            {
                _clears = false;
                ObservableItemsSource.Clear();
            }
        }

        private void OnScrollChanged(object sender, RoutedEventArgs e0)
        {
            var e = (ScrollChangedEventArgs)e0;
            Debug.Assert(e != null);

            // When scrolled to bottom (except for the case things are too few to scroll)
            if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
            {
                var task = FetchNextItems();
            }
        }

        private new void SelectAll()
        {
            // Ignore
        }
    }
}
