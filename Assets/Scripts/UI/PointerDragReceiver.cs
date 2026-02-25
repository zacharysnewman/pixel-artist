using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelArtist.UI
{
    /// <summary>
    /// Utility component. Fires OnDown and OnDrag events with a local-space point
    /// relative to the RectTransform it sits on.  Used by ColorPickerPanel to
    /// handle taps and drags on the HSB square.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PointerDragReceiver : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public event System.Action<Vector2> OnDown;
        public event System.Action<Vector2> OnDrag;

        RectTransform _rect;

        void Awake() => _rect = GetComponent<RectTransform>();

        public void OnPointerDown(PointerEventData e) => Fire(OnDown, e);
        public void OnDrag(PointerEventData e)        => Fire(OnDrag, e);

        void Fire(System.Action<Vector2> evt, PointerEventData e)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, e.position, e.pressEventCamera, out Vector2 local))
                evt?.Invoke(local);
        }
    }
}
