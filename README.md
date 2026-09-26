# ArkaControls

Flat, rounded-corner **Windows Forms controls** with full Visual Studio designer support. Apache-2.0 licensed, free for commercial use, no activation, no subscription.

| Control | Base | Highlights |
|---|---|---|
| `ArkaButton` | `Button` | radius, border, hover / pressed / disabled colours, image, shadow |
| `ArkaPanel` | `Panel` | radius, border, accent side stripe, shadow, hosts child controls |
| `ArkaTextBox` | composite over native `TextBox` | radius, focused border, placeholder, password, multiline |
| `ArkaComboBox` | `ComboBox` | rounded face + chevron, owner-drawn items, standard data binding |
| `ArkaLabel` | `Label` | transparent by default |
| `ArkaHtmlLabel` | `Control` | `<b> <i> <u> <br> <div align>` only — nothing else is ever interpreted |
| `ArkaToggleSwitch` | `Control` | animated thumb, keyboard support |
| `ArkaSeparator` | `Control` | horizontal / vertical rule |
| `ArkaGridStyle` | helper | one-line theme for a stock `DataGridView` |
| `ArkaTheme` | palette | app-wide defaults, `Changed` event |

Targets **.NET Framework 4.8** and **.NET 8 (Windows)**. No runtime dependencies.

## Use

```csharp
var pay = new ArkaButton { Text = "Pay", BorderRadius = 10 };
pay.HoverState.FillColor = Color.FromArgb(29, 78, 216);
pay.ShadowDecoration.Enabled = true;

ArkaGridStyle.Apply(myDataGridView);
```

Everything is also available in the toolbox and Properties window (Category **Arka**).

## Migrating from a commercial control suite

Property names deliberately follow the common convention (`FillColor`, `BorderRadius`, `BorderThickness`, `BorderColor`, `HoverState`, `DisabledState`, `FocusedState`, `ShadowDecoration`, `PlaceholderText`), so most forms migrate by renaming types. See [docs/migrating.md](docs/migrating.md).

## Status

`0.1.0` — first cut. Drawing code is verified visually with `samples/ArkaControls.Demo` on Windows; unit tests cover the logic that does not need a display (markup parser, colour maths, theme). Not yet done: live re-theming of existing controls, per-monitor DPI audit, high-contrast mode, Narrator pass. See issues.

## Attribution: please keep it

If you redistribute ArkaControls, or a work derived from it, you must keep the copyright headers, ship the [LICENSE](LICENSE) and [NOTICE](NOTICE) files, and state your changes (Apache-2.0 Section 4). Applications that merely **use** the NuGet package should list it in their open-source notices; `ArkaAbout.Notice` returns the line to show. The name "ArkaControls" and its logo are trademarks of DataHub Software: forks must use their own name and say they are a fork of https://github.com/DataHub-Software/ArkaControls (see [TRADEMARKS.md](TRADEMARKS.md)).

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) first: sign your commits (`git commit -s`), do not submit code derived from other UI libraries, and attach screenshots for visual changes. Project rules and how licence violations are handled are in [GOVERNANCE.md](GOVERNANCE.md). Security reports: [SECURITY.md](SECURITY.md).

## Independence

ArkaControls is an independent implementation written from scratch. It is not affiliated with, derived from, or endorsed by any commercial control vendor. Matching property names is for migration convenience only; no third-party code, assets or documentation were used.

## Build

```
dotnet build ArkaControls.sln
dotnet test tests/ArkaControls.Tests
```

Builds on any OS with the .NET SDK (Windows targeting enabled); running the demo and the designer requires Windows.

## Licence

[Apache-2.0](LICENSE) - Copyright (c) 2026 DataHub Software
