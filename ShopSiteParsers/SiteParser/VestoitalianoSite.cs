using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using ShopSiteParsers.Constants;
using ShopSiteParsers.Models;

namespace ShopSiteParsers.SiteParser
{
    public class VestoitalianoSite
    {

        private string _cookie = string.Empty;

        public event Action<MerchandiseItem> ItemAdded;
        public event Action ParsingFinished;

        private readonly KeyValuePair<string, string>[] _urls = new []
        {
            new KeyValuePair<string, string> ("http://www.vestoitaliano.it/index.php", "ж"),
            new KeyValuePair<string, string> ("http://www.vestoitaliano.it/index.php?cPath=38_108&cPathName=A%D0%BA%D1%81%D0%B5%D1%81%D1%81%D1%83%D0%B0%D1%80%D1%8B", "ж"),
            new KeyValuePair<string, string> ("http://www.vestoitaliano.it/index.php?cPath=1_2&cPathName=ABBIGLIAMENTO", "м"),
            new KeyValuePair<string, string> ("http://www.vestoitaliano.it/index.php?cPath=1_94&cPathName=A%D0%BA%D1%81%D0%B5%D1%81%D1%81%D1%83%D0%B0%D1%80%D1%8B", "м"),
   
        };

        private const string ManUrl = "http://www.vestoitaliano.it/index.php?cPath=1_2&cPathName=ABBIGLIAMENTO";

        private void Login()
        {
            var preRequest = (HttpWebRequest)WebRequest.Create("http://www.vestoitaliano.it/create_account.php");

            _cookie = string.Format("__session:{0}:=http:;", new Random().NextDouble().ToString(CultureInfo.InvariantCulture));

            preRequest.Headers["Cookie"] = "cookie_test=please_accept_for_session;" + _cookie;
            //preRequest.CookieContainer = new CookieContainer(2);

            using (var preResponse = (HttpWebResponse) preRequest.GetResponse())
            {
                var setCookie = preResponse.Headers[HttpResponseHeader.SetCookie];

                var session = Regex.Match(setCookie, @"Store360=.+?;").Value;
                _cookie = "cookie_test=please_accept_for_session;" + session;

            }
            
            
            var request = (HttpWebRequest) WebRequest.Create("http://www.vestoitaliano.it/login.php?action=process");

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers["Cookie"] = _cookie;

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write("email_address=tograilx%40gmail.com&password=caamY");
            }

            using ((HttpWebResponse)request.GetResponse()) {}
        }


        private string GetSiteData(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers["Cookie"] = _cookie;

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write("language=ru");
            }


            var response = (HttpWebResponse)request.GetResponse();

            var stream = response.GetResponseStream();

            string text;

            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }

        private string GetProductData(string productId)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://www.vestoitaliano.it/product_info_ajax.php");

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers["Cookie"] = _cookie;

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write("pid="+productId);
            }


            var response = (HttpWebResponse)request.GetResponse();

            var stream = response.GetResponseStream();

            string text;

            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }


        public void Run()
        {

            Login();

            foreach (var url in _urls)
            {
                var data = GetSiteData(url.Key);

                var document = new HtmlDocument();

                document.LoadHtml(data);

                var menu = document.DocumentNode.SelectNodes("//ul[@class=\"categories\"]")[2];

                foreach (var childNode in menu.ChildNodes)
                {
                    var a = childNode.Descendants("a").First();
                    var link = HttpUtility.HtmlDecode(a.GetAttributeValue("href", ""));
                    var name = HttpUtility.HtmlDecode(a.InnerText);

                    ParseCategory(name, link, url.Value);
                }
            }

            if (ParsingFinished != null)
                ParsingFinished();
        }

        private void ParseCategory(string name, string url, string sex)
        {
            var data = GetSiteData(url);

            var document = new HtmlDocument();

            document.LoadHtml(data);

            var menu = document.DocumentNode.SelectNodes("//ul[@class=\"categories\"]")[2];

            var subcategories = menu.SelectNodes(".//li//a[not(span)]");

            foreach (var subcategory in subcategories)
            {
                var subcat = HttpUtility.HtmlDecode(subcategory.Descendants("div").First().InnerText);
                var link = HttpUtility.HtmlDecode(subcategory.GetAttributeValue("href", ""));

                ParseSubcategory(name, subcat, link, sex);
            }
        }

        private void ParseSubcategory(string category, string subcategory, string url, string sex)
        {
            do
            {
                var data = GetSiteData(url);

                var document = new HtmlDocument();

                document.LoadHtml(data);

                var options = document.DocumentNode.SelectNodes("//li[@class=\"wrapper_prods hover first last\"]");

                if (options == null)
                {
                    return;
                }

                foreach (var option in options)
                {
                    ParseProperty(category, subcategory, option, sex);
                }

                var pages = document.DocumentNode.SelectNodes("//a[@class=\"pageResults\"]");

                var link = pages != null ? pages.FirstOrDefault(node => node.InnerText == ">>") : null;

                url = link != null ? HttpUtility.HtmlDecode(link.GetAttributeValue("href", "")) : string.Empty;

            } while (!string.IsNullOrEmpty(url));
        }

        private void ParseProperty(string category, string subcategory, HtmlNode option, string sex)
        {
            var price =
                Regex.Match(option.InnerHtml, @"<h2 class=""price price_padd fl_left"">(?<price>[0-9.]+?) &euro;</h2>",
                    RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Groups["price"].Value;

            var url = option.Descendants("h2").First().Descendants("a").First().GetAttributeValue("href", "");

            var merchandiseData = option.Descendants("h2").First().Descendants("a").First().InnerHtml;

            var titleMatch = Regex.Match(merchandiseData, @"^(?<name>.+?)<br>(?<code>.+?)<br>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var title = titleMatch.Groups["name"].Value.Trim();
            var code = titleMatch.Groups["code"].Value;

            var sizes =
                Regex.Matches(option.InnerHtml,
                    @"<div class='colTaglieColori'>(?<color>[a-z\s]+?)</div><div class='colTaglieColori'>(?<size>[a-z0-9]+?)</div><div class='colTaglieColori'>(?<quantity>[0-9]+?)</div>",
                    RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(
                        m =>
                            new MerchandiseItem.Availability
                            {
                                Color = m.Groups["color"].Value,
                                Size = m.Groups["size"].Value,
                                Quantity = int.Parse(m.Groups["quantity"].Value)
                            })
                    .ToArray();

            var consist =
                Regex.Matches(option.InnerHtml,
                    @"<p>(.+?)</p>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(
                        m => m.Groups[1].Value).ToArray();

            if(!consist.Any())
            consist =
                Regex.Matches(option.InnerHtml,
                    @"<div style=""color: #666666;font-size: 12px;"">(.+?)</div>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(
                        m => m.Groups[1].Value).ToArray();
            
            
                            


            var productId = Regex.Match(url, @"products_id=(.+?)&", RegexOptions.IgnoreCase).Groups[1].Value;

            var productData = GetProductData(productId);


            var productDocument = new HtmlDocument();
            productDocument.LoadHtml(productData);

            var images =
                productDocument.DocumentNode.SelectNodes("//li[@class=\"wrapper_pic_div\"]//img")
                    .Select(node => node.Attributes["src"].Value).ToArray();

            if (ItemAdded == null) return;

            foreach (var item in sizes.SelectMany(size => images.Select(image => new MerchandiseItem
            {
                Category = category,
                Subcategory = subcategory,
                Code = string.Join("-", code, size.Color),
                Name = title,
                Avail = new MerchandiseItem.Availability
                {
                    Color = RusColors.Colors[size.Color],
                    Size = size.Size,
                    Quantity = sizes.Where(size1 => size.Color == size1.Color).Sum(size1 => size1.Quantity)
                },
                Image = image,
                Price = price,
                Consist = consist.Any() ? consist.Last() : string.Empty,
                Sex = sex
            })))
            {
                ItemAdded(item);
            }
        }
    }
}
