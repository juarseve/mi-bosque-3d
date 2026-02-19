using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Script de ejemplo para usar CursorManager en botones
/// Alternativa simplificada a CursorHandler cuando usas CursorManager
/// </summary>
public class ButtonCursorExample : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CursorManager.CursorType hoverCursorType = CursorManager.CursorType.Hand;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor(hoverCursorType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ResetCursor();
        }
    }
}
