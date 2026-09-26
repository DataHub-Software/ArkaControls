using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ArkaControls.Drawing;

namespace ArkaControls
{
    /// <summary>
    /// Label with a deliberately tiny markup subset: <c>&lt;b&gt; &lt;i&gt; &lt;u&gt; &lt;br/&gt;</c> and
    /// <c>&lt;div align='left|center|right'&gt;</c>. Everything else is shown as plain text; entities
    /// <c>&amp;lt; &amp;gt; &amp;amp; &amp;quot; &amp;nbsp;</c> are decoded. No scripts, links or images are ever interpreted.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty("Text")]
    public class ArkaHtmlLabel : Control
    {
        private bool _autoSize = true;
        private ContentAlignment _textAlign = ContentAlignment.TopLeft;
        private List<Line> _lines = new List<Line>();

        public ArkaHtmlLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(100, 20);
            Reparse();
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
#pragma warning disable CS8765 // base.Text nullability differs between target frameworks
        public override string Text { get => base.Text; set => base.Text = value; }
#pragma warning restore CS8765

        [Browsable(true), DefaultValue(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new bool AutoSize
        {
            get => _autoSize;
            set { _autoSize = value; if (value) FitToContent(); Invalidate(); }
        }

        [Category("Appearance"), DefaultValue(ContentAlignment.TopLeft)]
        public ContentAlignment TextAlign { get => _textAlign; set { _textAlign = value; Invalidate(); } }

        protected override void OnTextChanged(EventArgs e) { Reparse(); FitToContent(); Invalidate(); base.OnTextChanged(e); }
        protected override void OnFontChanged(EventArgs e) { FitToContent(); Invalidate(); base.OnFontChanged(e); }

        // ── Parsing ───────────────────────────────────────────────────
        internal sealed class Run { public string Text = ""; public bool Bold, Italic, Underline; }
        internal sealed class Line { public readonly List<Run> Runs = new List<Run>(); public HorizontalAlignment? Align; }

        private static readonly Regex TagRx = new Regex(@"<\s*(/?)\s*(b|strong|i|em|u|br|div)\b([^>]*)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AlignAttr = new Regex(@"align\s*=\s*['""]?(left|center|right)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private void Reparse() => _lines = Parse(Text ?? string.Empty);

        internal static List<Line> Parse(string html)
        {
            var lines = new List<Line>();
            var line = new Line();
            lines.Add(line);
            int bold = 0, italic = 0, under = 0;
            var aligns = new Stack<HorizontalAlignment?>();
            HorizontalAlignment? align = null;

            void AddText(string raw)
            {
                if (raw.Length == 0) return;
                string decoded = raw.Replace("&nbsp;", " ").Replace("&lt;", "<").Replace("&gt;", ">")
                                    .Replace("&quot;", "\"").Replace("&amp;", "&");
                var parts = decoded.Replace("\r\n", "\n").Split('\n');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i > 0) { line = new Line { Align = align }; lines.Add(line); }
                    if (parts[i].Length > 0)
                        line.Runs.Add(new Run { Text = parts[i], Bold = bold > 0, Italic = italic > 0, Underline = under > 0 });
                }
                if (line.Align == null) line.Align = align;
            }

            int pos = 0;
            foreach (Match m in TagRx.Matches(html))
            {
                AddText(html.Substring(pos, m.Index - pos));
                pos = m.Index + m.Length;
                bool closing = m.Groups[1].Value == "/";
                switch (m.Groups[2].Value.ToLowerInvariant())
                {
                    case "b": case "strong": bold = Math.Max(0, bold + (closing ? -1 : 1)); break;
                    case "i": case "em": italic = Math.Max(0, italic + (closing ? -1 : 1)); break;
                    case "u": under = Math.Max(0, under + (closing ? -1 : 1)); break;
                    case "br": line = new Line { Align = align }; lines.Add(line); break;
                    case "div":
                        if (closing) { align = aligns.Count > 0 ? aligns.Pop() : null; if (line.Runs.Count > 0) { line = new Line { Align = align }; lines.Add(line); } }
                        else
                        {
                            if (line.Runs.Count > 0) { line = new Line(); lines.Add(line); }
                            aligns.Push(align);
                            var am = AlignAttr.Match(m.Groups[3].Value);
                            if (am.Success)
                                align = am.Groups[1].Value.ToLowerInvariant() == "right" ? HorizontalAlignment.Right
                                      : am.Groups[1].Value.ToLowerInvariant() == "center" ? HorizontalAlignment.Center
                                      : HorizontalAlignment.Left;
                            line.Align = align;
                        }
                        break;
                }
            }
            AddText(html.Substring(pos));
            return lines;
        }

        // ── Measuring & painting ──────────────────────────────────────
        private const TextFormatFlags Flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.Left | TextFormatFlags.Top;

        private Font StyleFont(Run r)
        {
            var style = FontStyle.Regular;
            if (r.Bold || Font.Bold) style |= FontStyle.Bold;
            if (r.Italic || Font.Italic) style |= FontStyle.Italic;
            if (r.Underline || Font.Underline) style |= FontStyle.Underline;
            return new Font(Font, style);
        }

        private Size MeasureLine(Line l)
        {
            int w = 0, h = Font.Height;
            foreach (var r in l.Runs)
                using (var f = StyleFont(r))
                {
                    var s = TextRenderer.MeasureText(r.Text, f, new Size(int.MaxValue, int.MaxValue), Flags);
                    w += s.Width; h = Math.Max(h, s.Height);
                }
            return new Size(w, h);
        }

        /// <summary>Total size of the rendered markup.</summary>
        public Size MeasureContent()
        {
            int w = 0, h = 0;
            foreach (var l in _lines) { var s = MeasureLine(l); w = Math.Max(w, s.Width); h += s.Height; }
            return new Size(w, Math.Max(h, Font.Height));
        }

        private void FitToContent()
        {
            if (!_autoSize || IsDisposed) return;
            var s = MeasureContent();
            if (Size != s) Size = s;
        }

        protected override void OnPaintBackground(PaintEventArgs e) => ParentBackground.Paint(this, e);

        protected override void OnPaint(PaintEventArgs e)
        {
            var total = MeasureContent();
            int y = _textAlign.ToString().StartsWith("Bottom") ? ClientSize.Height - total.Height
                  : _textAlign.ToString().StartsWith("Middle") ? (ClientSize.Height - total.Height) / 2 : 0;

            foreach (var l in _lines)
            {
                var ls = MeasureLine(l);
                var align = l.Align ?? (_textAlign.ToString().EndsWith("Right") ? HorizontalAlignment.Right
                                       : _textAlign.ToString().EndsWith("Center") ? HorizontalAlignment.Center
                                       : HorizontalAlignment.Left);
                int x = align == HorizontalAlignment.Right ? ClientSize.Width - ls.Width
                      : align == HorizontalAlignment.Center ? (ClientSize.Width - ls.Width) / 2 : 0;
                foreach (var r in l.Runs)
                {
                    using (var f = StyleFont(r))
                    {
                        var size = TextRenderer.MeasureText(r.Text, f, new Size(int.MaxValue, int.MaxValue), Flags);
                        TextRenderer.DrawText(e.Graphics, r.Text, f, new Point(x, y), ForeColor, Flags);
                        x += size.Width;
                    }
                }
                y += ls.Height;
            }
        }
    }
}
