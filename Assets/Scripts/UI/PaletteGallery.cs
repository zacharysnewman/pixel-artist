using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelArtist.UI
{
    /// <summary>
    /// Displays the built-in palette list.  Each palette row shows the palette name
    /// and a preview strip of its colors.  Tapping a row expands it to show all
    /// swatches; tapping a swatch fires OnSwatchSelected.
    /// </summary>
    public class PaletteGallery : MonoBehaviour
    {
        public event System.Action<Color32> OnSwatchSelected;

        [SerializeField] Transform listContainer;
        [SerializeField] GameObject paletteRowPrefab;
        [SerializeField] GameObject swatchPrefab;

        void Start() => Rebuild();

        void Rebuild()
        {
            foreach (Transform child in listContainer) Destroy(child.gameObject);

            foreach (BuiltInPalette palette in BuiltInPalettes.All)
            {
                GameObject row = Instantiate(paletteRowPrefab, listContainer);

                // Name label
                var label = row.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = palette.Name;

                // Header button toggles the swatch grid
                var headerBtn = row.GetComponentInChildren<Button>();
                Transform swatchContainer = row.transform.Find("SwatchContainer");
                if (swatchContainer != null) swatchContainer.gameObject.SetActive(false);

                if (headerBtn != null && swatchContainer != null)
                {
                    headerBtn.onClick.AddListener(() =>
                        swatchContainer.gameObject.SetActive(!swatchContainer.gameObject.activeSelf));
                }

                // Swatches
                if (swatchContainer != null)
                {
                    foreach (Color32 color in palette.Colors)
                    {
                        GameObject sw = Instantiate(swatchPrefab, swatchContainer);
                        var img = sw.GetComponent<Image>();
                        if (img != null) img.color = new Color(color.r / 255f, color.g / 255f, color.b / 255f);

                        var btn = sw.GetComponent<Button>();
                        Color32 captured = color;
                        if (btn != null) btn.onClick.AddListener(() => OnSwatchSelected?.Invoke(captured));
                    }
                }
            }
        }
    }

    // ── Palette definitions ────────────────────────────────────────────────────

    public class BuiltInPalette
    {
        public string Name;
        public Color32[] Colors;
    }

    public static class BuiltInPalettes
    {
        public static IReadOnlyList<BuiltInPalette> All { get; } = new List<BuiltInPalette>
        {
            new BuiltInPalette
            {
                Name = "Pico-8",
                Colors = new Color32[]
                {
                    new Color32(  0,   0,   0, 255), // black
                    new Color32( 29,  43,  83, 255), // dark blue
                    new Color32(126,  37,  83, 255), // dark purple
                    new Color32(  0, 135,  81, 255), // dark green
                    new Color32(171,  82,  54, 255), // brown
                    new Color32( 95,  87,  79, 255), // dark grey
                    new Color32(194, 195, 199, 255), // light grey
                    new Color32(255, 241, 232, 255), // white
                    new Color32(255,   0,  77, 255), // red
                    new Color32(255, 163,   0, 255), // orange
                    new Color32(255, 236,  39, 255), // yellow
                    new Color32(  0, 228,  54, 255), // green
                    new Color32( 41, 173, 255, 255), // blue
                    new Color32(131, 118, 156, 255), // lavender
                    new Color32(255, 119, 168, 255), // pink
                    new Color32(255, 204, 170, 255), // peach
                }
            },

            new BuiltInPalette
            {
                Name = "Gameboy",
                Colors = new Color32[]
                {
                    new Color32( 15,  56,  15, 255),
                    new Color32( 48,  98,  48, 255),
                    new Color32(139, 172,  15, 255),
                    new Color32(155, 188,  15, 255),
                }
            },

            new BuiltInPalette
            {
                Name = "NES",
                Colors = new Color32[]
                {
                    new Color32(124, 124, 124, 255),
                    new Color32(  0,   0, 252, 255),
                    new Color32(  0,   0, 188, 255),
                    new Color32( 68,  40, 188, 255),
                    new Color32(148,   0, 132, 255),
                    new Color32(168,   0,  32, 255),
                    new Color32(168,  16,   0, 255),
                    new Color32(136,  20,   0, 255),
                    new Color32( 80,  48,   0, 255),
                    new Color32(  0, 120,   0, 255),
                    new Color32(  0, 104,   0, 255),
                    new Color32(  0,  88,   0, 255),
                    new Color32(  0,  64,  88, 255),
                    new Color32(  0,   0,   0, 255),
                    new Color32(188, 188, 188, 255),
                    new Color32(248, 248, 248, 255),
                }
            },

            new BuiltInPalette
            {
                Name = "Endesga 32",
                Colors = new Color32[]
                {
                    new Color32(190,  74,  47, 255),
                    new Color32(215, 118,  67, 255),
                    new Color32(234, 212,  170,255),
                    new Color32(228, 166,  114,255),
                    new Color32(184, 111,  80, 255),
                    new Color32(116,  63,  57, 255),
                    new Color32( 69,  40,  60, 255),
                    new Color32(162,  38,  51, 255),
                    new Color32(228,  59,  68, 255),
                    new Color32(246, 100,  68, 255),
                    new Color32(252, 156,  86, 255),
                    new Color32(246, 214, 189, 255),
                    new Color32(252, 246, 188, 255),
                    new Color32(232, 233,  94, 255),
                    new Color32(155, 209,  50, 255),
                    new Color32( 58, 189,  83, 255),
                    new Color32( 59, 145, 118, 255),
                    new Color32( 51,  99, 142, 255),
                    new Color32( 27,  60, 100, 255),
                    new Color32( 18,  30,  80, 255),
                    new Color32( 10,  16,  40, 255),
                    new Color32( 73,  66, 152, 255),
                    new Color32(133,  82, 186, 255),
                    new Color32(208, 133, 208, 255),
                    new Color32(208, 181, 230, 255),
                    new Color32(255, 255, 255, 255),
                    new Color32(195, 205, 230, 255),
                    new Color32(140, 159, 196, 255),
                    new Color32( 82, 110, 170, 255),
                    new Color32( 50,  50,  50, 255),
                    new Color32(100, 100, 100, 255),
                    new Color32(200, 200, 200, 255),
                }
            },

            new BuiltInPalette
            {
                Name = "Grayscale",
                Colors = new Color32[]
                {
                    new Color32(  0,   0,   0, 255),
                    new Color32( 36,  36,  36, 255),
                    new Color32( 73,  73,  73, 255),
                    new Color32(109, 109, 109, 255),
                    new Color32(146, 146, 146, 255),
                    new Color32(182, 182, 182, 255),
                    new Color32(219, 219, 219, 255),
                    new Color32(255, 255, 255, 255),
                }
            },
        };
    }
}
