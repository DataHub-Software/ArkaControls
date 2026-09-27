// =============================================================
//  ArkaControls.Screenshots
// =============================================================
//  Renders every control in every visual state into PNG files so a
//  human (or a diff tool) can review them without opening the designer.
//
//    ArkaControls.Screenshots.exe [outputDir] [--dpi-note text]
//
//  Capture strategy: show each scene in a borderless form at (40,40) and copy the pixels from the
//  screen (this includes the ComboBox face, which is painted on WM_PAINT and is invisible to
//  DrawToBitmap). If the desktop is not interactive the capture comes back black and the tool falls
//  back to DrawToBitmap, and says so in report.txt.
// =============================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ArkaControls;
using ArkaControls.Theming;

namespace ArkaControls.Screenshots
{
    internal static class Program
    {
        private static string _out = "screenshots";
        private static readonly StringBuilder Report = new StringBuilder();
        private static int _failed;

        [STAThread]
        private static int Main(string[] args)
        {
            _out = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : "screenshots";
            Directory.CreateDirectory(_out);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Report.AppendLine("ArkaControls screenshot run " + DateTime.Now.ToString("u"));
            Report.AppendLine("OS: " + Environment.OSVersion + "  64-bit: " + Environment.Is64BitOperatingSystem);
            Report.AppendLine("Screen: " + Screen.PrimaryScreen.Bounds + "  DPI scale: " + DpiScale());
            Report.AppendLine("CLR: " + Environment.Version);
            Report.AppendLine();

            foreach (var scene in Scenes.All())
            {
                try { Capture(scene.Name, scene.Build); }
                catch (Exception ex)
                {
                    _failed++;
                    Report.AppendLine("FAIL  " + scene.Name + ": " + ex.GetType().Name + ": " + ex.Message);
                    Console.Error.WriteLine("FAIL " + scene.Name + ": " + ex);
                }
            }

            Report.AppendLine();
            Report.AppendLine(_failed == 0 ? "All scenes captured." : _failed + " scene(s) failed.");
            File.WriteAllText(Path.Combine(_out, "report.txt"), Report.ToString());
            Console.WriteLine(Report.ToString());
            return _failed == 0 ? 0 : 1;
        }

        private static float DpiScale()
        {
            using (var g = Graphics.FromHwnd(IntPtr.Zero)) return g.DpiX / 96f;
        }

        // ── Capture ───────────────────────────────────────────────────
        private static void Capture(string name, Func<Form> build)
        {
            using (var form = build())
            {
                form.FormBorderStyle = FormBorderStyle.None;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(40, 40);
                form.ShowInTaskbar = false;
                form.TopMost = true;
                form.Show();
                form.Activate();
                Pump(350);

                // scene-specific late setup that needs a live handle (focus, hover simulation)
                if (form.Tag is Action<Form> late) { late(form); Pump(250); }

                var size = form.ClientSize;
                string path = Path.Combine(_out, name + ".png");
                string how;
                using (var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
                {
                    var origin = form.PointToScreen(Point.Empty);
                    using (var g = Graphics.FromImage(bmp))
                        g.CopyFromScreen(origin, Point.Empty, size);
                    how = "screen";
                    if (LooksBlank(bmp))
                    {
                        form.DrawToBitmap(bmp, new Rectangle(Point.Empty, size));
                        how = "DrawToBitmap (screen capture was blank)";
                    }
                    bmp.Save(path, ImageFormat.Png);
                }
                Report.AppendLine("ok    " + name + "  " + size.Width + "x" + size.Height + "  via " + how);
                form.Close();
            }
        }

        private static bool LooksBlank(Bitmap bmp)
        {
            // Sample a grid; a black or uniformly single-colour image means the desktop was not capturable.
            var seen = new HashSet<int>();
            for (int y = 0; y < bmp.Height; y += Math.Max(1, bmp.Height / 12))
                for (int x = 0; x < bmp.Width; x += Math.Max(1, bmp.Width / 12))
                    seen.Add(bmp.GetPixel(x, y).ToArgb());
            return seen.Count <= 1;
        }

        private static void Pump(int ms)
        {
            var end = DateTime.UtcNow.AddMilliseconds(ms);
            while (DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(10); }
        }
    }

    internal sealed class Scene
    {
        public string Name = "";
        public Func<Form> Build = () => new Form();
    }

    internal static class Scenes
    {
        private static readonly Color Page = Color.FromArgb(243, 244, 246);

        private static Form Canvas(int w, int h, Color? back = null) =>
            new Form { ClientSize = new Size(w, h), BackColor = back ?? Page };

        private static ArkaLabel Caption(string text, int x, int y) =>
            new ArkaLabel { Text = text, Location = new Point(x, y), Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(107, 114, 128) };

        // Simulates interaction states without moving the real mouse.
        private static void SetField(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            if (f == null) throw new MissingFieldException(target.GetType().Name, field);
            f.SetValue(target, value);
            ((Control)target).Invalidate();
        }

        public static IEnumerable<Scene> All()
        {
            yield return new Scene { Name = "01-buttons-states", Build = ButtonsStates };
            yield return new Scene { Name = "02-buttons-shapes", Build = ButtonsShapes };
            yield return new Scene { Name = "03-panels", Build = Panels };
            yield return new Scene { Name = "04-textboxes", Build = TextBoxes };
            yield return new Scene { Name = "05-combobox", Build = ComboBoxes };
            yield return new Scene { Name = "06-toggle-separator-labels", Build = Misc };
            yield return new Scene { Name = "07-grid", Build = Grid };
            yield return new Scene { Name = "08-dark-parent", Build = DarkParent };
            yield return new Scene { Name = "09-pos-payment-mock", Build = PosMock };
        }

        // 1 ─ interaction states side by side
        private static Form ButtonsStates()
        {
            var f = Canvas(760, 170);
            string[] names = { "normal", "hover", "pressed", "disabled", "focused" };
            var btns = new ArkaButton[5];
            for (int i = 0; i < 5; i++)
            {
                btns[i] = new ArkaButton { Text = "Pay", Location = new Point(20 + i * 145, 40), Size = new Size(130, 44) };
                f.Controls.Add(btns[i]);
                f.Controls.Add(Caption(names[i], 20 + i * 145, 96));
            }
            btns[3].Enabled = false;
            f.Tag = (Action<Form>)(_ =>
            {
                SetField(btns[1], "_hover", true);
                SetField(btns[2], "_hover", true); SetField(btns[2], "_pressed", true);
                btns[4].Focus();
            });
            return f;
        }

        // 2 ─ radius, border, shadow, outline, icon-less variants
        private static Form ButtonsShapes()
        {
            var f = Canvas(760, 240);
            int x = 20;
            foreach (int r in new[] { 0, 4, 8, 16, 22 })
            {
                f.Controls.Add(new ArkaButton { Text = "r=" + r, BorderRadius = r, Location = new Point(x, 20), Size = new Size(130, 44) });
                x += 145;
            }
            var outline = new ArkaButton { Text = "Outline", FillColor = Color.White, ForeColor = Color.FromArgb(17, 24, 39), BorderThickness = 1, Location = new Point(20, 90), Size = new Size(130, 44) };
            var danger = new ArkaButton { Text = "Void", FillColor = Color.FromArgb(220, 38, 38), Location = new Point(165, 90), Size = new Size(130, 44) };
            var thick = new ArkaButton { Text = "Border 3", FillColor = Color.White, ForeColor = Color.FromArgb(37, 99, 235), BorderColor = Color.FromArgb(37, 99, 235), BorderThickness = 3, Location = new Point(310, 90), Size = new Size(130, 44) };
            var shadow = new ArkaButton { Text = "Shadow", Location = new Point(455, 80), Size = new Size(150, 64) };
            shadow.ShadowDecoration.Enabled = true; shadow.ShadowDecoration.Depth = 8;
            var tall = new ArkaButton { Text = "Tall pill", BorderRadius = 30, Location = new Point(620, 70), Size = new Size(120, 64) };
            var small = new ArkaButton { Text = "S", Location = new Point(20, 170), Size = new Size(28, 28), Font = new Font("Segoe UI", 8F, FontStyle.Bold), BorderRadius = 6 };
            var wide = new ArkaButton { Text = "A very long button caption that must be trimmed with an ellipsis", Location = new Point(70, 165), Size = new Size(300, 40) };
            f.Controls.AddRange(new Control[] { outline, danger, thick, shadow, tall, small, wide });
            return f;
        }

        // 3 ─ panels: nested, shadow, accent stripe, on different parents
        private static Form Panels()
        {
            var f = Canvas(760, 330);
            var a = new ArkaPanel { Location = new Point(20, 20), Size = new Size(220, 130), BorderThickness = 1 };
            a.Controls.Add(new ArkaLabel { Text = "border 1", Location = new Point(14, 12) });

            var b = new ArkaPanel { Location = new Point(260, 20), Size = new Size(220, 130) };
            b.ShadowDecoration.Enabled = true; b.ShadowDecoration.Depth = 10;
            b.Controls.Add(new ArkaLabel { Text = "shadow depth 10", Location = new Point(24, 22) });

            var c = new ArkaPanel { Location = new Point(500, 20), Size = new Size(240, 130), CustomBorderThickness = 5, CustomBorderSides = ArkaBorderSides.Left };
            c.Controls.Add(new ArkaLabel { Text = "accent stripe (left)", Location = new Point(20, 12) });

            // nested: rounded panel inside rounded panel inside tinted panel
            var outer = new ArkaPanel { Location = new Point(20, 170), Size = new Size(460, 140), FillColor = Color.FromArgb(219, 234, 254), BorderRadius = 16 };
            var inner = new ArkaPanel { Location = new Point(20, 20), Size = new Size(200, 100), BorderThickness = 1 };
            var inner2 = new ArkaPanel { Location = new Point(240, 20), Size = new Size(200, 100), FillColor = Color.FromArgb(37, 99, 235), BorderRadius = 20 };
            inner2.Controls.Add(new ArkaLabel { Text = "nested radius 20", ForeColor = Color.White, Location = new Point(20, 40) });
            outer.Controls.AddRange(new Control[] { inner, inner2 });

            // radius extremes
            var r0 = new ArkaPanel { Location = new Point(500, 170), Size = new Size(110, 60), BorderRadius = 0, BorderThickness = 1 };
            var r30 = new ArkaPanel { Location = new Point(630, 170), Size = new Size(110, 60), BorderRadius = 30, BorderThickness = 1 };
            var thick = new ArkaPanel { Location = new Point(500, 250), Size = new Size(240, 60), BorderThickness = 4, BorderColor = Color.FromArgb(37, 99, 235) };
            f.Controls.AddRange(new Control[] { a, b, c, outer, r0, r30, thick });
            return f;
        }

        // 4 ─ text boxes
        private static Form TextBoxes()
        {
            var f = Canvas(760, 330);
            var tb = new ArkaTextBox[8];
            tb[0] = new ArkaTextBox { PlaceholderText = "Placeholder (empty)" };
            tb[1] = new ArkaTextBox { Text = "Filled text" };
            tb[2] = new ArkaTextBox { Text = "Focused", PlaceholderText = "x" };
            tb[3] = new ArkaTextBox { Text = "Disabled", Enabled = false };
            tb[4] = new ArkaTextBox { Text = "secret", UseSystemPasswordChar = true };
            tb[5] = new ArkaTextBox { Text = "No border radius", BorderRadius = 0 };
            tb[6] = new ArkaTextBox { Text = "Pill radius 18", BorderRadius = 18, Height = 40 };
            tb[7] = new ArkaTextBox { Text = "Line one\r\nLine two\r\nLine three", Multiline = true, Height = 90 };
            string[] cap = { "placeholder", "text", "focused (blue border)", "disabled", "password", "radius 0", "radius 18", "multiline" };
            for (int i = 0; i < 7; i++)
            {
                int col = i % 2, row = i / 2;
                tb[i].Location = new Point(20 + col * 370, 30 + row * 70);
                tb[i].Size = new Size(340, tb[i].Height);
                f.Controls.Add(tb[i]); f.Controls.Add(Caption(cap[i], tb[i].Left, tb[i].Top - 16));
            }
            tb[7].Location = new Point(390, 240); tb[7].Size = new Size(340, 80);
            f.Controls.Add(tb[7]); f.Controls.Add(Caption(cap[7], 390, 224));
            f.Tag = (Action<Form>)(_ => tb[2].Focus());
            return f;
        }

        // 5 ─ combo boxes
        private static Form ComboBoxes()
        {
            var f = Canvas(760, 200);
            ArkaComboBox Make(int x, int y, string caption, Action<ArkaComboBox>? cfg = null)
            {
                var c = new ArkaComboBox { Location = new Point(x, y), Size = new Size(220, 34) };
                c.Items.AddRange(new object[] { "Cash", "Card", "UPI", "Wallet", "Split" });
                cfg?.Invoke(c);
                f.Controls.Add(c); f.Controls.Add(Caption(caption, x, y - 16));
                return c;
            }
            Make(20, 40, "no selection");
            Make(260, 40, "selected", c => c.SelectedIndex = 2);
            var foc = Make(500, 40, "focused", c => c.SelectedIndex = 1);
            Make(20, 110, "disabled", c => { c.SelectedIndex = 0; c.Enabled = false; });
            Make(260, 110, "radius 0", c => { c.BorderRadius = 0; c.SelectedIndex = 3; });
            Make(500, 110, "radius 17", c => { c.BorderRadius = 17; c.SelectedIndex = 4; });
            f.Tag = (Action<Form>)(_ => foc.Focus());
            return f;
        }

        // 6 ─ toggle, separators, labels
        private static Form Misc()
        {
            var f = Canvas(760, 250);
            var on = new ArkaToggleSwitch { Animated = false, Checked = true, Location = new Point(20, 30) };
            var off = new ArkaToggleSwitch { Animated = false, Location = new Point(100, 30) };
            var dis = new ArkaToggleSwitch { Animated = false, Checked = true, Enabled = false, Location = new Point(180, 30) };
            var big = new ArkaToggleSwitch { Animated = false, Checked = true, Location = new Point(260, 24), Size = new Size(70, 34) };
            f.Controls.AddRange(new Control[] { on, off, dis, big, Caption("on", 20, 60), Caption("off", 100, 60), Caption("disabled", 180, 60), Caption("70x34", 260, 66) });

            f.Controls.Add(new ArkaSeparator { Location = new Point(20, 95), Width = 700 });
            f.Controls.Add(new ArkaSeparator { Location = new Point(400, 20), Orientation = Orientation.Vertical, Height = 60 });

            f.Controls.Add(new ArkaLabel { Text = "ArkaLabel transparent on page", Location = new Point(20, 110), Font = new Font("Segoe UI", 11F) });
            f.Controls.Add(new ArkaHtmlLabel { Text = "<b>Bold</b> <i>italic</i> <u>underline</u> normal", Location = new Point(20, 140), Font = new Font("Segoe UI", 11F) });
            f.Controls.Add(new ArkaHtmlLabel { Text = "Line one<br/><b>Line two</b><br>Line three", Location = new Point(20, 170), Font = new Font("Segoe UI", 10F) });
            var right = new ArkaHtmlLabel { AutoSize = false, Location = new Point(400, 110), Size = new Size(320, 26), Text = "<div align='right'>₹ 1,24,500.00</div>", Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            var center = new ArkaHtmlLabel { AutoSize = false, Location = new Point(400, 140), Size = new Size(320, 26), Text = "<div align='center'><b>Centered</b> total</div>", Font = new Font("Segoe UI", 11F) };
            f.Controls.AddRange(new Control[] { right, center });
            return f;
        }

        // 7 ─ styled vs stock grid
        private static Form Grid()
        {
            var f = Canvas(760, 250);
            DataGridView Make(int x, bool styled)
            {
                var g = new DataGridView { Location = new Point(x, 30), Size = new Size(350, 190), AllowUserToAddRows = false, ReadOnly = true };
                g.Columns.Add("i", "Item"); g.Columns.Add("q", "Qty"); g.Columns.Add("t", "Total");
                g.Rows.Add("Tea 250g", 2, "₹ 240.00"); g.Rows.Add("Cookies 200g", 1, "₹ 30.00");
                g.Rows.Add("Oil 1L", 1, "₹ 165.00"); g.Rows.Add("Salt 1kg", 3, "₹ 66.00");
                foreach (DataGridViewColumn c in g.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (styled) ArkaGridStyle.Apply(g);
                return g;
            }
            f.Controls.Add(Make(20, false)); f.Controls.Add(Make(390, true));
            f.Controls.Add(Caption("stock DataGridView", 20, 12)); f.Controls.Add(Caption("ArkaGridStyle.Apply", 390, 12));
            return f;
        }

        // 8 ─ everything on a dark parent (corners must show the dark colour)
        private static Form DarkParent()
        {
            var dark = Color.FromArgb(17, 24, 39);
            var f = Canvas(760, 250, dark);
            var p = new ArkaPanel { Location = new Point(20, 20), Size = new Size(720, 210), FillColor = Color.FromArgb(31, 41, 55), BorderRadius = 18 };
            p.Controls.Add(new ArkaButton { Text = "Pay", Location = new Point(20, 20), Size = new Size(120, 44) });
            p.Controls.Add(new ArkaButton { Text = "Cancel", FillColor = Color.FromArgb(55, 65, 81), Location = new Point(155, 20), Size = new Size(120, 44) });
            p.Controls.Add(new ArkaTextBox { Text = "Dark text box", FillColor = Color.FromArgb(17, 24, 39), ForeColor = Color.White, BorderColor = Color.FromArgb(75, 85, 99), Location = new Point(20, 90), Size = new Size(255, 40) });
            var combo = new ArkaComboBox { Location = new Point(300, 96), Size = new Size(200, 34), FillColor = Color.FromArgb(17, 24, 39), ForeColor = Color.White, BorderColor = Color.FromArgb(75, 85, 99) };
            combo.Items.Add("Cash"); combo.SelectedIndex = 0;
            p.Controls.Add(combo);
            p.Controls.Add(new ArkaToggleSwitch { Animated = false, Checked = true, Location = new Point(300, 30) });
            p.Controls.Add(new ArkaLabel { Text = "Label on dark", ForeColor = Color.White, Location = new Point(20, 150) });
            p.Controls.Add(new ArkaHtmlLabel { Text = "<b>Markup</b> on dark", ForeColor = Color.White, Location = new Point(160, 150) });
            f.Controls.Add(p);
            return f;
        }

        // 9 ─ a POS-like payment screen assembled from the controls (layout stress test)
        private static Form PosMock()
        {
            var f = Canvas(760, 470);
            var card = new ArkaPanel { Location = new Point(20, 20), Size = new Size(720, 430), BorderThickness = 1 };
            card.ShadowDecoration.Enabled = true; card.ShadowDecoration.Depth = 8;
            f.Controls.Add(card);

            card.Controls.Add(new ArkaHtmlLabel { Text = "<b>Payment</b>", Font = new Font("Segoe UI", 16F), Location = new Point(28, 24) });
            card.Controls.Add(new ArkaHtmlLabel { AutoSize = false, Size = new Size(300, 30), Location = new Point(392, 24), Font = new Font("Segoe UI", 16F, FontStyle.Bold), Text = "<div align='right'>₹ 1,248.00</div>" });
            card.Controls.Add(new ArkaSeparator { Location = new Point(28, 66), Width = 664 });

            string[] modes = { "Cash", "Card", "UPI", "Wallet", "Split" };
            for (int i = 0; i < modes.Length; i++)
            {
                var b = new ArkaButton
                {
                    Text = modes[i], Location = new Point(28 + i * 133, 84), Size = new Size(124, 48),
                    FillColor = i == 0 ? Color.FromArgb(37, 99, 235) : Color.White,
                    ForeColor = i == 0 ? Color.White : Color.FromArgb(17, 24, 39),
                    BorderThickness = i == 0 ? 0 : 1
                };
                if (i != 0) b.HoverState.FillColor = Color.FromArgb(243, 244, 246);
                card.Controls.Add(b);
            }

            card.Controls.Add(new ArkaLabel { Text = "Tendered", Location = new Point(28, 156), ForeColor = Color.FromArgb(107, 114, 128) });
            card.Controls.Add(new ArkaTextBox { Text = "1500.00", Location = new Point(28, 178), Size = new Size(320, 44), Font = new Font("Segoe UI", 14F), TextAlign = HorizontalAlignment.Right });
            card.Controls.Add(new ArkaLabel { Text = "Change due", Location = new Point(372, 156), ForeColor = Color.FromArgb(107, 114, 128) });
            card.Controls.Add(new ArkaHtmlLabel { Text = "<b>₹ 252.00</b>", Location = new Point(372, 184), Font = new Font("Segoe UI", 18F), ForeColor = Color.FromArgb(5, 150, 105) });

            int n = 1;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                {
                    card.Controls.Add(new ArkaButton
                    {
                        Text = (n++).ToString(), Location = new Point(28 + c * 68, 240 + r * 56), Size = new Size(60, 48),
                        FillColor = Color.FromArgb(243, 244, 246), ForeColor = Color.FromArgb(17, 24, 39), Font = new Font("Segoe UI", 13F, FontStyle.Bold)
                    });
                }
            card.Controls.Add(new ArkaButton { Text = "Confirm payment", Location = new Point(372, 300), Size = new Size(320, 60), FillColor = Color.FromArgb(5, 150, 105), Font = new Font("Segoe UI", 13F, FontStyle.Bold) });
            card.Controls.Add(new ArkaButton { Text = "Cancel", Location = new Point(372, 370), Size = new Size(320, 40), FillColor = Color.White, ForeColor = Color.FromArgb(220, 38, 38), BorderColor = Color.FromArgb(254, 202, 202), BorderThickness = 1 });
            card.Controls.Add(new ArkaToggleSwitch { Animated = false, Checked = true, Location = new Point(372, 246) });
            card.Controls.Add(new ArkaLabel { Text = "Print receipt", Location = new Point(428, 249) });
            return f;
        }
    }
}
