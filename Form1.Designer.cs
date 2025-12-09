namespace CNetTest
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.roiCanvas = new CNetTest.Controls.RoiCanvas();
            this.loadImageButton = new System.Windows.Forms.Button();
            this.loadedImageLabel = new System.Windows.Forms.Label();
            this.datasetPathText = new System.Windows.Forms.TextBox();
            this.saveOkButton = new System.Windows.Forms.Button();
            this.saveNgButton = new System.Windows.Forms.Button();
            this.roiList = new System.Windows.Forms.ListBox();
            this.roiNameText = new System.Windows.Forms.TextBox();
            this.roiXNumeric = new System.Windows.Forms.NumericUpDown();
            this.roiYNumeric = new System.Windows.Forms.NumericUpDown();
            this.roiWNumeric = new System.Windows.Forms.NumericUpDown();
            this.roiHNumeric = new System.Windows.Forms.NumericUpDown();
            this.applyRoiButton = new System.Windows.Forms.Button();
            this.deleteRoiButton = new System.Windows.Forms.Button();
            this.loadModelButton = new System.Windows.Forms.Button();
            this.modelPathLabel = new System.Windows.Forms.Label();
            this.inputWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.inputHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.inferButton = new System.Windows.Forms.Button();
            this.resultsGrid = new System.Windows.Forms.DataGridView();
            this.colRoi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.roiCanvas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiXNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiYNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiWNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiHNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // roiCanvas
            // 
            this.roiCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.roiCanvas.BackColor = System.Drawing.Color.Black;
            this.roiCanvas.Location = new System.Drawing.Point(12, 12);
            this.roiCanvas.Name = "roiCanvas";
            this.roiCanvas.Size = new System.Drawing.Size(512, 512);
            this.roiCanvas.TabIndex = 0;
            this.roiCanvas.TabStop = false;
            // 
            // loadImageButton
            // 
            this.loadImageButton.Location = new System.Drawing.Point(530, 12);
            this.loadImageButton.Name = "loadImageButton";
            this.loadImageButton.Size = new System.Drawing.Size(140, 30);
            this.loadImageButton.TabIndex = 1;
            this.loadImageButton.Text = "이미지 로드";
            this.loadImageButton.UseVisualStyleBackColor = true;
            this.loadImageButton.Click += new System.EventHandler(this.loadImageButton_Click);
            // 
            // loadedImageLabel
            // 
            this.loadedImageLabel.AutoSize = true;
            this.loadedImageLabel.Location = new System.Drawing.Point(676, 20);
            this.loadedImageLabel.Name = "loadedImageLabel";
            this.loadedImageLabel.Size = new System.Drawing.Size(83, 12);
            this.loadedImageLabel.TabIndex = 2;
            this.loadedImageLabel.Text = "(미선택 상태)";
            // 
            // datasetPathText
            // 
            this.datasetPathText.Location = new System.Drawing.Point(530, 58);
            this.datasetPathText.Name = "datasetPathText";
            this.datasetPathText.Size = new System.Drawing.Size(310, 21);
            this.datasetPathText.TabIndex = 3;
            this.datasetPathText.Text = "dataset";
            // 
            // saveOkButton
            // 
            this.saveOkButton.Location = new System.Drawing.Point(846, 54);
            this.saveOkButton.Name = "saveOkButton";
            this.saveOkButton.Size = new System.Drawing.Size(75, 27);
            this.saveOkButton.TabIndex = 4;
            this.saveOkButton.Text = "OK 저장";
            this.saveOkButton.UseVisualStyleBackColor = true;
            this.saveOkButton.Click += new System.EventHandler(this.saveOkButton_Click);
            // 
            // saveNgButton
            // 
            this.saveNgButton.Location = new System.Drawing.Point(927, 54);
            this.saveNgButton.Name = "saveNgButton";
            this.saveNgButton.Size = new System.Drawing.Size(75, 27);
            this.saveNgButton.TabIndex = 5;
            this.saveNgButton.Text = "NG 저장";
            this.saveNgButton.UseVisualStyleBackColor = true;
            this.saveNgButton.Click += new System.EventHandler(this.saveNgButton_Click);
            // 
            // roiList
            // 
            this.roiList.FormattingEnabled = true;
            this.roiList.ItemHeight = 12;
            this.roiList.Location = new System.Drawing.Point(530, 98);
            this.roiList.Name = "roiList";
            this.roiList.Size = new System.Drawing.Size(200, 148);
            this.roiList.TabIndex = 6;
            this.roiList.SelectedIndexChanged += new System.EventHandler(this.roiList_SelectedIndexChanged);
            // 
            // roiNameText
            // 
            this.roiNameText.Location = new System.Drawing.Point(736, 98);
            this.roiNameText.Name = "roiNameText";
            this.roiNameText.Size = new System.Drawing.Size(170, 21);
            this.roiNameText.TabIndex = 7;
            // 
            // roiXNumeric
            // 
            this.roiXNumeric.Location = new System.Drawing.Point(736, 125);
            this.roiXNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.roiXNumeric.Name = "roiXNumeric";
            this.roiXNumeric.Size = new System.Drawing.Size(70, 21);
            this.roiXNumeric.TabIndex = 8;
            // 
            // roiYNumeric
            // 
            this.roiYNumeric.Location = new System.Drawing.Point(836, 125);
            this.roiYNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.roiYNumeric.Name = "roiYNumeric";
            this.roiYNumeric.Size = new System.Drawing.Size(70, 21);
            this.roiYNumeric.TabIndex = 9;
            // 
            // roiWNumeric
            // 
            this.roiWNumeric.Location = new System.Drawing.Point(736, 152);
            this.roiWNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.roiWNumeric.Name = "roiWNumeric";
            this.roiWNumeric.Size = new System.Drawing.Size(70, 21);
            this.roiWNumeric.TabIndex = 10;
            this.roiWNumeric.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // roiHNumeric
            // 
            this.roiHNumeric.Location = new System.Drawing.Point(836, 152);
            this.roiHNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.roiHNumeric.Name = "roiHNumeric";
            this.roiHNumeric.Size = new System.Drawing.Size(70, 21);
            this.roiHNumeric.TabIndex = 11;
            this.roiHNumeric.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // applyRoiButton
            // 
            this.applyRoiButton.Location = new System.Drawing.Point(736, 179);
            this.applyRoiButton.Name = "applyRoiButton";
            this.applyRoiButton.Size = new System.Drawing.Size(75, 23);
            this.applyRoiButton.TabIndex = 12;
            this.applyRoiButton.Text = "ROI 수정";
            this.applyRoiButton.UseVisualStyleBackColor = true;
            this.applyRoiButton.Click += new System.EventHandler(this.applyRoiButton_Click);
            // 
            // deleteRoiButton
            // 
            this.deleteRoiButton.Location = new System.Drawing.Point(831, 179);
            this.deleteRoiButton.Name = "deleteRoiButton";
            this.deleteRoiButton.Size = new System.Drawing.Size(75, 23);
            this.deleteRoiButton.TabIndex = 13;
            this.deleteRoiButton.Text = "ROI 삭제";
            this.deleteRoiButton.UseVisualStyleBackColor = true;
            this.deleteRoiButton.Click += new System.EventHandler(this.deleteRoiButton_Click);
            // 
            // loadModelButton
            // 
            this.loadModelButton.Location = new System.Drawing.Point(530, 260);
            this.loadModelButton.Name = "loadModelButton";
            this.loadModelButton.Size = new System.Drawing.Size(140, 27);
            this.loadModelButton.TabIndex = 14;
            this.loadModelButton.Text = "ONNX 모델 로드";
            this.loadModelButton.UseVisualStyleBackColor = true;
            this.loadModelButton.Click += new System.EventHandler(this.loadModelButton_Click);
            // 
            // modelPathLabel
            // 
            this.modelPathLabel.AutoSize = true;
            this.modelPathLabel.Location = new System.Drawing.Point(676, 267);
            this.modelPathLabel.Name = "modelPathLabel";
            this.modelPathLabel.Size = new System.Drawing.Size(83, 12);
            this.modelPathLabel.TabIndex = 15;
            this.modelPathLabel.Text = "(모델 미로딩)";
            // 
            // inputWidthNumeric
            // 
            this.inputWidthNumeric.Location = new System.Drawing.Point(530, 296);
            this.inputWidthNumeric.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.inputWidthNumeric.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.inputWidthNumeric.Name = "inputWidthNumeric";
            this.inputWidthNumeric.Size = new System.Drawing.Size(70, 21);
            this.inputWidthNumeric.TabIndex = 16;
            this.inputWidthNumeric.Value = new decimal(new int[] {
            224,
            0,
            0,
            0});
            // 
            // inputHeightNumeric
            // 
            this.inputHeightNumeric.Location = new System.Drawing.Point(606, 296);
            this.inputHeightNumeric.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.inputHeightNumeric.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.inputHeightNumeric.Name = "inputHeightNumeric";
            this.inputHeightNumeric.Size = new System.Drawing.Size(70, 21);
            this.inputHeightNumeric.TabIndex = 17;
            this.inputHeightNumeric.Value = new decimal(new int[] {
            224,
            0,
            0,
            0});
            // 
            // inferButton
            // 
            this.inferButton.Location = new System.Drawing.Point(682, 294);
            this.inferButton.Name = "inferButton";
            this.inferButton.Size = new System.Drawing.Size(120, 25);
            this.inferButton.TabIndex = 18;
            this.inferButton.Text = "추론 실행";
            this.inferButton.UseVisualStyleBackColor = true;
            this.inferButton.Click += new System.EventHandler(this.inferButton_Click);
            // 
            // resultsGrid
            // 
            this.resultsGrid.AllowUserToAddRows = false;
            this.resultsGrid.AllowUserToDeleteRows = false;
            this.resultsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRoi,
            this.colLabel,
            this.colOk,
            this.colNg});
            this.resultsGrid.Location = new System.Drawing.Point(530, 333);
            this.resultsGrid.Name = "resultsGrid";
            this.resultsGrid.ReadOnly = true;
            this.resultsGrid.RowHeadersVisible = false;
            this.resultsGrid.RowTemplate.Height = 23;
            this.resultsGrid.Size = new System.Drawing.Size(472, 191);
            this.resultsGrid.TabIndex = 19;
            // 
            // colRoi
            // 
            this.colRoi.HeaderText = "ROI";
            this.colRoi.Name = "colRoi";
            this.colRoi.ReadOnly = true;
            this.colRoi.Width = 120;
            // 
            // colLabel
            // 
            this.colLabel.HeaderText = "판정";
            this.colLabel.Name = "colLabel";
            this.colLabel.ReadOnly = true;
            this.colLabel.Width = 80;
            // 
            // colOk
            // 
            this.colOk.HeaderText = "OK Score";
            this.colOk.Name = "colOk";
            this.colOk.ReadOnly = true;
            // 
            // colNg
            // 
            this.colNg.HeaderText = "NG Score";
            this.colNg.Name = "colNg";
            this.colNg.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1014, 536);
            this.Controls.Add(this.resultsGrid);
            this.Controls.Add(this.inferButton);
            this.Controls.Add(this.inputHeightNumeric);
            this.Controls.Add(this.inputWidthNumeric);
            this.Controls.Add(this.modelPathLabel);
            this.Controls.Add(this.loadModelButton);
            this.Controls.Add(this.deleteRoiButton);
            this.Controls.Add(this.applyRoiButton);
            this.Controls.Add(this.roiHNumeric);
            this.Controls.Add(this.roiWNumeric);
            this.Controls.Add(this.roiYNumeric);
            this.Controls.Add(this.roiXNumeric);
            this.Controls.Add(this.roiNameText);
            this.Controls.Add(this.roiList);
            this.Controls.Add(this.saveNgButton);
            this.Controls.Add(this.saveOkButton);
            this.Controls.Add(this.datasetPathText);
            this.Controls.Add(this.loadedImageLabel);
            this.Controls.Add(this.loadImageButton);
            this.Controls.Add(this.roiCanvas);
            this.Name = "Form1";
            this.Text = "Multi-Head ROI 검사";
            ((System.ComponentModel.ISupportInitialize)(this.roiCanvas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiXNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiYNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiWNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiHNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CNetTest.Controls.RoiCanvas roiCanvas;
        private System.Windows.Forms.Button loadImageButton;
        private System.Windows.Forms.Label loadedImageLabel;
        private System.Windows.Forms.TextBox datasetPathText;
        private System.Windows.Forms.Button saveOkButton;
        private System.Windows.Forms.Button saveNgButton;
        private System.Windows.Forms.ListBox roiList;
        private System.Windows.Forms.TextBox roiNameText;
        private System.Windows.Forms.NumericUpDown roiXNumeric;
        private System.Windows.Forms.NumericUpDown roiYNumeric;
        private System.Windows.Forms.NumericUpDown roiWNumeric;
        private System.Windows.Forms.NumericUpDown roiHNumeric;
        private System.Windows.Forms.Button applyRoiButton;
        private System.Windows.Forms.Button deleteRoiButton;
        private System.Windows.Forms.Button loadModelButton;
        private System.Windows.Forms.Label modelPathLabel;
        private System.Windows.Forms.NumericUpDown inputWidthNumeric;
        private System.Windows.Forms.NumericUpDown inputHeightNumeric;
        private System.Windows.Forms.Button inferButton;
        private System.Windows.Forms.DataGridView resultsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoi;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOk;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNg;
    }
}
