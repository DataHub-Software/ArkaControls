# Verification plan: replacing a commercial control suite with ArkaControls

Goal: prove, on Windows, that swapping the controls changes **nothing the user or the data can notice** before the old dependency is removed. The rule throughout: **side by side, never in place.** The old build stays buildable, installable and shippable until every exit criterion below is met.

## 0. Ground rules

1. **UI-only change.** Do not ship it together with schema migrations, business-logic fixes or new features. Then any difference in behaviour is attributable to the controls.
2. **Two branches.** `master` keeps the old controls. `migrate/arka` holds the swap. Never merge until section 9 is signed off.
3. **Coexistence.** The two libraries have different namespaces, so one assembly can reference both. This allows a form-by-form rollout (section 8) instead of one big-bang change.
4. **Baseline first.** Capture evidence from the old build *before* touching any form (section 2). You cannot compare against something you did not record.
5. **Rollback is a redeploy** of the previous installer. Because the database schema is unchanged, no data migration is needed to go back.

## 1. Test environments

| Machine | Why |
|---|---|
| Developer PC, 1920x1080, 100% DPI | Reference rendering |
| Same PC at 125% and 150% DPI | Scaling bugs show up here first |
| Low-end counter PC (4 GB RAM, integrated GPU, Windows 10) | Paint performance, GDI handle limits |
| 1366x768 touch monitor (the counter target) | Real layout, touch targets |
| Second display for the customer screen | Multi-monitor and DPI mismatch |
| Windows 11 and Windows 10 | Theme and text rendering differences |
| RDP session | Remote sessions disable some rendering paths |

Record the OS build, DPI, resolution and GPU with every result.

## 2. Baseline capture (old build, before any change)

Build two small tools on the migration branch. Neither changes app behaviour.

**a) Screenshot harness.** A test-only executable that constructs every form with fake session data, shows it, waits for layout, calls `DrawToBitmap` and saves `<Form>_<dpi>_<resolution>.png`. Include the states that matter: empty cart, cart with 5 lines, a scheme fired, payment dialog in each mode, disabled buttons, validation errors.

**b) Control-tree dump.** Walk every open form recursively and write JSON: control type, name, bounds, Enabled, Visible, TabIndex, Text, colours, font, and for the styled controls the fill/border/radius values. Two dumps (old vs new) can be diffed as text, which catches things eyes miss: a shifted control, a changed tab order, a lost `Enabled`.

Commit the baseline images and JSON to a separate `baseline/` artefact store, not the source tree.

## 3. Library tests (ArkaControls repo, on Windows)

Automated, run in CI on `windows-latest`:

- **Unit tests:** markup parser, colour maths, theme events (already present).
- **Designer round trip:** create each control, set every public property, serialise with the WinForms `CodeDomSerializer`, deserialise, and assert equality. A property that does not survive is a designer bug.
- **Handle leak test:** create and dispose 2,000 of each control in a loop and assert GDI objects and USER objects (`GetGuiResources`) return to baseline. Windows caps a process at 10,000 GDI objects, and a POS runs all day.
- **Disposal:** timers (toggle), fonts and brushes must not leak; verify with a weak reference after dispose and GC.
- **Thread safety of theme:** changing the theme while forms are open must not throw.

Manual, using `samples/ArkaControls.Demo`:

- Every control in every state at 100/125/150% DPI, on light and dark parents.
- Rounded corners show the *parent* colour, not a square of the wrong colour, on nested panels, on a gradient/picture parent and on a tab page.
- Hover, press, keyboard focus ring, disabled.
- Resize the window: nothing clips or flickers; dragging a panel border is smooth.
- Text: long text ellipsis, right-to-left text, non-Latin text, emoji, 8 pt to 20 pt fonts.

## 4. Designer verification (the part most likely to bite)

1. Open the demo form and two large real forms in Visual Studio's designer. They must render, not show "Object reference not set".
2. Drag each control from the toolbox onto a form; confirm Category "Arka" appears in the Properties window and expandable state nodes (`HoverState`, `ShadowDecoration`) edit and persist.
3. Edit, save, close, reopen: no properties reset. Check `git diff` of the `.Designer.cs`: only intended lines changed.
4. Change a colour to the default and confirm the line disappears from the designer file (the `ShouldSerialize`/`Reset` logic).
5. Copy and paste a control, undo/redo, and resize by handles.

## 5. Mechanical migration checks

On `migrate/arka`, migrate with a script, not by hand:

1. Script rename of types and `using` directives in `*.Designer.cs` and code-behind. Log every replacement.
2. **The build must pass with zero new warnings.** Every missing property is a compile error, which is exactly what you want.
3. Review the Designer diff: it should contain only type names, usings, and known differences from `migrating.md`. Any other changed line is a finding.
4. Search for leftovers: `grep -r "Guna"` must return nothing before removal, and each remaining hit must be explained.

## 6. Automated visual comparison

Run the screenshot harness on both builds, same machine, same DPI and resolution.

- Diff each image pair pixel by pixel with a small tolerance (anti-aliasing at rounded corners always differs by a few pixels).
- Fail a screen when more than a small percentage of pixels differ **or** when any connected difference region exceeds a size threshold (a moved or missing control shows as a solid block).
- Produce a heat-map PNG per failing screen; a person reviews only those.
- Also diff the control-tree JSON: any change in bounds, TabIndex, Enabled or Visible is a failure.

Expected, accepted differences: corner anti-aliasing, shadow softness, focus-ring style. Write each accepted difference down with a screenshot so the same question is not re-asked later.

## 7. Behavioural tests specific to this POS

These are the places a control swap can silently break a counter. Test each on both builds and compare.

**Text boxes**
- Barcode entry: a scanner types characters quickly; the 300 ms auto-submit debounce depends on `TextChanged` firing per character, and Enter/Tab must reach `KeyDown`. Scan 50 barcodes in a row, including ones with leading zeros, and confirm none is dropped, doubled or truncated.
- `Focus()` after showing a form, after a dialog closes, and after adding an item; the cashier must be able to scan immediately without clicking.
- Paste, select-all, `SelectedText`, caret position, `MaxLength`, `CharacterCasing`, password masking, numeric-only fields.
- Placeholder appears when empty and disappears on typing; is not included in `.Text`.
- Tab order through a full form is identical to baseline (from the tree diff).

**Buttons and keyboard shortcuts**
- F-key shortcuts, Enter as default button, Escape as cancel, Space to press when focused.
- Double-click and rapid tapping: a payment button must not fire twice.
- Disabled buttons do not respond and look disabled.
- Touch: finger tap works with no hover state stuck on afterwards.

**Combo boxes**
- Open, arrow keys, type-ahead, mouse wheel (should not change value when the list is closed and unfocused), selection event count, data binding, very long lists.

**Panels**
- Scrolling panels with many children (the cart list), `AutoScroll`, docking and anchoring on resize, z-order, clicks reaching child controls.

**Labels**
- The 13 markup labels render bold and right-aligned exactly as before; long amounts never clip; currency symbol glyph present in the font.

**Dialogs**
- Payment modal, supervisor PIN pad, variant pickers: open, use, cancel, reopen 100 times without growth in memory or handles.

## 8. Business-level equivalence (the strongest evidence)

Write one scripted scenario, executed by a person or a UI automation tool (for example FlaUI), and run it identically on old and new builds against a copy of the same starting database:

open shift; scan 10 items; change quantities; apply a percentage discount; trigger a scheme; pay by cash with change, by card, by UPI and split; print the receipt; void one bill; return part of another; close shift and day end.

Then compare **the databases**, not the screens: dump `bill_header`, `bill_line`, `payment_split`, `stock_movement`, `audit_log` (excluding ids and timestamps) from both runs and diff. They must be identical. Compare the printed receipt text byte for byte. This proves the control swap did not change behaviour even where the screen looks right.

Repeat the scenario on one migrated form at a time (see the staged rollout below) to localise any difference.

## 9. Staged rollout

Because both libraries can coexist, migrate in this order and run sections 5-8 after each step:

1. Reports, Settings and management screens (low risk, low frequency).
2. Dialogs (PIN pad, pickers, receipt preview).
3. Login and shift screens.
4. The billing screen and payment modal **last** (highest risk, highest frequency).

Ship each step to a test counter as its own build so a problem points at one screen.

## 10. Performance, endurance and resource checks

- **Paint time:** measure time to first paint of the billing screen and of a 100-line cart on the low-end PC; the new build must not be slower than the old by more than 10%.
- **Flicker:** record a screen capture while adding items and scrolling the cart; look for redraw flashes.
- **Soak test:** a script that adds items, pays, voids and returns in a loop for 8 hours. Track working set, GDI objects and USER objects every minute; they must be flat. A rising line is a leak.
- **Startup time** compared with baseline.

## 11. Pilot

Install the migrated build on one real counter in a test store for one to two weeks, next to a known-good old build on another counter. Cashiers report anything odd; you check logs and the sync monitor daily. Keep the old installer ready. A billing error, a lost barcode or a frozen dialog is an immediate rollback.

## 12. Exit criteria (all must be true)

- [ ] Library CI green; handle-leak and designer round-trip tests pass.
- [ ] Both large designer forms open and round-trip cleanly in Visual Studio.
- [ ] Migration build has zero new warnings; no `Guna` references remain outside the removed package line.
- [ ] Every screen passes the visual diff, or its differences are recorded and accepted.
- [ ] Control-tree diff shows no change in TabIndex, Enabled, Visible or bounds beyond accepted ones.
- [ ] Barcode test (50 scans x 3 sessions) shows no lost, doubled or truncated scan.
- [ ] Scripted business scenario produces identical database dumps and receipts on old and new builds.
- [ ] 8-hour soak shows flat memory and handle counts on the low-end PC.
- [ ] Pilot counter ran the required period with no billing-affecting defect.
- [ ] Rollback rehearsed once: installer swap restores the old build with data intact.

Only then remove the old package reference, delete the old library from the repository, and update the README. Keep the old installer archived for two releases.

## 13. What to record

For every failure: build, machine, DPI, the screen or control, a screenshot or recording, and the baseline it deviates from. Keep results in a table so the exit checklist is answerable at a glance.
