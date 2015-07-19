using ShopSiteParsers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShopSiteParsers.SiteParser
{
    public abstract class SiteBase : ISite
    {
        #region Fields
        protected string cookie;
        #endregion

        #region ISite Implementation
        public abstract event Action<MerchandiseItem> ItemAdded;

        public abstract event Action<IEnumerable<MerchandiseItem>> ParsingFinished;

        public abstract void Run();
        #endregion

        protected virtual string RemoveTags(string source)
        {
            return Regex.Replace(source, "<[^>]+>", string.Empty).Trim();
        }

        protected virtual string LoadPage(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            if(cookie != null)
                request.Headers[HttpRequestHeader.Cookie] = cookie;

            var response = (HttpWebResponse)request.GetResponse();

            var stream = response.GetResponseStream();

            string text;

            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}