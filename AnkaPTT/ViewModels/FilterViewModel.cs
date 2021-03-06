﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT.ViewModels
{
    class FilterViewModel : ObservableObject
    {
        bool _enabled = true;
        bool _enabledStartTime = false;
        DateTime _startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        bool _enabledEndTime = false;
        DateTime _endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

        bool _enableContainsText;
        string _containsText;

        bool _containsPush = true;
        bool _containsUnlike = false;
        bool _containsArrow = false;
        bool _excludeSameId = false;

        int _beginAt = 1;
        int _step = 0;
        int _takeCount = 0;
        int _sameTimes = 0;
        bool _highlightResults = true;
        bool _reverseView = false;

        public bool Enabled { get { return _enabled; } set { SetField(ref _enabled, value); } }


        public bool EnabledStartTime { get { return _enabledStartTime; } set { SetField(ref _enabledStartTime, value); }  }
        public DateTime StartTime { get { return _startTime; } set { SetField(ref _startTime, value); } }

        public bool EnabledEndTime { get { return _enabledEndTime; } set { SetField(ref _enabledEndTime, value); } }
        public DateTime EndTime { get { return _endTime; } set { SetField(ref _endTime, value); } }

        public bool EnableContainsText { get { return _enableContainsText; } set { SetField(ref _enableContainsText, value); } }
        public string ContainsText { get { return _containsText; } set { SetField(ref _containsText, value); } }

        public bool ContainsPush { get { return _containsPush; } set { SetField(ref _containsPush, value); } }
        public bool ContainsUnlike { get { return _containsUnlike; } set { SetField(ref _containsUnlike, value); } }
        public bool ContainsArrow { get { return _containsArrow; } set { SetField(ref _containsArrow, value); } }

        public bool ExcludeSameId { get { return _excludeSameId; } set { SetField(ref _excludeSameId, value); } }


        public int BeginAt { get { return _beginAt; } set { SetField(ref _beginAt, value); } }
        public int Step { get { return _step; } set { SetField(ref _step, value); } }

        public int TakeCount { get { return _takeCount; } set { SetField(ref _takeCount, value); } }

        public int SameTimes { get { return _sameTimes; } set { SetField(ref _sameTimes, value); } }

        public bool HighlightResults { get { return _highlightResults; } set { SetField(ref _highlightResults, value); } }
        
        public bool ReverseView { get { return _reverseView; } set { SetField(ref _reverseView, value); } }


        public int HighlightKey { get; set; }

        private MainViewModel _mainViewModel;

        PushCollectionViewModel _monitorPushCollection;
        public PushCollectionViewModel MonitorPushCollection {
            get { return _monitorPushCollection; }
            private set
            {
                if (_monitorPushCollection == value) return;
                if (_monitorPushCollection != null)
                {
                    _monitorPushCollection.CollectionChanged -= MonitorPushCollection_CollectionChanged;
                    FilteredPushCollection.Clear();
                }
                _monitorPushCollection = value;
                if (_monitorPushCollection != null)
                {
                    _monitorPushCollection.CollectionChanged += MonitorPushCollection_CollectionChanged;
                    MonitorPushCollection_CollectionChanged(_monitorPushCollection, 
                        new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
                }
            }
        }

        public PushCollectionViewModel FilteredPushCollection { get; } = new PushCollectionViewModel();

        public FilterViewModel()
        {
        }

        public void SetMainViewModel(MainViewModel mainViewModel)
        {
            if (_mainViewModel != null) throw new Exception("SetMainViewModel only can be called once.");
            _mainViewModel = mainViewModel;
            FilteredPushCollection.Dispatcher = _mainViewModel.Dispatcher;
            MonitorPushCollection = _mainViewModel.AllPushCollection;
        }

        internal void Dispose()
        {
            MonitorPushCollection = null;
            _mainViewModel.ReleseFilter(this);
        }

        private void MonitorPushCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplyFilterAsync();
        }

        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            ApplyFilterAsync();
        }

        Task ApplyFilterAsync()
        {
            return ApplyFilterAsync(MonitorPushCollection)
                .ContinueWith((r) => FilteredPushCollection.ResetTo(r.Result));
        }

        private Task<IEnumerable<PushViewModel>> ApplyFilterAsync(IEnumerable<PushViewModel> pushes)
        {
            return Task.Run(() => ApplyFilter(pushes));
        }

        private IEnumerable<PushViewModel> ApplyFilter(IEnumerable<PushViewModel> pushes)
        {
            if (!Enabled) return pushes;

            IEnumerable<PushViewModel> query = pushes;

            // Time filter
            if (EnabledStartTime)
                query = query.SkipWhile(p => !p.GuessDateTime.HasValue || p.GuessDateTime < StartTime);
            if (EnabledEndTime)
                query = query.TakeWhile(p => !p.GuessDateTime.HasValue || p.GuessDateTime < EndTime);

            if (EnableContainsText)
            {
                string containsText = ContainsText;
                var compareInfo = System.Globalization.CultureInfo.GetCultureInfo("zh-TW").CompareInfo;
                if (containsText.Length > 0)
                    query = query.Where(p => compareInfo.IndexOf(p.Content, containsText, System.Globalization.CompareOptions.IgnoreCase) >= 0);
            }

            // Tag filter
            HashSet<string> tags = new HashSet<string>();
            if (ContainsPush) tags.Add("推");
            if (ContainsUnlike) tags.Add("噓");
            if (ContainsArrow) tags.Add("→");

            query = query.Where(p => tags.Contains(p.Tag));

            // begin
            query = query.Skip(BeginAt - 1);

            // step pick
            var enumerable = stepEnumerate(query, Step);



            // Exclude same id 
            if (ExcludeSameId)
            {
                HashSet<string> addedId = new HashSet<string>();
                enumerable = enumerable.Where(p => addedId.Add(p.Userid));
            }
            // group by content
            if (SameTimes >= 2)
            {
                Dictionary<string, List<PushViewModel>> dic_of_content = new Dictionary<string, List<PushViewModel>>();
                List<List<PushViewModel>> listofListMoreThanSameTimes = new List<List<PushViewModel>>();
                foreach (var p in enumerable)
                {
                    List<PushViewModel> list;
                    string compareContent = string.Join(" ", p.Content.Split(new char[] { ' ', '　', '\t' }, StringSplitOptions.RemoveEmptyEntries));
                    // string compareContent = p.Content;
                    if (!dic_of_content.TryGetValue(compareContent, out list))
                    {
                        list = new List<PushViewModel>();
                        dic_of_content[compareContent] = list;
                    }
                    list.Add(p);
                    if (list.Count == SameTimes)
                        listofListMoreThanSameTimes.Add(list);
                }
                enumerable = (TakeCount > 0 ? listofListMoreThanSameTimes.Take(TakeCount): listofListMoreThanSameTimes).SelectMany(list => list);
            }
            else
                enumerable = TakeCount > 0 ? enumerable.Take(TakeCount) : enumerable;

            if (ReverseView) enumerable = enumerable.Reverse();
            return enumerable.ToList();
        }

        static IEnumerable<PushViewModel> stepEnumerate(IEnumerable<PushViewModel> query, int step)
        {
            var enumerator = query.GetEnumerator();
            int stepSkip = step - 1;
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
                for (int i = 0; i < stepSkip; i++)
                    enumerator.MoveNext();
            }
        }

        public event EventHandler<PushClickedEventArgs> PushDoubleClicked;

        public void OnPushDoubleClicked(PushViewModel push) => PushDoubleClicked?.Invoke(this, new PushClickedEventArgs(push));

    }

    class PushClickedEventArgs : EventArgs
    {
        public PushViewModel Push { get; set; }
        public PushClickedEventArgs(PushViewModel push)
        {
            Push = push;
        }
    }
}
