// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ArkaControls.Drawing
{
    /// <summary>Anti-aliased rounded-rectangle helpers shared by every control.</summary>
    public static class RoundedGraphics
    {
        /// <summary>Builds a rounded-rectangle path. The radius is clamped so it never exceeds half of the shorter side.</summary>
        public static GraphicsPath CreatePath(RectangleF rect, int radius)
        {
            var path = new GraphicsPath();
            float r = Math.Max(0, Math.Min(radius, (int)Math.Floor(Math.Min(rect.Width, rect.Height) / 2f)));
            if (r <= 0.5f || rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = r * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Rectangle to stroke so a border of the given thickness stays fully inside the control bounds
        /// (GDI+ centres the pen on the path and the last pixel column/row is exclusive).
        /// </summary>
        public static RectangleF BorderRect(Rectangle client, float thickness)
        {
            float inset = thickness / 2f;
            return new RectangleF(client.X + inset, client.Y + inset,
                Math.Max(0f, client.Width - thickness - 1f), Math.Max(0f, client.Height - thickness - 1f));
        }

        /// <summary>Fills and strokes a rounded rectangle with anti-aliasing, restoring graphics state afterwards.</summary>
        public static void FillAndBorder(Graphics g, Rectangle bounds, int radius, Color fill, Color border, float borderThickness)
        {
            var saved = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = BorderRect(bounds, Math.Max(0f, borderThickness));
            using (var path = CreatePath(rect, radius))
            {
                if (fill.A > 0)
                    using (var brush = new SolidBrush(fill))
                        g.FillPath(brush, path);

                if (borderThickness > 0 && border.A > 0)
                    using (var pen = new Pen(border, borderThickness) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
            }
            g.SmoothingMode = saved;
        }

        /// <summary>Returns <paramref name="color"/> darkened (negative amount) or lightened (positive) by a fraction 0..1.</summary>
        public static Color Shade(Color color, float amount)
        {
            amount = Math.Max(-1f, Math.Min(1f, amount));
            float Mix(int c) => amount < 0 ? c * (1 + amount) : c + (255 - c) * amount;
            return Color.FromArgb(color.A, (int)Mix(color.R), (int)Mix(color.G), (int)Mix(color.B));
        }
    }
}

namespace ArkaControls.Drawing
{
    internal static class ShadowPainter
    {
        /// <summary>The area left for the control body once room for the shadow has been reserved.</summary>
        public static Rectangle ContentBounds(Rectangle client, ShadowStyle shadow)
        {
            if (!shadow.Enabled || shadow.Depth <= 0) return client;
            int d = Math.Min(shadow.Depth, Math.Min(client.Width, client.Height) / 4);
            return new Rectangle(client.X + d, client.Y + d / 2, client.Width - 2 * d, client.Height - 2 * d);
        }

        /// <summary>Paints a stack of translucent rounded rectangles around <paramref name="body"/>.</summary>
        public static void Paint(Graphics g, Rectangle body, int radius, ShadowStyle shadow)
        {
            if (!shadow.Enabled || shadow.Depth <= 0) return;
            var saved = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int layers = Math.Max(1, Math.Min(shadow.Depth, 30));
            for (int i = layers; i >= 1; i--)
            {
                // alpha falls off quadratically towards the outside
                float t = 1f - (float)i / (layers + 1);
                int alpha = Math.Max(0, Math.Min(255, (int)(shadow.Opacity * t * t / Math.Max(1, layers / 3f))));
                var r = new RectangleF(body.X - i, body.Y - i + layers / 3f, body.Width + 2 * i, body.Height + 2 * i);
                using (var path = RoundedGraphics.CreatePath(r, radius + i))
                using (var brush = new SolidBrush(Color.FromArgb(alpha, shadow.Color)))
                    g.FillPath(brush, path);
            }
            g.SmoothingMode = saved;
        }
    }
}
