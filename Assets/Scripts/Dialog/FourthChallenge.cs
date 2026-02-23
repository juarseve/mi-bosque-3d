using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FourthChallenge : MonoBehaviour
{
    [Header("Sistema Antiguo (Deprecated)")]
    public PickupObject hamsterCage;
    public ChallengePass4 passScript;
    
    [Header("Sistema Nuevo - Quiz del Gavilán")]
    public QuizGavilan quizGavilan;
    public bool usarNuevoSistema = true;
    
    [Header("Diálogos de Recordatorio (Nuevo Sistema)")]
    public GameObject dialogoDesafioCompleto;
    public GameObject dialogoDesafioPendiente;
    public GameObject recordatorio;
    
    // Flag para evitar múltiples ejecuciones (patrón de ChallengePass)
    private bool act = true;

    /// <summary>
    /// Update se ejecuta cada frame, igual que ChallengePass4.Update()
    /// Verifica si el quiz se completó y actualiza los recordatorios
    /// </summary>
    void Update()
    {
        // Solo ejecutar si usamos el nuevo sistema
        if (!usarNuevoSistema) return;
        
        // Verificar que quizGavilan esté asignado
        if (quizGavilan == null)
        {
            Debug.LogWarning("[FourthChallenge] quizGavilan no está asignado, no se puede verificar completado");
            return;
        }
        
        // PATRÓN DE ChallengePass4: if (condición && act)
        // Condición: El quiz está completado
        // act: Evita múltiples ejecuciones
        if (quizGavilan.EstaCompletado() && act)
        {
            Debug.Log("[FourthChallenge] ✅ Quiz detectado como completado en Update()");
            
            // Marcar inmediatamente para evitar múltiples ejecuciones
            act = false;
            
            // ACTUALIZAR RECORDATORIOS (igual que ChallengePass4)
            if (dialogoDesafioPendiente != null)
            {
                dialogoDesafioPendiente.SetActive(false);
                Debug.Log("[FourthChallenge] Diálogo pendiente desactivado");
            }
            
            if (dialogoDesafioCompleto != null)
            {
                dialogoDesafioCompleto.SetActive(true);
                Debug.Log("[FourthChallenge] Diálogo completado activado");
            }
            
            // Ocultar recordatorio si existe
            if (recordatorio != null)
            {
                recordatorio.SetActive(false);
                Debug.Log("[FourthChallenge] Recordatorio desactivado");
            }
            
            Debug.Log("[FourthChallenge] ✅ Recordatorios actualizados correctamente");
        }
    }

    public void ActivateFourthChallenge()
    {
        if (usarNuevoSistema)
        {
            // Nuevo sistema: Quiz de Cadenas Tróficas
            Debug.Log("[FourthChallenge] Activando nuevo sistema - Quiz del Gavilán");
            QuizGavilan.inicio = DateTime.Now;
            
            if (quizGavilan != null)
            {
                // El quiz se iniciará cuando el jugador interactúe con el trigger
                Debug.Log("[FourthChallenge] Quiz Gavilán configurado, esperando interacción del jugador");
                
                // Resetear el flag para permitir la detección de completado
                act = true;
                Debug.Log("[FourthChallenge] Flag 'act' reseteado a true");
            }
            else
            {
                Debug.LogWarning("[FourthChallenge] quizGavilan no está asignado en el inspector");
            }
            
            // Mostrar diálogo pendiente al inicio
            if (dialogoDesafioPendiente != null)
            {
                dialogoDesafioPendiente.SetActive(true);
                Debug.Log("[FourthChallenge] Diálogo pendiente activado al iniciar");
            }
            
            if (dialogoDesafioCompleto != null)
            {
                dialogoDesafioCompleto.SetActive(false);
                Debug.Log("[FourthChallenge] Diálogo completado desactivado al iniciar");
            }
        }
        else
        {
            // Sistema antiguo: Recolectar comida para el gavilán
            Debug.Log("[FourthChallenge] Usando sistema antiguo");
            ChallengePass4.inicio = DateTime.Now;
            
            if (passScript != null)
            {
                passScript.sendStartReq();
            }
            
            if (hamsterCage != null)
            {
                hamsterCage.OnActivateFourthChallenge();
            }
            
            GameObject[] recolectables = GameObject.FindGameObjectsWithTag("Pick");
            foreach (GameObject g in recolectables)
            {
                Pick pickComponent = g.GetComponent<Pick>();
                if (pickComponent != null)
                {
                    pickComponent.activate = true;
                }
            }
        }
    }
}
