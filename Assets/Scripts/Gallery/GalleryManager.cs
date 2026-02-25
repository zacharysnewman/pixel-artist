using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using PixelArtist.Data;

namespace PixelArtist.Gallery
{
    /// <summary>
    /// Singleton. Manages persisting, loading, duplicating, and deleting artworks.
    /// All files live in Application.persistentDataPath:
    ///   gallery.json         — ordered list of artwork IDs
    ///   {guid}.json          — individual ArtworkData records
    /// </summary>
    public class GalleryManager : MonoBehaviour
    {
        public static GalleryManager Instance { get; private set; }

        public event Action OnGalleryChanged;

        // In-memory ordered list of loaded artworks.
        readonly List<ArtworkData> _artworks = new List<ArtworkData>();
        public IReadOnlyList<ArtworkData> Artworks => _artworks;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAll();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Save (create or overwrite) an artwork. Adds to ordered list if new.</summary>
        public void Save(ArtworkData artwork)
        {
            // Update in-memory list
            int idx = _artworks.FindIndex(a => a.id == artwork.id);
            if (idx < 0)
                _artworks.Add(artwork);
            else
                _artworks[idx] = artwork;

            PersistArtwork(artwork);
            PersistManifest();
            OnGalleryChanged?.Invoke();
        }

        /// <summary>Duplicate one or more artworks (appends " copy", generates new IDs).</summary>
        public void Duplicate(IEnumerable<string> ids)
        {
            foreach (string id in ids)
            {
                ArtworkData src = _artworks.Find(a => a.id == id);
                if (src == null) continue;
                ArtworkData copy = src.DeepCopy();
                _artworks.Add(copy);
                PersistArtwork(copy);
            }
            PersistManifest();
            OnGalleryChanged?.Invoke();
        }

        /// <summary>Delete one or more artworks by ID.</summary>
        public void Delete(IEnumerable<string> ids)
        {
            foreach (string id in ids)
            {
                _artworks.RemoveAll(a => a.id == id);
                string path = ArtworkPath(id);
                if (File.Exists(path)) File.Delete(path);
            }
            PersistManifest();
            OnGalleryChanged?.Invoke();
        }

        public ArtworkData Find(string id) => _artworks.Find(a => a.id == id);

        // ── Persistence ────────────────────────────────────────────────────────

        void LoadAll()
        {
            _artworks.Clear();
            GalleryManifest manifest = LoadManifest();

            foreach (string id in manifest.orderedIds)
            {
                string path = ArtworkPath(id);
                if (!File.Exists(path)) continue;
                try
                {
                    ArtworkData artwork = JsonUtility.FromJson<ArtworkData>(File.ReadAllText(path));
                    if (artwork != null) _artworks.Add(artwork);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[GalleryManager] Failed to load artwork {id}: {e.Message}");
                }
            }
        }

        GalleryManifest LoadManifest()
        {
            string path = ManifestPath();
            if (!File.Exists(path)) return new GalleryManifest();
            try { return JsonUtility.FromJson<GalleryManifest>(File.ReadAllText(path)) ?? new GalleryManifest(); }
            catch { return new GalleryManifest(); }
        }

        void PersistArtwork(ArtworkData artwork)
        {
            File.WriteAllText(ArtworkPath(artwork.id), JsonUtility.ToJson(artwork, prettyPrint: false));
        }

        void PersistManifest()
        {
            var manifest = new GalleryManifest();
            foreach (ArtworkData a in _artworks) manifest.orderedIds.Add(a.id);
            File.WriteAllText(ManifestPath(), JsonUtility.ToJson(manifest, prettyPrint: false));
        }

        string ArtworkPath(string id) => Path.Combine(Application.persistentDataPath, $"{id}.json");
        string ManifestPath() => Path.Combine(Application.persistentDataPath, "gallery.json");
    }
}
