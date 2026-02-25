# Pixel Artist — Claude Working Notes

## Living Documents

**These files must be kept up to date as the project evolves. Update them whenever
code, scene structure, or design decisions change.**

| File | Purpose |
|---|---|
| `spec.md` | Product spec — what the app does and why; source of truth for features and behavior |
| `guides/*.md` | Editor guides — step-by-step Unity editor instructions to implement each feature |

When you add a new feature or change an existing one:
1. Update `spec.md` to reflect the new/changed behavior
2. Update or add the relevant file in `guides/` so the editor steps stay accurate
3. If a guide covers a feature that no longer exists, delete or archive it

---

## Project Structure

```
Assets/Scripts/
  Canvas/         — PixelCanvas, CanvasRenderer, CheckerboardBackground, GridOverlay, UndoSystem
  Data/           — ArtworkData (serializable model), GalleryManifest
  Gallery/        — GalleryManager (singleton, persistence)
  Input/          — TouchInputHandler (draw, pan, zoom)
  Tools/          — ITool, PencilTool, EraserTool, FillTool, EyedropperTool
  UI/             — AppController, CanvasScreen, GalleryScreen, GalleryItemView,
                    ColorPickerPanel, PaletteGallery, PointerDragReceiver

guides/           — Unity editor implementation guides (one per feature)
spec.md           — Product specification
CLAUDE.md         — This file
```

---

## Key Architecture Notes

- **No MonoBehaviour tools** — `PencilTool`, `EraserTool`, etc. are plain C# classes;
  `CanvasScreen` owns them and passes the active one to `TouchInputHandler`.
- **CanvasScreen is the hub** — it wires every canvas-related component at runtime via
  `Open(ArtworkData, bool)`. No inspector cross-references between canvas sub-components.
- **AppController** manages screen switching by toggling `GalleryScreen` and `CanvasScreen`
  GameObjects active/inactive.
- **GalleryManager** is a persistent singleton (`DontDestroyOnLoad`). It loads/saves
  artworks to `Application.persistentDataPath` as individual JSON files plus a manifest.
- **Canvas display stack** (bottom to top, same RectTransform size):
  1. `CheckerboardBackground` (RawImage) — transparency pattern
  2. Canvas `RawImage` — live pixel data from `CanvasRenderer`
  3. `GridOverlay` (RawImage) — pixel grid lines
- **ColorPickerPanel** is full-screen (`anchorMin=(0,0)`, `anchorMax=(1,1)`, all offsets=0).
  It lives as a child of the CanvasScreen and is shown/hidden via `SetActive`.

---

## Guides Index

| Guide | Feature |
|---|---|
| `guides/01-app-setup.md` | Scene root, Canvas Scaler, AppController |
| `guides/02-gallery-screen.md` | GalleryScreen UI, sort bar, select mode, dialogs |
| `guides/03-gallery-item-prefab.md` | GalleryItemView prefab |
| `guides/04-canvas-editor-screen.md` | CanvasScreen, toolbar, canvas container wiring |
| `guides/05-canvas-display.md` | CheckerboardBackground + GridOverlay layer stack |
| `guides/06-touch-input.md` | TouchInputHandler |
| `guides/07-color-picker-panel.md` | Full-screen ColorPickerPanel, HSB tab, Palettes tab |
