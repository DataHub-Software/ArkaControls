// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ArkaControls
{
    /// <summary>A <see cref="Label"/> that is transparent by default, for use on rounded panels.</summary>
    [ToolboxItem(true)]
    public class ArkaLabel : Label
    {
        public ArkaLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            AutoSize = true;
            UseMnemonic = false;
        }
    }
}
