# Contributing to ArkaControls

Contributions are welcome: bug reports, fixes, tests, documentation, visual polish, and (after discussion) new controls.

## Ground rules

1. **Discuss first for anything big.** Open an issue before adding a control, a public API, a dependency or a new target framework.
2. **Keep it small and focused.** One change per pull request.
3. **Clean-room rule (mandatory).** Never submit code, assets, icons, documentation or decompiled output taken from Guna UI, Siticone, Bunifu, Telerik, DevExpress, Syncfusion, ReaLTaiizor, Krypton or any other library or product, and never derive an implementation from one. Matching common property names (`FillColor`, `BorderRadius`) for migration convenience is fine; copying implementations is not. If you are unsure, say so in the pull request.
4. **You must own or have the right to submit what you contribute** (see DCO below).

## Sign your commits (DCO)

We use the [Developer Certificate of Origin](https://developercertificate.org/). Add a sign-off to every commit:

```
git commit -s -m "fix: clip focus ring on rounded buttons"
```

This appends `Signed-off-by: Your Name <you@example.com>`, certifying that you wrote the change or have the right to submit it under the project licence. Pull requests with unsigned commits are not merged. Under Apache-2.0 Section 5 your contribution is licensed to the project and to all users under the same licence; no separate agreement is needed.

## Build and test

```
dotnet build ArkaControls.sln
dotnet test tests/ArkaControls.Tests
```

Requires the .NET SDK. The library builds on any OS; running the demo, the screenshot tool and the designer needs Windows.

## Drawing and control changes

Anything that changes how a control looks or behaves must include:

- a **before / after screenshot** produced by `dotnet run --project tools/ArkaControls.Screenshots -c Release -- out` (attach the relevant PNGs);
- a test for any logic that does not need a display;
- a note about DPI (100%, 125%, 150%) and about the designer if you added or changed a public property (`[Category("Arka")]`, `ShouldSerialize`/`Reset`, `DefaultValue`).

## Code style

- C# latest, `Nullable` enabled, zero warnings; no new package dependencies without discussion.
- Public members have XML docs. Keep the surface small.
- Every source file starts with:

```csharp
// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0
```

## Pull request process

1. Fork, create a branch from `main`, make your change, sign off your commits.
2. Open a pull request using the template and complete the checklist.
3. A maintainer reviews (code owner approval is required). Address comments by pushing more commits.
4. A maintainer merges. Releases are tagged and published by maintainers only.

## Reporting bugs and security issues

Bugs: open an issue with the template. Security issues: see [SECURITY.md](SECURITY.md); do **not** file them publicly.

By participating you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).
