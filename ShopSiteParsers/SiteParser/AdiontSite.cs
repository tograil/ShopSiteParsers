using ShopSiteParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopSiteParsers.SiteParser
{
    public class AdiontSite : SiteBase
    {
        #region Fields
        private string siteAddress = "http://www.adiont.ru";
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


        }
        #endregion

        private void Login()
        {
            //const string userName = "solars_888@mail.ru";
            //const string password = ">8t$Pe$u";

            //var req = WebRequest.Create("http://leostore.ru/customers/login");

            //using (var resp = req.GetResponse())
            //{
            //    //SHOPS_SID=6bhekjtpopt56q2h6vkhqnuh40; path=/; domain=leostore.ru
            //    var setCookie = resp.Headers[HttpResponseHeader.SetCookie];

            //    cookie = Regex.Match(setCookie, "SHOPS_SID=.+?;").Value;
            //}

            //req = WebRequest.Create("http://leostore.ru/customers/login");
            //req.Method = "POST";
            //req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            //req.Headers[HttpRequestHeader.Cookie] = cookie;

            //using (var writer = new StreamWriter(req.GetRequestStream()))
            //{
            //    writer.Write(string.Format("customer_email={0}&customer_password={1}", userName.Replace("@", "%40"), password));
            //}

            //using (var r = req.GetResponse()) { }
        }


        private string[] GetGroups()
        {
            var page = LoadPage(siteAddress);
            //<li class='parent'><a href="/shop/sunwear-vesna-leto-2015">

            return null;
        }

        private IEnumerable<MerchandiseItem> GetProductsForGroup(string address)
        {
            return null;
        }
    }
}