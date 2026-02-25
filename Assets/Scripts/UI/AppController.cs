using UnityEngine;
using PixelArtist.Data;

namespace PixelArtist.UI
{
    /// <summary>
    /// Root screen manager. Holds references to GalleryScreen and CanvasScreen,
    /// and handles navigation between them.
    ///
    /// Scene setup:
    ///   - GalleryManager (DontDestroyOnLoad singleton) somewhere in the scene
    ///   - GalleryScreen  GameObject (starts active)
    ///   - CanvasScreen   GameObject (starts inactive)
    /// </summary>
    public class AppController : MonoBehaviour
    {
        [SerializeField] GalleryScreen galleryScreen;
        [SerializeField] CanvasScreen  canvasScreen;

        void Start()
        {
            galleryScreen.OnOpenArtwork = OpenArtwork;
            canvasScreen.OnNavigateBack = OpenGallery;

            OpenGallery();
        }

        void OpenArtwork(ArtworkData artwork)
        {
            // Determine whether this artwork is "new" (not yet in the gallery).
            // GalleryManager won't have it if it was just created by GalleryScreen.
            bool isNew = Gallery.GalleryManager.Instance.Find(artwork.id) == null;

            galleryScreen.gameObject.SetActive(false);
            canvasScreen.gameObject.SetActive(true);
            canvasScreen.Open(artwork, isNew);
        }

        void OpenGallery()
        {
            canvasScreen.gameObject.SetActive(false);
            galleryScreen.gameObject.SetActive(true);
        }
    }
}
