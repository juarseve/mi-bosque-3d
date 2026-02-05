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
            }
            else
            {
                Debug.LogWarning("[FourthChallenge] quizGavilan no está asignado en el inspector");
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
