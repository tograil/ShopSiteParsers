using Excel;
using ShopSiteParsers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ShopSiteParsers.SiteParser
{
    public class AdiontSite : SiteBase
    {
        #region Fields
        private string siteAddress = "http://www.adiont.ru";
		private List<AvailabilityInfo> productAvailabilities = new List<AvailabilityInfo>();
        #endregion

        #region ISite Implementation
        public override event Action<MerchandiseItem> ItemAdded;

        public override event Action<IEnumerable<MerchandiseItem>> ParsingFinished;

        public override void Run()
        {
            //Login();
            //var forMale = GetProductsForGender("male", "для него");
            //var forFemale = GetProductsForGender("female", "для нее");

            //var result = forMale.Union(forFemale);
            //if (ParsingFinished != null)
            //    ParsingFinished(result);
			Login();
			GetProductsAvailability();
			
			var grpLinks = GetGroups();
			//foreach (var l in grpLinks)
			//{
			//	GetProductsForGroup(l);
			//}
        }
        #endregion

        private void Login()
        {
			const string userName = "import_data";
            const string password = ">8t$Pe$u";

            var req = (HttpWebRequest)WebRequest.Create(siteAddress);
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";

            using (var resp = req.GetResponse())
            {
                var setCookie = resp.Headers[HttpResponseHeader.SetCookie];

				cookie = setCookie;//Regex.Match(setCookie, "virtuemart=.+?;").Value;
            }
			//2dc87bd3c455f21273b9d4e4d537b256=4d58b54da9993328d5661c57d6efe52f; virtuemart=4d58b54da9993328d5661c57d6efe52f; _ym_visorc_25511984=w
			req = (HttpWebRequest)WebRequest.Create(siteAddress);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.Headers[HttpRequestHeader.Cookie] = cookie;
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
			var ttt = System.Uri.EscapeDataString(password);
			using (var writer = new StreamWriter(req.GetRequestStream()))
			{
				writer.Write(string.Format("username={0}&passwd={1}&Submit=&option=com_user&task=login", userName, System.Uri.EscapeDataString(password)));
			}

			using (var r = req.GetResponse())
			{
				var resp = r.GetResponseStream();
				using (var reader = new StreamReader(resp))
				{
					var text = reader.ReadToEnd();
				}
			}
			var p = LoadPage(siteAddress);
        }

        private string[] GetGroups()
        {
            var page = LoadPage(siteAddress);
            var result = Regex.Matches(page, @"<li class='parent'><a href=""(?<link>[^""]+)"">").Cast<Match>().Select(x => string.Format("{0}{1}?vm-all-pages=1", siteAddress, x.Groups["link"].Value)).ToArray();

            return result;
        }

        private IEnumerable<MerchandiseItem> GetProductsForGroup(string grpLink)
        {
            var page = LoadPage(grpLink);

            var productLinks = Regex.Matches(page, @"<div class=""good-title""[^>]+>\s*<a href=""(?<link>[^""]+)""").Cast<Match>().Select(x => string.Format("{0}{1}", siteAddress, x.Groups["link"].Value)).ToArray();

            List<MerchandiseItem> products = new List<MerchandiseItem>(productLinks.Count());

            foreach (var pl in productLinks)
            {
                //products = GetProduct(string.Format("{0}{1}", siteAddress, pl));
                //Array.ForEach(products, (x) => { x.Sex = genderTitle; if (ItemAdded != null) ItemAdded(x); });
                //result.AddRange(products);
            }

            return null;
        }

		private void GetProductsAvailability()
		{
			 FileStream stream = File.Open("d:\\1\\blank_zakaza_18072015.xls", FileMode.Open, FileAccess.Read);
			 var excelReader = ExcelReaderFactory.CreateBinaryReader(stream);

			//var ms = DownloadFile("http://www.adiont.ru/images/files/blank_zakaza_18072015.xls");

            //var excelReader = ExcelReaderFactory.CreateBinaryReader(ms);

			string s;
			for (int i = 8; i < excelReader.AsDataSet().Tables[0].Rows.Count; i++)
			{
				s = excelReader.AsDataSet().Tables[0].Rows[i][0].ToString();
				var t = Regex.Match(s, @"(?<name>.+?)(\s*(?<color>цвет [^\s]+))?(\s*размер)?\s+(?<size>\S+?)$");
				if (t != null)
					productAvailabilities.Add(new AvailabilityInfo { NameParts = t.Groups["name"].Value.Split(' '), Size = t.Groups["size"].Value, Color = t.Groups["color"] != null ? t.Groups["color"].Value : null });
			}            
		}

		public class AvailabilityInfo
		{
			public string[] NameParts { get; set; }
			public string Size { get; set; }
			public string Color { get; set; }
		}
    }
}