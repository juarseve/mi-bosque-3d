using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Nest : MonoBehaviour, IInteractable
{
    public static bool home = false;
    public GameObject feedback;
    public Text message;
    private bool active = true;
    public GameObject recordatorio;

    [Header("Configuración de Distancia")]
    [Tooltip("Objeto de referencia para calcular la distancia (generalmente el jugador o cámara)")]
    public Transform objetoReferencia;

    [Tooltip("Distancia máxima del jugador al nest para completar el desafío (en metros)")]
    public float maxDistance = 10f;

    [Header("Configuración del Conejo")]
    [Tooltip("Transform del conejo (Squirrel) para verificar su distancia al nest")]
    public Transform squirrelTransform;

    [Tooltip("Distancia mínima del conejo al nest para considerarlo en posición")]
    public float distanciaConejoCerca = 2f;

    // Referencia al puntero para feedback visual
    private GameObject puntero;

    // Flag para evitar doble interacción
    private bool isInteracting = false;

    void Start()
    {
        puntero = GameObject.Find("Crosshair/Image");

        // Si no se asignó en el inspector, intentar buscar el jugador automáticamente
        if (objetoReferencia == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                objetoReferencia = player.transform;
                Debug.LogWarning("[Nest] objetoReferencia no asignado. Usando el GameObject con tag 'Player' automáticamente.");
            }
            else
            {
                Debug.LogError("[Nest] objetoReferencia no asignado y no se encontró GameObject con tag 'Player'!");
            }
        }

        // Si no se asignó el squirrel, intentar buscarlo
        if (squirrelTransform == null)
        {
            GameObject squirrelObj = GameObject.FindGameObjectWithTag("Squirrel");
            if (squirrelObj != null)
            {
                squirrelTransform = squirrelObj.transform;
                Debug.LogWarning("[Nest] squirrelTransform no asignado. Usando el GameObject con tag 'Squirrel' automáticamente.");
            }
        }

        // Resetear el estado al iniciar
        home = false;
    }

    void Update()
    {
        // Verificar continuamente si se deben cumplir las condiciones para completar el desafío
        if (!home && active && Squirrel.caught && Squirrel.activate)
        {
            // Verificar si el conejo está cerca del nest
            bool conejoEnPosicion = VerificarDistanciaConejo();

            // Verificar si el jugador está en rango
            bool jugadorEnRango = VerificarDistanciaJugador();

            // Si AMBAS condiciones se cumplen, completar el desafío
            if (conejoEnPosicion && jugadorEnRango)
            {
                Debug.Log("[Nest] ¡Condiciones cumplidas! Conejo y jugador en posición. Completando desafío...");
                CompletarMision();
            }
        }
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============

    public void OnInteract()
    {
        if (isInteracting) return;

        Debug.Log("[Nest] OnInteract llamado. active=" + active + ", Squirrel.caught=" + Squirrel.caught + ", Squirrel.activate=" + Squirrel.activate);

        // Solo permitir interacción si:
        // 1. La madriguera está activa (no se ha completado)
        // 2. El conejo está capturado (Squirrel.caught == true)
        // 3. El juego no está pausado
        if (active && Squirrel.caught && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            // Verificar ambas distancias
            if (!VerificarDistanciaJugador())
            {
                Debug.Log("[Nest] El jugador está demasiado lejos. Distancia actual: " + GetDistanceToJugador() + "m, máximo permitido: " + maxDistance + "m");
                return;
            }

            if (!VerificarDistanciaConejo())
            {
                Debug.Log("[Nest] El conejo está demasiado lejos. Distancia actual: " + GetDistanceToConejo() + "m, máximo permitido: " + distanciaConejoCerca + "m");
                return;
            }

            isInteracting = true;
            Debug.Log("[Nest] ¡Completando misión del conejo por interacción!");

            // Reproducir animación de soltar (pickup inverso)
            if (InteractionDetector.instance != null)
            {
                InteractionDetector.instance.PlayPickupAnimation();
            }

            // Completar la misión inmediatamente
            CompletarMision();

            Invoke("ResetInteracting", 1f);
        }
        else
        {
            // Dar feedback de por qué no se puede interactuar
            if (!active)
            {
                Debug.Log("[Nest] La misión ya fue completada");
            }
            else if (!Squirrel.caught)
            {
                Debug.Log("[Nest] Necesitas capturar al conejo primero (Squirrel.caught = false)");
            }
        }
    }

    /// <summary>
    /// Verifica si el jugador está dentro del rango de distancia permitido
    /// </summary>
    /// <returns>True si está dentro del rango, false si está muy lejos</returns>
    private bool VerificarDistanciaJugador()
    {
        if (objetoReferencia == null)
        {
            Debug.LogError("[Nest] No se puede verificar distancia del jugador: objetoReferencia no está asignado");
            return false;
        }

        float distance = Vector3.Distance(transform.position, objetoReferencia.position);
        return distance <= maxDistance;
    }

    /// <summary>
    /// Verifica si el conejo está dentro del rango de distancia al nest
    /// </summary>
    /// <returns>True si está cerca, false si está lejos</returns>
    private bool VerificarDistanciaConejo()
    {
        if (squirrelTransform == null)
        {
            Debug.LogError("[Nest] No se puede verificar distancia del conejo: squirrelTransform no está asignado");
            return false;
        }

        float distance = Vector3.Distance(transform.position, squirrelTransform.position);
        return distance <= distanciaConejoCerca;
    }

    /// <summary>
    /// Obtiene la distancia actual al jugador (para debugging)
    /// </summary>
    /// <returns>Distancia en metros</returns>
    private float GetDistanceToJugador()
    {
        if (objetoReferencia == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(transform.position, objetoReferencia.position);
    }

    /// <summary>
    /// Obtiene la distancia actual al conejo (para debugging)
    /// </summary>
    /// <returns>Distancia en metros</returns>
    private float GetDistanceToConejo()
    {
        if (squirrelTransform == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(transform.position, squirrelTransform.position);
    }

    /// <summary>
    /// Completa la misión del conejo
    /// </summary>
    private void CompletarMision()
    {
        if (!active || home)
        {
            Debug.Log("[Nest] CompletarMision llamado pero la misión ya estaba completada");
            return;
        }

        Debug.Log("[Nest] Iniciando CompletarMision");

        // Marcar como completado inmediatamente
        home = true;
        active = false;

        // IMPORTANTE: Primero desactivar activate para detener el Update() del conejo
        // Luego marcar caught como false
        Squirrel.activate = false;
        Squirrel.caught = false; // El conejo ya no está "capturado" porque está en casa

        // Mostrar feedback
        StartCoroutine(ShowFeedback());

        // Ocultar recordatorio
        if (recordatorio != null)
        {
            recordatorio.SetActive(false);
        }

        // Desactivar el collider bloqueador (tag "Rabbit")
        DesactivarBloqueador();

        Debug.Log("[Nest] Misión completada. home=" + home);
    }

    /// <summary>
    /// Desactiva el collider que bloquea el paso
    /// </summary>
    private void DesactivarBloqueador()
    {
        GameObject[] rabbitObjects = GameObject.FindGameObjectsWithTag("Rabbit");

        foreach (GameObject rabbitObj in rabbitObjects)
        {
            // Desactivar todos los colliders
            Collider[] colliders = rabbitObj.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
                Debug.Log("[Nest] Collider desactivado en " + rabbitObj.name + ": " + col.GetType().Name);
            }

            // También buscar en hijos
            Collider[] childColliders = rabbitObj.GetComponentsInChildren<Collider>();
            foreach (Collider col in childColliders)
            {
                col.enabled = false;
            }
        }
    }

    private void ResetInteracting()
    {
        isInteracting = false;
    }

    public void OnLookAt()
    {
        // Solo mostrar puntero si el conejo está capturado y la misión no está completa
        if (!Squirrel.caught || !active)
        {
            return;
        }

        // Verificar distancias antes de mostrar el puntero
        if (!VerificarDistanciaJugador() || !VerificarDistanciaConejo())
        {
            return;
        }

        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar(); // Mostrar mano para indicar que puede soltar el conejo
            }
        }
    }

    public void OnLookAway()
    {
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.mira();
            }
        }
    }

    // Mantener OnMouseDown para compatibilidad con sistemas antiguos
    private void OnMouseDown()
    {
        if (InteractionDetector.instance == null)
        {
            if (active && Squirrel.caught && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                if (VerificarDistanciaJugador() && VerificarDistanciaConejo())
                {
                    CompletarMision();
                }
                else
                {
                    Debug.Log("[Nest] (OnMouseDown) Distancias no cumplen requisitos. Jugador: " + GetDistanceToJugador() + "m, Conejo: " + GetDistanceToConejo() + "m");
                }
            }
        }
    }

    IEnumerator ShowFeedback()
    {
        if (message != null)
        {
            message.text = LanguageManager.Instancia.ObtenerTexto("recordatorios.feedback");
        }
        if (feedback != null)
        {
            feedback.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            feedback.SetActive(false);
        }
    }

    // ============== DEBUGGING (Solo para Unity Editor) ==============

#if UNITY_EDITOR
    /// <summary>
    /// Dibuja el rango de distancia en el editor para visualizar
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Dibujar el rango de distancia permitido para el JUGADOR (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Dibujar el rango de distancia requerido para el CONEJO (azul)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaConejoCerca);

        // Si hay un objeto de referencia asignado (jugador), dibujar línea hacia él
        if (objetoReferencia != null)
        {
            float distance = Vector3.Distance(transform.position, objetoReferencia.position);

            // Color verde si está en rango, rojo si está fuera
            Gizmos.color = distance <= maxDistance ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, objetoReferencia.position);

            // Dibujar una esfera pequeña en el jugador
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(objetoReferencia.position, 0.5f);
        }
        else
        {
            // Si no hay objeto de referencia, intentar buscar el jugador para visualización
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                Gizmos.color = distance <= maxDistance ? Color.cyan : Color.magenta;
                Gizmos.DrawLine(transform.position, player.transform.position);
            }
        }

        // Si hay un squirrel asignado, dibujar línea hacia él
        if (squirrelTransform != null)
        {
            float distance = Vector3.Distance(transform.position, squirrelTransform.position);

            // Color cyan si está en rango, magenta si está fuera
            Gizmos.color = distance <= distanciaConejoCerca ? Color.cyan : Color.magenta;
            Gizmos.DrawLine(transform.position, squirrelTransform.position);

            // Dibujar una esfera pequeña en el conejo
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(squirrelTransform.position, 0.5f);
        }
    }
#endif
}