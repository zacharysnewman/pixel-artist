# Pixel Artist — Project Spec

## Overview

A Unity mobile app (iOS & Android) for creating pixel art. The initial focus is a
canvas system with intuitive touch controls, a clean drawing experience, a color
picker, and a gallery for managing saved artworks.

---

## Canvas

- Square dimensions only: **8×8**, **16×16**, **32×32**
- Internal representation: `Color32[]` flat array, row-major (`index = y * width + x`)
- Width/height derived from array length: `width = (int)Mathf.Sqrt(pixels.Length)`
- Rendered via `Texture2D` (RGBA32, no mipmaps, `FilterMode.Point`)
- Full texture flush on every change (`SetPixels32` + `Apply(false, false)`)

---

## Mobile Controls

### Drawing Mode (default)

| Gesture | Action |
|---|---|
| Single finger tap / drag | Draw with active tool |
| Two finger drag | Pan canvas |
| Pinch | Zoom canvas |

### Pan Mode (toggled via mode switch)

| Gesture | Action |
|---|---|
| Single finger drag | Pan canvas |
| Pinch | Zoom canvas |

- A visible **mode switch** UI button toggles between Draw Mode and Pan Mode
- Pan Mode is indicated clearly in the UI so the user knows their finger won't draw

### Zoom & Pan Behavior

- Pinch-to-zoom centered on the midpoint between the two fingers
- Zoom clamped to a sensible range (e.g. 1× – 16×)
- Canvas panned by dragging (two-finger in Draw Mode, one-finger in Pan Mode)
- Canvas stays within reasonable bounds — cannot be panned fully off screen

---

## Canvas Display

### Transparency Background
- Checkerboard pattern rendered behind the canvas (one checker square per canvas pixel)
- Colors: `#CCCCCC` (light) alternating with `#999999` (dark)
- Implemented as a separate RawImage behind the canvas RawImage; transparent canvas pixels
  let the checkerboard show through via normal alpha blending

### Pixel Grid
- Semi-transparent dark grid lines between every canvas pixel
- On by default; user can toggle off via a grid button in the toolbar
- **Density scaling**: grid interval is the smallest power-of-2 (in canvas pixels) such that
  the resulting gap between lines is ≥ 4 screen pixels — prevents rendering imperceptibly
  fine lines at low zoom
  - e.g. at 1× zoom on a 32×32 canvas shown at 300px: ~9px/pixel → interval = 1 (every pixel)
  - e.g. at 0.5× zoom same setup: ~4.7px/pixel → interval = 1; at even smaller zoom → interval = 2, 4, …
- Implemented as a tiled RawImage overlay using a 2×2 repeating grid-cell texture + uvRect

---

## Undo / Redo

- Full-snapshot undo (acceptable at max 32×32 = 4KB per snapshot)
- Stack capped at ~50 steps to bound memory usage
- Standard undo/redo buttons in the UI

---

## Tools

- **Pencil** — single pixel draw on touch
- **Eraser** — set pixel to transparent
- **Fill** — flood fill from touched pixel
- **Eyedropper** — tap a pixel to set the active color to that pixel's color

---

## Color Picker

Full-screen panel (anchored 0,0 → 1,1, offsets zero) that overlays the canvas editor.

**Dismissal rules:**
- **X button** (top corner) — close without changing anything
- **Done button** (Picker tab) — confirm current HSB/Hex selection and dismiss
- **Swatch tap** (Palettes tab) — immediately select that color and dismiss
- **HEX submit** — entering a valid hex and pressing Return selects that color and dismisses

The active color updates live while dragging the HSB square or hue slider so the
color chip in the toolbar always reflects the current state.

### Tab 1: Picker
- **HSB square selector** — 2D square where the horizontal axis is saturation (0–100%)
  and the vertical axis is brightness (0–100%); hue selected separately
- **Hue slider** — horizontal slider to select hue (0–360°)
- **HEX input** — 6-digit hex field; submitting a valid value selects the color and closes
- **Done button** — confirms the current selection and dismisses the panel

### Tab 2: Palettes
- Grid of named palettes, each previewed as a row of color swatches
- Tap a palette to expand it and show all its swatches
- Tap a swatch to set it as the active color and immediately dismiss the panel
- Built-in palettes shipped with the app (see below)

### Built-in Palettes

| Palette | Colors |
|---|---|
| Pico-8 | 16 colors |
| Gameboy | 4 colors |
| NES | 16-color subset |
| Endesga 32 | 32 colors |
| Grayscale | 8 shades |

---

## Gallery

The home screen. Artworks have no user-visible names — the gallery is purely
a tiled grid of thumbnails. Artworks are identified only by their thumbnail.

### Sort Controls
- **Sort-by button** — toggles between "Created" and "Modified" date
- **Direction button** — toggles "↓ Newest" (descending) and "↑ Oldest" (ascending)
- Default: sort by Created, descending (newest artwork at top-left)
- Sorting is presentation-only; GalleryManager stores artworks in insertion order

### Normal Mode
- Tiled grid of thumbnails, ordered by the active sort setting; no name labels
- Single tap on a thumbnail → opens the canvas editor for that artwork
- **"Select"** button (top-right) → enters Select Mode

### Select Mode
- Tap thumbnails to toggle selection (checkmark overlay)
- **"Duplicate"** button — copies selected artwork(s), appending " copy" to the name
- **"Delete"** button — deletes selected artwork(s) after a confirmation prompt
- **"Cancel"** button (replaces "Select") → exits Select Mode, clears selection

### New Canvas
- **"+"** button opens a dimension picker (8×8, 16×16, 32×32)
- Confirming creates a blank canvas and opens the editor

---

## Save Behavior

Triggered when the user taps the back / close button in the canvas editor:

| Situation | Behavior |
|---|---|
| New canvas, no pixels drawn | Silently discard — do not add to gallery |
| New canvas, pixels drawn | Auto-save to gallery (no confirmation) |
| Existing canvas, no changes | Navigate back silently |
| Existing canvas, unsaved changes | Show **"Save or Discard?"** dialog |

"New canvas" = an artwork that has never been saved (no persisted ID).
"Pixels drawn" = at least one non-transparent pixel exists.

### Save or Discard Dialog
- **Save** — persists changes, returns to gallery
- **Discard** — reverts to last saved state, returns to gallery
- *(No cancel — the dialog cannot be dismissed without choosing)*

---

## Serialization

- Artworks saved to `Application.persistentDataPath`
- One JSON file per artwork: `<guid>.json`
- Pixel data stored as base64-encoded raw `Color32` bytes (R, G, B, A per pixel)
- Artwork manifest file `gallery.json` keeps an ordered list of artwork IDs
- Design does not preclude future cloud save

### ArtworkData schema
```json
{
  "id": "string (GUID)",
  "name": "string",
  "size": 16,
  "pixelsBase64": "string",
  "createdAt": 1234567890,
  "modifiedAt": 1234567890
}
```

---

## Out of Scope (v1)

- Layers
- Animation / frames
- Export (PNG, etc.)
- Undo across sessions
- Custom user-created palettes
- Cloud save
