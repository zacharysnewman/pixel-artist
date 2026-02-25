# Pixel Artist — Project Spec

## Overview

A Unity mobile app (iOS & Android) for creating pixel art. The initial focus is a
canvas system with intuitive touch controls and a clean drawing experience.

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

## Tools (initial set)

- **Pencil** — single pixel draw on touch
- **Eraser** — set pixel to transparent
- **Fill** — flood fill from touched pixel

---

## Serialization

- Canvas saved as raw `Color32[]` bytes + canvas size
- Simple binary format or JSON for now; design should not preclude future cloud save

---

## Out of Scope (v1)

- Layers
- Animation / frames
- Custom palettes
- Export (PNG, etc.)
- Undo across sessions
