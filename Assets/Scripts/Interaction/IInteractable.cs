using UnityEngine;

/// <summary>
/// Interfaz para todos los objetos interactuables con la tecla E
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Se llama cuando el jugador interactúa con el objeto (tecla E)
    /// </summary>
    void OnInteract();
    
    /// <summary>
    /// Se llama cuando el jugador mira al objeto (para mostrar indicador visual)
    /// </summary>
    void OnLookAt();
    
    /// <summary>
    /// Se llama cuando el jugador deja de mirar al objeto
    /// </summary>
    void OnLookAway();
}
