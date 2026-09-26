// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System.Drawing;
using System.Windows.Forms;

namespace ArkaControls.Theming
{
    /// <summary>Applies the Arka look to a stock <see cref="DataGridView"/> (flat, banded, no gridline noise).</summary>
    public static class ArkaGridStyle
    {
        public static void Apply(DataGridView grid) => Apply(grid, ArkaTheme.Current);

        public static void Apply(DataGridView grid, ArkaTheme theme)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = theme.Surface;
            grid.GridColor = theme.Border;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.AllowUserToResizeRows = false;
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 38;

            grid.ColumnHeadersDefaultCellStyle.BackColor = theme.SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = theme.Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = theme.SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = theme.Text;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            grid.DefaultCellStyle.BackColor = theme.Surface;
            grid.DefaultCellStyle.ForeColor = theme.Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            grid.DefaultCellStyle.SelectionForeColor = theme.Text;
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 251, 253);
        }
    }
}
