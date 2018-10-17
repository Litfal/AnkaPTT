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
        internal Dispatcher Dispatcher;

        public PushCollectionViewModel ViewPushCollection { get; } = new PushCollectionViewModel();

        public PushCollectionViewModel AllPushCollection { get; } = new PushCollectionViewModel();

        public FilterViewModel FilterViewModel { get; } = new FilterViewModel();
        public ChromiumWebBrowser WebBrowser { get; internal set; }

        public MainViewModel()
        {
            FilterViewModel.AllPushCollection = this.AllPushCollection;
            FilterViewModel.FilteredPushCollection.CollectionChanged +=
                (sender, e) => Dispatcher.Invoke(() =>
                {
                    ViewPushCollection.ResetTo(FilterViewModel.FilteredPushCollection);
                    if (FilterViewModel.HighlightResults)
                    {
                        WebBrowser.GetMainFrame().EvaluateScriptAsync($"highlight([{string.Join(",", ViewPushCollection.Select(p => p.Index))}])");
                    }
                    else
                    {
                        WebBrowser.GetMainFrame().EvaluateScriptAsync($"highlight([])");
                    }
                });
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


        
    }
}
