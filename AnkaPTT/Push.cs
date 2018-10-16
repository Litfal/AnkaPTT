using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT
{
    class Push
    {
        public int index { get; set; }
        public string tag { get; set; }
        public string userid { get; set; }
        public string content { get; set; }
        public string ipdatetime { get; set; }
        public string ip { get; set; }
        public string date { get; set; }
        public string time { get; set; }

        public static Push MapByDynamic(IDictionary<string, object> src)
        {
            return new Push()
            {
                tag = src["tag"] as string,
                userid = src["userid"] as string,
                content = src["content"] as string,
                ipdatetime = src["ipdatetime"] as string,
                index = (int)src["index"],
                ip = src["ip"] as string,
                date = src["date"] as string,
                time = src["time"] as string,
            };
        }

        public static IEnumerable<Push> MapByDynamicList(IEnumerable<dynamic> list)
        {
            if (list == null) yield break;
            foreach (ExpandoObject item in list)
            {
                Push p = null;
                try
                {
                    p = MapByDynamic(item);

                }
                catch (Exception)
                {

                    throw;
                }
                if (p != null) yield return p;
            }
        }

        public override string ToString()
        {
            return $"{index}F\t{ipdatetime}\t{tag}{userid}: {content}";
            // return base.ToString();
        }
    }
}
