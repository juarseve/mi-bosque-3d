using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recordControl : MonoBehaviour
{
    private GameObject recordatorio;
    private Animator animator;
    
    [Header("Configuración")]
    [Tooltip("Tiempo en segundos antes de resetear la animación")]
    public float tiempoResetAnimacion = 3f;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs en consola")]
    public bool mostrarLogs = false;
    
    private Coroutine coroutinaReset;
    
    void Start()
    {
        recordatorio = this.gameObject;
        animator = recordatorio.GetComponent<Animator>();
        
        if (animator == null && mostrarLogs)
        {
            Debug.LogWarning($"[recordControl] ⚠️ No se encontró Animator en '{gameObject.name}'");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            MostrarRecordatorio();
        }
    }
    
    /// <summary>
    /// Muestra el recordatorio y programa el reset de la animación
    /// </summary>
    private void MostrarRecordatorio()
    {
        if (animator != null)
        {
            // Activar el GameObject si está desactivado
            if (!recordatorio.activeSelf)
            {
                recordatorio.SetActive(true);
            }
            
            // Activar la animación
            animator.SetBool("show", true);
            
            if (mostrarLogs)
            {
                Debug.Log($"[recordControl] 👁️ Mostrando recordatorio '{gameObject.name}' con animación");
            }
            
            // Cancelar cualquier coroutina de reset anterior
            if (coroutinaReset != null)
            {
                StopCoroutine(coroutinaReset);
            }
            
            // Iniciar nueva coroutina para resetear la animación
            coroutinaReset = StartCoroutine(ResetearAnimacionDespuesDeTiempo());
        }
        else
        {
            // Si no hay Animator, solo activar el GameObject
            if (!recordatorio.activeSelf)
            {
                recordatorio.SetActive(true);
                
                if (mostrarLogs)
                {
                    Debug.Log($"[recordControl] 👁️ Mostrando recordatorio '{gameObject.name}' sin animación");
                }
            }
        }
    }
    
    /// <summary>
    /// Resetea el bool "show" de la animación después del tiempo configurado
    /// </summary>
    private IEnumerator ResetearAnimacionDespuesDeTiempo()
    {
        yield return new WaitForSeconds(tiempoResetAnimacion);
        
        if (animator != null)
        {
            animator.SetBool("show", false);
            
            if (mostrarLogs)
            {
                Debug.Log($"[recordControl] 🔄 Animación reseteada para '{gameObject.name}'");
            }
        }
        
        coroutinaReset = null;
    }
    
    /// <summary>
    /// Método público para resetear manualmente la animación
    /// </summary>
    public void ResetearAnimacion()
    {
        if (animator != null)
        {
            animator.SetBool("show", false);
            
            if (mostrarLogs)
            {
                Debug.Log($"[recordControl] 🔄 Animación reseteada manualmente para '{gameObject.name}'");
            }
        }
    }
    
    /// <summary>
    /// Método público para ocultar el recordatorio completamente
    /// </summary>
    public void OcultarRecordatorio()
    {
        if (animator != null)
        {
            animator.SetBool("show", false);
        }
        
        // Cancelar coroutina de reset si existe
        if (coroutinaReset != null)
        {
            StopCoroutine(coroutinaReset);
            coroutinaReset = null;
        }
        
        recordatorio.SetActive(false);
        
        if (mostrarLogs)
        {
            Debug.Log($"[recordControl] ❌ Recordatorio '{gameObject.name}' ocultado");
        }
    }
}
