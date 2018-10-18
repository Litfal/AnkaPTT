using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AnkaPTT.ViewModels
{
    class PushCollectionViewModel : ObservableCollection<PushViewModel>
    {
        /// <summary>
        /// OnCollectionChanged will be invoke by Dispatcher if it's set. !!! Be careful deadlock on UI thread !!!
        /// </summary>
        public Dispatcher Dispatcher { get; set; }

        public void AddRangeAligned(IEnumerable<PushViewModel> pushes)
        {
            int originCount = Items.Count;
            foreach (var item in pushes.Skip(originCount))
                Items.Add(item);
            if (Items.Count != originCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void AddRangeAligned(IEnumerable<Push> pushes)
        {
            int originCount = Items.Count;
            foreach (var item in pushes.Skip(originCount))
                Items.Add(new PushViewModel(item));
            if (Items.Count != originCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void AddRange(IEnumerable<PushViewModel> pushes)
        {
            int originCount = Items.Count;
            foreach (var item in pushes)
                Items.Add(item);
            if (Items.Count != originCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ResetTo(IEnumerable<PushViewModel> pushes)
        {
            Items.Clear();
            foreach (var item in pushes)
                Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Dispatcher == null)
                base.OnCollectionChanged(e);
            else if (Dispatcher.CheckAccess())
                base.OnCollectionChanged(e);
            else if (!Dispatcher.HasShutdownStarted)
                Dispatcher.Invoke(() => base.OnCollectionChanged(e), DispatcherPriority.DataBind);
        }
    }
}
