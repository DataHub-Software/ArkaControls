using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ArkaControls.Drawing;
using ArkaControls.Theming;

namespace ArkaControls
{
    /// <summary>Flat button with rounded corners, hover/pressed/disabled colours and an optional shadow.</summary>
    [ToolboxItem(true)]
    [DefaultEvent("Click")]
    public class ArkaButton : Button
    {
        private int _borderRadius = ArkaTheme.Current.Radius;
        private int _borderThickness;
        private Color _fillColor = ArkaTheme.Current.Primary;
        private Color _borderColor = ArkaTheme.Current.Border;
        private bool _hover, _pressed;

        public ArkaButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Size = new Size(120, 40);
            Cursor = Cursors.Hand;
            HoverState.FillColor = ArkaTheme.Current.PrimaryHover;
            DisabledState.FillColor = ArkaTheme.Current.Disabled;
            DisabledState.ForeColor = ArkaTheme.Current.TextMuted;
        }

        // ── Appearance ────────────────────────────────────────────────
        [Category("Arka"), Description("Corner radius in pixels.")]
        [DefaultValue(8)]
        public int BorderRadius { get => _borderRadius; set { _borderRadius = Math.Max(0, value); Invalidate(); } }

        [Category("Arka"), Description("Border thickness in pixels (0 = none).")]
        [DefaultValue(0)]
        public int BorderThickness { get => _borderThickness; set { _borderThickness = Math.Max(0, value); Invalidate(); } }

        [Category("Arka"), Description("Background colour of the button body.")]
        public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        private bool ShouldSerializeFillColor() => _fillColor != ArkaTheme.Current.Primary;
        private void ResetFillColor() => FillColor = ArkaTheme.Current.Primary;

        [Category("Arka"), Description("Border colour.")]
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        private bool ShouldSerializeBorderColor() => _borderColor != ArkaTheme.Current.Border;
        private void ResetBorderColor() => BorderColor = ArkaTheme.Current.Border;

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StateStyle HoverState { get; } = new StateStyle();

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StateStyle PressedState { get; } = new StateStyle();

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StateStyle DisabledState { get; } = new StateStyle();

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ShadowStyle ShadowDecoration { get; } = new ShadowStyle();

        // ── Interaction ───────────────────────────────────────────────
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        // ── Painting ──────────────────────────────────────────────────
        protected override void OnPaintBackground(PaintEventArgs pevent) => ParentBackground.Paint(this, pevent);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var body = ShadowPainter.ContentBounds(ClientRectangle, ShadowDecoration);
            ShadowPainter.Paint(g, body, _borderRadius, ShadowDecoration);

            Color fill = _fillColor, fore = ForeColor, border = _borderColor;
            if (!Enabled)
            {
                fill = StateStyle.Pick(DisabledState.FillColor, fill);
                fore = StateStyle.Pick(DisabledState.ForeColor, fore);
                border = StateStyle.Pick(DisabledState.BorderColor, border);
            }
            else if (_pressed)
            {
                fill = StateStyle.Pick(PressedState.FillColor, RoundedGraphics.Shade(StateStyle.Pick(HoverState.FillColor, fill), -0.08f));
                fore = StateStyle.Pick(PressedState.ForeColor, StateStyle.Pick(HoverState.ForeColor, fore));
                border = StateStyle.Pick(PressedState.BorderColor, StateStyle.Pick(HoverState.BorderColor, border));
            }
            else if (_hover)
            {
                fill = StateStyle.Pick(HoverState.FillColor, fill);
                fore = StateStyle.Pick(HoverState.ForeColor, fore);
                border = StateStyle.Pick(HoverState.BorderColor, border);
            }

            RoundedGraphics.FillAndBorder(g, body, _borderRadius, fill, border, _borderThickness);

            // image + text
            var content = Rectangle.Inflate(body, -Padding.Left - _borderThickness, -Padding.Top - _borderThickness);
            if (Image != null)
            {
                var imgSize = Image.Size;
                var pos = new Point(content.X + (content.Width - imgSize.Width) / 2, content.Y + (content.Height - imgSize.Height) / 2);
                if (!string.IsNullOrEmpty(Text)) pos.X = content.X + 6;
                pos.Offset(ImageOffset);
                g.DrawImage(Image, new Rectangle(pos, imgSize));
                if (!string.IsNullOrEmpty(Text)) { content.X += imgSize.Width + 10; content.Width -= imgSize.Width + 10; }
            }

            TextRenderer.DrawText(g, Text, Font, content, fore, ToFlags(TextAlign));

            if (Focused && ShowFocusCues)
            {
                var focus = Rectangle.Inflate(body, -4, -4);
                ControlPaint.DrawFocusRectangle(g, focus, fore, Color.Empty);
            }
        }

        /// <summary>Pixel offset applied to the image (Guna-compatible).</summary>
        [Category("Arka"), DefaultValue(typeof(Point), "0, 0")]
        public Point ImageOffset { get; set; }

        private static TextFormatFlags ToFlags(ContentAlignment a)
        {
            var f = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            switch (a)
            {
                case ContentAlignment.TopLeft: return f | TextFormatFlags.Top | TextFormatFlags.Left;
                case ContentAlignment.TopCenter: return f | TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.TopRight: return f | TextFormatFlags.Top | TextFormatFlags.Right;
                case ContentAlignment.MiddleLeft: return f | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                case ContentAlignment.MiddleRight: return f | TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                case ContentAlignment.BottomLeft: return f | TextFormatFlags.Bottom | TextFormatFlags.Left;
                case ContentAlignment.BottomCenter: return f | TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.BottomRight: return f | TextFormatFlags.Bottom | TextFormatFlags.Right;
                default: return f | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
            }
        }
    }
}
