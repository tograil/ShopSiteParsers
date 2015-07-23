using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ShopSiteParsers.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace ShopSiteParsers.SiteParser
{
    public class ToBuySite : ISite
    {
        private class Category
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public readonly List<Category> Children = new List<Category>();
            public readonly List<Good> Goods = new List<Good>(); 
        }

        private class Good
        {
            public string Id { get; set; }
            public string ParentId { get; set; }
            public string Price { get; set; }
            public string Name { get; set; }
            public string Vendor { get; set; }
            public string VendorCode { get; set; }
            public string Description { get; set; }
            public string Country { get; set; }
            public string Material { get; set; }
            public List<GoodParam> Params { get; set; } 
            public List<string> Photos { get; set; }
            
        }

        private class GoodParam
        {
            public string Color { get; set; }
            public string Size { get; set; }
            public string Quantity { get; set; }
        }

        public void Run()
        {
            var categories = GetCategories().ToList();

            AddMaterials(categories);

			var items = PushToCallback(categories, new string[] { });

            if (ParsingFinished != null)
                ParsingFinished(items);
        }

        private static IEnumerable<string[]> GetSiteData(string url)
        {
            const string username = "import_data";
            const string password = ">8t$Pe$u";
            var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add("Authorization", "Basic " + encoded);

            var response = (HttpWebResponse)request.GetResponse();

            var stream = response.GetResponseStream();

            string text;

            using (var reader = new StreamReader(stream ?? Stream.Null))
            {
                text = reader.ReadToEnd();
            }

            return text.Trim('\n').Split('\n').Select(line => line.Trim(',').Split(',').Select(value => value.Trim('"').Replace("&amp;", "&")).ToArray()).ToList();

            
        }

        private static IEnumerable<Category> GetCategories()
        {
            var result = GetSiteData("http://www.2-buy.ru/csv/category.csv");

            var list = new List<Category>();

            foreach (var item in result.Skip(1).Select(x => new {Id = x[0], ParentId = x[1], Name = x[2]}))
            {
                if (item.ParentId == string.Empty)
                {
                    list.Add(new Category {Id = item.Id, Name = item.Name});
                }
                else
                {
                    list.ForEach(p => AddParent(p, new Category {Id = item.Id, Name = item.Name}, item.ParentId));
                }
            }

            return list;

        }

        private static void AddParent(Category parent, Category child, string parentId)
        {
            if (parent.Id == parentId)
            {
                parent.Children.Add(child);
            }
            else
            {
                parent.Children.ForEach(p => AddParent(p, child, parentId));
            }
        }

        private static void AddMaterials(List<Category> categories)
        {
            var goodmaterials = GetSiteData("http://www.2-buy.ru/csv/goodmaterial.csv").Skip(1).ToArray();

            var goodquantities = GetSiteData("http://www.2-buy.ru/csv/goodquantity.csv").Skip(1).ToArray();

            var photos = GetSiteData("http://www.2-buy.ru/csv/photo.csv").Skip(1).ToArray();


            GetSiteData("http://www.2-buy.ru/csv/good.csv").Skip(1).Select(line => 
                line.Length > 8 ? new Good
                {
                    Id = line[0],
                    ParentId = line[5],
                    Price = line[2],
                    Name = line[6].Replace("купить оптом со склада в Москве", string.Empty),
                    Vendor = line[7],
                    VendorCode = line[8],
                    Description = line.Length >= 10 ?  line[9] : string.Empty,
                    Country = line.Length >= 11 ? line[10] : string.Empty,
                    Material = string.Join(" ", goodmaterials.Where(material => string.Equals(material[0].Trim(), line[0].Trim())).Select(material => string.Join(" ", material.Skip(1).Concat(new [] { "%" })))),
                    Params = goodquantities.Where(quan => string.Equals(quan[0], line[0])).Select(quan => new GoodParam { Color = quan[1], Size = quan[2], Quantity = quan[3]}).ToList(),
                    Photos = photos.Where(photo => string.Equals(photo[1], line[0])).Select(photo => photo[3]).ToList()
                    
                } : null).Where(y => y!=null).ToList().ForEach(good => categories.ForEach(category => AppendGood(category, good, good.ParentId)));

        }

        private static void AppendGood(Category category, Good good, string categoryId)
        {
            if(category.Id == categoryId)
                category.Goods.Add(good);
            else
            {
                category.Children.ForEach(cat => AppendGood(cat, good, categoryId));
            }
        }

        private static IEnumerable<MerchandiseItem> PushToCallback(List<Category> categories, string[] parentCategory)
        {
            var x = categories.SelectMany(category => 
                category.Goods.SelectMany(good => 
                    good.Photos.SelectMany(photo =>
                        good.Params.Select(param =>
                    
                new MerchandiseItem
                {
                    CategoriesPath = parentCategory.Concat(new [] { category.Name }).ToArray(),
                    //Subcategory = string.IsNullOrEmpty(parentCategory) ? string.Empty : category.Name,
                    Avail = new MerchandiseItem.Availability
                    {
                        Color = param.Color,
                        Quantity = int.Parse(param.Quantity.Trim()),
                        Size = param.Size.Trim()
                    },
                    Price = good.Price,
                    Name = good.Name,
                    Code = good.VendorCode,
                    Consist = good.Material,
					Sex = parentCategory.Concat(new[] { category.Name }).Any(cat => cat.Contains("мужск") || cat.Contains("мальчи")) ? "для него" :
						parentCategory.Concat(new[] { category.Name }).Any(cat => cat.Contains("жен") || cat.Contains("девоч")) ? "для нее" : string.Empty,
                    Image = photo,
					Country = good.Country

                })))).ToList();

			x.AddRange(categories.SelectMany(category => PushToCallback(category.Children, parentCategory.Concat(new[] { category.Name }).ToArray())));

            return x;
        }

        public event Action<MerchandiseItem> ItemAdded;

        public event Action<IEnumerable<MerchandiseItem>> ParsingFinished;
    }
}
