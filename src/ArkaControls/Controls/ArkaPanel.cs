using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using ArkaControls.Drawing;
using ArkaControls.Theming;

namespace ArkaControls
{
    /// <summary>Container panel with rounded corners, border, optional per-side accent borders and shadow.</summary>
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    [ToolboxItem(true)]
    [DefaultProperty("FillColor")]
    public class ArkaPanel : Panel
    {
        private int _borderRadius = ArkaTheme.Current.Radius;
        private int _borderThickness;
        private Color _fillColor = ArkaTheme.Current.Surface;
        private Color _borderColor = ArkaTheme.Current.Border;
        private int _customBorderThickness;
        private Color _customBorderColor = ArkaTheme.Current.Primary;
        private ArkaBorderSides _customBorderSides = ArkaBorderSides.Left;

        public ArkaPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(200, 100);
        }

        [Category("Arka"), DefaultValue(8)]
        public int BorderRadius { get => _borderRadius; set { _borderRadius = Math.Max(0, value); Invalidate(); } }

        [Category("Arka"), DefaultValue(0)]
        public int BorderThickness { get => _borderThickness; set { _borderThickness = Math.Max(0, value); Invalidate(); } }

        [Category("Arka")]
        public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        private bool ShouldSerializeFillColor() => _fillColor != ArkaTheme.Current.Surface;
        private void ResetFillColor() => FillColor = ArkaTheme.Current.Surface;

        [Category("Arka")]
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        private bool ShouldSerializeBorderColor() => _borderColor != ArkaTheme.Current.Border;
        private void ResetBorderColor() => BorderColor = ArkaTheme.Current.Border;

        /// <summary>Thickness of an accent stripe drawn on the sides in <see cref="CustomBorderSides"/> (0 = none).</summary>
        [Category("Arka"), DefaultValue(0)]
        public int CustomBorderThickness { get => _customBorderThickness; set { _customBorderThickness = Math.Max(0, value); Invalidate(); } }

        [Category("Arka")]
        public Color CustomBorderColor { get => _customBorderColor; set { _customBorderColor = value; Invalidate(); } }
        private bool ShouldSerializeCustomBorderColor() => _customBorderColor != ArkaTheme.Current.Primary;
        private void ResetCustomBorderColor() => CustomBorderColor = ArkaTheme.Current.Primary;

        [Category("Arka"), DefaultValue(ArkaBorderSides.Left)]
        public ArkaBorderSides CustomBorderSides { get => _customBorderSides; set { _customBorderSides = value; Invalidate(); } }

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ShadowStyle ShadowDecoration { get; } = new ShadowStyle();

        protected override void OnPaintBackground(PaintEventArgs e) => ParentBackground.Paint(this, e);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var body = ShadowPainter.ContentBounds(ClientRectangle, ShadowDecoration);
            ShadowPainter.Paint(g, body, _borderRadius, ShadowDecoration);
            RoundedGraphics.FillAndBorder(g, body, _borderRadius, _fillColor, _borderColor, _borderThickness);

            if (_customBorderThickness > 0 && _customBorderSides != ArkaBorderSides.None)
            {
                var saved = g.Clip;
                using (var clip = RoundedGraphics.CreatePath(new RectangleF(body.X, body.Y, body.Width, body.Height), _borderRadius))
                using (var brush = new SolidBrush(_customBorderColor))
                {
                    g.SetClip(clip, System.Drawing.Drawing2D.CombineMode.Intersect);
                    int t = _customBorderThickness;
                    if (_customBorderSides.HasFlag(ArkaBorderSides.Left))   g.FillRectangle(brush, body.X, body.Y, t, body.Height);
                    if (_customBorderSides.HasFlag(ArkaBorderSides.Right))  g.FillRectangle(brush, body.Right - t, body.Y, t, body.Height);
                    if (_customBorderSides.HasFlag(ArkaBorderSides.Top))    g.FillRectangle(brush, body.X, body.Y, body.Width, t);
                    if (_customBorderSides.HasFlag(ArkaBorderSides.Bottom)) g.FillRectangle(brush, body.X, body.Bottom - t, body.Width, t);
                    g.Clip = saved;
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e) { Invalidate(); base.OnPaddingChanged(e); }
    }

    [Flags]
    public enum ArkaBorderSides { None = 0, Left = 1, Top = 2, Right = 4, Bottom = 8, All = 15 }
}
