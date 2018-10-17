using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AnkaPTT.ViewModels
{
    class MainViewModel
    {
        internal Dispatcher Dispatcher;

        public PushCollectionViewModel ViewPushCollection { get; } = new PushCollectionViewModel();

        public PushCollectionViewModel AllPushCollection { get; } = new PushCollectionViewModel();

        private List<PushViewModel> _allPushes;

        public FilterViewModel FilterViewModel { get; } = new FilterViewModel();

        public MainViewModel()
        {
            FilterViewModel.AllPushCollection = this.AllPushCollection;
            FilterViewModel.FilteredPushCollection.CollectionChanged +=
                (sender, e) => Dispatcher.Invoke(() =>
                {
                    ViewPushCollection.ResetTo(FilterViewModel.FilteredPushCollection);
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

            //if (FilteredPushCollection.Count == pushes.Length) return;
            //_allPushes = new List<PushViewModel>(PushViewModel.ToViewModels(pushes));
            //var filteredPushes = FilterViewModel.ApplyFilter(_allPushes).ToList();
            //System.Diagnostics.Debug.WriteLine(filteredPushes.Count);
            //Dispatcher.Invoke(() => FilteredPushCollection.ResetTo(filteredPushes));


        }


        
    }
}
