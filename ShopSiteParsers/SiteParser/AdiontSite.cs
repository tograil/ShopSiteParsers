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

			//cookie = "2dc87bd3c455f21273b9d4e4d537b256=e2ab840e7fb1cbb59b2337a98ae55315; virtuemart=e2ab840e7fb1cbb59b2337a98ae55315; _ym_visorc_25511984=w";
			//cookie = "2dc87bd3c455f21273b9d4e4d537b256=0d4b9c6970d63c182edaed78b8c8499a; virtuemart=e2ab840e7fb1cbb59b2337a98ae55315;";
			//var p = LoadPage("http://www.adiont.ru/");
			var xx = GetProduct("http://www.adiont.ru/shop/rasprodazha/kollekts-ya-figl/m361-briuki-bezh");
			Login();
			GetProductsAvailability();
			
			//var grpLinks = GetGroups();
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
			string token;

			var req = (HttpWebRequest)WebRequest.Create(siteAddress);
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";

			using (var resp = req.GetResponse())
			{
				var setCookie = resp.Headers[HttpResponseHeader.SetCookie];

				cookie = setCookie;//Regex.Match(setCookie, "virtuemart=.+?;").Value;

				var stream = resp.GetResponseStream();
				using (var reader = new StreamReader(stream))
				{
					var text = reader.ReadToEnd();
					token = Regex.Match(text, @"<input type=""hidden"" name=""(?<token>[^""]+)"" value=""1"" />").Groups["token"].Value;
				}
			}

			req = (HttpWebRequest)WebRequest.Create(siteAddress);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			req.AllowAutoRedirect = false;
			req.Headers[HttpRequestHeader.Cookie] = cookie;
			//req.Headers[HttpRequestHeader.Cookie] = cookie = "2dc87bd3c455f21273b9d4e4d537b256=9b3353f2058bb8c19a053ce9b2638cee; virtuemart=9b3353f2058bb8c19a053ce9b2638cee; _ym_visorc_25511984=w";
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
			var ttt = System.Uri.EscapeDataString(password);
			using (var writer = new StreamWriter(req.GetRequestStream()))
			{
				//var ttt1 = string.Format("username=import_data&passwd=%3E8t%24Pe%24u&Submit=&option=com_user&task=login&return=http%3A%2F%2Fwww.adiont.ru%2F&{2}=1", userName, System.Uri.EscapeDataString(password), token);
				writer.Write(string.Format("username={0}&passwd={1}&Submit=&option=com_user&task=login&{2}=1", userName, System.Uri.EscapeDataString(password), token));
				//writer.Write(string.Format("username=import_data&passwd=%3E8t%24Pe%24u&Submit=&option=com_user&task=login&return=http%3A%2F%2Fwww.adiont.ru%2F&eda8a2ff995604e1d6a656c01ce38183=1"));
			}
			
			using (var r = (HttpWebResponse)req.GetResponse())
			{
				var newCookie = r.Headers[HttpResponseHeader.SetCookie];
				if (newCookie != null)
				{
					var match = Regex.Match(newCookie.Replace("path=/", ""), "(?<key>.+?)=(?<value>.+?);");
					cookie = Regex.Replace(cookie, string.Format("{0}=[^;]+", match.Groups["key"].Value), string.Format("{0}={1}", match.Groups["key"].Value, match.Groups["value"].Value));
				}
			}
			//var p = LoadPage(siteAddress);
        }

        private string[] GetGroupLinks()
        {
            var page = LoadPage(siteAddress);
            var result = Regex.Matches(page, @"<li class='parent'><a href=""(?<link>[^""]+)"">").Cast<Match>().Select(x => string.Format("{0}{1}?vm-all-pages=1", siteAddress, x.Groups["link"].Value)).ToArray();

            return result;
        }

        private IEnumerable<MerchandiseItem> GetProductsForGroup(string grpLink)
        {
            var page = LoadPage(grpLink);

            var productLinks = Regex.Matches(page, @"<div class=""good-title""[^>]+>\s*<a href=""(?<link>[^""]+)""").Cast<Match>().Select(x => string.Format("{0}{1}", siteAddress, x.Groups["link"].Value)).ToArray();

            MerchandiseItem[] products;

            foreach (var pl in productLinks)
            {
                products = GetProduct(pl);
                //Array.ForEach(products, (x) => { x.Sex = genderTitle; if (ItemAdded != null) ItemAdded(x); });
                //result.AddRange(products);
            }

            return null;
        }

		private MerchandiseItem[] GetProduct(string url)
		{
			MerchandiseItem[] result;

			var document = new HtmlAgilityPack.HtmlDocument();

			var page = LoadPage(url);
			document.LoadHtml(page);
			var t = document.DocumentNode.SelectNodes("//div[@class=\"vitrina\"]//div[@class=\"content\"]").First().SelectNodes("div").Skip(1).First().InnerHtml;

			var title = Regex.Match(t, @"</a>\s*/\s*(?<title>[^<]+)").Groups["title"].Value.Trim();
			t = document.DocumentNode.SelectNodes("//div[@class=\"text\"]").First().InnerHtml;
			var consist = Regex.Match(t, @"Состав(:)?(?<consist>[^<]+)").Groups["consist"].Value.Trim();

			//t = document.DocumentNode.SelectNodes("//div[@class=\"detailprice\"]").First().InnerHtml;
			//var price = Regex.Replace(t.Replace("</span>", string.Empty), @"<span[^>]*>", " ").Trim();

			return null;
		}

		private void GetProductsAvailability()
		{
			var page = LoadPage("http://www.adiont.ru/opt-blank");
			var l = Regex.Match(page, @"href=""(?<link>.+?)""[^>]*>Бланк заказа по наличию").Groups["link"].Value;
			if (!string.IsNullOrWhiteSpace(l))
			{
				//FileStream stream = File.Open("d:\\1\\blank_zakaza_18072015.xls", FileMode.Open, FileAccess.Read);
				//var excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
				var ms = DownloadFile(string.Format("{0}{1}", siteAddress, l));
				var excelReader = ExcelReaderFactory.CreateBinaryReader(ms);

				string s;
				for (int i = 8; i < excelReader.AsDataSet().Tables[0].Rows.Count; i++)
				{
					s = excelReader.AsDataSet().Tables[0].Rows[i][0].ToString();
					var t = Regex.Match(s, @"(?<name>.+?)(\s*(?<color>цвет [^\s]+))?(\s*размер)?\s+(?<size>\S+?)$");
					if (t != null)
						productAvailabilities.Add(new AvailabilityInfo { NameParts = t.Groups["name"].Value.Split(' '), Size = t.Groups["size"].Value, Color = t.Groups["color"] != null ? t.Groups["color"].Value : null });
				}
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