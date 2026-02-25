using UnityEngine;
using UnityEngine.UI;

namespace PixelArtist.Canvas
{
    /// <summary>
    /// Syncs a PixelCanvas to a Texture2D and displays it via a RawImage.
    /// Call Refresh() after any canvas change to push pixels to the GPU.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class CanvasRenderer : MonoBehaviour
    {
        RawImage _rawImage;
        Texture2D _texture;
        PixelCanvas _canvas;

        public void Init(PixelCanvas canvas)
        {
            _canvas = canvas;
            _rawImage = GetComponent<RawImage>();

            if (_texture != null) Destroy(_texture);
            _texture = new Texture2D(canvas.Size, canvas.Size, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Refresh();
            _rawImage.texture = _texture;
        }

        /// <summary>Push all pixels to the GPU. Call after any canvas mutation.</summary>
        public void Refresh()
        {
            if (_canvas == null || _texture == null) return;
            _texture.SetPixels32(_canvas.Pixels);
            _texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        }

        void OnDestroy()
        {
            if (_texture != null) Destroy(_texture);
        }
    }
}
