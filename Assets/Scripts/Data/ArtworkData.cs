using System;
using UnityEngine;

namespace PixelArtist.Data
{
    [Serializable]
    public class ArtworkData
    {
        public string id;
        public int size;          // 8, 16, or 32
        public string pixelsBase64;
        public long createdAt;    // Unix timestamp (seconds)
        public long modifiedAt;

        /// <summary>Creates a blank ArtworkData with a new GUID and current timestamps.</summary>
        public static ArtworkData Create(int size)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new ArtworkData
            {
                id = Guid.NewGuid().ToString(),
                size = size,
                pixelsBase64 = EncodePixels(new Color32[size * size]),
                createdAt = now,
                modifiedAt = now
            };
        }

        public Color32[] DecodePixels()
        {
            if (string.IsNullOrEmpty(pixelsBase64))
                return new Color32[size * size];

            byte[] bytes = Convert.FromBase64String(pixelsBase64);
            Color32[] pixels = new Color32[bytes.Length / 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(
                    bytes[i * 4],
                    bytes[i * 4 + 1],
                    bytes[i * 4 + 2],
                    bytes[i * 4 + 3]
                );
            }
            return pixels;
        }

        public void SetPixels(Color32[] pixels)
        {
            pixelsBase64 = EncodePixels(pixels);
            modifiedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public bool HasAnyPixels()
        {
            Color32[] pixels = DecodePixels();
            foreach (Color32 c in pixels)
                if (c.a > 0) return true;
            return false;
        }

        public ArtworkData DeepCopy()
        {
            return new ArtworkData
            {
                id = Guid.NewGuid().ToString(),
                size = size,
                pixelsBase64 = pixelsBase64,
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                modifiedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        static string EncodePixels(Color32[] pixels)
        {
            byte[] bytes = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                bytes[i * 4]     = pixels[i].r;
                bytes[i * 4 + 1] = pixels[i].g;
                bytes[i * 4 + 2] = pixels[i].b;
                bytes[i * 4 + 3] = pixels[i].a;
            }
            return Convert.ToBase64String(bytes);
        }
    }

    [Serializable]
    public class GalleryManifest
    {
        public System.Collections.Generic.List<string> orderedIds
            = new System.Collections.Generic.List<string>();
    }
}
