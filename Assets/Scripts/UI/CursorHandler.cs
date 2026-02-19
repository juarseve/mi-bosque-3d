using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Script que cambia el cursor a una manita cuando se interactúa con botones clickeables
/// Asigna este script a cualquier botón que quieras que tenga el cursor de mano en hover
/// </summary>
public class CursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Texture2D handCursor; // La textura del cursor de mano
    [SerializeField] private Vector2 hotspot = new Vector2(5, 0); // Punto de referencia del cursor
    [Tooltip("Si está activado, usará un cursor de sistema en lugar de una textura personalizada")]
    [SerializeField] private bool useSystemCursor = false;
    
    private bool isPointerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        ChangeCursorToHand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        ResetCursor();
    }
    
    /// <summary>
    /// Se ejecuta cuando se presiona el botón del mouse
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // El cursor se mantiene si aún estamos sobre el botón
        if (isPointerOver)
        {
            ChangeCursorToHand();
        }
    }
    
    /// <summary>
    /// Se ejecuta cuando se suelta el botón del mouse
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Resetea el cursor al soltar el click
        ResetCursor();
    }
    
    private void ChangeCursorToHand()
    {
        if (useSystemCursor)
        {
            // Usa cursor del sistema (arrow)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        else if (handCursor != null)
        {
            // Cambia al cursor de mano personalizado
            Cursor.SetCursor(handCursor, hotspot, CursorMode.Auto);
        }
    }
    
    private void ResetCursor()
    {
        // Vuelve al cursor por defecto
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// Obtiene el componente Button - Retorna null si no lo tiene
    /// </summary>
    private Button GetButtonComponent()
    {

        return GetComponent<Button>();
    }
}
