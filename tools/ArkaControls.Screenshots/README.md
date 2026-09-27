# ArkaControls.Screenshots

Renders every control in every visual state into PNG files for review and regression comparison.

```
dotnet run --project tools/ArkaControls.Screenshots -c Release -- artifacts/screenshots
```

Windows only, needs an interactive desktop. Output: `01-buttons-states.png` ... `09-pos-payment-mock.png` and `report.txt` (OS, screen size, DPI scale, capture method per scene).
Run it at 100%, 125% and 150% display scaling and compare against the previous run before merging any change to drawing code.
