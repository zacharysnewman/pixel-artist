using UnityEngine;
using PixelArtist.Canvas;

namespace PixelArtist.Tools
{
    /// <summary>Draws the active color one pixel at a time.</summary>
    public class PencilTool : ITool
    {
        public void OnPointerDown(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            undo.Push(canvas);
            canvas.SetPixel(x, y, activeColor);
        }

        public void OnPointerDrag(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            canvas.SetPixel(x, y, activeColor);
        }

        public void OnPointerUp(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor) { }
    }
}
