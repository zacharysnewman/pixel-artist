using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelArtist.Data;
using PixelArtist.Gallery;

namespace PixelArtist.UI
{
    /// <summary>
    /// Controls the gallery home screen.
    ///
    /// Normal Mode:
    ///   - Grid of artwork thumbnails sorted by the active sort setting
    ///   - Sort-by button toggles Created / Modified date
    ///   - Direction button toggles ascending / descending (default: newest first)
    ///   - "Select" button (top-right) enters Select Mode
    ///   - "+" button opens the new-canvas dimension picker
    ///
    /// Select Mode:
    ///   - Tap thumbnails to toggle selection (checkmark overlay)
    ///   - "Duplicate" / "Delete" action buttons appear in the toolbar
    ///   - "Cancel" exits Select Mode
    /// </summary>
    public class GalleryScreen : MonoBehaviour
    {
        [Header("Toolbar — Sort")]
        [SerializeField] Button sortByButton;        // toggles Created ↔ Modified
        [SerializeField] TMP_Text sortByLabel;       // "Created" | "Modified"
        [SerializeField] Button sortDirButton;       // toggles asc ↔ desc
        [SerializeField] TMP_Text sortDirLabel;      // "↓ Newest" | "↑ Oldest"

        [Header("Toolbar — Actions")]
        [SerializeField] Button newArtworkButton;
        [SerializeField] Button selectButton;
        [SerializeField] Button cancelSelectButton;
        [SerializeField] Button duplicateButton;
        [SerializeField] Button deleteButton;

        [Header("Grid")]
        [SerializeField] Transform gridContainer;
        [SerializeField] GameObject galleryItemPrefab;

        [Header("New Canvas Panel")]
        [SerializeField] GameObject newCanvasPicker;
        [SerializeField] Button size8Button;
        [SerializeField] Button size16Button;
        [SerializeField] Button size32Button;
        [SerializeField] Button newCanvasCancelButton;

        [Header("Delete Confirmation Dialog")]
        [SerializeField] GameObject deleteConfirmDialog;
        [SerializeField] Button deleteConfirmYesButton;
        [SerializeField] Button deleteConfirmNoButton;
        [SerializeField] TMP_Text deleteConfirmText;

        // Injected by AppController
        public System.Action<ArtworkData> OnOpenArtwork;

        // ── Sort state ─────────────────────────────────────────────────────────

        enum SortBy { CreatedAt, ModifiedAt }
        SortBy _sortBy = SortBy.CreatedAt;
        bool _sortAscending = false; // false = newest first (descending)

        // ── Selection state ────────────────────────────────────────────────────

        bool _selectMode;
        readonly HashSet<string> _selectedIds = new HashSet<string>();
        readonly List<GalleryItemView> _itemViews = new List<GalleryItemView>();

        // ── Lifecycle ──────────────────────────────────────────────────────────

        void OnEnable()
        {
            GalleryManager.Instance.OnGalleryChanged += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (GalleryManager.Instance != null)
                GalleryManager.Instance.OnGalleryChanged -= Rebuild;
        }

        void Start()
        {
            newArtworkButton.onClick.AddListener(OpenNewCanvasPicker);
            selectButton.onClick.AddListener(EnterSelectMode);
            cancelSelectButton.onClick.AddListener(ExitSelectMode);
            duplicateButton.onClick.AddListener(DuplicateSelected);
            deleteButton.onClick.AddListener(PromptDeleteSelected);
            size8Button.onClick.AddListener(() => CreateNewArtwork(8));
            size16Button.onClick.AddListener(() => CreateNewArtwork(16));
            size32Button.onClick.AddListener(() => CreateNewArtwork(32));
            newCanvasCancelButton.onClick.AddListener(() => newCanvasPicker.SetActive(false));
            deleteConfirmYesButton.onClick.AddListener(ConfirmDelete);
            deleteConfirmNoButton.onClick.AddListener(() => deleteConfirmDialog.SetActive(false));
            sortByButton.onClick.AddListener(ToggleSortBy);
            sortDirButton.onClick.AddListener(ToggleSortDirection);

            SetSelectMode(false);
            RefreshSortLabels();
        }

        // ── Sort ───────────────────────────────────────────────────────────────

        void ToggleSortBy()
        {
            _sortBy = _sortBy == SortBy.CreatedAt ? SortBy.ModifiedAt : SortBy.CreatedAt;
            RefreshSortLabels();
            Rebuild();
        }

        void ToggleSortDirection()
        {
            _sortAscending = !_sortAscending;
            RefreshSortLabels();
            Rebuild();
        }

        void RefreshSortLabels()
        {
            if (sortByLabel != null)
                sortByLabel.text = _sortBy == SortBy.CreatedAt ? "Created" : "Modified";
            if (sortDirLabel != null)
                sortDirLabel.text = _sortAscending ? "↑ Oldest" : "↓ Newest";
        }

        List<ArtworkData> GetSortedArtworks()
        {
            var list = new List<ArtworkData>(GalleryManager.Instance.Artworks);
            list.Sort((a, b) =>
            {
                long ta = _sortBy == SortBy.CreatedAt ? a.createdAt : a.modifiedAt;
                long tb = _sortBy == SortBy.CreatedAt ? b.createdAt : b.modifiedAt;
                int cmp = ta.CompareTo(tb);
                return _sortAscending ? cmp : -cmp;
            });
            return list;
        }

        // ── Gallery Build ──────────────────────────────────────────────────────

        void Rebuild()
        {
            foreach (Transform child in gridContainer) Destroy(child.gameObject);
            _itemViews.Clear();
            _selectedIds.Clear();
            UpdateActionButtons();

            foreach (ArtworkData artwork in GetSortedArtworks())
            {
                GameObject go = Instantiate(galleryItemPrefab, gridContainer);
                var view = go.GetComponent<GalleryItemView>();
                view.Init(artwork, OnItemTap);
                _itemViews.Add(view);
            }
        }

        // ── Item Tap ───────────────────────────────────────────────────────────

        void OnItemTap(GalleryItemView view)
        {
            if (_selectMode)
            {
                if (_selectedIds.Contains(view.ArtworkId))
                    _selectedIds.Remove(view.ArtworkId);
                else
                    _selectedIds.Add(view.ArtworkId);

                view.SetSelected(_selectedIds.Contains(view.ArtworkId));
                UpdateActionButtons();
            }
            else
            {
                ArtworkData artwork = GalleryManager.Instance.Find(view.ArtworkId);
                if (artwork != null) OnOpenArtwork?.Invoke(artwork);
            }
        }

        // ── Select Mode ────────────────────────────────────────────────────────

        void EnterSelectMode() => SetSelectMode(true);
        void ExitSelectMode()  => SetSelectMode(false);

        void SetSelectMode(bool active)
        {
            _selectMode = active;
            if (!active) { _selectedIds.Clear(); RefreshSelectionOverlays(); }

            selectButton.gameObject.SetActive(!active);
            cancelSelectButton.gameObject.SetActive(active);
            UpdateActionButtons();
        }

        void UpdateActionButtons()
        {
            bool hasSelection = _selectedIds.Count > 0;
            duplicateButton.gameObject.SetActive(_selectMode);
            deleteButton.gameObject.SetActive(_selectMode);
            duplicateButton.interactable = hasSelection;
            deleteButton.interactable = hasSelection;
        }

        void RefreshSelectionOverlays()
        {
            foreach (GalleryItemView v in _itemViews)
                v.SetSelected(_selectedIds.Contains(v.ArtworkId));
        }

        // ── Actions ────────────────────────────────────────────────────────────

        void DuplicateSelected()
        {
            GalleryManager.Instance.Duplicate(new List<string>(_selectedIds));
            ExitSelectMode();
        }

        void PromptDeleteSelected()
        {
            int count = _selectedIds.Count;
            deleteConfirmText.text = count == 1
                ? "Delete 1 artwork? This cannot be undone."
                : $"Delete {count} artworks? This cannot be undone.";
            deleteConfirmDialog.SetActive(true);
        }

        void ConfirmDelete()
        {
            GalleryManager.Instance.Delete(new List<string>(_selectedIds));
            deleteConfirmDialog.SetActive(false);
            ExitSelectMode();
        }

        // ── New Canvas ─────────────────────────────────────────────────────────

        void OpenNewCanvasPicker() => newCanvasPicker.SetActive(true);

        void CreateNewArtwork(int size)
        {
            newCanvasPicker.SetActive(false);
            ArtworkData newArtwork = ArtworkData.Create("Untitled", size);
            OnOpenArtwork?.Invoke(newArtwork);
        }
    }
}
