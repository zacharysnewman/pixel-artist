using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelArtist.Data;

namespace PixelArtist.UI
{
    /// <summary>
    /// One cell in the gallery grid. Renders the artwork thumbnail and shows
    /// the name + a checkmark overlay when in select mode.
    /// </summary>
    public class GalleryItemView : MonoBehaviour
    {
        [SerializeField] RawImage thumbnail;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] GameObject selectedOverlay; // checkmark GameObject
        [SerializeField] Button tapButton;

        public string ArtworkId { get; private set; }
        System.Action<GalleryItemView> _onTap;

        public void Init(ArtworkData artwork, System.Action<GalleryItemView> onTap)
        {
            ArtworkId = artwork.id;
            _onTap = onTap;
            nameLabel.text = artwork.name;
            selectedOverlay.SetActive(false);
            tapButton.onClick.RemoveAllListeners();
            tapButton.onClick.AddListener(() => _onTap?.Invoke(this));

            BuildThumbnail(artwork);
        }

        public void SetSelected(bool selected) => selectedOverlay.SetActive(selected);

        void BuildThumbnail(ArtworkData artwork)
        {
            Color32[] pixels = artwork.DecodePixels();
            var tex = new Texture2D(artwork.size, artwork.size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels32(pixels);
            tex.Apply(false, true); // makeNoLongerReadable = true, we never need to read it back
            thumbnail.texture = tex;
        }

        void OnDestroy()
        {
            if (thumbnail.texture != null) Destroy(thumbnail.texture);
        }
    }
}
