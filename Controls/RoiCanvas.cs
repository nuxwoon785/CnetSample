using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CNetTest.Vision;

namespace CNetTest.Controls
{
    public class RoiCanvas : PictureBox
    {
        private bool _isDrawing;
        private Point _startPoint;
        private RoiDefinition _selected;
        private BindingList<RoiDefinition> _rois = new BindingList<RoiDefinition>();

        public event EventHandler<RoiDefinition> RoiSelected;
        public event EventHandler<RoiDefinition> RoiAdded;
        public event EventHandler<RoiDefinition> RoiChanged;
        public event EventHandler<RoiDefinition> RoiDeleted;

        public BindingList<RoiDefinition> Rois
        {
            get => _rois;
            set => _rois = value ?? new BindingList<RoiDefinition>();
        }

        public RoiCanvas()
        {
            SizeMode = PictureBoxSizeMode.StretchImage;
            BackColor = Color.Black;
            DoubleBuffered = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Image == null) return;

            var hit = HitTest(e.Location);
            if (hit != null)
            {
                _selected = hit;
                RoiSelected?.Invoke(this, hit);
                if (e.Button == MouseButtons.Right)
                {
                    _rois.Remove(hit);
                    RoiDeleted?.Invoke(this, hit);
                }
                return;
            }

            _isDrawing = true;
            _startPoint = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_isDrawing || Image == null) return;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_isDrawing || Image == null) return;
            _isDrawing = false;

            var end = e.Location;
            var rect = FromPoints(_startPoint, end);
            if (rect.Width < 5 || rect.Height < 5) return;

            var roiRect = DisplayToImage(rect);
            var roi = new RoiDefinition
            {
                Name = $"ROI {_rois.Count + 1}",
                Bounds = roiRect
            };

            _rois.Add(roi);
            RoiAdded?.Invoke(this, roi);
            _selected = roi;
            RoiSelected?.Invoke(this, roi);
            Invalidate();
        }

        public void DeleteSelected()
        {
            if (_selected == null) return;
            var temp = _selected;
            _rois.Remove(temp);
            _selected = null;
            RoiDeleted?.Invoke(this, temp);
            Invalidate();
        }

        public void UpdateSelected(RectangleF bounds)
        {
            if (_selected == null) return;
            _selected.Bounds = bounds;
            RoiChanged?.Invoke(this, _selected);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (Image == null) return;

            float scaleX = (float)Image.Width / ClientSize.Width;
            float scaleY = (float)Image.Height / ClientSize.Height;
            using var pen = new Pen(Color.Lime, 2);
            using var selectedPen = new Pen(Color.DeepSkyBlue, 2);
            using var font = new Font(FontFamily.GenericSansSerif, 9f);

            foreach (var roi in _rois)
            {
                var rect = new RectangleF(roi.Bounds.X / scaleX, roi.Bounds.Y / scaleY, roi.Bounds.Width / scaleX, roi.Bounds.Height / scaleY);
                var drawPen = roi == _selected ? selectedPen : pen;
                pe.Graphics.DrawRectangle(drawPen, rect.X, rect.Y, rect.Width, rect.Height);
                pe.Graphics.DrawString(roi.Name, font, Brushes.Yellow, rect.Location);
            }

            if (_isDrawing)
            {
                var rect = FromPoints(_startPoint, PointToClient(MousePosition));
                pe.Graphics.DrawRectangle(Pens.Red, rect);
            }
        }

        private RoiDefinition HitTest(Point point)
        {
            if (Image == null) return null;
            float scaleX = (float)Image.Width / ClientSize.Width;
            float scaleY = (float)Image.Height / ClientSize.Height;
            foreach (var roi in _rois.Reverse())
            {
                var rect = new RectangleF(roi.Bounds.X / scaleX, roi.Bounds.Y / scaleY, roi.Bounds.Width / scaleX, roi.Bounds.Height / scaleY);
                if (rect.Contains(point))
                    return roi;
            }

            return null;
        }

        private Rectangle DisplayToImage(Rectangle rect)
        {
            float scaleX = (float)Image.Width / ClientSize.Width;
            float scaleY = (float)Image.Height / ClientSize.Height;
            return new Rectangle
            {
                X = (int)(rect.X * scaleX),
                Y = (int)(rect.Y * scaleY),
                Width = (int)(rect.Width * scaleX),
                Height = (int)(rect.Height * scaleY)
            };
        }

        private Rectangle FromPoints(Point start, Point end)
        {
            int x1 = Math.Min(start.X, end.X);
            int y1 = Math.Min(start.Y, end.Y);
            int x2 = Math.Max(start.X, end.X);
            int y2 = Math.Max(start.Y, end.Y);
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
    }
}
