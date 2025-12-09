using System;
using System.Drawing;

namespace CNetTest.Vision
{
    /// <summary>
    /// Describes a region-of-interest in image coordinates.
    /// </summary>
    public class RoiDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Display name of the ROI.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The rectangle in source image pixel coordinates.
        /// </summary>
        public RectangleF Bounds { get; set; }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Id.ToString() : Name;
    }
}
