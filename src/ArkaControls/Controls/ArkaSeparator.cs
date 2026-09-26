using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ArkaControls.Drawing;
using ArkaControls.Theming;

namespace ArkaControls
{
    /// <summary>A thin horizontal or vertical divider line.</summary>
    [ToolboxItem(true)]
    public class ArkaSeparator : Control
    {
        private Color _fillColor = ArkaTheme.Current.Border;
        private Orientation _orientation = Orientation.Horizontal;
        private int _thickness = 1;

        public ArkaSeparator()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = false;
            Size = new Size(200, 3);
        }

        [Category("Arka")]
        public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        private bool ShouldSerializeFillColor() => _fillColor != ArkaTheme.Current.Border;
        private void ResetFillColor() => FillColor = ArkaTheme.Current.Border;

        [Category("Arka"), DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation { get => _orientation; set { _orientation = value; Invalidate(); } }

        [Category("Arka"), DefaultValue(1)]
        public int LineThickness { get => _thickness; set { _thickness = Math.Max(1, value); Invalidate(); } }

        protected override void OnPaintBackground(PaintEventArgs e) => ParentBackground.Paint(this, e);

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(_fillColor))
            {
                if (_orientation == Orientation.Horizontal)
                    e.Graphics.FillRectangle(brush, 0, (Height - _thickness) / 2, Width, _thickness);
                else
                    e.Graphics.FillRectangle(brush, (Width - _thickness) / 2, 0, _thickness, Height);
            }
        }
    }
}
