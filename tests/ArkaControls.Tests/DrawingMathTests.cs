using System.Drawing;
using ArkaControls.Drawing;
using ArkaControls.Theming;
using Xunit;

namespace ArkaControls.Tests
{
    public class DrawingMathTests
    {
        [Fact]
        public void Shade_darkens_and_lightens_and_keeps_alpha()
        {
            var c = Color.FromArgb(200, 100, 150, 200);
            var dark = RoundedGraphics.Shade(c, -0.5f);
            var light = RoundedGraphics.Shade(c, 0.5f);
            Assert.Equal(200, dark.A);
            Assert.Equal(50, dark.R);
            Assert.True(light.R > c.R && light.G > c.G && light.B > c.B);
            Assert.Equal(c, RoundedGraphics.Shade(c, 0f));
        }

        [Fact]
        public void Shade_clamps_out_of_range_amounts()
        {
            Assert.Equal(0, RoundedGraphics.Shade(Color.FromArgb(10, 20, 30), -5f).R);
            Assert.Equal(255, RoundedGraphics.Shade(Color.FromArgb(10, 20, 30), 5f).B);
        }

        [Fact]
        public void Border_rect_stays_inside_the_client_area()
        {
            var r = RoundedGraphics.BorderRect(new Rectangle(0, 0, 100, 40), 2);
            Assert.True(r.X >= 1f && r.Y >= 1f);
            Assert.True(r.Right + 1f <= 100f && r.Bottom + 1f <= 40f);
        }

        [Fact]
        public void Border_rect_never_has_negative_size()
        {
            var r = RoundedGraphics.BorderRect(new Rectangle(0, 0, 1, 1), 6);
            Assert.True(r.Width >= 0 && r.Height >= 0);
        }

        [Fact]
        public void Theme_change_raises_event_and_restores()
        {
            var original = ArkaTheme.Current;
            int raised = 0;
            System.EventHandler h = (s, e) => raised++;
            ArkaTheme.Changed += h;
            try
            {
                ArkaTheme.Current = new ArkaTheme { Radius = 12 };
                Assert.Equal(12, ArkaTheme.Current.Radius);
                Assert.Equal(1, raised);
            }
            finally { ArkaTheme.Changed -= h; ArkaTheme.Current = original; }
        }

        [Fact]
        public void State_style_empty_colours_fall_back_to_normal()
        {
            var normal = Color.Red;
            Assert.Equal(normal, StateStyle.Pick(Color.Empty, normal));
            Assert.Equal(Color.Blue, StateStyle.Pick(Color.Blue, normal));
        }
    }
}
