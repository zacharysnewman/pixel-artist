# Guide 07 — Color Picker Panel (ColorPickerPanel)

> Update this guide whenever the panel layout, tab structure, dismiss behavior,
> or palette list changes.

## What this covers

The full-screen `ColorPickerPanel`: two-tab layout (HSB Picker + Palettes),
the HSB square texture, hue slider, hex input, Done/X dismiss buttons, and
the `PaletteGallery` sub-component.

---

## 1. Root Panel

1. Create a child of `CanvasEditorScreen` named `ColorPickerPanel`
2. `RectTransform` — **full screen**:
   - `anchorMin`: (0, 0)
   - `anchorMax`: (1, 1)
   - All offsets (left/right/top/bottom): **0**
3. Add an `Image` component as the background (opaque or near-opaque dark color
   so it fully covers the canvas behind it)
4. Add component: `PixelArtist.UI.ColorPickerPanel`
5. Set **inactive** by default — `CanvasScreen.OpenColorPicker()` activates it

---

## 2. Tab Bar

Create a child `GameObject` named `TabBar` with a `HorizontalLayoutGroup`,
anchored to the top of the panel.

| Name | Label | Slot |
|---|---|---|
| `PickerTabButton` | "Picker" | `pickerTabButton` |
| `PalettesTabButton` | "Palettes" | `palettesTabButton` |

---

## 3. Close (X) Button

1. Create a `Button` named `CloseButton` anchored to a corner (e.g. top-right)
   - Label: "✕" or an X icon
2. Wire → `ColorPickerPanel.closeButton`

---

## 4. Picker Tab Content

Create a child `GameObject` named `PickerTab` that fills the remaining panel area
below the tab bar.

### 4a. HSB Square

1. Create **UI → Raw Image**, name it `HSBSquare`
   - Square aspect ratio (e.g. fill the panel width, equal height)
   - `RawImage.texture`: leave empty — generated at runtime (256×256 RGBA32)
   - `raycastTarget`: **true** — PointerDragReceiver is added at runtime by code
2. Wire `HSBSquare`'s `RawImage` → `ColorPickerPanel.hsbSquareImage`

3. Create a child `Image` or `RectTransform` named `HSBCursor`
   - Small circle or crosshair indicator (e.g. 20×20 Image with a circle sprite)
   - Position is set at runtime via `anchoredPosition`
4. Wire `HSBCursor`'s `RectTransform` → `ColorPickerPanel.hsbCursor`

> **Note**: `ColorPickerPanel.Start()` dynamically adds a `PointerDragReceiver`
> component to `HSBSquare` — do not add one manually.

### 4b. Hue Slider

1. Create **UI → Slider**, name it `HueSlider`
   - Direction: Left to Right
   - Min: 0, Max: 1, Whole Numbers: off
   - The slider fill/background area: add a child `RawImage` named `HueSliderBG`
     - `raycastTarget`: false — purely decorative
     - Texture assigned at runtime (rainbow gradient)
2. Wire `HueSlider` → `ColorPickerPanel.hueSlider`
3. Wire `HueSliderBG`'s `RawImage` → `ColorPickerPanel.hueSliderImage`

### 4c. HEX Input

1. Create **UI → TMP → Input Field**, name it `HexInput`
   - Character limit: 6
   - Content type: Alphanumeric (or Standard — code handles validation)
   - Placeholder text: "RRGGBB"
2. Wire → `ColorPickerPanel.hexInput`

### 4d. Color Preview

1. Create **UI → Image**, name it `PreviewSwatch`
   - Fixed size (e.g. 60×60), square
   - Color: updated at runtime to show the current selection
2. Wire → `ColorPickerPanel.previewImage`

### 4e. Done Button

1. Create a `Button` named `DoneButton`
   - Label: "Done" or "Select"
   - Placed below the HEX input or alongside it
2. Wire → `ColorPickerPanel.doneButton`

Wire `PickerTab`'s `GameObject` → `ColorPickerPanel.pickerTab`

---

## 5. Palettes Tab Content

Create a child `GameObject` named `PalettesTab`.

### 5a. PaletteGallery

1. Inside `PalettesTab`, create a `ScrollView` (vertical scroll)
2. Inside its `Content`, create a `GameObject` named `PaletteList`
   - Add `VerticalLayoutGroup`
3. Add component `PixelArtist.UI.PaletteGallery` to `PalettesTab` (or a sub-object)
4. Wire `PaletteGallery`'s inspector fields:

   | Field | Value |
   |---|---|
   | `listContainer` | `PaletteList` Transform |
   | `paletteRowPrefab` | palette row prefab (see note below) |
   | `swatchPrefab` | swatch prefab (see note below) |

5. Wire `PaletteGallery` component → `ColorPickerPanel.paletteGallery`

> **Palette row prefab**: a horizontal layout containing a label and a row of swatch
> Images. Each swatch is the `swatchPrefab` — a small square Button with a solid
> color background.

Wire `PalettesTab`'s `GameObject` → `ColorPickerPanel.palettesTab`

---

## 6. Final Inspector Wiring Summary

```
Tabs
  pickerTabButton    → PickerTabButton
  palettesTabButton  → PalettesTabButton
  pickerTab          → PickerTab (GameObject)
  palettesTab        → PalettesTab (GameObject)

HSB Square
  hsbSquareImage     → HSBSquare (RawImage)
  hsbCursor          → HSBCursor (RectTransform)

Hue Slider
  hueSlider          → HueSlider (Slider)
  hueSliderImage     → HueSliderBG (RawImage)

HEX Input
  hexInput           → HexInput (TMP_InputField)

Preview
  previewImage       → PreviewSwatch (Image)

Close / Done
  closeButton        → CloseButton
  doneButton         → DoneButton

Palette Gallery
  paletteGallery     → PaletteGallery component
```

---

## 7. Dismiss Behavior (recap)

| Action | Result |
|---|---|
| Tap X (`closeButton`) | Panel hides, color unchanged since last live update |
| Tap Done (`doneButton`) | Panel hides, color already applied (live updates) |
| Tap palette swatch | Color applied, panel hides immediately |
| Submit valid HEX | Color applied, panel hides immediately |

Live updates: dragging the HSB square or moving the hue slider calls
`OnColorChanged` immediately, so the color chip in the toolbar updates in
real time while the panel is open.

---

## 8. Hierarchy at This Stage

```
ColorPickerPanel  [Image, ColorPickerPanel]  ← inactive by default
├── TabBar
│   ├── PickerTabButton
│   └── PalettesTabButton
├── CloseButton
├── PickerTab
│   ├── HSBSquare       [RawImage]  ← PointerDragReceiver added at runtime
│   │   └── HSBCursor   [RectTransform + Image]
│   ├── HueSlider       [Slider]
│   │   └── HueSliderBG [RawImage]
│   ├── HexInput        [TMP_InputField]
│   ├── PreviewSwatch   [Image]
│   └── DoneButton
└── PalettesTab
    └── ScrollView
        └── Viewport
            └── Content
                └── PaletteList  [VerticalLayoutGroup, PaletteGallery]
```
