// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;

namespace ArkaControls.Theming
{
    /// <summary>
    /// Application-wide default palette. Controls read these values when they are created
    /// (and again after <see cref="Changed"/> for controls that opt in through <c>ApplyTheme</c>),
    /// so a whole application can be restyled by changing one object.
    /// </summary>
    public sealed class ArkaTheme
    {
        public Color Primary      { get; set; } = Color.FromArgb(37, 99, 235);
        public Color PrimaryHover { get; set; } = Color.FromArgb(29, 78, 216);
        public Color Surface      { get; set; } = Color.White;
        public Color SurfaceAlt   { get; set; } = Color.FromArgb(248, 249, 250);
        public Color Border       { get; set; } = Color.FromArgb(229, 231, 235);
        public Color BorderFocus  { get; set; } = Color.FromArgb(37, 99, 235);
        public Color Text         { get; set; } = Color.FromArgb(17, 24, 39);
        public Color TextMuted    { get; set; } = Color.FromArgb(156, 163, 175);
        public Color Disabled     { get; set; } = Color.FromArgb(200, 205, 215);
        public Color Success      { get; set; } = Color.FromArgb(5, 150, 105);
        public Color Danger       { get; set; } = Color.FromArgb(220, 38, 38);
        public int   Radius       { get; set; } = 8;

        private static ArkaTheme _current = new ArkaTheme();

        /// <summary>The active theme. Assigning a new instance raises <see cref="Changed"/>.</summary>
        public static ArkaTheme Current
        {
            get => _current;
            set { _current = value ?? throw new ArgumentNullException(nameof(value)); Changed?.Invoke(null, EventArgs.Empty); }
        }

        /// <summary>Raised after <see cref="Current"/> is replaced or <see cref="NotifyChanged"/> is called.</summary>
        public static event EventHandler? Changed;

        /// <summary>Call after mutating properties of <see cref="Current"/>.</summary>
        public static void NotifyChanged() => Changed?.Invoke(null, EventArgs.Empty);
    }
}
