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
    [Tooltip("Habilitar sistema de recordatorios para esta estación")]
    public bool habilitarRecordatorios = true;
    
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
        
        // NO activar automáticamente - debe hacerse desde WallTrigger cuando el jugador entra
        // ActivarEstaEstacion(); // REMOVIDO
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] Estación {ID} inicializada (recordatorio NO activado aún)");
        }
    }
    
    private void Update()
    {
        // Solo procesar input de recordatorio si esta es la estación actual Y está habilitado
        if (habilitarRecordatorios && estacionActual == this && recordatorioActivo)
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
        // Destruir recordatorio de la estación anterior (solo si tienen recordatorios habilitados)
        if (estacionActual != null && estacionActual != this)
        {
            if (estacionActual.habilitarRecordatorios)
            {
                estacionActual.DestruirRecordatorio();
            }
        }
        
        // Establecer esta como la estación actual
        estacionActual = this;
        
        // Activar el recordatorio si está configurado Y habilitado
        if (habilitarRecordatorios && activarRecordatorioAlInicio)
        {
            ActivarRecordatorio();
        }
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] ✅ Estación {ID} activada como estación actual (Recordatorios: {(habilitarRecordatorios ? "HABILITADOS" : "DESHABILITADOS")})");
        }
    }
    
    /// <summary>
    /// Activa el recordatorio de esta estación (se podrá mostrar con la tecla H)
    /// </summary>
    public void ActivarRecordatorio()
    {
        // Si los recordatorios están deshabilitados, no hacer nada
        if (!habilitarRecordatorios)
        {
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] ⏸️ Recordatorios DESHABILITADOS para Estación {ID}");
            }
            return;
        }
        
        if (recordatorioEstacion == null)
        {
            if (mostrarLogs)
            {
                Debug.LogWarning($"[Estacion] ⚠️ No hay recordatorio asignado para la Estación {ID}");
            }
            return;
        }
        
        recordatorioActivo = true;
        
        // Desactivar completamente el recordatorio al inicio (se mostrará con H)
        recordatorioEstacion.SetActive(false);
        
        // Si tiene recordControl.cs, deshabilitarlo para evitar conflictos
        recordControl recordControlScript = recordatorioEstacion.GetComponent<recordControl>();
        if (recordControlScript != null)
        {
            recordControlScript.enabled = false;
            
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] 🔇 Script recordControl deshabilitado en '{recordatorioEstacion.name}' para evitar conflictos");
            }
        }
        
        // Si tiene Animator, resetear el parámetro "show"
        Animator animator = recordatorioEstacion.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("show", false);
        }
        
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
    /// Destruye el recordatorio de esta estación (usado al cambiar de estación)
    /// </summary>
    public void DestruirRecordatorio()
    {
        if (recordatorioEstacion == null) return;
        
        recordatorioActivo = false;
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] 💥 DESTRUYENDO recordatorio de Estación {ID} (GameObject: '{recordatorioEstacion.name}')");
        }
        
        Destroy(recordatorioEstacion);
        recordatorioEstacion = null;
    }
    
    /// <summary>
    /// Muestra el recordatorio visualmente (con animación si tiene Animator)
    /// </summary>
    private void MostrarRecordatorio()
    {
        // Validación temprana: si los recordatorios están deshabilitados, no hacer nada
        if (!habilitarRecordatorios)
        {
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] ⏸️ Recordatorios deshabilitados para Estación {ID}");
            }
            return;
        }
        
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
        
        // Activar el GameObject primero
        bool estabaDesactivado = !recordatorioEstacion.activeSelf;
        recordatorioEstacion.SetActive(true);
        
        // Intentar usar Animator si existe
        Animator animator = recordatorioEstacion.GetComponent<Animator>();
        
        if (animator != null)
        {
            // Si estaba desactivado, necesitamos esperar un frame para que el Animator se inicialice
            if (estabaDesactivado)
            {
                StartCoroutine(ActivarAnimacionConDelay(animator));
            }
            else
            {
                // Si ya estaba activo, activar inmediatamente
                animator.SetBool("show", true);
                StartCoroutine(ResetearAnimacionRecordatorio(animator, 3f));
            }
            
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] 👁️ Mostrando recordatorio de Estación {ID} con animación");
            }
        }
        else
        {
            // Método 2: Solo mantener activo si no tiene Animator
            if (mostrarLogs)
            {
                Debug.Log($"[Estacion] 👁️ Mostrando recordatorio de Estación {ID} sin animación");
            }
        }
    }
    
    /// <summary>
    /// Activa la animación con un pequeño delay para asegurar que el Animator esté inicializado
    /// </summary>
    private IEnumerator ActivarAnimacionConDelay(Animator animator)
    {
        // Esperar un frame para que el GameObject se active completamente
        yield return null;
        
        if (animator != null)
        {
            animator.SetBool("show", true);
            StartCoroutine(ResetearAnimacionRecordatorio(animator, 3f));
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
    
    /// <summary>
    /// Habilita el sistema de recordatorios para esta estación
    /// </summary>
    public void HabilitarRecordatorios()
    {
        habilitarRecordatorios = true;
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] ✅ Recordatorios HABILITADOS para Estación {ID}");
        }
    }
    
    /// <summary>
    /// Deshabilita el sistema de recordatorios para esta estación
    /// </summary>
    public void DeshabilitarRecordatorios()
    {
        habilitarRecordatorios = false;
        
        // Si hay un recordatorio activo, desactivarlo
        if (recordatorioActivo)
        {
            DesactivarRecordatorio();
        }
        
        if (mostrarLogs)
        {
            Debug.Log($"[Estacion] ❌ Recordatorios DESHABILITADOS para Estación {ID}");
        }
    }
    
    /// <summary>
    /// Verifica si los recordatorios están habilitados para esta estación
    /// </summary>
    public bool RecordatoriosHabilitados()
    {
        return habilitarRecordatorios;
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


