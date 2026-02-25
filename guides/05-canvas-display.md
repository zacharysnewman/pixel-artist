# Guide 05 — Canvas Display Stack (Checkerboard, Canvas, Grid Overlay)

> Update this guide if the layer order, display component scripts, or grid/checker
> settings change.

## What this covers

The three-layer display stack that lives inside `CanvasContainer`:

| Layer (bottom → top) | Component | Purpose |
|---|---|---|
| `CheckerboardImage` | `CheckerboardBackground` | Transparent pixel preview (#CCC/#999 squares) |
| `CanvasImage` | `CanvasRenderer` | Live pixel data (Texture2D, FilterMode.Point) |
| `GridOverlayImage` | `GridOverlay` | Pixel grid lines (tiled, density-scaled) |

All three `RectTransform`s must be **identical** — same anchor, same size, same position.
The visual stacking is determined purely by sibling order in the hierarchy.

---

## 1. Shared RectTransform Setup

All three images live directly inside `CanvasContainer` and must share the same
layout. Use **stretch-stretch** anchors within `CanvasContainer` with all offsets 0,
or give them all the same explicit size (e.g. 800 × 800). The simplest approach:

- Set each `RectTransform` anchor preset to **stretch-stretch** (`anchorMin=(0,0)`,
  `anchorMax=(1,1)`, all offsets=0).
- `CanvasContainer` itself controls the visible size via its own `RectTransform`.

---

## 2. CheckerboardImage (layer 0 — bottom)

1. Inside `CanvasContainer`, create **UI → Raw Image**, name it `CheckerboardImage`
   - `RectTransform`: stretch to fill `CanvasContainer`
   - `RawImage.color`: white (1,1,1,1)
   - `RawImage.texture`: leave empty — generated at runtime
2. Add component: `PixelArtist.Canvas.CheckerboardBackground`
   - No inspector fields to wire — texture is created in `Init(canvasSize)`
3. Set sibling index **0** (bottom of CanvasContainer)

---

## 3. CanvasImage (layer 1 — middle)

1. Create **UI → Raw Image**, name it `CanvasImage`
   - `RectTransform`: stretch to fill `CanvasContainer`
   - `RawImage.color`: white (1,1,1,1)
   - `RawImage.texture`: leave empty — assigned by `CanvasRenderer.Init()`
   - **Important**: `RawImage` must use the **default UI material** (alpha-blending) so
     transparent canvas pixels let the checkerboard below show through
2. Add component: `PixelArtist.Canvas.CanvasRenderer`
   - No inspector fields — uses `Init(PixelCanvas)` called by `CanvasScreen`
3. Set sibling index **1** (above CheckerboardImage)

---

## 4. GridOverlayImage (layer 2 — top)

1. Create **UI → Raw Image**, name it `GridOverlayImage`
   - `RectTransform`: stretch to fill `CanvasContainer`
   - `RawImage.color`: white (1,1,1,1) — tint is controlled by the tile texture alpha
   - `RawImage.texture`: leave empty — 2×2 tile generated at runtime
   - `RawImage` must use the **default UI material** (alpha-blending)
   - `RawImage.raycastTarget`: **false** — the grid is purely visual, should not
     block touches from reaching `CanvasImage`
2. Add component: `PixelArtist.Canvas.GridOverlay`
   - Inspector fields (optional tweaks — defaults are fine):
     - `Min Screen Px Between Lines`: `4` — minimum screen-pixel gap before density halves
     - `Line Color`: `(0, 0, 0, 80)` — semi-transparent dark lines
3. Set sibling index **2** (top of stack)
4. Grid is **enabled by default** — `CanvasScreen.ToggleGrid()` toggles it off/on

---

## 5. Wire to CanvasScreen

Back in `CanvasScreen`'s inspector (guide 04), wire:

| Slot | Value |
|---|---|
| `canvasRenderer` | `CanvasImage` → `CanvasRenderer` component |
| `checkerboard` | `CheckerboardImage` → `CheckerboardBackground` component |
| `gridOverlay` | `GridOverlayImage` → `GridOverlay` component |
| `canvasRect` | `CanvasImage` → `RectTransform` |
| `canvasParentRect` | `CanvasContainer` → `RectTransform` |

---

## 6. Final Hierarchy

```
CanvasContainer  [RectTransform]
├── CheckerboardImage  [RectTransform, RawImage, CheckerboardBackground]  ← sibling 0
├── CanvasImage        [RectTransform, RawImage, CanvasRenderer]           ← sibling 1
└── GridOverlayImage   [RectTransform, RawImage, GridOverlay]              ← sibling 2
    raycastTarget = false
```

---

## 6. How It Works at Runtime

1. `CanvasScreen.Open(artwork, isNew)` calls:
   - `checkerboard.Init(artwork.size)` — generates the NxN checker texture
   - `canvasRenderer.Init(pixelCanvas)` — creates/updates the pixel Texture2D
   - `gridOverlay.Init(artwork.size, canvasRect, canvasParentRect)` — sets up the
     tiled grid texture and starts updating `uvRect` every frame based on zoom
2. `GridOverlay.Update()` runs every frame; it only rebuilds the `uvRect` when the
   computed interval changes (debounced by `_currentInterval` cache).
3. Toggling the grid: `CanvasScreen.ToggleGrid()` sets `gridOverlay.Visible`.
