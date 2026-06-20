using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
public class ClickableUI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if(TryGetComponent(out CardFlip flipper))
        {
            flipper.Flip();
        }
    }
}
