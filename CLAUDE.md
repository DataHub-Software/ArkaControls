# ArkaControls — guidance for Claude Code sessions

Flat, rounded-corner Windows Forms controls, **Apache-2.0**, owner **DataHub Software**, public at https://github.com/DataHub-Software/ArkaControls. Built to replace the paid GunaUI2 suite in the SnapptrixPOS product. **This file is the shared memory for every session on every machine. Keep it current; do not rely on chat history or per-machine `~/.claude` memory.**

## Layout and commands

```
src/ArkaControls/                 the library (net48 + net8.0-windows)
tests/ArkaControls.Tests/         xUnit, pure logic only (no display needed)
samples/ArkaControls.Demo/        visual smoke test app (Windows)
tools/ArkaControls.Screenshots/   renders every control state to PNG (Windows, interactive desktop)
docs/                             verification-plan.md, migrating.md
```

```
dotnet build ArkaControls.sln
dotnet test tests/ArkaControls.Tests
dotnet run --project tools/ArkaControls.Screenshots -c Release -- artifacts/screenshots   # Windows only
```

The library builds on macOS/Linux; **rendering, the designer and the demo can only be verified on Windows.** Never claim a control "looks right" unless a Windows screenshot was reviewed.

## Non-negotiable rules

1. **Clean room.** Never copy, port, decompile or derive from Guna UI, Siticone, Bunifu, ReaLTaiizor, Krypton or any other UI library or product, and do not ask for or paste their code. Matching common property names (`FillColor`, `BorderRadius`, `HoverState`...) for migration convenience is allowed; implementations must be written from scratch.
2. Every `.cs` file starts with `// Copyright (c) 2026 DataHub Software` and `// SPDX-License-Identifier: Apache-2.0`.
3. Every commit is signed off (`git commit -s`); the DCO check enforces it. `NOTICE`, `LICENSE` and copyright headers must never be removed.
4. `main` is branch-protected (PR, one code-owner approval, no force-push). Work on a branch and open a PR. One writer per branch at a time; pull before starting, push before stopping.
5. No new package dependencies without an issue. Nullable enabled, zero warnings, both target frameworks.

## Design notes (things that are easy to break)

- Rounded corners need the parent's pixels behind them: controls use `ParentBackground.Paint` (reflection over `InvokePaintBackground`/`InvokePaint`) with `SupportsTransparentBackColor`.
- Shadows are painted **inside** the control bounds (`ShadowPainter`); a shadowed control loses `Depth` px of body per side.
- `ArkaTextBox` wraps a native `TextBox`; placeholder uses the Windows cue banner (`EM_SETCUEBANNER`), so it does not work for multiline yet. Keep `TextChanged`, `KeyDown`, `KeyPress`, `Focus()` and `SelectedText` behaviour intact: the POS barcode entry depends on them.
- `ArkaComboBox` paints its closed face over the native control on `WM_PAINT` (`DropDownList` only); `DrawToBitmap` cannot capture it, which is why the screenshot tool captures from the screen.
- Designer support: `[Category("Arka")]`, `DefaultValue`/`ShouldSerialize*`/`Reset*`, and `DesignerSerializationVisibility.Content` on state sub-objects. Any new public property needs these.

## Current state (update when it changes)

- Built: button, panel, text box, combo box, label, HTML-subset label, toggle switch, separator, `ArkaGridStyle`, `ArkaTheme`, `ArkaAbout`. 16 unit tests pass (pure logic).
- Open PRs: licence/governance (#1), screenshot tool. GitHub **Actions is disabled** on the repo (owner decision pending), so CI and DCO workflows do not run yet.
- **Not yet done:** first Windows run of the demo and screenshot tool; designer round-trip check in Visual Studio; DPI (125/150%) and high-contrast audit; live re-theming of existing controls; report viewer on MigraDoc/PDFsharp 6; charts via LiveCharts2 in the POS dashboard; NuGet publishing (not before the demo looks right).
- **SnapptrixPOS must not be changed until this library is complete and `docs/verification-plan.md` has passed.** The POS uses only 7 Guna controls; the migration is a scripted type rename, form by form, billing screen last.

## Windows verification loop

The Windows PC is the only place rendering can be checked.

```
git fetch && git checkout <branch> && git pull
dotnet run --project tools/ArkaControls.Screenshots -c Release -- artifacts\screenshots
```

Run at 100%, 125% and 150% display scaling. Put the PNGs and `report.txt` on a `verify/<date>` branch (folder `verification/<date>/`) or attach them to the PR — never on `main`. The Mac session reads them and fixes drawing code. Also open `samples/ArkaControls.Demo` and two large real forms in the Visual Studio designer (see `docs/verification-plan.md` §4).
