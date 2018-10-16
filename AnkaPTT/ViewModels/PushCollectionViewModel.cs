using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT.ViewModels
{
    class PushCollectionViewModel : ObservableCollection<PushViewModel>
    {
        public void AddRangeAligned(IEnumerable<PushViewModel> pushes)
        {
            int originCount = Items.Count;
            foreach (var item in pushes.Skip(originCount))
                Items.Add(item);
            if (Items.Count != originCount)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        public void AddRangeAligned(IEnumerable<Push> pushes)
        {
            int originCount = Items.Count;
            foreach (var item in pushes.Skip(originCount))
                Items.Add(new PushViewModel(item));
            if (Items.Count != originCount)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        public void AddRange(IEnumerable<PushViewModel> pushes)
        {
            int originCount = Items.Count;
            foreach (var item in pushes)
                Items.Add(item);
            if (Items.Count != originCount)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        public void ResetTo(IEnumerable<PushViewModel> pushes)
        {
            Items.Clear();
            foreach (var item in pushes)
                Items.Add(item);
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
    }
}
