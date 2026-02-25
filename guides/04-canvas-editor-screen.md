# Guide 04 — Canvas Editor Screen (CanvasScreen)

> Update this guide whenever toolbar buttons, the canvas container hierarchy,
> or the save/discard dialog change.

## What this covers

The full canvas editor screen: the `CanvasScreen` MonoBehaviour, all toolbar
buttons, the canvas container (which holds the checkerboard, canvas RawImage,
and grid overlay — see guide 05), the `TouchInputHandler`, and the save/discard
dialog.

---

## 1. Root GameObject

1. Inside `UIRoot`, create **GameObject → Create Empty**, name it `CanvasEditorScreen`
2. `RectTransform` — stretch to fill parent (anchor preset: stretch-stretch, all offsets 0)
3. Add component: `PixelArtist.UI.CanvasScreen`
4. Set inactive by default — `AppController` enables it when opening an artwork

---

## 2. Toolbar

Create a child `GameObject` named `Toolbar` with a `HorizontalLayoutGroup`.
Anchor to the top of the screen (height ~80px, width stretch).

Create the following Button children:

| Name | Label | SerializeField slot |
|---|---|---|
| `BackButton` | "←" | `backButton` |
| `PencilButton` | "Pencil" | `pencilButton` |
| `EraserButton` | "Eraser" | `eraserButton` |
| `FillButton` | "Fill" | `fillButton` |
| `EyedropperButton` | "Picker" | `eyedropperButton` |
| `UndoButton` | "↩" | `undoButton` |
| `RedoButton` | "↪" | `redoButton` |
| `DrawModeButton` | "Draw" | `drawModeButton` |
| `PanModeButton` | "Pan" | `panModeButton` |
| `GridToggleButton` | "Grid" | `gridToggleButton` |
| `ColorChipButton` | *(colored square)* | `colorChipButton` |

For `ColorChipButton`, add a child `Image` (not the button's own graphic) that shows
the active color swatch. Wire it to `colorChipImage`.

---

## 3. Canvas Container

This is the panned/zoomed parent. The display stack lives inside it (guide 05).

1. Create a child `GameObject` named `CanvasContainer`
   - `RectTransform` — **do not stretch**; instead set a fixed square size
     (e.g. 800 × 800 for a generous base) centered in the remaining screen space below the toolbar
   - This is the object `TouchInputHandler` scales and translates for pan/zoom
2. Wire `CanvasContainer`'s `RectTransform` → `CanvasScreen.canvasParentRect`

---

## 4. Touch Input Handler

1. On `CanvasEditorScreen` (or a dedicated child), add component:
   `PixelArtist.Input.TouchInputHandler`
2. Wire its fields:

   | Field | Value |
   |---|---|
   | `canvasRect` | `CanvasImage` RectTransform (the pixel RawImage — see guide 05 step 2) |
   | `canvasParent` | `CanvasContainer` RectTransform |
   | `minZoom` | `1` |
   | `maxZoom` | `16` |

3. Wire `TouchInputHandler` component → `CanvasScreen.touchInput`

> `CanvasScreen.Open()` sets `touchInput.PixelCanvas`, `.UndoSystem`, `.ActiveColor`,
> and `.ActiveTool` at runtime — no inspector wiring needed for those.

---

## 5. Color Picker Panel

The full-screen panel lives as a child of `CanvasEditorScreen` so it overlays it.
See **guide 07** for full setup. Once built:
- Wire the `ColorPickerPanel` component → `CanvasScreen.colorPickerPanel`
- Set it **inactive** by default

---

## 6. Save / Discard Dialog

1. Create a child `GameObject` named `SaveDiscardDialog` — centered panel with background
2. Inside it:

   | Name | Label | Slot |
   |---|---|---|
   | `SaveButton` | "Save" | `saveButton` |
   | `DiscardButton` | "Discard" | `discardButton` |

3. Wire `SaveDiscardDialog` GameObject → `CanvasScreen.saveDiscardDialog`
4. Set **inactive** by default

---

## 7. Final Inspector Wiring Summary

```
Core
  canvasRenderer    → CanvasImage (CanvasRenderer component — see guide 05)
  checkerboard      → CheckerboardImage (CheckerboardBackground — see guide 05)
  gridOverlay       → GridOverlayImage (GridOverlay — see guide 05)
  canvasRect        → CanvasImage (RectTransform)
  canvasParentRect  → CanvasContainer (RectTransform)
  touchInput        → TouchInputHandler component

Toolbar — Tools
  pencilButton      → PencilButton
  eraserButton      → EraserButton
  fillButton        → FillButton
  eyedropperButton  → EyedropperButton

Toolbar — Undo/Redo
  undoButton        → UndoButton
  redoButton        → RedoButton

Toolbar — Mode
  drawModeButton    → DrawModeButton
  panModeButton     → PanModeButton

Toolbar — Grid
  gridToggleButton  → GridToggleButton

Toolbar — Color & Navigation
  colorChipButton   → ColorChipButton
  colorChipImage    → ColorChipButton/ColorSwatch (Image)
  backButton        → BackButton

Color Picker Panel
  colorPickerPanel  → ColorPickerPanel component (see guide 07)

Save / Discard Dialog
  saveDiscardDialog → SaveDiscardDialog (GameObject)
  saveButton        → SaveButton
  discardButton     → DiscardButton
```

---

## 8. Hierarchy at This Stage

```
CanvasEditorScreen  [CanvasScreen]  ← inactive by default
├── Toolbar
│   ├── BackButton
│   ├── PencilButton
│   ├── EraserButton
│   ├── FillButton
│   ├── EyedropperButton
│   ├── UndoButton
│   ├── RedoButton
│   ├── DrawModeButton
│   ├── PanModeButton
│   ├── GridToggleButton
│   └── ColorChipButton
│       └── ColorSwatch  [Image]
├── CanvasContainer  [RectTransform, TouchInputHandler]
│   └── (see guide 05 — display stack)
├── ColorPickerPanel  [ColorPickerPanel]  ← inactive by default (see guide 07)
└── SaveDiscardDialog                     ← inactive by default
    ├── SaveButton
    └── DiscardButton
```
