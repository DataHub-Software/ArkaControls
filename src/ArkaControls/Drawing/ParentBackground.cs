// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ArkaControls.Drawing
{
    /// <summary>
    /// Paints the parent's background (and anything behind the control) so rounded corners show what is
    /// really underneath instead of a square of <see cref="Control.BackColor"/>.
    /// </summary>
    internal static class ParentBackground
    {
        private static readonly MethodInfo? InvokePaintBackground =
            typeof(Control).GetMethod("InvokePaintBackground", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo? InvokePaint =
            typeof(Control).GetMethod("InvokePaint", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Paint(Control control, PaintEventArgs e)
        {
            var parent = control.Parent;
            if (parent == null || InvokePaintBackground == null || InvokePaint == null)
            {
                e.Graphics.Clear(control.BackColor.A == 0 ? SystemColors.Control : control.BackColor);
                return;
            }

            var state = e.Graphics.Save();
            try
            {
                e.Graphics.TranslateTransform(-control.Left, -control.Top);
                var clip = new Rectangle(control.Location, control.Size);
                using (var pe = new PaintEventArgs(e.Graphics, clip))
                {
                    InvokePaintBackground.Invoke(parent, new object[] { parent, pe });
                    InvokePaint.Invoke(parent, new object[] { parent, pe });
                }
            }
            finally { e.Graphics.Restore(state); }
        }
    }
}
