namespace ShopSiteParsers
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStartParse = new System.Windows.Forms.Button();
            this.lvTovars = new System.Windows.Forms.ListView();
            this.clCategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clSubcategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clPrice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clConsist = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btExport = new System.Windows.Forms.Button();
            this.cbCategories = new System.Windows.Forms.ComboBox();
            this.tbPriceMultiplier = new System.Windows.Forms.TextBox();
            this.chMult = new System.Windows.Forms.CheckBox();
            this.cbSubcat = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnStartParse
            // 
            this.btnStartParse.Location = new System.Drawing.Point(527, 285);
            this.btnStartParse.Name = "btnStartParse";
            this.btnStartParse.Size = new System.Drawing.Size(75, 23);
            this.btnStartParse.TabIndex = 0;
            this.btnStartParse.Text = "Start parse";
            this.btnStartParse.UseVisualStyleBackColor = true;
            this.btnStartParse.Click += new System.EventHandler(this.btnStartParse_Click);
            // 
            // lvTovars
            // 
            this.lvTovars.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clCategory,
            this.clSubcategory,
            this.clCode,
            this.clName,
            this.clPrice,
            this.clConsist});
            this.lvTovars.FullRowSelect = true;
            this.lvTovars.GridLines = true;
            this.lvTovars.Location = new System.Drawing.Point(12, 12);
            this.lvTovars.MultiSelect = false;
            this.lvTovars.Name = "lvTovars";
            this.lvTovars.Size = new System.Drawing.Size(590, 267);
            this.lvTovars.TabIndex = 1;
            this.lvTovars.UseCompatibleStateImageBehavior = false;
            this.lvTovars.View = System.Windows.Forms.View.Details;
            this.lvTovars.VirtualMode = true;
            this.lvTovars.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvTovars_RetrieveVirtualItem);
            // 
            // clCategory
            // 
            this.clCategory.Text = "Category";
            // 
            // clSubcategory
            // 
            this.clSubcategory.Text = "Subcategory";
            this.clSubcategory.Width = 80;
            // 
            // clCode
            // 
            this.clCode.Text = "Code";
            this.clCode.Width = 70;
            // 
            // clName
            // 
            this.clName.Text = "Name";
            this.clName.Width = 150;
            // 
            // clPrice
            // 
            this.clPrice.Text = "Price";
            // 
            // clConsist
            // 
            this.clConsist.Text = "Consist";
            this.clConsist.Width = 120;
            // 
            // btExport
            // 
            this.btExport.Enabled = false;
            this.btExport.Location = new System.Drawing.Point(446, 285);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(75, 23);
            this.btExport.TabIndex = 2;
            this.btExport.Text = "Export";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // cbCategories
            // 
            this.cbCategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCategories.FormattingEnabled = true;
            this.cbCategories.Location = new System.Drawing.Point(12, 287);
            this.cbCategories.Name = "cbCategories";
            this.cbCategories.Size = new System.Drawing.Size(181, 21);
            this.cbCategories.TabIndex = 3;
            this.cbCategories.Visible = false;
            this.cbCategories.SelectedValueChanged += new System.EventHandler(this.cbCategories_SelectedValueChanged);
            // 
            // tbPriceMultiplier
            // 
            this.tbPriceMultiplier.Location = new System.Drawing.Point(199, 288);
            this.tbPriceMultiplier.Name = "tbPriceMultiplier";
            this.tbPriceMultiplier.Size = new System.Drawing.Size(100, 20);
            this.tbPriceMultiplier.TabIndex = 4;
            this.tbPriceMultiplier.Visible = false;
            // 
            // chMult
            // 
            this.chMult.AutoSize = true;
            this.chMult.Location = new System.Drawing.Point(306, 290);
            this.chMult.Name = "chMult";
            this.chMult.Size = new System.Drawing.Size(46, 17);
            this.chMult.TabIndex = 5;
            this.chMult.Text = "Mult";
            this.chMult.UseVisualStyleBackColor = true;
            this.chMult.Visible = false;
            // 
            // cbSubcat
            // 
            this.cbSubcat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSubcat.FormattingEnabled = true;
            this.cbSubcat.Location = new System.Drawing.Point(12, 314);
            this.cbSubcat.Name = "cbSubcat";
            this.cbSubcat.Size = new System.Drawing.Size(181, 21);
            this.cbSubcat.TabIndex = 6;
            this.cbSubcat.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 349);
            this.Controls.Add(this.cbSubcat);
            this.Controls.Add(this.chMult);
            this.Controls.Add(this.tbPriceMultiplier);
            this.Controls.Add(this.cbCategories);
            this.Controls.Add(this.btExport);
            this.Controls.Add(this.lvTovars);
            this.Controls.Add(this.btnStartParse);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Main Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartParse;
        private System.Windows.Forms.ListView lvTovars;
        private System.Windows.Forms.ColumnHeader clCategory;
        private System.Windows.Forms.ColumnHeader clCode;
        private System.Windows.Forms.ColumnHeader clName;
        private System.Windows.Forms.ColumnHeader clPrice;
        private System.Windows.Forms.ColumnHeader clConsist;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.ComboBox cbCategories;
        private System.Windows.Forms.TextBox tbPriceMultiplier;
        private System.Windows.Forms.CheckBox chMult;
        private System.Windows.Forms.ColumnHeader clSubcategory;
        private System.Windows.Forms.ComboBox cbSubcat;
    }
}

