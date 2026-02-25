using UnityEngine;
using UnityEngine.UI;

namespace PixelArtist.Canvas
{
    /// <summary>
    /// Generates a checkerboard texture behind the canvas to represent transparency.
    /// One checker square = one canvas pixel.
    ///
    /// Place this component on a RawImage that sits directly behind the canvas RawImage
    /// in the same RectTransform container — it will match the canvas size automatically.
    ///
    /// Standard pixel-art transparent preview colors:
    ///   Light square: #CCCCCC   Dark square: #999999
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class CheckerboardBackground : MonoBehaviour
    {
        static readonly Color32 LightSquare = new Color32(0xCC, 0xCC, 0xCC, 0xFF);
        static readonly Color32 DarkSquare  = new Color32(0x99, 0x99, 0x99, 0xFF);

        RawImage _rawImage;
        Texture2D _texture;

        public void Init(int canvasSize)
        {
            _rawImage = GetComponent<RawImage>();

            if (_texture != null) Destroy(_texture);
            _texture = new Texture2D(canvasSize, canvasSize, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[canvasSize * canvasSize];
            for (int y = 0; y < canvasSize; y++)
                for (int x = 0; x < canvasSize; x++)
                    pixels[y * canvasSize + x] = ((x + y) % 2 == 0) ? LightSquare : DarkSquare;

            _texture.SetPixels32(pixels);
            _texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            _rawImage.texture = _texture;
        }

        void OnDestroy()
        {
            if (_texture != null) Destroy(_texture);
        }
    }
}
