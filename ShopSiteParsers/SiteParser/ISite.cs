

using System;
using System.Collections.Generic;
using ShopSiteParsers.Models;

namespace ShopSiteParsers.SiteParser
{
    interface ISite
    {
        void Run();

        event Action<MerchandiseItem> ItemAdded;
        event Action<IEnumerable<MerchandiseItem>> ParsingFinished;
    }
}