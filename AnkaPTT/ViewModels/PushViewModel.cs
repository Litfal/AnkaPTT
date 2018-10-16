using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AnkaPTT.ViewModels
{
    class PushViewModel
    {
        public int Index { get; set; }
        public string Tag { get; set; }
        public string Userid { get; set; }
        public string Content { get; set; }
        public string IPDatetime { get; set; }
        public string IP { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public DateTime? GuessDateTime { get; set; }
        public int Floor { get { return Index + 1; } }

        static Brush _pushBrush = Brushes.LawnGreen;
        static Brush _unlikeBrush = Brushes.Red;
        static Brush _normalBrush = Brushes.White;


        public Brush TagBrush
        {
            get
            {
                switch (Tag)
                {
                    case "推": return _pushBrush;
                    case "噓": return _unlikeBrush;
                    default: return _normalBrush;
                }
            }
        }

        public PushViewModel()
        {

        }

        public PushViewModel(Push push)
        {
            Index = push.index;
            Tag = push.tag;
            Userid = push.userid;
            Content = push.content;
            IPDatetime = push.ipdatetime;
            IP = push.ip;
            Date = push.date;
            Time = push.time;
        }

        static public IEnumerable<PushViewModel> ToViewModels(IEnumerable<Push> pushes)
        {
            return toViewModelsReverse(pushes).Reverse();
        }

        static IEnumerable<PushViewModel> toViewModelsReverse(IEnumerable<Push> pushes)
        {
            // push from latest to oldest
            // guesstime from now to oldest 
            var cultureInfo = CultureInfo.GetCultureInfo("zh-TW");
            int currYear = DateTime.Now.Year;
            DateTime latestDateTime = DateTime.Now;
            foreach (var item in pushes.Reverse())
            {
                var viewModel = new PushViewModel(item);
                DateTime guessDT;
                if (DateTime.TryParseExact($"{currYear}/{item.date} {item.time}", "yyyy/MM/dd HH:mm", cultureInfo, DateTimeStyles.AssumeLocal, out guessDT))
                {
                    if (guessDT > latestDateTime) guessDT = guessDT.AddYears(-1);
                    latestDateTime = guessDT;
                    viewModel.GuessDateTime = guessDT;
                }
                yield return viewModel;
            }
        }
    }
}
