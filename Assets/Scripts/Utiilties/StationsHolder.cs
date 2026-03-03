using UnityEngine;
using System.Collections;

public class StationsHolder : MonoBehaviour {
    public Estacion[] estaciones;

    private void Awake() {
        // Retrasar la carga de eventos de estaciones para asegurar que GameManager esté listo
        StartCoroutine(InicializarEstacionesConDelay());
    }
    
    private IEnumerator InicializarEstacionesConDelay() {
        // Esperar hasta que GameManager esté completamente inicializado
        int intentos = 0;
        int maxIntentos = 50; // Máximo 5 segundos (50 * 0.1s)
        
        while (intentos < maxIntentos) {
            if (GameManager.instance != null && GameManager.instance.playerData != null) {
                Debug.Log("[StationsHolder] ? GameManager y playerData disponibles, inicializando estaciones...");
                break;
            }
            
            intentos++;
            if (intentos % 10 == 0) {
                Debug.LogWarning($"[StationsHolder] ? Esperando GameManager... (intento {intentos}/{maxIntentos})");
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        if (GameManager.instance == null || GameManager.instance.playerData == null) {
            Debug.LogError("[StationsHolder] ? No se pudo inicializar: GameManager o playerData es NULL después de esperar");
            yield break;
        }
        
        if (GameManager.instance.playerData.eventosEstaciones == null) {
            Debug.LogError("[StationsHolder] ? eventosEstaciones es NULL en playerData");
            yield break;
        }
        
        // Ahora sí, cargar los eventos de las estaciones
        for (int i = 0; i < GameManager.instance.playerData.eventosEstaciones.Length && i < estaciones.Length; i++)
        {
            if (GameManager.instance.playerData.eventosEstaciones[i] != null)
            {
                estaciones[i].activos = GameManager.instance.playerData.eventosEstaciones[i];
                Debug.Log($"[StationsHolder] ? Eventos cargados para estación {i + 1}");
            }
            else
            {
                Debug.LogWarning($"[StationsHolder] ?? Eventos NULL para estación {i + 1}, usando valores por defecto");
            }
        }
        
        // NUEVO: Activar la estación inicial basada en GameManager.currentStation
        int estacionActual = GameManager.instance.currentStation;
        if (estacionActual > 0 && estacionActual <= estaciones.Length)
        {
            // El array es 0-indexed, pero las estaciones son 1-indexed
            Estacion estacionInicial = estaciones[estacionActual - 1];
            if (estacionInicial != null)
            {
                estacionInicial.ActivarEstaEstacion();
                Debug.Log($"[StationsHolder] ?? Estación inicial {estacionActual} activada (recordatorio disponible)");
            }
            else
            {
                Debug.LogWarning($"[StationsHolder] ?? No se encontró la estación {estacionActual} en el array");
            }
        }
        else
        {
            Debug.LogWarning($"[StationsHolder] ?? currentStation inválido: {estacionActual}");
        }
        
        Debug.Log("[StationsHolder] ? Inicialización de estaciones completada");
    }
}