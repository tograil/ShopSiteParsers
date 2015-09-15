using HtmlAgilityPack;
using ShopSiteParsers.Constants;
using ShopSiteParsers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace ShopSiteParsers.SiteParser
{
    public class LeoStoreSite : SiteBase
    {
        #region Fields
        private string siteAddress = "http://leostore.ru";
        #endregion

        #region ISite Implementation
        public override event Action<MerchandiseItem> ItemAdded;

        public override event Action<IEnumerable<MerchandiseItem>> ParsingFinished;

        public override void Run()
        {
            Login();
			var forMale = GetProductsForGender("male", "для него");
            var forFemale = GetProductsForGender("female", "для нее");

			var result = forMale.Union(forFemale);
			if (ParsingFinished != null)
				ParsingFinished(result);
        }
		#endregion

		private void Login()
        {
            const string userName = "solars_888@mail.ru";
            const string password = ">8t$Pe$u";

            var req = WebRequest.Create("http://leostore.ru/customers/login");

            using (var resp = req.GetResponse())
            {
                var setCookie = resp.Headers[HttpResponseHeader.SetCookie];

                cookie = Regex.Match(setCookie, "SHOPS_SID=.+?;").Value;
            }

            req = WebRequest.Create("http://leostore.ru/customers/login");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Headers[HttpRequestHeader.Cookie] = cookie;

            using (var writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write(string.Format("customer_email={0}&customer_password={1}", userName.Replace("@", "%40"), password));
            }

            using (var r = req.GetResponse()) { }
        }

		private IEnumerable<MerchandiseItem> GetProductsForGender(string gender, string genderTitle)
        {
			var result = new List<MerchandiseItem>();

            var url = string.Format("{0}/catalog/section/{1}?pageno=", siteAddress, gender);
            var page = LoadPage(url);

            var pc = Regex.Match(page, @">(?<cnt>\d+)</a>\s*</li>\s*</ul>").Groups["cnt"].Value;

            int pagesCount = 0, ind = 0;
            int.TryParse(pc, out pagesCount);
            string[] productLinks;
			MerchandiseItem[] products;
            do
            {
                productLinks = Regex.Matches(page, @"<strong class=""title"">\s*<a href=""(?<link>[^""]+)"" class=""layer"">").Cast<Match>().Select(x => x.Groups["link"].Value).ToArray();

                foreach (var pl in productLinks)
                {
                    products = GetProduct(string.Format("{0}{1}", siteAddress, pl),genderTitle);
					Array.ForEach(products, x => { if (ItemAdded != null) ItemAdded(x); });
					result.AddRange(products);
                }

                if (++ind < pagesCount)
                    page = LoadPage(string.Format("{0}{1}", url, ind));
            }
            while (ind < pagesCount);

			return result;
        }

        private MerchandiseItem[] GetProduct(string url, string genderTitle)
        {
			MerchandiseItem[] result;

			var document = new HtmlDocument();

			var page = HttpUtility.HtmlDecode(LoadPage(url)).Replace("&amp;", "&");
            document.LoadHtml(page);

			var category = Regex.Match(page, @"<strong class=""label"">Категория:</strong>\s*<ul class=""values"">\s*<li><a [^>]+>(?<category>[^<]+)</a>", RegexOptions.IgnoreCase).Groups["category"].Value;

			var t = Regex.Match(page, @"<div class=""article_details"">\s*<h1>(?<name>.+?)</h1>\s*<div class=""left"">\s*<ul class=""article_list"">\s*<li>\s*<strong>Артикул:</strong>(?<code>[^<]+)</li>", RegexOptions.IgnoreCase);
			var name = t.Groups["name"].Value.Trim();
            var code = t.Groups["code"].Value.Trim();

			var price = HttpUtility.HtmlDecode(Regex.Match(page, @"id=""price""[^>]*>(?<price>[^<]+)</span>", RegexOptions.IgnoreCase).Groups["price"].Value.Trim());

			string[] parts = new string[4];
			parts[0] = RemoveTags(Regex.Match(page, @">\s*(Material )?Shell\s*(:\s*)?((</b>)?\s*</span>|</span>\s*(</b>)?|</strong>\s*</span>|</strong>)(<span[^>]*>)?\s*(:\s*)?(?<material>.+?)(</span>|</li>|</font>)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["material"].Value);
			parts[1] = RemoveTags(Regex.Match(page, @">\s*(Material )?Lining\s*(:\s*)?((</b>)?\s*</span>|</span>\s*(</b>)?|</strong>\s*</span>|</strong>)(<span[^>]*>)?\s*(:\s*)?(?<material>.+?)(</span>|</li>|</font>)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["material"].Value);
			parts[2] = RemoveTags(Regex.Match(page, @">\s*(Material )?Outside\s*(:\s*)?((</b>)?\s*</span>|</span>\s*(</b>)?|</strong>\s*</span>|</strong>)(<span[^>]*>)?\s*(:\s*)?(?<material>.+?)(</span>|</li>|</font>)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["material"].Value);
			parts[3] = RemoveTags(Regex.Match(page, @">\s*(Material )?Sole\s*(:\s*)?((</b>)?\s*</span>|</span>\s*(</b>)?|</strong>\s*</span>|</strong>)(<span[^>]*>)?\s*(:\s*)?(?<material>.+?)(</span>|</li>|</font>)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["material"].Value);

			string consist = string.Empty;

			if (parts.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 0)
			{
				consist = RemoveTags(Regex.Match(page, @">\s*Material\s*(:\s*)?((</b>\s*)?</span>|</span>(\s*</b>)?|</strong>(\s*</span>)?)\s*(<span[^>]*\s*)?(:\s*)?(?<material>.+?)(</span>|</li>|</font>)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["material"].Value);
			}
			else
			{
				consist = string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select((value, index) => string.Format("{0}: {1}", index == 0 ? "Shell" : index == 1 ? "Lining" : index == 2 ? "Outside" : index == 3 ? "Sole" : string.Empty, value)));
			}

			consist = consist
				.Replace("Acrylic", "Акрил")
				.Replace("Cotton", "Хлопок")
				.Replace("Polyester", "Полиэстер")
				.Replace("Elastane", "Эластан")
				.Replace("Viscose", "Вискоза")
				.Replace("Baumwolle", "Хлопок")
				.Replace("Polyester", "Полиэстер")
				.Replace("Rubber", "Резина")
				.Replace("Wool", "Шерсть")
				.Replace("Lycra", "Лайкра")
				.Replace("Nylon", "Нейлон")
				.Replace("Metal Thread", "Металическая нить")
				.Replace("Polyamide", "Полиамид")
				.Replace("Polyurethane", "Полиуретан")
				.Replace("Polyuretane", "Полиуретан")
				.Replace("Linen", "Лен")
				.Replace("Lyocell", "Лиоцелл")
				.Replace("Synthetic", "Синтетика")
				.Replace("Leather", "Кожа")
				.Replace("Alpaca", "Альпака")
				.Replace("Lurex", "Люрекс")
				.Replace("Angora", "Ангора")
				.Replace("Acrylique", "Акрил")
				.Replace("Wolle", "Шерсть")
				.Replace("Lyocell", "Лиоцелл")
				.Replace("Textile", "Текстиль")
				.Replace("Shell", "Снаружи")
				.Replace("Lining", "Подкладка")
				.Replace("Outside", "Снаружи")
				.Replace("Sole", "Подошва");
			if (consist.StartsWith(">"))
				consist = consist.Substring(1).Trim();
			if (consist.StartsWith(":"))
				consist = consist.Substring(1);

			var color = Regex.Match(page, @"Цвета:</b>\s*</strong>\s*<ul class=""values"">\s*<li>\s*<a.+?background-color: (?<color>[#0-9A-Fa-f]+)", RegexOptions.IgnoreCase).Groups["color"].Value;
			var avails = document.DocumentNode.SelectNodes(@"//td[@class=""key h1 vmiddle""]").Select(x => Regex.Matches(x.InnerHtml, @"(?<size>[^\(]+)\((?<quantity>[\d+])\s*в наличии\)", RegexOptions.IgnoreCase).Cast<Match>().Select(y => new MerchandiseItem.Availability() { Size = y.Groups["size"].Value.Trim(), Quantity = int.Parse(y.Groups["quantity"].Value), Color = RusColors.Colors[color] }).FirstOrDefault()).Where(val => val != null).ToArray();

			var images = document.DocumentNode.SelectNodes(@"//div[@class=""article_images""]//img").Select(x => x.GetAttributeValue("src", "").IndexOf("nopic") == -1 ? string.Format("{0}{1}", siteAddress, x.GetAttributeValue("src", "").Replace("_small_", "_zoom_")) : string.Empty);

            result = images.SelectMany(x => avails.Select(y => new MerchandiseItem { CategoriesPath = new [] { genderTitle, category, string.Empty }, Name = name, Code = code, Price = price.Replace(" €", string.Empty).Replace(",", "."), Consist = consist.Trim().Replace("Снаружи:", string.Empty), Avail = y, Image = x, Sex = genderTitle, Country = "Германия"})).ToArray();
			return result;
        }
    }
}