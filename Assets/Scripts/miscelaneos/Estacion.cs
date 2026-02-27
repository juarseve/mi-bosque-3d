using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Estacion : MonoBehaviour
{
    [Header("Identificación")]
    public int ID;

    [Header("Gestión de Eventos")]
    public GameObject[] eventosActDesc;
    public GameObject spawn;
    
    [Header("MARCAR TODO COMO TRUE EN EL INSPECTOR")]
    public bool[] activos;
    
    [Header("Sistema de Recordatorios")]
    [Tooltip("GameObject del recordatorio específico de esta estación (se mostrará al presionar H)")]
    public GameObject recordatorioEstacion;
    
    [Tooltip("Activar automáticamente el recordatorio al entrar a la estación")]
    public bool activarRecordatorioAlInicio = true;
    
    [Tooltip("Tecla para mostrar/ocultar el recordatorio")]
    public KeyCode teclaMostrarRecordatorio = KeyCode.H;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs de debugging en consola")]
    public bool mostrarLogs = false;
    
    private Dictionary<string, int> dicEvntIndex = new Dictionary<string, int>();
    
    // Singleton estático para acceso global
    private static Estacion estacionActual;
    
    // Flag para saber si el recordatorio está activo
    private bool recordatorioActivo = false;

    private void Start()
    {
        // Inicializar eventos
        for (int i = 0; i < eventosActDesc.Length; i++)
        {
            dicEvntIndex.Add(eventosActDesc[i].name, i);
            eventosActDesc[i].SetActive(activos[i]);
        }
        
        // Activar esta estación como la actual
        ActivarEstaEstacion();
    }
    
    private void Update()
    {
        // Solo procesar input de recordatorio si esta es la estación actual
        if (estacionActual == this && recordatorioActivo)
        {
            // Detectar tecla para mostrar recordatorio
            if (Input.GetKeyDown(teclaMostrarRecordatorio))
            {
                MostrarRecordatorio();
            }
        }
    }
    
    /// <summary>
    /// Activa esta estación como la actual y su recordatorio
    /// </summary>
    public void ActivarEstaEstacion()
    {
        // Desactivar recordatorio de la estación anterior
        if (estacionActual != null && estacionActual != this)
        {
            estacionActual.DesactivarRecordatorio();
        }
        
        // Establecer esta como la estación actual
        estacionActual = this;
        
        // Activar el recordatorio si está configurado
        if (activarRecordatorioAlInicio)
        {
            ActivarRecordatorio();
        }
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] ✅ Estación {ID} activada como estación actual");
        }
    }
    
    /// <summary>
    /// Activa el recordatorio de esta estación (se podrá mostrar con la tecla H)
    /// </summary>
    public void ActivarRecordatorio()
    {
        if (recordatorioEstacion == null)
        {
            if (mostrarLogs)
            {
                Debug.LogWarning($"[Estacion] ⚠️ No hay recordatorio asignado para la Estación {ID}");
            }
            return;
        }
        
        recordatorioActivo = true;
        
        // Ocultar el recordatorio al inicio (se mostrará con H)
        recordatorioEstacion.SetActive(false);
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] 📝 Recordatorio de Estación {ID} activado (GameObject: '{recordatorioEstacion.name}')");
            Debug.Log($"[Estacion] Presiona '{teclaMostrarRecordatorio}' para mostrar el recordatorio");
        }
    }
    
    /// <summary>
    /// Desactiva el recordatorio de esta estación
    /// </summary>
    public void DesactivarRecordatorio()
    {
        if (recordatorioEstacion == null) return;
        
        recordatorioActivo = false;
        recordatorioEstacion.SetActive(false);
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] ❌ Recordatorio de Estación {ID} desactivado");
        }
    }
    
    /// <summary>
    /// Muestra el recordatorio visualmente (con animación si tiene Animator)
    /// </summary>
    private void MostrarRecordatorio()
    {
        if (recordatorioEstacion == null)
        {
            if (mostrarLogs)
            {
                Debug.LogWarning($"[Estacion] ⚠️ No hay recordatorio para mostrar en Estación {ID}");
            }
            return;
        }
        
        // Verificar si el juego está pausado
        if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
        {
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] ⏸️ Juego pausado, no se muestra recordatorio");
            }
            return;
        }
        
        // Activar el GameObject si está desactivado
        if (!recordatorioEstacion.activeSelf)
        {
            recordatorioEstacion.SetActive(true);
        }
        
        // Intentar usar Animator si existe (como recordControl.cs)
        Animator animator = recordatorioEstacion.GetComponent<Animator>();
        
        if (animator != null)
        {
            // Método 1: Usar animación con trigger "show"
            animator.SetBool("show", true);
            
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] 👁️ Mostrando recordatorio de Estación {ID} con animación");
            }
            
            // NUEVO: Resetear la animación después de un tiempo para permitir reproducirla de nuevo
            StartCoroutine(ResetearAnimacionRecordatorio(animator, 3f));
        }
        else
        {
            // Método 2: Solo activar si no tiene Animator
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] 👁️ Mostrando recordatorio de Estación {ID} con SetActive");
            }
        }
    }
    
    /// <summary>
    /// Resetea el parámetro "show" del Animator después de un tiempo
    /// </summary>
    /// <param name="animator">Animator a resetear</param>
    /// <param name="tiempo">Tiempo en segundos antes de resetear</param>
    private IEnumerator ResetearAnimacionRecordatorio(Animator animator, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        
        if (animator != null)
        {
            animator.SetBool("show", false);
            
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] 🔄 Animación de recordatorio reseteada para Estación {ID}");
            }
        }
    }
    
    /// <summary>
    /// Desactiva un evento específico
    /// </summary>
    /// <param name="GOEvento">GameObject del evento a desactivar</param>
    public void DesactivarEvento(GameObject GOEvento)
    {
        int ind = 0;
        if (dicEvntIndex.TryGetValue(GOEvento.name, out ind))
        {
            eventosActDesc[ind].SetActive(false);
            Destroy(eventosActDesc[ind]);
            activos[ind] = false;
        }
    }
    
    /// <summary>
    /// Obtiene la estación actual (método estático para acceso global)
    /// </summary>
    /// <returns>Estación actual o null si no hay ninguna</returns>
    public static Estacion ObtenerEstacionActual()
    {
        return estacionActual;
    }
    
    /// <summary>
    /// Obtiene el ID de la estación actual
    /// </summary>
    /// <returns>ID de la estación o -1 si no hay estación activa</returns>
    public static int ObtenerIDEstacionActual()
    {
        return estacionActual != null ? estacionActual.ID : -1;
    }
    
    private void OnDestroy()
    {
        // Limpiar la referencia estática si esta es la estación actual
        if (estacionActual == this)
        {
            estacionActual = null;
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Método de contexto para testear el recordatorio en el editor
    /// </summary>
    [ContextMenu("Testear Mostrar Recordatorio")]
    private void TestMostrarRecordatorio()
    {
        if (recordatorioEstacion == null)
        {
            Debug.LogError($"[Estacion] ❌ No hay recordatorio asignado en Estación {ID}");
            return;
        }
        
        Debug.Log($"[Estacion] 🧪 TEST: Mostrando recordatorio de Estación {ID}");
        MostrarRecordatorio();
    }
    
    [ContextMenu("Testear Activar Esta Estación")]
    private void TestActivarEstacion()
    {
        Debug.Log($"[Estacion] 🧪 TEST: Activando Estación {ID}");
        ActivarEstaEstacion();
    }
    #endif
}


