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

Two-tab panel accessible from the canvas editor:

### Tab 1: Picker
- **HSB square selector** — 2D square where the horizontal axis is saturation (0–100%)
  and the vertical axis is brightness (0–100%); hue selected separately
- **Hue slider** — horizontal slider (or ring) to select hue (0–360°)
- **HEX input** — text field to enter/copy a 6-digit hex color (e.g. `#FF8800`);
  updates the square and slider when submitted

### Tab 2: Palettes
- Grid of named palettes, each previewed as a row of color swatches
- Tap a palette to expand it and show all its swatches
- Tap a swatch to set it as the active color and close the picker
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

The home screen showing all saved artworks as thumbnails.

### Normal Mode
- Thumbnails displayed in a grid
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
