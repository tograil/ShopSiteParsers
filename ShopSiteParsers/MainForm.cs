using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShopSiteParsers.Models;
using ShopSiteParsers.Properties;
using ShopSiteParsers.SiteParser;

namespace ShopSiteParsers
{
    public partial class MainForm : Form
    {
        private readonly List<MerchandiseItem> _merchandiseItems = new List<MerchandiseItem>();

        private const double Kurs = 61.41;
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStartParse_Click(object sender, EventArgs e)
        {
            //var parser = new ToBuySite();
            var parser = new AdiontSite();

            //var parser = new LeoStoreSite();

            parser.ItemAdded += parser_ItemAdded;
            parser.ParsingFinished += parser_ParsingFinished;
            btnStartParse.Enabled = false;

            Task.Factory.StartNew(() => parser.Run());
        }

        void parser_ParsingFinished(IEnumerable<MerchandiseItem> items)
        {
            lvTovars.Invoke(new Action(() =>
            {
                if (items != null)
                {
                    _merchandiseItems.AddRange(items);
                    lvTovars.VirtualListSize = _merchandiseItems.Count;
                }

                cbCategories.Items.Add("All");
                //cbCategories.Items.AddRange(_merchandiseItems.GroupBy(item => item.Category).Select(catItem => (object)catItem.Key).ToArray());
                //cbCategories.SelectedIndex = 0;
                tbPriceMultiplier.Visible = true;
                chMult.Visible = true;
                cbCategories.Visible = true;
                btExport.Enabled = true;
            }));
        }

        void parser_ItemAdded(MerchandiseItem obj)
        {
            
            _merchandiseItems.Add(obj);

            lvTovars.Invoke(new Action(() => lvTovars.VirtualListSize = _merchandiseItems.Count));
            
        }

        private void lvTovars_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var valueToShow = _merchandiseItems[e.ItemIndex];

            e.Item = new ListViewItem(valueToShow.Category);

            e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, valueToShow.Subcategory));
            e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, valueToShow.Code));
            e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, valueToShow.Name));
            e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, valueToShow.Price));
            e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, valueToShow.Consist));
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            double updater;

            if (!double.TryParse(tbPriceMultiplier.Text,NumberStyles.Any, CultureInfo.InvariantCulture ,out updater ))
            {
                MessageBox.Show(this, Resources.MainForm_btExport_Click_Incorrect_number);
                return;
            }

            MerchandiseItem[] listToExport;

			if (cbCategories.SelectedItem== null || cbCategories.SelectedItem.ToString() == "All")
                listToExport = _merchandiseItems.ToArray();
            else
                listToExport = _merchandiseItems.GroupBy(item => item.Category)
                .First(item => item.Key == cbCategories.SelectedItem.ToString()).ToArray();

	        var maxCategories = _merchandiseItems.Max(item => item.CategoriesPath != null ? item.CategoriesPath.Length : 2);

            if (_merchandiseItems.Count > 20000)
            {
                listToExport = _merchandiseItems.Take(20000).ToArray();

                Save(listToExport, updater, 0, maxCategories);

                listToExport = _merchandiseItems.Skip(20000).ToArray();

                Save(listToExport, updater, 1, maxCategories);

                return;
            }
            

            Save(listToExport, updater, 0, maxCategories);
        }

        private void Save(IEnumerable<MerchandiseItem> listToExport, double updater, int index, int maxCategories)
        {
            var sb = new StringBuilder();

            foreach (var merchRow in listToExport.Select(listOfMerch => string.Join(",",
				string.Join(",", listOfMerch.CategoriesPath != null ?
				listOfMerch.CategoriesPath.Select(AddQuotes).Concat(Enumerable.Repeat(AddQuotes(string.Empty), maxCategories - listOfMerch.CategoriesPath.Length)) :
                new[] { AddQuotes(listOfMerch.Category), AddQuotes(listOfMerch.Subcategory) }),
                AddQuotes(listOfMerch.Sex),
                AddQuotes(listOfMerch.Code),
                AddQuotes(listOfMerch.Name),
                AddQuotes(listOfMerch.Image),
                string.Join(",", AddQuotes(listOfMerch.Avail.Color),
                    AddQuotes(listOfMerch.Avail.Quantity.ToString(CultureInfo.InvariantCulture)),
                    AddQuotes(listOfMerch.Avail.Size)),
                AddQuotes(listOfMerch.Price),
                ((chMult.Checked
                    ? double.Parse(listOfMerch.Price, CultureInfo.InvariantCulture)*updater
                    : double.Parse(listOfMerch.Price, CultureInfo.InvariantCulture) + updater)*Kurs).ToString("0.00",
                        CultureInfo.InvariantCulture),
                AddQuotes(listOfMerch.Consist),
				AddQuotes(listOfMerch.CategoriesPath != null ?  listOfMerch.CategoriesPath.Last() : listOfMerch.Subcategory),
                AddQuotes(listOfMerch.Country)
                )))
            {
                sb.AppendLine(merchRow);
            }

            using (TextWriter writer = File.CreateText(string.Format("./fn_{0}_{1}.csv", cbCategories.SelectedItem ?? "All", index)))
            {
                writer.Write(sb.ToString());
            }
        }

        private static string AddQuotes(string baseString)
        {
            return string.Format("\"{0}\"", baseString);
        }

    }
}
