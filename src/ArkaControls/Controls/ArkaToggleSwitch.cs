// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ArkaControls.Drawing;
using ArkaControls.Theming;

namespace ArkaControls
{
    /// <summary>On/off switch with an animated thumb.</summary>
    [ToolboxItem(true)]
    [DefaultEvent("CheckedChanged")]
    [DefaultProperty("Checked")]
    public class ArkaToggleSwitch : Control
    {
        private bool _checked;
        private float _position;              // 0 = off, 1 = on
        private readonly Timer _timer = new Timer { Interval = 15 };

        public ArkaToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable | ControlStyles.StandardClick, true);
            BackColor = Color.Transparent;
            Size = new Size(44, 22);
            Cursor = Cursors.Hand;
            CheckedState.FillColor = ArkaTheme.Current.Primary;
            CheckedState.InnerColor = Color.White;
            UncheckedState.FillColor = ArkaTheme.Current.Disabled;
            UncheckedState.InnerColor = Color.White;
            _timer.Tick += (s, e) => Step();
        }

        public event EventHandler? CheckedChanged;

        [Category("Arka"), DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                if (Animated && !DesignMode && IsHandleCreated) _timer.Start(); else { _position = value ? 1f : 0f; Invalidate(); }
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Arka"), DefaultValue(true)]
        public bool Animated { get; set; } = true;

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToggleStyle CheckedState { get; } = new ToggleStyle();

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToggleStyle UncheckedState { get; } = new ToggleStyle();

        private void Step()
        {
            float target = _checked ? 1f : 0f;
            float delta = 0.18f;
            _position = _position < target ? Math.Min(target, _position + delta) : Math.Max(target, _position - delta);
            if (Math.Abs(_position - target) < 0.001f) _timer.Stop();
            Invalidate();
        }

        protected override void OnClick(EventArgs e) { if (Enabled) Checked = !Checked; base.OnClick(e); }
        protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space) { Checked = !Checked; e.Handled = true; } base.OnKeyDown(e); }
        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || base.IsInputKey(keyData);
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void Dispose(bool disposing) { if (disposing) _timer.Dispose(); base.Dispose(disposing); }
        protected override void OnPaintBackground(PaintEventArgs e) => ParentBackground.Paint(this, e);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var st = _checked ? CheckedState : UncheckedState;
            var other = _checked ? UncheckedState : CheckedState;

            Color track = Blend(other.FillColor, st.FillColor, _position);
            if (!Enabled) track = Color.FromArgb(120, track);
            RoundedGraphics.FillAndBorder(g, ClientRectangle, Height / 2, track, Color.Empty, 0);

            int pad = 3;
            int d = Height - pad * 2;
            float x = pad + (Width - d - pad * 2) * _position;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(st.InnerColor))
                g.FillEllipse(brush, x, pad, d, d);

            if (Focused)
                using (var pen = new Pen(Color.FromArgb(120, ArkaTheme.Current.BorderFocus), 2))
                using (var path = RoundedGraphics.CreatePath(new RectangleF(1, 1, Width - 3, Height - 3), Height / 2))
                    g.DrawPath(pen, path);
        }

        private static Color Blend(Color a, Color b, float t) => Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t), (int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    }

    /// <summary>Track and thumb colours of one toggle position.</summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ToggleStyle
    {
        public Color FillColor { get; set; }
        public Color InnerColor { get; set; }
        private bool ShouldSerializeFillColor() => false;
        private bool ShouldSerializeInnerColor() => false;
        public override string ToString() => "(colours)";
    }
}
