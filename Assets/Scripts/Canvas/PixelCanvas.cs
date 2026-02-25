using UnityEngine;

namespace PixelArtist.Canvas
{
    /// <summary>
    /// Core canvas data model. Holds the flat Color32 pixel array and tracks dirty state.
    /// Row-major: index = y * size + x.  (0,0) is bottom-left to match Texture2D convention.
    /// </summary>
    public class PixelCanvas
    {
        public int Size { get; private set; }
        public Color32[] Pixels { get; private set; }
        public bool IsDirty { get; private set; }

        public PixelCanvas(int size, Color32[] pixels = null)
        {
            Size = size;
            Pixels = pixels != null ? (Color32[])pixels.Clone() : new Color32[size * size];
        }

        public Color32 GetPixel(int x, int y)
        {
            if (!InBounds(x, y)) return new Color32(0, 0, 0, 0);
            return Pixels[y * Size + x];
        }

        public bool SetPixel(int x, int y, Color32 color)
        {
            if (!InBounds(x, y)) return false;
            int idx = y * Size + x;
            if (Pixels[idx].r == color.r && Pixels[idx].g == color.g &&
                Pixels[idx].b == color.b && Pixels[idx].a == color.a)
                return false;

            Pixels[idx] = color;
            IsDirty = true;
            return true;
        }

        public void ClearDirty() => IsDirty = false;

        /// <summary>Returns a full copy of the pixel array (for undo snapshots).</summary>
        public Color32[] Snapshot() => (Color32[])Pixels.Clone();

        /// <summary>Restores pixels from a snapshot without marking dirty (undo does this manually).</summary>
        public void Restore(Color32[] snapshot)
        {
            System.Array.Copy(snapshot, Pixels, Pixels.Length);
            IsDirty = true;
        }

        public bool HasAnyPixels()
        {
            foreach (Color32 c in Pixels)
                if (c.a > 0) return true;
            return false;
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;
    }
}
