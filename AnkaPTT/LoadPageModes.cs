using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT
{
    enum LoadPageModes : int
    {
        Browser = 0,
        HttpClient = 1,
        CompareWholePage = 2,
        ComparePollurl = 4,

        HttpClientAndCompareCacheKey = HttpClient | ComparePollurl,
    }
}
