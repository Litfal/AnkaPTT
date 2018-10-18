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
                FilterViewModel.FilteredPushCollection.Dispatcher = value;
            }
        }

        public PushCollectionViewModel AllPushCollection { get; } = new PushCollectionViewModel();

        public FilterViewModel FilterViewModel { get; }

        private List<FilterViewModel> _filterViewModels = new List<FilterViewModel>();

        Queue<int> _highlightKeys = new Queue<int>(Enumerable.Range(0, 6));

        public ChromiumWebBrowser WebBrowser { get; internal set; }

        public MainViewModel()
        {
            FilterViewModel = CreateNewFilterViewModel();
            FilterViewModel.HighlightResults = true;
        }

        public FilterViewModel CreateNewFilterViewModel()
        {
            if (_highlightKeys.Count == 0) return null;
            var filterViewModel = new FilterViewModel();
            filterViewModel.HighlightKey = _highlightKeys.Dequeue();
            filterViewModel.PushDoubleClicked += (sender, e) => EvaluateScriptAsync($"selectPush({e.Push.Index})");
            filterViewModel.FilteredPushCollection.CollectionChanged += (sender, e) =>
            {
                IEnumerable<PushViewModel> list = (IEnumerable<PushViewModel>)sender;
                string param = filterViewModel.HighlightResults ? string.Join(",", list.Select(p => p.Index)) : "";
                EvaluateScriptAsync($"highlight({filterViewModel.HighlightKey},[{param}])");
            };
            filterViewModel.SetMainViewModel(this);
            _filterViewModels.Add(filterViewModel);
            return filterViewModel;
        }

        internal void ReleseFilter(FilterViewModel filterViewModel)
        {
            _highlightKeys.Enqueue(filterViewModel.HighlightKey);
        }

        private Task<JavascriptResponse> EvaluateScriptAsync(string script)
        {
            return WebBrowser?.GetMainFrame()?.EvaluateScriptAsync(script);
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
