using UnityEngine;
using PixelArtist.Canvas;

namespace PixelArtist.Tools
{
    /// <summary>
    /// Samples the color of the tapped pixel and sets it as the active color.
    /// Automatically switches the active tool back to the previously active tool after sampling.
    /// </summary>
    public class EyedropperTool : ITool
    {
        /// <summary>Invoked with the sampled color. The CanvasScreen should handle switching tools.</summary>
        public event System.Action<Color32> OnColorSampled;

        public void OnPointerDown(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            Color32 sampled = canvas.GetPixel(x, y);
            if (sampled.a == 0) return; // Don't sample transparent pixels

            activeColor = sampled;
            OnColorSampled?.Invoke(sampled);
        }

        public void OnPointerDrag(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            // Live preview while dragging — update color but don't switch tool yet
            Color32 sampled = canvas.GetPixel(x, y);
            if (sampled.a > 0) activeColor = sampled;
        }

        public void OnPointerUp(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor) { }
    }
}
