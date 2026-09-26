using System.ComponentModel;
using System.Drawing;

namespace ArkaControls
{
    /// <summary>
    /// Colours applied while a control is in a given interaction state (hover, pressed, focused, disabled).
    /// <see cref="Color.Empty"/> means "keep the normal colour". Shown as an expandable node in the Properties window.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class StateStyle
    {
        [DefaultValue(typeof(Color), "")] public Color FillColor   { get; set; } = Color.Empty;
        [DefaultValue(typeof(Color), "")] public Color ForeColor   { get; set; } = Color.Empty;
        [DefaultValue(typeof(Color), "")] public Color BorderColor { get; set; } = Color.Empty;

        /// <summary>Returns <paramref name="normal"/> unless this state overrides it.</summary>
        internal static Color Pick(Color stateColor, Color normal) => stateColor.IsEmpty ? normal : stateColor;

        public override string ToString() =>
            FillColor.IsEmpty && ForeColor.IsEmpty && BorderColor.IsEmpty ? "(default)" : "(customised)";
    }

    /// <summary>Soft drop shadow drawn behind a control (drawn inside the control's own bounds).</summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShadowStyle
    {
        [DefaultValue(false)] public bool Enabled { get; set; }
        [DefaultValue(typeof(Color), "Black")] public Color Color { get; set; } = Color.Black;
        /// <summary>Blur spread in pixels, 0-30.</summary>
        [DefaultValue(5)] public int Depth { get; set; } = 5;
        /// <summary>Peak opacity 0-255.</summary>
        [DefaultValue(40)] public int Opacity { get; set; } = 40;

        public override string ToString() => Enabled ? "Enabled" : "Disabled";
    }
}
