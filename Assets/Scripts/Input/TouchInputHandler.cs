using UnityEngine;
using UnityEngine.EventSystems;
using PixelArtist.Canvas;
using PixelArtist.Tools;

namespace PixelArtist.Input
{
    /// <summary>
    /// Sits on the RawImage canvas quad. Translates touch/pointer events into
    /// pixel coordinates and dispatches them to the active ITool, or handles
    /// two-finger pan + pinch-to-zoom.
    ///
    /// Draw Mode:   1-finger → tool  |  2-finger → pan + zoom
    /// Pan Mode:    1-finger → pan   |  2-finger → pan + zoom
    /// </summary>
    public class TouchInputHandler : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("References")]
        [SerializeField] RectTransform canvasRect;   // The RawImage RectTransform
        [SerializeField] RectTransform canvasParent; // Parent that is panned/zoomed

        [Header("Zoom")]
        [SerializeField] float minZoom = 1f;
        [SerializeField] float maxZoom = 16f;

        // Set by CanvasScreen
        public PixelCanvas PixelCanvas { get; set; }
        public UndoSystem UndoSystem { get; set; }
        public ITool ActiveTool { get; set; }
        public Color32 ActiveColor { get; set; } = new Color32(0, 0, 0, 255);
        public bool PanMode { get; set; }

        public event System.Action OnCanvasChanged;

        // Internal state
        int _lastPixelX = -1, _lastPixelY = -1;
        bool _drawingThisStroke;

        // Two-finger tracking
        Touch _prevTouch0, _prevTouch1;
        bool _wasTwoFinger;

        // ── Unity Event Handlers ───────────────────────────────────────────────

        // On mobile we use Unity's Touch system in Update rather than
        // pointer events (which don't multi-touch well), but we implement
        // the pointer interfaces for Editor play-mode mouse support.

        void Update()
        {
            if (UnityEngine.Input.touchCount == 0) return;

            if (UnityEngine.Input.touchCount >= 2)
            {
                HandleTwoFinger();
                _wasTwoFinger = true;
            }
            else if (UnityEngine.Input.touchCount == 1 && !_wasTwoFinger)
            {
                HandleOneFinger(UnityEngine.Input.GetTouch(0));
            }
            else if (UnityEngine.Input.touchCount == 0)
            {
                _wasTwoFinger = false;
            }
        }

        // ── One-Finger ─────────────────────────────────────────────────────────

        void HandleOneFinger(Touch touch)
        {
            if (PanMode)
            {
                if (touch.phase == TouchPhase.Moved)
                    Pan(touch.deltaPosition);
                return;
            }

            // Draw mode — dispatch to tool
            if (touch.phase == TouchPhase.Began)
            {
                if (!TryGetPixelCoords(touch.position, out int px, out int py)) return;
                _lastPixelX = px; _lastPixelY = py;
                _drawingThisStroke = true;
                Color32 color = ActiveColor;
                ActiveTool?.OnPointerDown(px, py, PixelCanvas, UndoSystem, ref color);
                ActiveColor = color;
                OnCanvasChanged?.Invoke();
            }
            else if (touch.phase == TouchPhase.Moved && _drawingThisStroke)
            {
                if (!TryGetPixelCoords(touch.position, out int px, out int py)) return;
                if (px == _lastPixelX && py == _lastPixelY) return; // same pixel, skip
                _lastPixelX = px; _lastPixelY = py;
                Color32 color = ActiveColor;
                ActiveTool?.OnPointerDrag(px, py, PixelCanvas, UndoSystem, ref color);
                ActiveColor = color;
                OnCanvasChanged?.Invoke();
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && _drawingThisStroke)
            {
                _drawingThisStroke = false;
                TryGetPixelCoords(touch.position, out int px, out int py);
                Color32 color = ActiveColor;
                ActiveTool?.OnPointerUp(px, py, PixelCanvas, UndoSystem, ref color);
                ActiveColor = color;
            }
        }

        // ── Two-Finger (pan + zoom) ────────────────────────────────────────────

        void HandleTwoFinger()
        {
            Touch t0 = UnityEngine.Input.GetTouch(0);
            Touch t1 = UnityEngine.Input.GetTouch(1);

            if (!_wasTwoFinger)
            {
                // Just entered two-finger — cancel any active draw stroke
                _drawingThisStroke = false;
                _prevTouch0 = t0;
                _prevTouch1 = t1;
                return;
            }

            // Pinch zoom
            float prevDist = Vector2.Distance(_prevTouch0.position, _prevTouch1.position);
            float currDist = Vector2.Distance(t0.position, t1.position);
            if (prevDist > 0)
            {
                float scaleDelta = currDist / prevDist;
                Vector3 scale = canvasParent.localScale * scaleDelta;
                float uniform = Mathf.Clamp(scale.x, minZoom, maxZoom);
                canvasParent.localScale = new Vector3(uniform, uniform, 1f);
            }

            // Pan — move by average delta of both fingers
            Vector2 prevMid = (_prevTouch0.position + _prevTouch1.position) * 0.5f;
            Vector2 currMid = (t0.position + t1.position) * 0.5f;
            Pan(currMid - prevMid);

            _prevTouch0 = t0;
            _prevTouch1 = t1;
        }

        void Pan(Vector2 delta)
        {
            canvasParent.anchoredPosition += delta;
            ClampCanvasPosition();
        }

        void ClampCanvasPosition()
        {
            // Keep at least half the canvas visible on screen
            Vector2 pos = canvasParent.anchoredPosition;
            float halfW = canvasParent.rect.width  * canvasParent.localScale.x * 0.5f;
            float halfH = canvasParent.rect.height * canvasParent.localScale.y * 0.5f;

            Vector2 screenHalf = new Vector2(Screen.width, Screen.height) * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -halfW, screenHalf.x * 2 - halfW);
            pos.y = Mathf.Clamp(pos.y, -halfH, screenHalf.y * 2 - halfH);
            canvasParent.anchoredPosition = pos;
        }

        // ── Editor Mouse Support (implements pointer interfaces) ───────────────

        public void OnPointerDown(PointerEventData e)
        {
            if (UnityEngine.Input.touchCount > 0) return; // mobile handles it
            if (!TryGetPixelCoords(e.position, out int px, out int py)) return;
            _lastPixelX = px; _lastPixelY = py;
            _drawingThisStroke = true;
            Color32 color = ActiveColor;
            ActiveTool?.OnPointerDown(px, py, PixelCanvas, UndoSystem, ref color);
            ActiveColor = color;
            OnCanvasChanged?.Invoke();
        }

        public void OnDrag(PointerEventData e)
        {
            if (UnityEngine.Input.touchCount > 0) return;
            if (!_drawingThisStroke) return;
            if (!TryGetPixelCoords(e.position, out int px, out int py)) return;
            if (px == _lastPixelX && py == _lastPixelY) return;
            _lastPixelX = px; _lastPixelY = py;
            Color32 color = ActiveColor;
            ActiveTool?.OnPointerDrag(px, py, PixelCanvas, UndoSystem, ref color);
            ActiveColor = color;
            OnCanvasChanged?.Invoke();
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (UnityEngine.Input.touchCount > 0) return;
            if (!_drawingThisStroke) return;
            _drawingThisStroke = false;
            TryGetPixelCoords(e.position, out int px, out int py);
            Color32 color = ActiveColor;
            ActiveTool?.OnPointerUp(px, py, PixelCanvas, UndoSystem, ref color);
            ActiveColor = color;
        }

        // ── Coordinate Mapping ─────────────────────────────────────────────────

        bool TryGetPixelCoords(Vector2 screenPos, out int px, out int py)
        {
            px = py = 0;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPos, null, out Vector2 local))
                return false;

            // local is in [-width/2, width/2] x [-height/2, height/2]
            Rect r = canvasRect.rect;
            float u = (local.x - r.xMin) / r.width;
            float v = (local.y - r.yMin) / r.height;

            if (u < 0 || u > 1 || v < 0 || v > 1) return false;

            px = Mathf.FloorToInt(u * PixelCanvas.Size);
            py = Mathf.FloorToInt(v * PixelCanvas.Size);
            px = Mathf.Clamp(px, 0, PixelCanvas.Size - 1);
            py = Mathf.Clamp(py, 0, PixelCanvas.Size - 1);
            return true;
        }
    }
}
