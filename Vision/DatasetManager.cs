using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CNetTest.Vision
{
    /// <summary>
    /// Manages saving ROI crops into dataset folders.
    /// </summary>
    public class DatasetManager
    {
        public string DatasetRoot { get; private set; } = Path.Combine(Environment.CurrentDirectory, "dataset");

        public void SetDatasetRoot(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentException("Dataset root cannot be empty", nameof(root));
            }

            DatasetRoot = root;
            Directory.CreateDirectory(DatasetRoot);
        }

        public string SaveRoi(Bitmap source, RoiDefinition roi, string label)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (roi == null) throw new ArgumentNullException(nameof(roi));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label is required", nameof(label));

            var roiRect = Rectangle.Round(roi.Bounds);
            roiRect.Intersect(new Rectangle(Point.Empty, source.Size));
            if (roiRect.Width <= 0 || roiRect.Height <= 0)
            {
                throw new InvalidOperationException("ROI is empty or outside the image");
            }

            string labelFolder = Path.Combine(DatasetRoot, roi.Name, label);
            Directory.CreateDirectory(labelFolder);

            using var clone = new Bitmap(roiRect.Width, roiRect.Height);
            using (var g = Graphics.FromImage(clone))
            {
                g.DrawImage(source, new Rectangle(0, 0, clone.Width, clone.Height), roiRect, GraphicsUnit.Pixel);
            }

            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{roi.Id}.png";
            string savePath = Path.Combine(labelFolder, fileName);
            clone.Save(savePath, ImageFormat.Png);
            return savePath;
        }
    }
}
