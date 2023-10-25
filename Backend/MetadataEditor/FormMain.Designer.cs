namespace MetadataEditor
{
    partial class FormMain
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtArtists = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.chkLanEnglish = new System.Windows.Forms.CheckBox();
            this.chkLanJapanese = new System.Windows.Forms.CheckBox();
            this.chkLanChinese = new System.Windows.Forms.CheckBox();
            this.chkLanOther = new System.Windows.Forms.CheckBox();
            this.chkIsReadTrue = new System.Windows.Forms.CheckBox();
            this.pctCover = new System.Windows.Forms.PictureBox();
            this.btnPrev = new System.Windows.Forms.Button();
            this.groupAlbum = new System.Windows.Forms.GroupBox();
            this.btnPopupCharacters = new System.Windows.Forms.Button();
            this.txtCharacters = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btnHp = new System.Windows.Forms.Button();
            this.btnSetName = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.RichTextBox();
            this.txtTitle = new System.Windows.Forms.RichTextBox();
            this.btnPostMetadata = new System.Windows.Forms.Button();
            this.btnTierPlus = new System.Windows.Forms.Button();
            this.btnTierMin = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.txtFileUpload = new System.Windows.Forms.TextBox();
            this.btnPost = new System.Windows.Forms.Button();
            this.btnPopupTags = new System.Windows.Forms.Button();
            this.btnExplore = new System.Windows.Forms.Button();
            this.rbOri = new System.Windows.Forms.Panel();
            this.rbOriPortrait = new System.Windows.Forms.RadioButton();
            this.rbOriLandscape = new System.Windows.Forms.RadioButton();
            this.rbCat = new System.Windows.Forms.Panel();
            this.rbCatManga = new System.Windows.Forms.RadioButton();
            this.rbCatCGSet = new System.Windows.Forms.RadioButton();
            this.rbCatSelfComp = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.txtTier = new System.Windows.Forms.TextBox();
            this.txtTags = new System.Windows.Forms.RichTextBox();
            this.chkIsWipTrue = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupPicture = new System.Windows.Forms.GroupBox();
            this.lbPage = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtNote = new System.Windows.Forms.TextBox();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.rbOriAuto = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pctCover)).BeginInit();
            this.groupAlbum.SuspendLayout();
            this.rbOri.SuspendLayout();
            this.rbCat.SuspendLayout();
            this.groupPicture.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Title";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 110);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "Artists";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 140);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "Tags";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 200);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 18);
            this.label4.TabIndex = 3;
            this.label4.Text = "Languages";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 230);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 18);
            this.label6.TabIndex = 5;
            this.label6.Text = "Category";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 350);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 18);
            this.label7.TabIndex = 6;
            this.label7.Text = "Tier";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 290);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 18);
            this.label8.TabIndex = 7;
            this.label8.Text = "Note";
            // 
            // txtArtists
            // 
            this.txtArtists.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtArtists.Location = new System.Drawing.Point(100, 110);
            this.txtArtists.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtArtists.Name = "txtArtists";
            this.txtArtists.Size = new System.Drawing.Size(252, 26);
            this.txtArtists.TabIndex = 10;
            // 
            // btnCreate
            // 
            this.btnCreate.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnCreate.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCreate.Location = new System.Drawing.Point(61, 465);
            this.btnCreate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(71, 52);
            this.btnCreate.TabIndex = 17;
            this.btnCreate.Text = "SAVE";
            this.btnCreate.UseVisualStyleBackColor = false;
            this.btnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
            // 
            // chkLanEnglish
            // 
            this.chkLanEnglish.AutoSize = true;
            this.chkLanEnglish.Location = new System.Drawing.Point(100, 200);
            this.chkLanEnglish.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkLanEnglish.Name = "chkLanEnglish";
            this.chkLanEnglish.Size = new System.Drawing.Size(44, 22);
            this.chkLanEnglish.TabIndex = 18;
            this.chkLanEnglish.Text = "EN";
            this.chkLanEnglish.UseVisualStyleBackColor = true;
            // 
            // chkLanJapanese
            // 
            this.chkLanJapanese.AutoSize = true;
            this.chkLanJapanese.Location = new System.Drawing.Point(160, 200);
            this.chkLanJapanese.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkLanJapanese.Name = "chkLanJapanese";
            this.chkLanJapanese.Size = new System.Drawing.Size(40, 22);
            this.chkLanJapanese.TabIndex = 19;
            this.chkLanJapanese.Text = "JP";
            this.chkLanJapanese.UseVisualStyleBackColor = true;
            // 
            // chkLanChinese
            // 
            this.chkLanChinese.AutoSize = true;
            this.chkLanChinese.Location = new System.Drawing.Point(220, 200);
            this.chkLanChinese.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkLanChinese.Name = "chkLanChinese";
            this.chkLanChinese.Size = new System.Drawing.Size(44, 22);
            this.chkLanChinese.TabIndex = 21;
            this.chkLanChinese.Text = "CH";
            this.chkLanChinese.UseVisualStyleBackColor = true;
            // 
            // chkLanOther
            // 
            this.chkLanOther.AutoSize = true;
            this.chkLanOther.Location = new System.Drawing.Point(280, 200);
            this.chkLanOther.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkLanOther.Name = "chkLanOther";
            this.chkLanOther.Size = new System.Drawing.Size(63, 22);
            this.chkLanOther.TabIndex = 22;
            this.chkLanOther.Text = "Other";
            this.chkLanOther.UseVisualStyleBackColor = true;
            // 
            // chkIsReadTrue
            // 
            this.chkIsReadTrue.AutoSize = true;
            this.chkIsReadTrue.Location = new System.Drawing.Point(162, 380);
            this.chkIsReadTrue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkIsReadTrue.Name = "chkIsReadTrue";
            this.chkIsReadTrue.Size = new System.Drawing.Size(58, 22);
            this.chkIsReadTrue.TabIndex = 26;
            this.chkIsReadTrue.Text = "Read";
            this.chkIsReadTrue.UseVisualStyleBackColor = true;
            // 
            // pctCover
            // 
            this.pctCover.ImageLocation = "";
            this.pctCover.Location = new System.Drawing.Point(0, 0);
            this.pctCover.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pctCover.Name = "pctCover";
            this.pctCover.Size = new System.Drawing.Size(620, 900);
            this.pctCover.TabIndex = 53;
            this.pctCover.TabStop = false;
            this.pctCover.MouseHover += new System.EventHandler(this.PctCover_Hover);
            // 
            // btnPrev
            // 
            this.btnPrev.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnPrev.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPrev.Location = new System.Drawing.Point(61, 410);
            this.btnPrev.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(71, 52);
            this.btnPrev.TabIndex = 54;
            this.btnPrev.Text = "PREV";
            this.btnPrev.UseVisualStyleBackColor = false;
            this.btnPrev.Click += new System.EventHandler(this.BtnPrev_Click);
            // 
            // groupAlbum
            // 
            this.groupAlbum.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupAlbum.Controls.Add(this.btnPopupCharacters);
            this.groupAlbum.Controls.Add(this.txtCharacters);
            this.groupAlbum.Controls.Add(this.label10);
            this.groupAlbum.Controls.Add(this.btnHp);
            this.groupAlbum.Controls.Add(this.btnSetName);
            this.groupAlbum.Controls.Add(this.txtFolder);
            this.groupAlbum.Controls.Add(this.txtTitle);
            this.groupAlbum.Controls.Add(this.btnPostMetadata);
            this.groupAlbum.Controls.Add(this.btnTierPlus);
            this.groupAlbum.Controls.Add(this.btnTierMin);
            this.groupAlbum.Controls.Add(this.btnDelete);
            this.groupAlbum.Controls.Add(this.txtFileUpload);
            this.groupAlbum.Controls.Add(this.btnPost);
            this.groupAlbum.Controls.Add(this.btnPopupTags);
            this.groupAlbum.Controls.Add(this.btnExplore);
            this.groupAlbum.Controls.Add(this.rbOri);
            this.groupAlbum.Controls.Add(this.rbCat);
            this.groupAlbum.Controls.Add(this.label9);
            this.groupAlbum.Controls.Add(this.txtTier);
            this.groupAlbum.Controls.Add(this.txtTags);
            this.groupAlbum.Controls.Add(this.chkIsWipTrue);
            this.groupAlbum.Controls.Add(this.label5);
            this.groupAlbum.Controls.Add(this.groupPicture);
            this.groupAlbum.Controls.Add(this.btnNext);
            this.groupAlbum.Controls.Add(this.btnBrowse);
            this.groupAlbum.Controls.Add(this.btnPrev);
            this.groupAlbum.Controls.Add(this.chkIsReadTrue);
            this.groupAlbum.Controls.Add(this.chkLanOther);
            this.groupAlbum.Controls.Add(this.chkLanChinese);
            this.groupAlbum.Controls.Add(this.chkLanJapanese);
            this.groupAlbum.Controls.Add(this.chkLanEnglish);
            this.groupAlbum.Controls.Add(this.btnCreate);
            this.groupAlbum.Controls.Add(this.txtNote);
            this.groupAlbum.Controls.Add(this.txtArtists);
            this.groupAlbum.Controls.Add(this.label8);
            this.groupAlbum.Controls.Add(this.label7);
            this.groupAlbum.Controls.Add(this.label6);
            this.groupAlbum.Controls.Add(this.label4);
            this.groupAlbum.Controls.Add(this.label3);
            this.groupAlbum.Controls.Add(this.label2);
            this.groupAlbum.Controls.Add(this.label1);
            this.groupAlbum.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupAlbum.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.groupAlbum.Location = new System.Drawing.Point(3, -5);
            this.groupAlbum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupAlbum.Name = "groupAlbum";
            this.groupAlbum.Padding = new System.Windows.Forms.Padding(0);
            this.groupAlbum.Size = new System.Drawing.Size(988, 930);
            this.groupAlbum.TabIndex = 0;
            this.groupAlbum.TabStop = false;
            this.groupAlbum.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
            this.groupAlbum.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormMain_DragEnter);
            // 
            // btnPopupCharacters
            // 
            this.btnPopupCharacters.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnPopupCharacters.Location = new System.Drawing.Point(300, 320);
            this.btnPopupCharacters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPopupCharacters.Name = "btnPopupCharacters";
            this.btnPopupCharacters.Size = new System.Drawing.Size(52, 28);
            this.btnPopupCharacters.TabIndex = 91;
            this.btnPopupCharacters.Text = "Chars";
            this.btnPopupCharacters.UseVisualStyleBackColor = false;
            this.btnPopupCharacters.Click += new System.EventHandler(this.btnPopupCharacters_Click);
            // 
            // txtCharacters
            // 
            this.txtCharacters.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtCharacters.Location = new System.Drawing.Point(100, 320);
            this.txtCharacters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtCharacters.Name = "txtCharacters";
            this.txtCharacters.Size = new System.Drawing.Size(192, 26);
            this.txtCharacters.TabIndex = 90;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 320);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(73, 18);
            this.label10.TabIndex = 89;
            this.label10.Text = "Characters";
            // 
            // btnHp
            // 
            this.btnHp.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnHp.Location = new System.Drawing.Point(300, 290);
            this.btnHp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnHp.Name = "btnHp";
            this.btnHp.Size = new System.Drawing.Size(52, 28);
            this.btnHp.TabIndex = 88;
            this.btnHp.Text = "HP";
            this.btnHp.UseVisualStyleBackColor = false;
            this.btnHp.Click += new System.EventHandler(this.btnHp_Click);
            // 
            // btnSetName
            // 
            this.btnSetName.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnSetName.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSetName.Location = new System.Drawing.Point(300, 861);
            this.btnSetName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSetName.Name = "btnSetName";
            this.btnSetName.Size = new System.Drawing.Size(52, 59);
            this.btnSetName.TabIndex = 87;
            this.btnSetName.Text = "SET NAME";
            this.btnSetName.UseVisualStyleBackColor = false;
            this.btnSetName.Click += new System.EventHandler(this.btnSetName_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtFolder.Location = new System.Drawing.Point(6, 861);
            this.txtFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(286, 59);
            this.txtFolder.TabIndex = 86;
            this.txtFolder.Text = "";
            // 
            // txtTitle
            // 
            this.txtTitle.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtTitle.Location = new System.Drawing.Point(100, 20);
            this.txtTitle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(252, 84);
            this.txtTitle.TabIndex = 85;
            this.txtTitle.Text = "";
            // 
            // btnPostMetadata
            // 
            this.btnPostMetadata.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnPostMetadata.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPostMetadata.Location = new System.Drawing.Point(138, 485);
            this.btnPostMetadata.Name = "btnPostMetadata";
            this.btnPostMetadata.Size = new System.Drawing.Size(137, 32);
            this.btnPostMetadata.TabIndex = 84;
            this.btnPostMetadata.Text = "POST METADATA";
            this.btnPostMetadata.UseVisualStyleBackColor = false;
            this.btnPostMetadata.Click += new System.EventHandler(this.btnPostMetadata_Click);
            // 
            // btnTierPlus
            // 
            this.btnTierPlus.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnTierPlus.Location = new System.Drawing.Point(197, 350);
            this.btnTierPlus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnTierPlus.Name = "btnTierPlus";
            this.btnTierPlus.Size = new System.Drawing.Size(30, 26);
            this.btnTierPlus.TabIndex = 82;
            this.btnTierPlus.Text = "+";
            this.btnTierPlus.UseVisualStyleBackColor = false;
            this.btnTierPlus.Click += new System.EventHandler(this.btnTierPlus_Click);
            // 
            // btnTierMin
            // 
            this.btnTierMin.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnTierMin.Location = new System.Drawing.Point(162, 350);
            this.btnTierMin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnTierMin.Name = "btnTierMin";
            this.btnTierMin.Size = new System.Drawing.Size(30, 26);
            this.btnTierMin.TabIndex = 81;
            this.btnTierMin.Text = "-";
            this.btnTierMin.UseVisualStyleBackColor = false;
            this.btnTierMin.Click += new System.EventHandler(this.btnTierMin_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDelete.Location = new System.Drawing.Point(6, 410);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(50, 107);
            this.btnDelete.TabIndex = 80;
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // txtFileUpload
            // 
            this.txtFileUpload.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtFileUpload.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtFileUpload.Location = new System.Drawing.Point(6, 523);
            this.txtFileUpload.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtFileUpload.Multiline = true;
            this.txtFileUpload.Name = "txtFileUpload";
            this.txtFileUpload.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtFileUpload.Size = new System.Drawing.Size(346, 332);
            this.txtFileUpload.TabIndex = 79;
            // 
            // btnPost
            // 
            this.btnPost.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnPost.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPost.Location = new System.Drawing.Point(281, 465);
            this.btnPost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPost.Name = "btnPost";
            this.btnPost.Size = new System.Drawing.Size(71, 52);
            this.btnPost.TabIndex = 78;
            this.btnPost.Text = "POST";
            this.btnPost.UseVisualStyleBackColor = false;
            this.btnPost.Click += new System.EventHandler(this.btnPost_Click);
            // 
            // btnPopupTags
            // 
            this.btnPopupTags.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnPopupTags.Location = new System.Drawing.Point(300, 140);
            this.btnPopupTags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPopupTags.Name = "btnPopupTags";
            this.btnPopupTags.Size = new System.Drawing.Size(52, 56);
            this.btnPopupTags.TabIndex = 77;
            this.btnPopupTags.Text = "Tags";
            this.btnPopupTags.UseVisualStyleBackColor = false;
            this.btnPopupTags.Click += new System.EventHandler(this.btnPopupTags_Click);
            // 
            // btnExplore
            // 
            this.btnExplore.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnExplore.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExplore.Location = new System.Drawing.Point(138, 410);
            this.btnExplore.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExplore.Name = "btnExplore";
            this.btnExplore.Size = new System.Drawing.Size(137, 32);
            this.btnExplore.TabIndex = 76;
            this.btnExplore.Text = "OPEN FOLDER";
            this.btnExplore.UseVisualStyleBackColor = false;
            this.btnExplore.Click += new System.EventHandler(this.btnExplore_Click);
            // 
            // rbOri
            // 
            this.rbOri.Controls.Add(this.rbOriAuto);
            this.rbOri.Controls.Add(this.rbOriPortrait);
            this.rbOri.Controls.Add(this.rbOriLandscape);
            this.rbOri.Location = new System.Drawing.Point(100, 260);
            this.rbOri.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbOri.Name = "rbOri";
            this.rbOri.Size = new System.Drawing.Size(252, 25);
            this.rbOri.TabIndex = 75;
            // 
            // rbOriPortrait
            // 
            this.rbOriPortrait.AutoSize = true;
            this.rbOriPortrait.Location = new System.Drawing.Point(0, 0);
            this.rbOriPortrait.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbOriPortrait.Name = "rbOriPortrait";
            this.rbOriPortrait.Size = new System.Drawing.Size(44, 22);
            this.rbOriPortrait.TabIndex = 69;
            this.rbOriPortrait.TabStop = true;
            this.rbOriPortrait.Text = "Ptr";
            this.rbOriPortrait.UseVisualStyleBackColor = true;
            // 
            // rbOriLandscape
            // 
            this.rbOriLandscape.AutoSize = true;
            this.rbOriLandscape.Location = new System.Drawing.Point(60, 0);
            this.rbOriLandscape.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbOriLandscape.Name = "rbOriLandscape";
            this.rbOriLandscape.Size = new System.Drawing.Size(44, 22);
            this.rbOriLandscape.TabIndex = 70;
            this.rbOriLandscape.TabStop = true;
            this.rbOriLandscape.Text = "Lsc";
            this.rbOriLandscape.UseVisualStyleBackColor = true;
            // 
            // rbCat
            // 
            this.rbCat.Controls.Add(this.rbCatManga);
            this.rbCat.Controls.Add(this.rbCatCGSet);
            this.rbCat.Controls.Add(this.rbCatSelfComp);
            this.rbCat.Location = new System.Drawing.Point(100, 230);
            this.rbCat.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbCat.Name = "rbCat";
            this.rbCat.Size = new System.Drawing.Size(252, 25);
            this.rbCat.TabIndex = 74;
            // 
            // rbCatManga
            // 
            this.rbCatManga.AutoSize = true;
            this.rbCatManga.Location = new System.Drawing.Point(0, 0);
            this.rbCatManga.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbCatManga.Name = "rbCatManga";
            this.rbCatManga.Size = new System.Drawing.Size(47, 22);
            this.rbCatManga.TabIndex = 69;
            this.rbCatManga.TabStop = true;
            this.rbCatManga.Text = "MG";
            this.rbCatManga.UseVisualStyleBackColor = true;
            // 
            // rbCatCGSet
            // 
            this.rbCatCGSet.AutoSize = true;
            this.rbCatCGSet.Location = new System.Drawing.Point(60, 0);
            this.rbCatCGSet.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbCatCGSet.Name = "rbCatCGSet";
            this.rbCatCGSet.Size = new System.Drawing.Size(43, 22);
            this.rbCatCGSet.TabIndex = 70;
            this.rbCatCGSet.TabStop = true;
            this.rbCatCGSet.Text = "CG";
            this.rbCatCGSet.UseVisualStyleBackColor = true;
            // 
            // rbCatSelfComp
            // 
            this.rbCatSelfComp.AutoSize = true;
            this.rbCatSelfComp.Location = new System.Drawing.Point(120, 0);
            this.rbCatSelfComp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbCatSelfComp.Name = "rbCatSelfComp";
            this.rbCatSelfComp.Size = new System.Drawing.Size(41, 22);
            this.rbCatSelfComp.TabIndex = 71;
            this.rbCatSelfComp.TabStop = true;
            this.rbCatSelfComp.Text = "SC";
            this.rbCatSelfComp.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 260);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 18);
            this.label9.TabIndex = 68;
            this.label9.Text = "Orientation";
            // 
            // txtTier
            // 
            this.txtTier.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtTier.Location = new System.Drawing.Point(100, 350);
            this.txtTier.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtTier.Name = "txtTier";
            this.txtTier.Size = new System.Drawing.Size(54, 26);
            this.txtTier.TabIndex = 67;
            this.txtTier.Text = "0";
            // 
            // txtTags
            // 
            this.txtTags.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtTags.Location = new System.Drawing.Point(100, 140);
            this.txtTags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtTags.Name = "txtTags";
            this.txtTags.Size = new System.Drawing.Size(192, 56);
            this.txtTags.TabIndex = 66;
            this.txtTags.Text = "";
            // 
            // chkIsWipTrue
            // 
            this.chkIsWipTrue.AutoSize = true;
            this.chkIsWipTrue.Location = new System.Drawing.Point(100, 380);
            this.chkIsWipTrue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkIsWipTrue.Name = "chkIsWipTrue";
            this.chkIsWipTrue.Size = new System.Drawing.Size(52, 22);
            this.chkIsWipTrue.TabIndex = 61;
            this.chkIsWipTrue.Text = "WIP";
            this.chkIsWipTrue.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 380);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 18);
            this.label5.TabIndex = 60;
            this.label5.Text = "Flags";
            // 
            // groupPicture
            // 
            this.groupPicture.Controls.Add(this.lbPage);
            this.groupPicture.Controls.Add(this.pctCover);
            this.groupPicture.Location = new System.Drawing.Point(360, 20);
            this.groupPicture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupPicture.Name = "groupPicture";
            this.groupPicture.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupPicture.Size = new System.Drawing.Size(620, 900);
            this.groupPicture.TabIndex = 59;
            this.groupPicture.TabStop = false;
            // 
            // lbPage
            // 
            this.lbPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPage.BackColor = System.Drawing.Color.Transparent;
            this.lbPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbPage.ForeColor = System.Drawing.SystemColors.MenuText;
            this.lbPage.Location = new System.Drawing.Point(275, 880);
            this.lbPage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbPage.Name = "lbPage";
            this.lbPage.Size = new System.Drawing.Size(70, 21);
            this.lbPage.TabIndex = 56;
            this.lbPage.Text = "0/0";
            this.lbPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnNext.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnNext.Location = new System.Drawing.Point(281, 410);
            this.btnNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(71, 52);
            this.btnNext.TabIndex = 58;
            this.btnNext.Text = "NEXT";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnBrowse.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnBrowse.Location = new System.Drawing.Point(138, 447);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(137, 32);
            this.btnBrowse.TabIndex = 57;
            this.btnBrowse.Text = "BROWSE";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // txtNote
            // 
            this.txtNote.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtNote.Location = new System.Drawing.Point(100, 290);
            this.txtNote.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtNote.Name = "txtNote";
            this.txtNote.Size = new System.Drawing.Size(192, 26);
            this.txtNote.TabIndex = 16;
            // 
            // rbOriAuto
            // 
            this.rbOriAuto.AutoSize = true;
            this.rbOriAuto.Location = new System.Drawing.Point(120, 0);
            this.rbOriAuto.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbOriAuto.Name = "rbOriAuto";
            this.rbOriAuto.Size = new System.Drawing.Size(48, 22);
            this.rbOriAuto.TabIndex = 71;
            this.rbOriAuto.TabStop = true;
            this.rbOriAuto.Text = "Aut";
            this.rbOriAuto.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(994, 928);
            this.Controls.Add(this.groupAlbum);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormMain";
            this.Text = "Metadata Editor";
            this.Load += new System.EventHandler(this.FormMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pctCover)).EndInit();
            this.groupAlbum.ResumeLayout(false);
            this.groupAlbum.PerformLayout();
            this.rbOri.ResumeLayout(false);
            this.rbOri.PerformLayout();
            this.rbCat.ResumeLayout(false);
            this.rbCat.PerformLayout();
            this.groupPicture.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtArtists;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.CheckBox chkLanEnglish;
        private System.Windows.Forms.CheckBox chkLanJapanese;
        private System.Windows.Forms.CheckBox chkLanChinese;
        private System.Windows.Forms.CheckBox chkLanOther;
        private System.Windows.Forms.CheckBox chkIsReadTrue;
        private System.Windows.Forms.PictureBox pctCover;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.GroupBox groupAlbum;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.GroupBox groupPicture;
        private System.Windows.Forms.Label lbPage;
        private System.Windows.Forms.CheckBox chkIsWipTrue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RichTextBox txtTags;
        private System.Windows.Forms.TextBox txtTier;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtNote;
        private System.Windows.Forms.RadioButton rbCatSelfComp;
        private System.Windows.Forms.RadioButton rbCatCGSet;
        private System.Windows.Forms.RadioButton rbCatManga;
        private System.Windows.Forms.Panel rbCat;
        private System.Windows.Forms.Panel rbOri;
        private System.Windows.Forms.RadioButton rbOriPortrait;
        private System.Windows.Forms.RadioButton rbOriLandscape;
        private System.Windows.Forms.Button btnExplore;
        private System.Windows.Forms.Button btnPopupTags;
        private System.Windows.Forms.Button btnPost;
        private System.Windows.Forms.TextBox txtFileUpload;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnTierPlus;
        private System.Windows.Forms.Button btnTierMin;
        private System.Windows.Forms.Button btnPostMetadata;
        private System.Windows.Forms.RichTextBox txtTitle;
        private System.Windows.Forms.Button btnSetName;
        private System.Windows.Forms.RichTextBox txtFolder;
        private System.Windows.Forms.Button btnHp;
        private System.Windows.Forms.Button btnPopupCharacters;
        private System.Windows.Forms.TextBox txtCharacters;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.RadioButton rbOriAuto;
    }
}