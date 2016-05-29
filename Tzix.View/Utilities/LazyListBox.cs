using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Tzix.View.Utilities
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
            self.ObservableItemsSource.Clear();
            self._unloadedChunks = self.LazyItemsSource;
            self.FetchNextItems();
        }

        private IEnumerable<IEnumerable> _unloadedChunks;

        private void FetchNextItems()
        {
            if (_unloadedChunks == null || !_unloadedChunks.Any()) return;

            var source = ObservableItemsSource;
            if (source == null) return;

            var nextChunk = _unloadedChunks.First();
            _unloadedChunks = _unloadedChunks.Skip(1);
            foreach (var x in nextChunk)
            {
                source.Add(x);
            }
        }

        private void OnScrollChanged(object sender, RoutedEventArgs e0)
        {
            var e = (ScrollChangedEventArgs)e0;
            Debug.Assert(e != null);

            // When scrolled to bottom (except for the case things are too few to scroll)
            if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
            {
                FetchNextItems();
            }
        }

        private new void SelectAll()
        {
            // Ignore
        }
    }
}
