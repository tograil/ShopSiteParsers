using Excel;
using ShopSiteParsers.Constants;
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
			Login();
			GetProductsAvailability();
			var groups = GetGroupLinks();
			var result = groups.SelectMany(x => GetProductsForGroup(x)).ToArray();
			//SerializeToFile(t, @"d:\\o.txt");

			//var t = DeserializeFromFile(@"d:\\o.txt") as MerchandiseItem[];
			//var tt = t.Select(x => x.Name).Distinct().Count();
			//var result = UnionItemsWithAvailability(t);

			if (ParsingFinished != null)
				ParsingFinished(result);
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
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
			var ttt = System.Uri.EscapeDataString(password);
			using (var writer = new StreamWriter(req.GetRequestStream()))
			{
				writer.Write(string.Format("username={0}&passwd={1}&Submit=&option=com_user&task=login&{2}=1", userName, System.Uri.EscapeDataString(password), token));
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
        }

        private string[] GetGroupLinks()
        {
            var page = LoadPage(siteAddress);
            var result = Regex.Matches(page, @"<li class='parent'><a href=""(?<link>[^""]+)"">").Cast<Match>().Select(x => string.Format("{0}{1}?vm-all-pages=1", siteAddress, x.Groups["link"].Value)).ToArray();

            return result;
        }

        private IEnumerable<MerchandiseItem> GetProductsForGroup(string grpLink)
        {
            List<MerchandiseItem> result = new List<MerchandiseItem>();

            var page = LoadPage(grpLink);

            var productLinks = Regex.Matches(page, @"<div class=""good-title""[^>]+>\s*<a href=""(?<link>[^""]+)""").Cast<Match>().Select(x => string.Format("{0}{1}", siteAddress, x.Groups["link"].Value)).ToArray();

            List<MerchandiseItem> products;

            foreach (var pl in productLinks)
            {
                products = GetProduct(pl);
                //Array.ForEach(products, (x) => { x.Sex = "для неё"; if (ItemAdded != null) ItemAdded(x); });
                result.AddRange(products);
            }

            return result;
        }

		private List<MerchandiseItem> GetProduct(string url)
		{
			List<MerchandiseItem> result = new List<MerchandiseItem>();

			try
			{
				var document = new HtmlAgilityPack.HtmlDocument();

				var page = LoadPage(url);
				document.LoadHtml(page);
				var t = document.DocumentNode.SelectNodes("//div[@class=\"vitrina\"]//div[@class=\"content\"]").First().SelectNodes("div").Skip(1).First().InnerHtml;
				t = Regex.Replace(t, @"<a[^>]+.+?</a>", string.Empty).Replace("/", string.Empty).Trim();
				var title = Regex.Match(t, @"(?<title>[^<]+)<").Groups["title"].Value.Trim();
				t = document.DocumentNode.SelectNodes("//div[@class=\"detailprice\"]").First().InnerHtml;
				var price = Regex.Replace(t.Replace("</span>", string.Empty), @"<span[^>]*>", " ").Trim();

				t = document.DocumentNode.SelectNodes("//div[@class=\"text\"]").First().InnerHtml;
				var consist = Regex.Match(t, @"Состав(:)?(?<consist>[^<]+)").Groups["consist"].Value.Trim();
				var color = Regex.Match(page, @"Цвет (?<color>.+?)\.").Value.Trim();
				t = document.DocumentNode.SelectSingleNode("//ul[@id=\"thumblist\"]").InnerHtml;
				var images1 = Regex.Matches(t, @"largeimage:\s*'(?<img>[^']+)'").Cast<Match>().Select(x => x.Groups["img"].Value.StartsWith("http://") ? x.Groups["img"].Value : string.Format("{0}/{1}", siteAddress, x.Groups["img"].Value));
				var images2 = Regex.Matches(page, @"href=""(?<img>[^""]+)""\s*rel=""lightbox""").Cast<Match>().Select(x => x.Groups["img"].Value.IndexOf("javascript:void(0);") > -1 ? string.Empty : x.Groups["img"].Value.StartsWith("http://") ? x.Groups["img"].Value : string.Format("{0}/{1}", siteAddress, x.Groups["img"].Value));
				var images = images1.Union(images2);

				// Color записываю в параметр Subcategory, так как категории нет, а Avail будет формироваться на основании excel-документа
				string category = RusCategories.Categories.Where(x => title.IndexOf(x.Key, StringComparison.InvariantCultureIgnoreCase) > -1).Select(x => x.Value).FirstOrDefault();

				var tt = images.Select(x => new MerchandiseItem { Name = title, Category = category, Price = price, Consist = consist, Subcategory = color, Image = x }).ToArray();
				Array.ForEach(tt, x => result.AddRange(UnionItemWithAvailability(x)));
			}
			catch
			{

			}

			return result;
		}

		private void GetProductsAvailability()
		{
			var page = LoadPage("http://www.adiont.ru/opt-blank");
			var l = Regex.Match(page, @"href=""(?<link>.+?)""[^>]*>Бланк заказа по наличию").Groups["link"].Value;

			//string l = "dsadsd";
			//FileStream stream = File.Open("d:\\1\\blank_zakaza_01082015.xls", FileMode.Open, FileAccess.Read);
			//var excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
			if (!string.IsNullOrWhiteSpace(l))
			{
				var ms = DownloadFile(string.Format("{0}{1}", siteAddress, l));
				var excelReader = ExcelReaderFactory.CreateBinaryReader(ms);

				string s;
				for (int i = 8; i < excelReader.AsDataSet().Tables[0].Rows.Count; i++)
				{
					s = excelReader.AsDataSet().Tables[0].Rows[i][0].ToString();
					var t = Regex.Match(s, @"(?<name>.+?)(\s*(?<color>цвет [^\s]+))?(\s*размер)?\s+(?<size>\S+?)(\s*SALE)?$");
					if (t != null && !string.IsNullOrWhiteSpace(t.Groups["name"].Value))
						productAvailabilities.Add(new AvailabilityInfo { NameParts = t.Groups["name"].Value.Split(' '), Size = t.Groups["size"].Value, Color = t.Groups["color"] != null ? t.Groups["color"].Value : null });
				}
			}
		}

		private List<MerchandiseItem> UnionItemWithAvailability(MerchandiseItem item)
		{
			List<MerchandiseItem> result = new List<MerchandiseItem>();

			var t = productAvailabilities.Where(x => HasItemNameParts(item, x.NameParts)).Select(x => new { Item = item, Av = x }).ToArray();
			if (t.Length == 0)
				t = productAvailabilities.Where(x => HasItemNameParts(item, x.NameParts.Skip(1).ToArray())).Select(x => new { Item = item, Av = x }).ToArray();

			if (t.Length > 0)
			{
				var tt = t.Where(x => string.Compare(x.Item.Subcategory, x.Av.Color) == 0 || x.Item.Name.IndexOf(x.Av.Color, StringComparison.InvariantCultureIgnoreCase) > -1).ToArray();
				if (tt.Length > 0)
					t = tt;

				Array.ForEach(t, (x) =>
					{
						if (result.Where(y => string.Compare(y.Name, x.Item.Name) == 0 && string.Compare(y.Image, x.Item.Image) == 0 && string.Compare(y.Avail.Color, x.Av.Color) == 0 && string.Compare(y.Avail.Size, x.Av.Size) == 0).Count() == 0)
						{
							var xx = new MerchandiseItem { Name = x.Item.Name, Price = x.Item.Price, Consist = x.Item.Consist, Image = x.Item.Image, Avail = new MerchandiseItem.Availability { Color = x.Av.Color, Size = x.Av.Size, Quantity = 1 } };
							result.Add(xx);
							if (ItemAdded != null) ItemAdded(xx);
						}
					});
			}

			return result;
		}

		private bool HasItemNameParts(MerchandiseItem item, string[] nameParts)
		{
			if (item == null || string.IsNullOrWhiteSpace(item.Name) || nameParts == null || nameParts.Length == 0)
				return false;
			return nameParts.Count(x => item.Name.IndexOf(x) > -1) == nameParts.Length;
		}

		public class AvailabilityInfo
		{
			public string[] NameParts { get; set; }
			public string Size { get; set; }
			public string Color { get; set; }
		}
    }
}