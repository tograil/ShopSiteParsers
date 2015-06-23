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
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStartParse_Click(object sender, EventArgs e)
        {
            var parser = new VestoitalianoSite();

            parser.ItemAdded += parser_ItemAdded;
            parser.ParsingFinished += parser_ParsingFinished;
            btnStartParse.Enabled = false;

            Task.Factory.StartNew(() => parser.Run());
        }

        void parser_ParsingFinished()
        {
            lvTovars.Invoke(new Action(() =>
            {
                cbCategories.Items.Add("All");
                cbCategories.Items.AddRange(_merchandiseItems.GroupBy(item => item.Category).Select(catItem => (object)catItem.Key).ToArray());
                cbCategories.SelectedIndex = 0;
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

            if (cbCategories.SelectedItem.ToString() == "All")
                listToExport = _merchandiseItems.ToArray();
            else
                listToExport = _merchandiseItems.GroupBy(item => item.Category)
                .First(item => item.Key == cbCategories.SelectedItem.ToString()).ToArray();    
            

            

            var sb = new StringBuilder();

            foreach (var merchRow in listToExport.Select(listOfMerch => string.Join(",",
                AddQuotes(listOfMerch.Category),
                AddQuotes(listOfMerch.Subcategory),
                AddQuotes(listOfMerch.Sex),
                AddQuotes(listOfMerch.Code),
                AddQuotes(listOfMerch.Name),
                AddQuotes(listOfMerch.Image),
                string.Join(",", AddQuotes(listOfMerch.Avail.Color), AddQuotes(listOfMerch.Avail.Quantity.ToString(CultureInfo.InvariantCulture)), AddQuotes(listOfMerch.Avail.Size)),
                AddQuotes(listOfMerch.Price),
                (chMult.Checked ? double.Parse(listOfMerch.Price, CultureInfo.InvariantCulture) * updater : double.Parse(listOfMerch.Price, CultureInfo.InvariantCulture) + updater).ToString("0.00", CultureInfo.InvariantCulture),
                AddQuotes(listOfMerch.Consist)
                )))
            {
                sb.AppendLine(merchRow);
            }

            using (TextWriter writer = File.CreateText(string.Format("./fn_{0}.csv", cbCategories.SelectedItem))) 
            {
                writer.Write(sb.ToString());
            }

            var sb2 = new StringBuilder();

            foreach (var merchRow in listToExport.GroupBy(ls => ls.Avail.Color))
            {
                sb2.AppendLine(merchRow.Key);
            }

        }

        private string AddQuotes(string baseString)
        {
            return string.Format("\"{0}\"", baseString);
        }

    }
}
