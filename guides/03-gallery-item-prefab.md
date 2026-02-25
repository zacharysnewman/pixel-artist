# Guide 03 — Gallery Item Prefab (GalleryItemView)

> Update this guide if the thumbnail cell layout or selection overlay changes.

## What this covers

The prefab that `GalleryScreen` instantiates for each artwork in the grid.
It shows a pixel-art thumbnail and a checkmark overlay when selected.
No name label — purely visual.

---

## 1. Create the Prefab Root

1. **Project panel → right-click → Create → Prefab**, name it `GalleryItemPrefab`
   (or open a temporary scene to build the hierarchy and drag to Project)
2. The root `GameObject` should have:
   - `RectTransform` (size matches `GridLayoutGroup.cellSize`, e.g. 240 × 240)
   - `Image` component as background (optional, can be transparent)
   - `Button` component — the tap target for the whole cell
   - `GalleryItemView` script component

---

## 2. Thumbnail RawImage

1. Create a child `GameObject` named `Thumbnail`
   - `RectTransform`: stretch to fill parent (anchor preset stretch-stretch, offsets 0)
   - `RawImage` component
     - `Color`: white (texture tints)
     - Leave `Texture` empty — it's assigned at runtime by `GalleryItemView.BuildThumbnail()`
2. Wire `Thumbnail`'s `RawImage` → `GalleryItemView.thumbnail`

---

## 3. Selection Overlay

1. Create a child `GameObject` named `SelectedOverlay` (sibling of `Thumbnail`,
   above it in the hierarchy so it renders on top)
   - `RectTransform`: stretch to fill parent
   - `Image` component: semi-transparent blue or green tint (e.g. `#44AAFFAA`)
   - Add a child `Image` or `TMP_Text` to show a checkmark (✓) centered
2. Set `SelectedOverlay` **inactive** by default
3. Wire `SelectedOverlay` GameObject → `GalleryItemView.selectedOverlay`

---

## 4. Tap Button

The `Button` component is on the root GameObject (step 1).
Wire the root's `Button` component → `GalleryItemView.tapButton`

> `GalleryItemView.Init()` calls `tapButton.onClick.RemoveAllListeners()` then
> re-adds its own listener each time, so the inspector `OnClick` list should be
> left empty.

---

## 5. Final Inspector Wiring on GalleryItemView

```
thumbnail       → Thumbnail (RawImage)
selectedOverlay → SelectedOverlay (GameObject)
tapButton       → [root] (Button)
```

---

## 6. Prefab Hierarchy

```
GalleryItemPrefab  [RectTransform, Image, Button, GalleryItemView]
├── Thumbnail      [RectTransform, RawImage]
└── SelectedOverlay [RectTransform, Image]  ← inactive by default
    └── Checkmark   [Image or TMP_Text "✓"]
```
