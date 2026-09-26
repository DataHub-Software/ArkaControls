using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArkaControls.Drawing;
using ArkaControls.Theming;

namespace ArkaControls
{
    /// <summary>
    /// Combo box whose closed state is painted with rounded corners and a chevron and whose drop-down
    /// items are owner-drawn. Behaviour (data binding, selection, autocomplete) is the stock ComboBox.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("SelectedIndexChanged")]
    public class ArkaComboBox : ComboBox
    {
        private int _borderRadius = ArkaTheme.Current.Radius;
        private Color _fillColor = ArkaTheme.Current.Surface;
        private Color _borderColor = ArkaTheme.Current.Border;
        private Color _focusedColor = ArkaTheme.Current.BorderFocus;
        private Color _itemHoverColor = Color.FromArgb(239, 246, 255);
        private bool _hover;

        public ArkaComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            ItemHeight = 28;
            Font = new Font("Segoe UI", 10F);
            ForeColor = ArkaTheme.Current.Text;
        }

        [Category("Arka"), DefaultValue(8)]
        public int BorderRadius { get => _borderRadius; set { _borderRadius = Math.Max(0, value); Invalidate(); } }

        [Category("Arka")]
        public Color FillColor { get => _fillColor; set { _fillColor = value; BackColor = value; Invalidate(); } }
        private bool ShouldSerializeFillColor() => _fillColor != ArkaTheme.Current.Surface;
        private void ResetFillColor() => FillColor = ArkaTheme.Current.Surface;

        [Category("Arka")]
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        private bool ShouldSerializeBorderColor() => _borderColor != ArkaTheme.Current.Border;
        private void ResetBorderColor() => BorderColor = ArkaTheme.Current.Border;

        [Category("Arka")]
        public Color FocusedColor { get => _focusedColor; set { _focusedColor = value; Invalidate(); } }
        private bool ShouldSerializeFocusedColor() => _focusedColor != ArkaTheme.Current.BorderFocus;
        private void ResetFocusedColor() => FocusedColor = ArkaTheme.Current.BorderFocus;

        [Category("Arka")]
        public Color ItemHoverColor { get => _itemHoverColor; set { _itemHoverColor = value; } }
        private bool ShouldSerializeItemHoverColor() => _itemHoverColor != Color.FromArgb(239, 246, 255);
        private void ResetItemHoverColor() => ItemHoverColor = Color.FromArgb(239, 246, 255);

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnSelectedIndexChanged(EventArgs e) { Invalidate(); base.OnSelectedIndexChanged(e); }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // The native control repaints itself in WM_PAINT; paint the flat rounded face on top of it.
            if (m.Msg == NativeMethods.WM_PAINT && DropDownStyle == ComboBoxStyle.DropDownList && IsHandleCreated)
            {
                using (var g = Graphics.FromHwnd(Handle))
                    PaintFace(g);
            }
        }

        private void PaintFace(Graphics g)
        {
            var rect = ClientRectangle;
            Color parentBack = Parent?.BackColor ?? SystemColors.Control;
            using (var b = new SolidBrush(parentBack)) g.FillRectangle(b, rect);

            Color border = Focused || DroppedDown ? _focusedColor : (_hover ? RoundedGraphics.Shade(_borderColor, -0.15f) : _borderColor);
            Color fill = Enabled ? _fillColor : Color.FromArgb(248, 249, 250);
            RoundedGraphics.FillAndBorder(g, rect, _borderRadius, fill, border, 1);

            // text
            int arrowW = 28;
            var textRect = new Rectangle(10, 0, Math.Max(0, rect.Width - arrowW - 10), rect.Height);
            string text = SelectedItem != null ? (GetItemText(SelectedItem!) ?? string.Empty) : string.Empty;
            TextRenderer.DrawText(g, text, Font, textRect, Enabled ? ForeColor : SystemColors.GrayText,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // chevron
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int cx = rect.Right - arrowW / 2 - 2, cy = rect.Height / 2;
            using (var pen = new Pen(Enabled ? (DroppedDown ? _focusedColor : ArkaTheme.Current.TextMuted) : SystemColors.GrayText, 1.8f)
                   { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                g.DrawLines(pen, new[] { new Point(cx - 4, cy - 2), new Point(cx, cy + 2), new Point(cx + 4, cy - 2) });
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool hot = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color back = hot ? _itemHoverColor : _fillColor;
            using (var b = new SolidBrush(back)) e.Graphics.FillRectangle(b, e.Bounds);
            string text = GetItemText(Items[e.Index]!) ?? string.Empty;
            var bounds = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, text, Font, bounds, ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
