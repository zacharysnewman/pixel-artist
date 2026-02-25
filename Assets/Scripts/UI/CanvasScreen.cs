using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelArtist.Canvas;
using PixelArtist.Data;
using PixelArtist.Gallery;
using PixelArtist.Input;
using PixelArtist.Tools;

namespace PixelArtist.UI
{
    /// <summary>
    /// Canvas editor screen. Owns the PixelCanvas, UndoSystem, active tool, and
    /// active color.  Handles the "back to gallery" save logic:
    ///
    ///   New canvas + no pixels drawn  →  silently discard
    ///   New canvas + pixels drawn     →  auto-save (no dialog)
    ///   Existing + no changes         →  navigate back silently
    ///   Existing + unsaved changes    →  show Save / Discard dialog
    /// </summary>
    public class CanvasScreen : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] CanvasRenderer canvasRenderer;
        [SerializeField] TouchInputHandler touchInput;

        [Header("Toolbar — Tools")]
        [SerializeField] Button pencilButton;
        [SerializeField] Button eraserButton;
        [SerializeField] Button fillButton;
        [SerializeField] Button eyedropperButton;

        [Header("Toolbar — Undo / Redo")]
        [SerializeField] Button undoButton;
        [SerializeField] Button redoButton;

        [Header("Toolbar — Draw / Pan Mode")]
        [SerializeField] Button drawModeButton;
        [SerializeField] Button panModeButton;

        [Header("Toolbar — Color & Navigation")]
        [SerializeField] Button colorChipButton;   // tapping opens the color picker panel
        [SerializeField] Image colorChipImage;     // shows the active color
        [SerializeField] Button backButton;

        [Header("Color Picker Panel")]
        [SerializeField] ColorPickerPanel colorPickerPanel;

        [Header("Save / Discard Dialog")]
        [SerializeField] GameObject saveDiscardDialog;
        [SerializeField] Button saveButton;
        [SerializeField] Button discardButton;

        // Set by AppController before showing this screen
        public System.Action OnNavigateBack;

        // ── State ──────────────────────────────────────────────────────────────

        PixelCanvas _canvas;
        UndoSystem _undo;

        ArtworkData _artwork;         // the data we opened (may be "new" / not yet persisted)
        bool _isNewArtwork;           // true if artwork has never been saved to GalleryManager
        Color32[] _savedSnapshot;     // pixel state at open time (for "Discard")
        Color32 _activeColor = new Color32(0, 0, 0, 255);

        ITool _activeTool;
        ITool _prevTool;             // restored after eyedropper pick

        PencilTool _pencilTool;
        EraserTool _eraserTool;
        FillTool _fillTool;
        EyedropperTool _eyedropperTool;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        void Awake()
        {
            _pencilTool      = new PencilTool();
            _eraserTool      = new EraserTool();
            _fillTool        = new FillTool();
            _eyedropperTool  = new EyedropperTool();

            _eyedropperTool.OnColorSampled += OnEyedropperSampled;
        }

        void Start()
        {
            pencilButton.onClick.AddListener(() => SetTool(_pencilTool));
            eraserButton.onClick.AddListener(() => SetTool(_eraserTool));
            fillButton.onClick.AddListener(() => SetTool(_fillTool));
            eyedropperButton.onClick.AddListener(() => SetTool(_eyedropperTool, saveAsPrev: true));
            undoButton.onClick.AddListener(DoUndo);
            redoButton.onClick.AddListener(DoRedo);
            drawModeButton.onClick.AddListener(() => SetPanMode(false));
            panModeButton.onClick.AddListener(() => SetPanMode(true));
            colorChipButton.onClick.AddListener(OpenColorPicker);
            backButton.onClick.AddListener(OnBackPressed);
            saveButton.onClick.AddListener(OnSaveDialog);
            discardButton.onClick.AddListener(OnDiscardDialog);

            colorPickerPanel.OnColorChanged += OnColorPickerChanged;
        }

        void Update()
        {
            // Android hardware back button
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                OnBackPressed();

            // Refresh renderer each frame if canvas has been mutated
            if (_canvas != null && _canvas.IsDirty)
            {
                canvasRenderer.Refresh();
                _canvas.ClearDirty();
                RefreshUndoButtons();
            }
        }

        // ── Public: Open ───────────────────────────────────────────────────────

        /// <summary>Called by AppController to open an artwork for editing.</summary>
        public void Open(ArtworkData artwork, bool isNew)
        {
            _artwork = artwork;
            _isNewArtwork = isNew;

            _canvas = new PixelCanvas(artwork.size, artwork.DecodePixels());
            _savedSnapshot = _canvas.Snapshot();

            _undo = new UndoSystem();
            canvasRenderer.Init(_canvas);

            touchInput.PixelCanvas = _canvas;
            touchInput.UndoSystem = _undo;
            touchInput.ActiveColor = _activeColor;
            touchInput.OnCanvasChanged += () => { /* dirty flag already set */ };

            SetTool(_pencilTool);
            SetPanMode(false);
            RefreshColorChip();
            RefreshUndoButtons();
            saveDiscardDialog.SetActive(false);
        }

        // ── Back / Save Logic ──────────────────────────────────────────────────

        void OnBackPressed()
        {
            bool hasDirtyPixels = !SnapshotsEqual(_canvas.Snapshot(), _savedSnapshot);

            if (_isNewArtwork)
            {
                if (!_canvas.HasAnyPixels())
                {
                    // Empty new canvas — silently discard
                    NavigateBack();
                    return;
                }
                // New canvas with pixels — auto-save
                SaveAndBack();
            }
            else
            {
                if (!hasDirtyPixels)
                {
                    // Existing, no changes — go back silently
                    NavigateBack();
                    return;
                }
                // Existing with unsaved changes — prompt
                saveDiscardDialog.SetActive(true);
            }
        }

        void OnSaveDialog()
        {
            saveDiscardDialog.SetActive(false);
            SaveAndBack();
        }

        void OnDiscardDialog()
        {
            saveDiscardDialog.SetActive(false);
            NavigateBack();
        }

        void SaveAndBack()
        {
            _artwork.SetPixels(_canvas.Snapshot());
            GalleryManager.Instance.Save(_artwork);
            NavigateBack();
        }

        void NavigateBack()
        {
            _canvas = null;
            OnNavigateBack?.Invoke();
        }

        // ── Tool Management ────────────────────────────────────────────────────

        void SetTool(ITool tool, bool saveAsPrev = false)
        {
            if (saveAsPrev) _prevTool = _activeTool;
            _activeTool = tool;
            touchInput.ActiveTool = tool;
            // TODO: highlight the active tool button
        }

        void OnEyedropperSampled(Color32 color)
        {
            _activeColor = color;
            touchInput.ActiveColor = color;
            RefreshColorChip();
            // Switch back to the previous tool
            if (_prevTool != null) SetTool(_prevTool);
        }

        // ── Color ──────────────────────────────────────────────────────────────

        void OpenColorPicker()
        {
            colorPickerPanel.gameObject.SetActive(true);
            colorPickerPanel.SetColor(_activeColor);
        }

        void OnColorPickerChanged(Color32 color)
        {
            _activeColor = color;
            touchInput.ActiveColor = color;
            RefreshColorChip();
        }

        void RefreshColorChip()
        {
            colorChipImage.color = new Color32(
                _activeColor.r, _activeColor.g, _activeColor.b, 255);
        }

        // ── Undo / Redo ────────────────────────────────────────────────────────

        void DoUndo()
        {
            _undo.Undo(_canvas);
            canvasRenderer.Refresh();
            RefreshUndoButtons();
        }

        void DoRedo()
        {
            _undo.Redo(_canvas);
            canvasRenderer.Refresh();
            RefreshUndoButtons();
        }

        void RefreshUndoButtons()
        {
            undoButton.interactable = _undo != null && _undo.CanUndo;
            redoButton.interactable = _undo != null && _undo.CanRedo;
        }

        // ── Draw / Pan Mode ────────────────────────────────────────────────────

        void SetPanMode(bool pan)
        {
            touchInput.PanMode = pan;
            drawModeButton.interactable = pan;   // dim if already in draw mode
            panModeButton.interactable  = !pan;  // dim if already in pan mode
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        static bool SnapshotsEqual(Color32[] a, Color32[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i].r != b[i].r || a[i].g != b[i].g || a[i].b != b[i].b || a[i].a != b[i].a)
                    return false;
            return true;
        }
    }
}
