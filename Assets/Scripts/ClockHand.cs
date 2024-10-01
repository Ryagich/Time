using UnityEngine;
using UnityEngine.EventSystems;

public class ClockHand : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private Clock clock;

    public void OnPointerDown(PointerEventData eventData)
    {
        clock.StartDraggingHand(transform);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        clock.StopDraggingHand();
    }

    public void OnDrag(PointerEventData eventData)
    {
        clock.DragHand(transform, eventData);
    }
}