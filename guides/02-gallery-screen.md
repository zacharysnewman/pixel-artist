# Guide 02 — Gallery Screen

> Update this guide whenever toolbar buttons, sort controls, dialogs, or the
> prefab reference change.

## What this covers

The `GalleryScreen` GameObject: toolbar with sort + action buttons, the thumbnail
grid, the new-canvas dimension picker panel, and the delete confirmation dialog.

---

## 1. Root GameObject

1. Inside `UIRoot`, create **GameObject → Create Empty**, name it `GalleryScreen`
2. Add a `RectTransform` — stretch to fill parent (anchor preset: stretch-stretch,
   all offsets 0)
3. Add component: `PixelArtist.UI.GalleryScreen`
4. Add a background `Image` component → set color to your app background color

---

## 2. Toolbar

Create a child `GameObject` named `Toolbar` with a horizontal `HorizontalLayoutGroup`.
Anchor it to the top of the screen (e.g. height 80–100px).

Inside `Toolbar`, create the following Button children in order:

| GameObject name | Button label | SerializeField slot |
|---|---|---|
| `SortByButton` | "Created" | `sortByButton` |
| `SortDirButton` | "↓ Newest" | `sortDirButton` |
| *(spacer)* | — | — |
| `NewArtworkButton` | "+" | `newArtworkButton` |
| `SelectButton` | "Select" | `selectButton` |
| `CancelSelectButton` | "Cancel" | `cancelSelectButton` |
| `DuplicateButton` | "Duplicate" | `duplicateButton` |
| `DeleteButton` | "Delete" | `deleteButton` |

For the label Text objects on sort buttons, also wire:
- `SortByButton` child TMP_Text → `sortByLabel`
- `SortDirButton` child TMP_Text → `sortDirLabel`

**Initial visibility** (handled in code, but set defaults in scene):
- `CancelSelectButton` → inactive
- `DuplicateButton` → inactive
- `DeleteButton` → inactive

---

## 3. Thumbnail Grid

1. Create a child `GameObject` named `ScrollView` — add `ScrollRect` component
   - Horizontal: off, Vertical: on
   - Drag its `Viewport` and `Content` as usual
2. Inside `Content`, create a child `GameObject` named `GridContainer`
   - Add `GridLayoutGroup` component:
     - `Cell Size`: e.g. `240 × 240`
     - `Spacing`: `16 × 16`
     - `Constraint`: `Fixed Column Count`, count = `3` (adjust for your layout)
     - `Start Corner`: `Upper Left`
3. Wire `GridContainer`'s `Transform` to `GalleryScreen.gridContainer`
4. Wire `galleryItemPrefab` → the GalleryItemView prefab (see guide 03)

---

## 4. New Canvas Picker Panel

1. Create a child `GameObject` named `NewCanvasPicker`
   - Add `Image` as background (semi-transparent overlay)
   - Stretch to fill screen or use a centered panel layout
2. Inside it, create three size buttons and a cancel button:

   | GameObject | Label | SerializeField slot |
   |---|---|---|
   | `Size8Button` | "8×8" | `size8Button` |
   | `Size16Button` | "16×16" | `size16Button` |
   | `Size32Button` | "32×32" | `size32Button` |
   | `CancelButton` | "Cancel" | `newCanvasCancelButton` |

3. Wire `NewCanvasPicker` GameObject → `GalleryScreen.newCanvasPicker`
4. Set `NewCanvasPicker` **inactive** by default in the Inspector

---

## 5. Delete Confirmation Dialog

1. Create a child `GameObject` named `DeleteConfirmDialog`
   - Center it on screen with a panel background
2. Inside it, create:

   | GameObject | Label | SerializeField slot |
   |---|---|---|
   | `MessageText` | *(dynamic)* | `deleteConfirmText` (TMP_Text) |
   | `YesButton` | "Delete" | `deleteConfirmYesButton` |
   | `NoButton` | "Cancel" | `deleteConfirmNoButton` |

3. Wire `DeleteConfirmDialog` GameObject → `GalleryScreen.deleteConfirmDialog`
4. Set `DeleteConfirmDialog` **inactive** by default

---

## 6. Final Inspector Wiring Summary

Open `GalleryScreen` component in the Inspector and fill every slot:

```
Toolbar — Sort
  sortByButton       → SortByButton
  sortByLabel        → SortByButton/Text (TMP_Text)
  sortDirButton      → SortDirButton
  sortDirLabel       → SortDirButton/Text (TMP_Text)

Toolbar — Actions
  newArtworkButton   → NewArtworkButton
  selectButton       → SelectButton
  cancelSelectButton → CancelSelectButton
  duplicateButton    → DuplicateButton
  deleteButton       → DeleteButton

Grid
  gridContainer      → GridContainer (Transform)
  galleryItemPrefab  → GalleryItemView prefab asset

New Canvas Panel
  newCanvasPicker    → NewCanvasPicker (GameObject)
  size8Button        → Size8Button
  size16Button       → Size16Button
  size32Button       → Size32Button
  newCanvasCancelButton → CancelButton

Delete Confirmation Dialog
  deleteConfirmDialog    → DeleteConfirmDialog (GameObject)
  deleteConfirmYesButton → YesButton
  deleteConfirmNoButton  → NoButton
  deleteConfirmText      → MessageText (TMP_Text)
```

---

## 7. Hierarchy at This Stage

```
GalleryScreen
├── Toolbar
│   ├── SortByButton
│   ├── SortDirButton
│   ├── NewArtworkButton
│   ├── SelectButton
│   ├── CancelSelectButton
│   ├── DuplicateButton
│   └── DeleteButton
├── ScrollView
│   └── Viewport
│       └── Content
│           └── GridContainer   ← gridContainer
├── NewCanvasPicker             ← inactive by default
│   ├── Size8Button
│   ├── Size16Button
│   ├── Size32Button
│   └── CancelButton
└── DeleteConfirmDialog         ← inactive by default
    ├── MessageText
    ├── YesButton
    └── NoButton
```
