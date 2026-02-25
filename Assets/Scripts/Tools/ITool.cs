using UnityEngine;
using PixelArtist.Canvas;

namespace PixelArtist.Tools
{
    public interface ITool
    {
        /// <summary>Called when the user first touches a canvas pixel.</summary>
        void OnPointerDown(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor);

        /// <summary>Called as the pointer drags to a new pixel (only when the pixel coords change).</summary>
        void OnPointerDrag(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor);

        /// <summary>Called when the touch is released.</summary>
        void OnPointerUp(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor);
    }
}
