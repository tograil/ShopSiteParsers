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
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";

            var response = (HttpWebResponse)request.GetResponse();

            var stream = response.GetResponseStream();

            string text;

            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }

		protected virtual MemoryStream DownloadFile(string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);

			if (cookie != null)
				request.Headers[HttpRequestHeader.Cookie] = cookie;
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";

			var response = (HttpWebResponse)request.GetResponse();

			var ms = new MemoryStream();

			if (response.ContentLength > 0)
			{
				using (var stream = response.GetResponseStream())
				{
					byte[] b = new byte[1000];
					int bytesRead = 0;
					do
					{
						bytesRead = stream.Read(b, 0, b.Length);
						ms.Write(b, 0, bytesRead);
						ms.Read(b, 0, bytesRead);
					}
					while (bytesRead > 0);

					ms.Position = 0;
				}
			}

			return ms;
		}
    }
}