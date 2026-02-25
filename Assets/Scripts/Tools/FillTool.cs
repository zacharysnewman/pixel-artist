using System.Collections.Generic;
using UnityEngine;
using PixelArtist.Canvas;

namespace PixelArtist.Tools
{
    /// <summary>Flood-fill from the touched pixel with the active color.</summary>
    public class FillTool : ITool
    {
        public void OnPointerDown(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor)
        {
            Color32 target = canvas.GetPixel(x, y);
            if (ColorsEqual(target, activeColor)) return;

            undo.Push(canvas);
            FloodFill(canvas, x, y, target, activeColor);
        }

        public void OnPointerDrag(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor) { }
        public void OnPointerUp(int x, int y, PixelCanvas canvas, UndoSystem undo, ref Color32 activeColor) { }

        // Iterative 4-connected BFS flood fill.
        static void FloodFill(PixelCanvas canvas, int startX, int startY, Color32 target, Color32 fill)
        {
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                if (!canvas.InBounds(cx, cy)) continue;
                if (!ColorsEqual(canvas.GetPixel(cx, cy), target)) continue;

                canvas.SetPixel(cx, cy, fill);

                queue.Enqueue((cx + 1, cy));
                queue.Enqueue((cx - 1, cy));
                queue.Enqueue((cx, cy + 1));
                queue.Enqueue((cx, cy - 1));
            }
        }

        static bool ColorsEqual(Color32 a, Color32 b) =>
            a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }
}
