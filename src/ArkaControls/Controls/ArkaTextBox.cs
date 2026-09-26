using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ArkaControls.Drawing;
using ArkaControls.Theming;

namespace ArkaControls
{
    /// <summary>
    /// Rounded text box: a painted frame around a native <see cref="TextBox"/> (so IME, selection,
    /// clipboard and accessibility keep working). Supports placeholder text and a focused border colour.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("TextChanged")]
    [DefaultProperty("Text")]
    public class ArkaTextBox : Control
    {
        private readonly TextBox _inner = new TextBox { BorderStyle = BorderStyle.None };
        private int _borderRadius = ArkaTheme.Current.Radius;
        private int _borderThickness = 1;
        private Color _fillColor = ArkaTheme.Current.Surface;
        private Color _borderColor = ArkaTheme.Current.Border;
        private string _placeholder = string.Empty;
        private Color _placeholderColor = ArkaTheme.Current.TextMuted;   // used only where the OS cue banner is unavailable
        private bool _focused;

        public ArkaTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.ContainerControl, true);
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F);
            ForeColor = ArkaTheme.Current.Text;
            Size = new Size(200, 36);
            FocusedState.BorderColor = ArkaTheme.Current.BorderFocus;
            DisabledState.FillColor = ArkaTheme.Current.SurfaceAlt;
            DisabledState.ForeColor = ArkaTheme.Current.TextMuted;

            _inner.TextChanged += (s, e) => OnTextChanged(e);
            _inner.GotFocus += (s, e) => { _focused = true; Invalidate(); OnGotFocus(e); };
            _inner.LostFocus += (s, e) => { _focused = false; Invalidate(); OnLostFocus(e); };
            _inner.KeyDown += (s, e) => OnKeyDown(e);
            _inner.KeyUp += (s, e) => OnKeyUp(e);
            _inner.KeyPress += (s, e) => OnKeyPress(e);
            Controls.Add(_inner);
            SyncInner();
        }

        // ── Text surface ──────────────────────────────────────────────
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
#pragma warning disable CS8765
        public override string Text { get => _inner.Text; set => _inner.Text = value ?? string.Empty; }
#pragma warning restore CS8765

        /// <summary>Alias of <see cref="Text"/> (the name existing code commonly uses).</summary>
        [Category("Arka"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DefaultText { get => Text; set => Text = value; }

        [Category("Arka"), DefaultValue("")]
        public string PlaceholderText { get => _placeholder; set { _placeholder = value ?? string.Empty; ApplyCue(); Invalidate(); } }

        [Category("Arka")]
        public Color PlaceholderForeColor { get => _placeholderColor; set { _placeholderColor = value; Invalidate(); } }
        private bool ShouldSerializePlaceholderForeColor() => _placeholderColor != ArkaTheme.Current.TextMuted;
        private void ResetPlaceholderForeColor() => PlaceholderForeColor = ArkaTheme.Current.TextMuted;

        [DefaultValue(false)] public bool UseSystemPasswordChar { get => _inner.UseSystemPasswordChar; set => _inner.UseSystemPasswordChar = value; }
        [DefaultValue('\0')] public char PasswordChar { get => _inner.PasswordChar; set => _inner.PasswordChar = value; }
        [DefaultValue(false)] public bool ReadOnly { get => _inner.ReadOnly; set { _inner.ReadOnly = value; Invalidate(); } }
        [DefaultValue(32767)] public int MaxLength { get => _inner.MaxLength; set => _inner.MaxLength = value; }
        [DefaultValue(CharacterCasing.Normal)] public CharacterCasing CharacterCasing { get => _inner.CharacterCasing; set => _inner.CharacterCasing = value; }
        [DefaultValue(HorizontalAlignment.Left)] public HorizontalAlignment TextAlign { get => _inner.TextAlign; set => _inner.TextAlign = value; }
        [DefaultValue(false)] public bool Multiline { get => _inner.Multiline; set { _inner.Multiline = value; SyncInner(); ApplyCue(); } }
        [Browsable(false)] public string SelectedText { get => _inner.SelectedText; set => _inner.SelectedText = value; }
        [Browsable(false)] public int SelectionStart { get => _inner.SelectionStart; set => _inner.SelectionStart = value; }
        [Browsable(false)] public int SelectionLength { get => _inner.SelectionLength; set => _inner.SelectionLength = value; }
        public void SelectAll() => _inner.SelectAll();
        public void Clear() => _inner.Clear();
        public override bool Focused => _inner.Focused;
        public new bool Focus() => _inner.Focus();

        // ── Appearance ────────────────────────────────────────────────
        [Category("Arka"), DefaultValue(8)]
        public int BorderRadius { get => _borderRadius; set { _borderRadius = Math.Max(0, value); SyncInner(); Invalidate(); } }

        [Category("Arka"), DefaultValue(1)]
        public int BorderThickness { get => _borderThickness; set { _borderThickness = Math.Max(0, value); SyncInner(); Invalidate(); } }

        [Category("Arka")]
        public Color FillColor { get => _fillColor; set { _fillColor = value; SyncInner(); Invalidate(); } }
        private bool ShouldSerializeFillColor() => _fillColor != ArkaTheme.Current.Surface;
        private void ResetFillColor() => FillColor = ArkaTheme.Current.Surface;

        [Category("Arka")]
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        private bool ShouldSerializeBorderColor() => _borderColor != ArkaTheme.Current.Border;
        private void ResetBorderColor() => BorderColor = ArkaTheme.Current.Border;

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StateStyle FocusedState { get; } = new StateStyle();

        [Category("Arka"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StateStyle DisabledState { get; } = new StateStyle();

        protected override void OnFontChanged(EventArgs e) { _inner.Font = Font; SyncInner(); base.OnFontChanged(e); }
        protected override void OnForeColorChanged(EventArgs e) { SyncInner(); base.OnForeColorChanged(e); }
        protected override void OnEnabledChanged(EventArgs e) { _inner.Enabled = Enabled; SyncInner(); Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnPaddingChanged(EventArgs e) { SyncInner(); base.OnPaddingChanged(e); }
        protected override void OnResize(EventArgs e) { base.OnResize(e); SyncInner(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyCue(); }
        protected override void OnGotFocus(EventArgs e) { if (!_inner.Focused) _inner.Focus(); base.OnGotFocus(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _inner.Focus(); base.OnMouseDown(e); }

        private Color EffectiveFill => Enabled ? _fillColor : StateStyle.Pick(DisabledState.FillColor, _fillColor);
        private Color EffectiveFore => Enabled ? ForeColor : StateStyle.Pick(DisabledState.ForeColor, ForeColor);

        private void SyncInner()
        {
            if (_inner == null) return;
            _inner.BackColor = EffectiveFill.A == 255 ? EffectiveFill : Color.White;
            _inner.ForeColor = EffectiveFore;
            int inset = Math.Max(_borderThickness, 0) + Math.Max(4, _borderRadius / 2);
            int padV = Math.Max(_borderThickness, 0) + 2;
            if (_inner.Multiline)
            {
                _inner.SetBounds(inset, padV + 2, Math.Max(10, Width - inset * 2), Math.Max(10, Height - (padV + 2) * 2));
            }
            else
            {
                int h = _inner.PreferredHeight;
                _inner.SetBounds(inset, Math.Max(0, (Height - h) / 2), Math.Max(10, Width - inset * 2), h);
            }
        }

        private void ApplyCue()
        {
            if (!IsHandleCreated || !_inner.IsHandleCreated)
            {
                if (_inner != null && !_inner.IsHandleCreated) _inner.HandleCreated += (s, e) => NativeMethods.SetCueBanner(_inner.Handle, _placeholder, true);
                return;
            }
            NativeMethods.SetCueBanner(_inner.Handle, _placeholder, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e) => ParentBackground.Paint(this, e);

        protected override void OnPaint(PaintEventArgs e)
        {
            Color border = _borderColor;
            if (!Enabled) border = StateStyle.Pick(DisabledState.BorderColor, border);
            else if (_focused) border = StateStyle.Pick(FocusedState.BorderColor, border);
            RoundedGraphics.FillAndBorder(e.Graphics, ClientRectangle, _borderRadius, EffectiveFill, border, _borderThickness);
        }
    }
}
