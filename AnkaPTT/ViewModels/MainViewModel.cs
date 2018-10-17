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

        public PushCollectionViewModel PushCollection { get; } = new PushCollectionViewModel();

        private List<PushViewModel> _allPushes;

        public FilterViewModel FilterViewModel { get; } = new FilterViewModel();



        internal void UpdatePushes(Push[] pushes)
        {
            if (PushCollection.Count == pushes.Length) return;
            _allPushes = new List<PushViewModel>(PushViewModel.ToViewModels(pushes));
            var filteredPushes = FilterViewModel.ApplyFilter(_allPushes).ToList();
            System.Diagnostics.Debug.WriteLine(filteredPushes.Count);
            Dispatcher.Invoke(() => PushCollection.ResetTo(filteredPushes));

            //if (PushCollection.Count > pushes.Length)
            //{
            //    Dispatcher.Invoke(() => PushCollection.ResetTo(_allPushes));
            //}
            //else
            //{
            //    Dispatcher.Invoke(() => PushCollection.AddRange(_allPushes.Skip(PushCollection.Count)));
            //}
        }


        
    }
}
