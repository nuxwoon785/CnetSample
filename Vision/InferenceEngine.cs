using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace CNetTest.Vision
{
    /// <summary>
    /// Thin wrapper around ONNX Runtime for multi-head classification models.
    /// </summary>
    public class InferenceEngine : IDisposable
    {
        private InferenceSession _session;

        public int InputWidth { get; set; } = 224;
        public int InputHeight { get; set; } = 224;

        public bool IsLoaded => _session != null;

        public void Load(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
                throw new ArgumentException("Model path is required", nameof(modelPath));
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Model file not found", modelPath);

            _session?.Dispose();
            _session = new InferenceSession(modelPath);
        }

        public IReadOnlyList<PredictionResult> Run(Bitmap source, IList<RoiDefinition> rois)
        {
            if (_session == null)
                throw new InvalidOperationException("Model has not been loaded.");
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rois == null || rois.Count == 0)
                throw new ArgumentException("At least one ROI is required", nameof(rois));

            var predictions = new List<PredictionResult>();
            string inputName = _session.InputMetadata.Keys.First();

            foreach (var roi in rois)
            {
                var tensor = Preprocess(source, roi);
                using var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };
                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

                // Multi-head: assume first output corresponds to the ROI
                var output = results.First();
                var scores = output.AsEnumerable<float>().ToArray();
                float ok = scores.Length > 0 ? scores[0] : 0f;
                float ng = scores.Length > 1 ? scores[1] : 0f;

                predictions.Add(new PredictionResult
                {
                    Roi = roi,
                    OkScore = ok,
                    NgScore = ng
                });
            }

            return predictions;
        }

        private DenseTensor<float> Preprocess(Bitmap source, RoiDefinition roi)
        {
            var roiRect = Rectangle.Round(roi.Bounds);
            roiRect.Intersect(new Rectangle(Point.Empty, source.Size));
            if (roiRect.Width <= 0 || roiRect.Height <= 0)
                throw new InvalidOperationException("ROI is empty or outside the image");

            using var cropped = new Bitmap(roiRect.Width, roiRect.Height);
            using (var g = Graphics.FromImage(cropped))
            {
                g.DrawImage(source, new Rectangle(0, 0, cropped.Width, cropped.Height), roiRect, GraphicsUnit.Pixel);
            }

            using var resized = new Bitmap(cropped, new Size(InputWidth, InputHeight));
            var tensor = new DenseTensor<float>(new[] { 1, 3, InputHeight, InputWidth });
            for (int y = 0; y < InputHeight; y++)
            {
                for (int x = 0; x < InputWidth; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    int idx = y * InputWidth + x;
                    tensor[0, 0, y, x] = pixel.R / 255f;
                    tensor[0, 1, y, x] = pixel.G / 255f;
                    tensor[0, 2, y, x] = pixel.B / 255f;
                }
            }

            return tensor;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
