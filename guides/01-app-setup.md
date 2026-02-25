# Guide 01 — App Setup (Scene Root, Canvas Scaler, AppController)

> Update this guide whenever the screen list changes or AppController gains new
> responsibilities.

## What this covers

The root of the scene: the Unity Canvas that hosts all UI, the Canvas Scaler for
mobile scaling, the `GalleryManager` singleton, and the `AppController` that
switches between the Gallery and Canvas Editor screens.

---

## 1. Create the Scene Canvas

1. **GameObject → UI → Canvas**
   - Rename to `UIRoot`
   - **Render Mode**: `Screen Space - Overlay`
2. Select the `Canvas` component on `UIRoot`:
   - **Canvas Scaler** component (added automatically):
     - `UI Scale Mode`: `Scale With Screen Size`
     - `Reference Resolution`: `1080 × 1920` (portrait phone)
     - `Screen Match Mode`: `Match Width Or Height`, match = `0.5`
3. Add a **GraphicRaycaster** component (already there by default — leave it).
4. Add an **EventSystem** GameObject if Unity didn't create one automatically
   (`GameObject → UI → Event System`).

---

## 2. Create the GalleryManager

1. **GameObject → Create Empty**, name it `GalleryManager`
2. Place it at the **scene root** (not inside `UIRoot`) — it needs to survive
   `DontDestroyOnLoad`.
3. Add component: `PixelArtist.Gallery.GalleryManager`
4. No inspector fields to wire — it self-initializes.

---

## 3. Create the AppController

1. Inside `UIRoot`, create **GameObject → Create Empty**, name it `AppController`
2. Add component: `PixelArtist.UI.AppController`
3. Create two immediate children of `UIRoot` (the screens — detailed in their own guides):
   - `GalleryScreen` (see guide 02)
   - `CanvasEditorScreen` (see guide 04)
4. Wire the `AppController` inspector fields:

   | Field | Value |
   |---|---|
   | `Gallery Screen` | drag `GalleryScreen` GameObject |
   | `Canvas Screen` | drag `CanvasEditorScreen` GameObject |

5. `AppController` activates `GalleryScreen` on Start and toggles screens when
   `OnOpenArtwork` / `OnNavigateBack` are invoked. No further setup needed.

---

## 4. Scene Hierarchy at This Stage

```
Scene
├── EventSystem
├── GalleryManager          ← persistent singleton (DontDestroyOnLoad)
└── UIRoot  [Canvas, CanvasScaler, GraphicRaycaster]
    ├── AppController       ← PixelArtist.UI.AppController
    ├── GalleryScreen       ← (see guide 02)
    └── CanvasEditorScreen  ← (see guide 04)
```
