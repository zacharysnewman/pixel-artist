using UnityEngine;
using PixelArtist.Canvas;

namespace PixelArtist.Tools
{
    /// <summary>Sets pixels to fully transparent.</summary>
    public class EraserTool : ITool
    {
        static readonly Color32 Transparent = new Color32(0, 0, 0, 0);

        public void OnPointerDown(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            undo.Push(canvas);
            canvas.SetPixel(x, y, Transparent);
        }

        public void OnPointerDrag(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            canvas.SetPixel(x, y, Transparent);
        }

        public void OnPointerUp(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor) { }
    }
}
