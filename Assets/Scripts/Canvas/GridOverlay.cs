using UnityEngine;
using UnityEngine.UI;

namespace PixelArtist.Canvas
{
    /// <summary>
    /// Renders a pixel grid over the canvas using a tiled RawImage texture.
    ///
    /// Grid density scales with zoom so grid lines never become sub-pixel:
    ///   interval = smallest power-of-2 such that (screen px per canvas px × interval) ≥ minScreenPx
    ///
    /// The grid line texture is a tiny repeating tile: a 1-pixel dark border
    /// on the left and bottom edges, transparent elsewhere.  The RawImage's
    /// uvRect is set to tile it (canvasSize / interval) times per axis.
    ///
    /// Scene setup:
    ///   - Place on a RawImage child of the canvas RectTransform, same size, in front (higher sibling index)
    ///   - Assign canvasParentRect (the RectTransform that gets scaled for zoom)
    ///   - Call Init() after the canvas is opened, then keep it alive
    ///
    /// Toggle visibility with the public Visible property (bound to the grid toggle button).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class GridOverlay : MonoBehaviour
    {
        [Tooltip("Minimum gap between grid lines in screen pixels before the density steps down.")]
        [SerializeField] float minScreenPxBetweenLines = 4f;

        [Tooltip("Color of the grid lines (semi-transparent dark works well on most art).")]
        [SerializeField] Color32 lineColor = new Color32(0, 0, 0, 80);

        // Set by CanvasScreen.Init()
        public RectTransform CanvasParentRect { get; set; }  // receives zoom via localScale
        public RectTransform CanvasRect       { get; set; }  // physical display size in UI units

        int _canvasSize;
        int _currentInterval = -1;          // cached so we skip rebuilds when nothing changed
        bool _visible = true;

        RawImage _rawImage;
        Texture2D _tileTex;                 // 2×2 grid cell tile

        // ── Public API ─────────────────────────────────────────────────────────

        public bool Visible
        {
            get => _visible;
            set { _visible = value; _rawImage.enabled = value && _canvasSize > 0; }
        }

        public void Init(int canvasSize, RectTransform canvasRect, RectTransform canvasParentRect)
        {
            _canvasSize       = canvasSize;
            CanvasRect        = canvasRect;
            CanvasParentRect  = canvasParentRect;
            _rawImage         = GetComponent<RawImage>();
            _currentInterval  = -1;

            BuildTileTexture();
            UpdateGrid();
        }

        // ── Unity ──────────────────────────────────────────────────────────────

        void Update()
        {
            if (_canvasSize <= 0 || !_visible) return;
            UpdateGrid();
        }

        void OnDestroy()
        {
            if (_tileTex != null) Destroy(_tileTex);
        }

        // ── Grid Update ────────────────────────────────────────────────────────

        void UpdateGrid()
        {
            int interval = ComputeInterval();
            if (interval == _currentInterval) return;

            _currentInterval = interval;

            // Number of grid cells per axis
            float cells = (float)_canvasSize / interval;
            _rawImage.uvRect = new Rect(0f, 0f, cells, cells);
        }

        /// <summary>
        /// Returns the smallest power-of-2 interval (in canvas pixels) such that
        /// the resulting screen-pixel gap between lines is ≥ minScreenPxBetweenLines.
        /// Capped at canvasSize (so there's always at least one grid line visible at
        /// the canvas border).
        /// </summary>
        int ComputeInterval()
        {
            if (CanvasRect == null || CanvasParentRect == null) return 1;

            // Width of the canvas in screen pixels at current zoom
            float uiWidth     = CanvasRect.rect.width;
            float zoom        = CanvasParentRect.localScale.x;
            float screenWidth = uiWidth * zoom;                    // approx screen pixels
            float pxPerCell   = screenWidth / _canvasSize;         // screen px per canvas pixel

            int interval = 1;
            while (pxPerCell * interval < minScreenPxBetweenLines && interval < _canvasSize)
                interval *= 2;

            return Mathf.Min(interval, _canvasSize);
        }

        // ── Tile Texture ───────────────────────────────────────────────────────

        /// <summary>
        /// Builds a 2×2 repeating tile with grid lines on the left (x=0) and
        /// bottom (y=0) edges.  The RawImage tiles this via uvRect.
        /// </summary>
        void BuildTileTexture()
        {
            if (_tileTex != null) Destroy(_tileTex);
            _tileTex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Repeat
            };

            Color32 clear = new Color32(0, 0, 0, 0);
            _tileTex.SetPixels32(new[]
            {
                lineColor, lineColor,   // y=0: bottom edge (grid line)
                lineColor, clear        // y=1: left edge only (grid line on left, clear on right)
            });
            _tileTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            _rawImage.texture  = _tileTex;
            _rawImage.material = null;  // default UI material (alpha blending)
        }
    }
}
