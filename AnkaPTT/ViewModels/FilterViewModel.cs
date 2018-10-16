using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT.ViewModels
{
    class FilterViewModel
    {
        public bool EnabledStartTime { get; set; } = true;
        public DateTime StartTime { get; set; } = DateTime.Now;

        public bool EnabledEndTime { get; set; } = false;
        public DateTime EndTime { get; set; } = DateTime.Now;

        public bool ContainsPush { get; set; } = true;
        public bool ContainsUnlike { get; set; } = false;
        public bool ContainsArrow { get; set; } = false;
        public bool ExcludeSameId { get; set; } = false;



        public int BeginAt { get; set; } = 1;
        public int Step { get; set; } = 0;
        public int TakeCount { get; set; } = 0;

        public int SameTimes { get; set; } = 0;


        public IEnumerable<PushViewModel> ApplyFilter(IQueryable<PushViewModel> pushes)
        {
            IQueryable<PushViewModel> query = pushes;

            // Time filter
            if (EnabledStartTime)
                query = query.SkipWhile(p => p.GuessDateTime < StartTime);

            // Tag filter
            HashSet<string> tags = new HashSet<string>();
            if (ContainsPush) tags.Add("推");
            if (ContainsUnlike) tags.Add("噓");
            if (ContainsArrow) tags.Add("→");
            query = query.Where(p => tags.Contains(p.Tag));
            
            query = query.Skip(BeginAt - 1);
            int lessCount = TakeCount;
            int stepSkip = Step - 1;
            var enumerator = query.GetEnumerator();
            HashSet<string> addedId = new HashSet<string>();

            while (lessCount > 0 && enumerator.MoveNext())
            {
                var curr = enumerator.Current;
                if (ExcludeSameId)
                {
                    if (addedId.Contains(curr.Userid))
                        continue;
                    addedId.Add(curr.Userid);
                }
                yield return enumerator.Current;
                for (int i = 0; i < stepSkip; i++)
                    enumerator.MoveNext();
            }
        }

        static IEnumerable<PushViewModel> stepEnumerate(IEnumerable<PushViewModel> query, int step, bool excludeSameId)
        {
            var enumerator = query.GetEnumerator();
            HashSet<string> addedId = new HashSet<string>();

            while (enumerator.MoveNext())
            {
                var curr = enumerator.Current;
                if (excludeSameId)
                {
                    if (addedId.Contains(curr.Userid))
                        continue;
                    addedId.Add(curr.Userid);
                }
                yield return enumerator.Current;
                for (int i = 0; i < step; i++)
                    enumerator.MoveNext();
            }
        }

    }
}
