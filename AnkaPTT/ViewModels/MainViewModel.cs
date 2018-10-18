using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;

namespace AnkaPTT.ViewModels
{
    class MainViewModel
    {
        Dispatcher _dispatcher { get; set; }
        internal Dispatcher Dispatcher
        {
            get { return _dispatcher; }
            set
            {
                _dispatcher = value;
                ViewPushCollection.Dispatcher = value;
            }
        }

        public PushCollectionViewModel ViewPushCollection { get; } = new PushCollectionViewModel();

        public PushCollectionViewModel AllPushCollection { get; } = new PushCollectionViewModel();

        public FilterViewModel FilterViewModel { get; } = new FilterViewModel();
        public ChromiumWebBrowser WebBrowser { get; internal set; }

        public MainViewModel()
        {
            FilterViewModel.AllPushCollection = this.AllPushCollection;
            FilterViewModel.FilteredPushCollection.CollectionChanged +=
                (sender, e) =>
                {
                    // UiThreadRunSync(() => ViewPushCollection.ResetTo(FilterViewModel.FilteredPushCollection));
                    ViewPushCollection.ResetTo(FilterViewModel.FilteredPushCollection);
                    if (FilterViewModel.HighlightResults)
                    {
                        WebBrowser.GetMainFrame().EvaluateScriptAsync($"highlight([{string.Join(",", ViewPushCollection.Select(p => p.Index))}])");
                    }
                    else
                    {
                        WebBrowser.GetMainFrame().EvaluateScriptAsync($"highlight([])");
                    }
                };
        }

        internal void UpdatePushes(List<Push> pushes)
        {
            var pushViewModels = PushViewModel.ToViewModels(pushes);
            if (AllPushCollection.Count > pushes.Count)
            {
                AllPushCollection.ResetTo(pushViewModels);
            }
            else
            {
                AllPushCollection.AddRange(pushViewModels.Skip(AllPushCollection.Count));
            }
        }

        /// <summary>
        /// Runs the specific Action on the Dispatcher in an async fashion
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        private void UiThreadRunAsync(Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else if (!Dispatcher.HasShutdownStarted)
            {
                Dispatcher.BeginInvoke(action, priority);
            }
        }

        /// <summary>
        /// Runs the specific Action on the Dispatcher in an sync fashion
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        private void UiThreadRunSync(Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else if (!Dispatcher.HasShutdownStarted)
            {
                Dispatcher.Invoke(action, priority);
            }
        }




    }
}
