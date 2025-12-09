using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CNetTest.Controls;
using CNetTest.Vision;

namespace CNetTest
{
    public partial class Form1 : Form
    {
        private readonly BindingList<RoiDefinition> _rois = new BindingList<RoiDefinition>();
        private readonly DatasetManager _datasetManager = new DatasetManager();
        private readonly InferenceEngine _inferenceEngine = new InferenceEngine();
        private Bitmap _currentImage;

        public Form1()
        {
            InitializeComponent();
            roiCanvas.Rois = _rois;
            roiList.DataSource = _rois;
            roiList.DisplayMember = nameof(RoiDefinition.Name);

            roiCanvas.RoiSelected += (_, roi) => ShowRoi(roi);
            roiCanvas.RoiAdded += (_, roi) => ShowRoi(roi);
            roiCanvas.RoiDeleted += (_, _) => ClearRoiFields();
        }

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            _currentImage?.Dispose();
            _currentImage = new Bitmap(dialog.FileName);
            roiCanvas.Image = _currentImage;
            loadedImageLabel.Text = Path.GetFileName(dialog.FileName);
        }

        private void saveOkButton_Click(object sender, EventArgs e) => SaveDataset("OK");

        private void saveNgButton_Click(object sender, EventArgs e) => SaveDataset("NG");

        private void SaveDataset(string label)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("먼저 이미지를 로드하세요.");
                return;
            }
            if (!_rois.Any())
            {
                MessageBox.Show("ROI를 추가하세요.");
                return;
            }

            try
            {
                _datasetManager.SetDatasetRoot(datasetPathText.Text);
                foreach (var roi in _rois)
                {
                    _datasetManager.SaveRoi(_currentImage, roi, label);
                }
                MessageBox.Show($"{label} 데이터가 저장되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dataset", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteRoiButton_Click(object sender, EventArgs e)
        {
            roiCanvas.DeleteSelected();
        }

        private void roiList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (roiList.SelectedItem is RoiDefinition roi)
            {
                roiCanvas.RoiSelected?.Invoke(this, roi);
                ShowRoi(roi);
            }
        }

        private void ShowRoi(RoiDefinition roi)
        {
            roiNameText.Text = roi?.Name ?? string.Empty;
            if (roi != null)
            {
                roiXNumeric.Value = (decimal)roi.Bounds.X;
                roiYNumeric.Value = (decimal)roi.Bounds.Y;
                roiWNumeric.Value = (decimal)roi.Bounds.Width;
                roiHNumeric.Value = (decimal)roi.Bounds.Height;
            }
            else
            {
                ClearRoiFields();
            }
        }

        private void ClearRoiFields()
        {
            roiNameText.Text = string.Empty;
            roiXNumeric.Value = roiYNumeric.Value = roiWNumeric.Value = roiHNumeric.Value = 0;
        }

        private void applyRoiButton_Click(object sender, EventArgs e)
        {
            if (roiList.SelectedItem is RoiDefinition roi)
            {
                roi.Name = roiNameText.Text;
                roi.Bounds = new RectangleF((float)roiXNumeric.Value, (float)roiYNumeric.Value, (float)roiWNumeric.Value, (float)roiHNumeric.Value);
                roiCanvas.UpdateSelected(roi.Bounds);
                roiList.Refresh();
            }
        }

        private void loadModelButton_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "ONNX model|*.onnx|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                _inferenceEngine.InputWidth = (int)inputWidthNumeric.Value;
                _inferenceEngine.InputHeight = (int)inputHeightNumeric.Value;
                _inferenceEngine.Load(dialog.FileName);
                modelPathLabel.Text = Path.GetFileName(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ONNX 로드", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void inferButton_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("먼저 이미지를 로드하세요.");
                return;
            }

            if (!_inferenceEngine.IsLoaded)
            {
                MessageBox.Show("ONNX 모델을 먼저 로드하세요.");
                return;
            }

            try
            {
                var results = _inferenceEngine.Run(_currentImage, _rois.ToList());
                resultsGrid.Rows.Clear();
                foreach (var result in results)
                {
                    resultsGrid.Rows.Add(result.Roi.Name, result.Label, result.OkScore, result.NgScore);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "추론 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
