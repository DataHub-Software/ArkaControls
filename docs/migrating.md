# Migrating to ArkaControls

## Type map

| From | To |
|---|---|
| `Guna2Button` | `ArkaButton` |
| `Guna2Panel` | `ArkaPanel` |
| `Guna2TextBox` | `ArkaTextBox` |
| `Guna2ComboBox` | `ArkaComboBox` |
| `Guna2HtmlLabel` | `ArkaHtmlLabel` (or `ArkaLabel` when the text has no markup) |
| `Guna2Separator` | `ArkaSeparator` |
| `Guna2ToggleSwitch` | `ArkaToggleSwitch` |
| `Guna2DataGridView` | stock `DataGridView` + `ArkaGridStyle.Apply(grid)` |

## Property map

Same name: `FillColor`, `BorderRadius`, `BorderThickness`, `BorderColor`, `HoverState.FillColor/ForeColor/BorderColor`, `PressedState.*`, `DisabledState.*`, `FocusedState.BorderColor`, `ShadowDecoration.Enabled/Color/Depth`, `PlaceholderText`, `PlaceholderForeColor`, `CustomBorderColor`, `CustomBorderThickness`, `CheckedState.FillColor/InnerColor`, `Animated`, `ImageOffset`.

Differences to check after renaming:

- `ArkaTextBox.DefaultText` exists as a code-only alias of `Text`; prefer `Text`.
- `ArkaPanel.CustomBorderThickness/Color` draws an accent stripe on the sides in `CustomBorderSides` (default Left).
- `ArkaHtmlLabel` supports only `<b> <i> <u> <br> <div align>`; `<font>`, links and images are shown as text.
- `ArkaComboBox` custom face applies to `DropDownStyle = DropDownList`; `DropDown` keeps the native edit box.
- `ShadowDecoration` is drawn inside the control bounds, so a shadowed control loses `Depth` pixels of body on each side; enlarge it accordingly.

## Mechanical steps

1. Add the `ArkaControls` package reference.
2. In `*.Designer.cs`: replace `Guna.UI2.WinForms.Guna2Xxx` with the mapped type; add `using ArkaControls;` where needed.
3. Build; fix the handful of differences above.
4. Open the two largest forms in the designer and compare against the old rendering.
5. Remove the old package reference.
