using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelArtist.UI
{
    /// <summary>
    /// Two-tab color picker panel.
    ///
    /// Tab 0 — Picker:
    ///   HSB square (saturation × brightness) + hue slider + HEX input field
    ///
    /// Tab 1 — Palettes:
    ///   PaletteGallery component handles rendering and swatch taps
    ///
    /// Raises OnColorChanged whenever the active color changes.
    /// </summary>
    public class ColorPickerPanel : MonoBehaviour
    {
        public event System.Action<Color32> OnColorChanged;

        [Header("Tabs")]
        [SerializeField] Button pickerTabButton;
        [SerializeField] Button palettesTabButton;
        [SerializeField] GameObject pickerTab;
        [SerializeField] GameObject palettesTab;

        [Header("HSB Square")]
        [SerializeField] RawImage hsbSquareImage;
        [SerializeField] RectTransform hsbCursor;

        [Header("Hue Slider")]
        [SerializeField] Slider hueSlider;        // 0–1 maps to 0–360°
        [SerializeField] RawImage hueSliderImage; // background showing hue gradient

        [Header("HEX Input")]
        [SerializeField] TMP_InputField hexInput;

        [Header("Preview")]
        [SerializeField] Image previewImage;

        [Header("Close")]
        [SerializeField] Button closeButton;

        [Header("Palette Gallery")]
        [SerializeField] PaletteGallery paletteGallery;

        // Current HSB values
        float _hue, _sat, _bri;
        bool _suppressCallbacks;

        // Textures (created at runtime)
        Texture2D _hsbTex;
        Texture2D _hueTex;

        const int HsbSize    = 256;
        const int HueTexSize = 256;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        void Awake()
        {
            _hsbTex = new Texture2D(HsbSize, HsbSize, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            hsbSquareImage.texture = _hsbTex;

            _hueTex = new Texture2D(HueTexSize, 1, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            BuildHueGradient();
            hueSliderImage.texture = _hueTex;
        }

        void Start()
        {
            pickerTabButton.onClick.AddListener(() => ShowTab(0));
            palettesTabButton.onClick.AddListener(() => ShowTab(1));
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            hueSlider.onValueChanged.AddListener(OnHueSliderChanged);
            hexInput.onEndEdit.AddListener(OnHexSubmitted);

            // Allow tapping / dragging on the HSB square
            var squareTap = hsbSquareImage.gameObject.AddComponent<PointerDragReceiver>();
            squareTap.OnDown  += OnHsbSquareTouched;
            squareTap.OnDrag  += OnHsbSquareTouched;

            paletteGallery.OnSwatchSelected += OnPaletteSwatchSelected;

            ShowTab(0);
            SetHSB(0f, 1f, 1f);
        }

        void OnDestroy()
        {
            if (_hsbTex != null) Destroy(_hsbTex);
            if (_hueTex != null) Destroy(_hueTex);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void SetColor(Color32 color)
        {
            Color.RGBToHSV(new Color(color.r / 255f, color.g / 255f, color.b / 255f),
                out float h, out float s, out float v);
            SetHSB(h, s, v);
        }

        // ── Tabs ───────────────────────────────────────────────────────────────

        void ShowTab(int index)
        {
            pickerTab.SetActive(index == 0);
            palettesTab.SetActive(index == 1);
            pickerTabButton.interactable   = index != 0;
            palettesTabButton.interactable = index != 1;
        }

        // ── HSB Square ─────────────────────────────────────────────────────────

        void SetHSB(float h, float s, float b)
        {
            _hue = h; _sat = s; _bri = b;
            BuildHsbTexture();
            UpdateCursorPosition();
            UpdatePreview();
            UpdateHexField();

            _suppressCallbacks = true;
            hueSlider.value = h;
            _suppressCallbacks = false;
        }

        void BuildHsbTexture()
        {
            Color32[] pixels = new Color32[HsbSize * HsbSize];
            for (int y = 0; y < HsbSize; y++)
            {
                float bri = y / (float)(HsbSize - 1);
                for (int x = 0; x < HsbSize; x++)
                {
                    float sat = x / (float)(HsbSize - 1);
                    pixels[y * HsbSize + x] = (Color32)Color.HSVToRGB(_hue, sat, bri);
                }
            }
            _hsbTex.SetPixels32(pixels);
            _hsbTex.Apply(false, false);
        }

        void BuildHueGradient()
        {
            Color32[] pixels = new Color32[HueTexSize];
            for (int x = 0; x < HueTexSize; x++)
            {
                float h = x / (float)(HueTexSize - 1);
                pixels[x] = (Color32)Color.HSVToRGB(h, 1f, 1f);
            }
            _hueTex.SetPixels32(pixels);
            _hueTex.Apply(false, false);
        }

        void UpdateCursorPosition()
        {
            if (hsbCursor == null) return;
            Rect r = hsbSquareImage.rectTransform.rect;
            float x = _sat * r.width  + r.xMin;
            float y = _bri * r.height + r.yMin;
            hsbCursor.anchoredPosition = new Vector2(x, y);
        }

        void OnHsbSquareTouched(Vector2 localPoint)
        {
            Rect r = hsbSquareImage.rectTransform.rect;
            float s = Mathf.Clamp01((localPoint.x - r.xMin) / r.width);
            float b = Mathf.Clamp01((localPoint.y - r.yMin) / r.height);
            _sat = s; _bri = b;
            UpdateCursorPosition();
            UpdatePreview();
            UpdateHexField();
            RaiseColorChanged();
        }

        void OnHueSliderChanged(float value)
        {
            if (_suppressCallbacks) return;
            _hue = value;
            BuildHsbTexture();
            UpdatePreview();
            UpdateHexField();
            RaiseColorChanged();
        }

        // ── HEX Field ──────────────────────────────────────────────────────────

        void UpdateHexField()
        {
            Color32 c = CurrentColor32();
            hexInput.SetTextWithoutNotify($"{c.r:X2}{c.g:X2}{c.b:X2}");
        }

        void OnHexSubmitted(string text)
        {
            text = text.TrimStart('#');
            if (text.Length != 6) { UpdateHexField(); return; }
            try
            {
                byte r = System.Convert.ToByte(text.Substring(0, 2), 16);
                byte g = System.Convert.ToByte(text.Substring(2, 2), 16);
                byte b = System.Convert.ToByte(text.Substring(4, 2), 16);
                SetColor(new Color32(r, g, b, 255));
                RaiseColorChanged();
            }
            catch { UpdateHexField(); }
        }

        // ── Palette Tab ────────────────────────────────────────────────────────

        void OnPaletteSwatchSelected(Color32 color)
        {
            SetColor(color);
            RaiseColorChanged();
            gameObject.SetActive(false); // close picker after swatch tap
        }

        // ── Preview & Output ───────────────────────────────────────────────────

        void UpdatePreview()
        {
            Color32 c = CurrentColor32();
            previewImage.color = new Color(c.r / 255f, c.g / 255f, c.b / 255f);
        }

        Color32 CurrentColor32()
        {
            Color c = Color.HSVToRGB(_hue, _sat, _bri);
            return new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), 255);
        }

        void RaiseColorChanged() => OnColorChanged?.Invoke(CurrentColor32());
    }
}
