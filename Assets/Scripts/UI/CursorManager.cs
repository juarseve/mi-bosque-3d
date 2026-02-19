using UnityEngine;

/// <summary>
/// Script para cambiar cursores globalmente en tu proyecto
/// Uso: CursorManager.Instance.SetCursor(CursorManager.CursorType.Hand);
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    public enum CursorType
    {
        Default,
        Hand,
        Pointer,
        Text,
        Custom
    }

    [SerializeField] private Texture2D handCursor;
    [SerializeField] private Texture2D pointerCursor;
    [SerializeField] private Texture2D textCursor;
    [SerializeField] private Vector2 handHotspot = new Vector2(5, 0);
    [SerializeField] private Vector2 pointerHotspot = Vector2.zero;
    [SerializeField] private Vector2 textHotspot = Vector2.zero;

    private CursorType currentCursorType = CursorType.Default;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Cambia el cursor al tipo especificado
    /// </summary>
    public void SetCursor(CursorType cursorType)
    {
        if (currentCursorType == cursorType) return;

        currentCursorType = cursorType;

        switch (cursorType)
        {
            case CursorType.Default:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;

            case CursorType.Hand:
                if (handCursor != null)
                    Cursor.SetCursor(handCursor, handHotspot, CursorMode.Auto);
                else
                    Debug.LogWarning("Hand cursor texture not assigned!");
                break;

            case CursorType.Pointer:
                if (pointerCursor != null)
                    Cursor.SetCursor(pointerCursor, pointerHotspot, CursorMode.Auto);
                else
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;

            case CursorType.Text:
                if (textCursor != null)
                    Cursor.SetCursor(textCursor, textHotspot, CursorMode.Auto);
                else
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;

            case CursorType.Custom:
                // Implementa aquí lógica personalizada
                break;
        }
    }

    /// <summary>
    /// Vuelve al cursor por defecto
    /// </summary>
    public void ResetCursor()
    {
        SetCursor(CursorType.Default);
    }
}
