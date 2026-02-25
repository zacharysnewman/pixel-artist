# Guide 06 — Touch Input Handler

> Update this guide if zoom limits, pan bounds, or gesture behavior change.

## What this covers

`TouchInputHandler` — the MonoBehaviour that translates raw Unity touch events into:
- Single-finger draw (tool invocation on `PixelCanvas`)
- Two-finger pan (Draw Mode) or single-finger pan (Pan Mode)
- Pinch-to-zoom centered on finger midpoint

---

## 1. Placement

Add `TouchInputHandler` as a component on `CanvasContainer` (the panned/zoomed
parent RectTransform). Keeping it here means it automatically receives input in
the correct screen region.

Alternatively it can live on `CanvasEditorScreen` — either works as long as the
serialized fields below are wired correctly.

---

## 2. Inspector Fields

| Field | Value | Notes |
|---|---|---|
| `canvasRect` | `CanvasImage` RectTransform | Used to convert screen → canvas pixel coordinates |
| `canvasParent` | `CanvasContainer` RectTransform | This transform is scaled/translated for zoom & pan |
| `minZoom` | `1` | Minimum local scale (1 = fit-to-container, no zoom out further) |
| `maxZoom` | `16` | Maximum local scale (16× pixel zoom) |

---

## 3. Runtime Properties (set by CanvasScreen.Open)

These are **not** inspector fields — `CanvasScreen` sets them in `Open()`:

| Property | Set from |
|---|---|
| `PixelCanvas` | the loaded `PixelCanvas` instance |
| `UndoSystem` | the `UndoSystem` for the open artwork |
| `ActiveTool` | the currently selected `ITool` |
| `ActiveColor` | current drawing `Color32` |
| `PanMode` | toggled by Draw/Pan mode buttons |

---

## 4. Gesture Behavior Summary

| Gesture | Draw Mode | Pan Mode |
|---|---|---|
| 1-finger tap/drag | Invoke active tool on touched pixel | Pan canvas |
| 2-finger drag | Pan canvas | Pan canvas |
| Pinch (2 fingers) | Zoom (centered on midpoint) | Zoom (centered on midpoint) |

Zoom is clamped to `[minZoom, maxZoom]` and applied as `canvasParent.localScale`.
Pan is clamped so the canvas cannot be dragged fully off screen.

---

## 5. No Additional Setup Needed

`TouchInputHandler` has no other dependencies. Once the two RectTransform fields are
wired and `CanvasScreen` sets the runtime properties, it is fully functional.
