using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT.ViewModels
{
    class FilterViewModel
    {
        public bool Enabled { get; set; } = true;

        public bool EnabledStartTime { get; set; } = true;
        public DateTime StartTime { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

        public bool EnabledEndTime { get; set; } = false;
        public DateTime EndTime { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

        public bool ContainsPush { get; set; } = true;
        public bool ContainsUnlike { get; set; } = false;
        public bool ContainsArrow { get; set; } = false;
        public bool ExcludeSameId { get; set; } = false;



        public int BeginAt { get; set; } = 1;
        public int Step { get; set; } = 0;
        public int TakeCount { get; set; } = 0;

        public int SameTimes { get; set; } = 0;


        public IEnumerable<PushViewModel> ApplyFilter(IEnumerable<PushViewModel> pushes)
        {
            if (!Enabled) return pushes;

            IEnumerable<PushViewModel> query = pushes;

            // Time filter
            if (EnabledStartTime)
                query = query.SkipWhile(p => !p.GuessDateTime.HasValue || p.GuessDateTime < StartTime);
            if (EnabledEndTime)
                query = query.TakeWhile(p => !p.GuessDateTime.HasValue || p.GuessDateTime < EndTime);

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
                    if (!dic_of_content.TryGetValue(p.Content, out list))
                    {
                        list = new List<PushViewModel>();
                        dic_of_content[p.Content] = list;
                    }
                    list.Add(p);
                    if (list.Count == SameTimes)
                        listofListMoreThanSameTimes.Add(list);
                }

                return listofListMoreThanSameTimes.SelectMany(list => list);


                enumerable = enumerable.ToList();
                var sameContentGroups = from p in enumerable
                                        group p by p.Content into g
                                        where g.Count() > SameTimes
                                        select new
                                        {
                                            Group = g,
                                            MinIndex = g.Min(p2 => p2.Index)
                                        };
                sameContentGroups = sameContentGroups.OrderBy(g => g.MinIndex);
                if (TakeCount > 0) sameContentGroups = sameContentGroups.Take(TakeCount);

                return sameContentGroups
                    .SelectMany(g => g.Group)
                    .OrderBy(p => p.Index);
            }
            else
                return TakeCount > 0 ? enumerable.Take(TakeCount) : enumerable;
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

    }
}
