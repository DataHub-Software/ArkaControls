// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using ArkaControls;
using ArkaControls.Theming;

namespace ArkaControls.Demo
{
    /// <summary>
    /// Visual smoke test: every control, every state, on a tinted background, with a theme switch.
    /// Run on Windows and compare against the screenshots in docs/ when changing drawing code.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DemoForm());
        }
    }

    internal sealed class DemoForm : Form
    {
        public DemoForm()
        {
            Text = "ArkaControls demo";
            ClientSize = new Size(760, 560);
            BackColor = Color.FromArgb(243, 244, 246);
            StartPosition = FormStartPosition.CenterScreen;

            var card = new ArkaPanel { Location = new Point(20, 20), Size = new Size(720, 520), BorderThickness = 1 };
            card.ShadowDecoration.Enabled = true;
            card.CustomBorderThickness = 4;
            Controls.Add(card);

            card.Controls.Add(new ArkaHtmlLabel { Location = new Point(24, 22), Text = "<b>ArkaControls</b> demo &nbsp; <i>rounded, flat, Apache-2.0</i>", Font = new Font("Segoe UI", 14F) });

            var name = new ArkaTextBox { Location = new Point(24, 70), Size = new Size(320, 40), PlaceholderText = "Search customer…" };
            var pass = new ArkaTextBox { Location = new Point(360, 70), Size = new Size(320, 40), PlaceholderText = "PIN", UseSystemPasswordChar = true };
            var combo = new ArkaComboBox { Location = new Point(24, 126), Size = new Size(320, 34) };
            combo.Items.AddRange(new object[] { "Cash", "Card", "UPI", "Wallet" });
            combo.SelectedIndex = 0;
            var toggle = new ArkaToggleSwitch { Location = new Point(360, 132), Checked = true };
            card.Controls.AddRange(new Control[] { name, pass, combo, toggle });

            card.Controls.Add(new ArkaSeparator { Location = new Point(24, 178), Width = 656 });

            var primary = new ArkaButton { Text = "Pay", Location = new Point(24, 200) };
            var outline = new ArkaButton { Text = "Cancel", Location = new Point(160, 200), FillColor = Color.White, ForeColor = ArkaTheme.Current.Text, BorderThickness = 1 };
            outline.HoverState.FillColor = Color.FromArgb(243, 244, 246);
            var danger = new ArkaButton { Text = "Void", Location = new Point(296, 200), FillColor = ArkaTheme.Current.Danger };
            danger.HoverState.FillColor = Color.FromArgb(185, 28, 28);
            var disabled = new ArkaButton { Text = "Disabled", Location = new Point(432, 200), Enabled = false };
            var shadow = new ArkaButton { Text = "Shadow", Location = new Point(560, 194), Size = new Size(130, 52) };
            shadow.ShadowDecoration.Enabled = true;
            card.Controls.AddRange(new Control[] { primary, outline, danger, disabled, shadow });

            var grid = new DataGridView { Location = new Point(24, 270), Size = new Size(656, 190), AllowUserToAddRows = false };
            grid.Columns.Add("item", "Item"); grid.Columns.Add("qty", "Qty"); grid.Columns.Add("total", "Total");
            grid.Rows.Add("Tea 250g", 2, "₹ 240.00"); grid.Rows.Add("Cookies 200g", 1, "₹ 30.00"); grid.Rows.Add("Oil 1L", 1, "₹ 165.00");
            ArkaGridStyle.Apply(grid);
            card.Controls.Add(grid);

            var dark = new ArkaButton { Text = "Toggle theme", Location = new Point(24, 476), Size = new Size(160, 32) };
            bool isDark = false;
            dark.Click += (s, e) =>
            {
                isDark = !isDark;
                BackColor = isDark ? Color.FromArgb(17, 24, 39) : Color.FromArgb(243, 244, 246);
                card.FillColor = isDark ? Color.FromArgb(31, 41, 55) : Color.White;
            };
            card.Controls.Add(dark);
        }
    }
}
