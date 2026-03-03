using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ChallengePass4 : MonoBehaviour
{
    [Header("Diálogos")]
    public GameObject dialogoDesafioCompleto;
    public GameObject dialogoDesafioPendiente;
    public GameObject recordatorio;
    
    [Header("Sistema Antiguo (Deprecated)")]
    public GameObject food;
    public PlateFood plateFood;
    public GameObject haloPlato;
    
    [Header("Sistema Nuevo - Quiz del Gavilán")]
    public QuizGavilan quizGavilan;
    public bool usarNuevoSistema = true;
    
    [Header("Referencias")]
    public GameObject LogroSist;
    public AudioVocals audioVocals;
    public GameObject fpscontroller;
    public GameObject cage;
    
    private int levelId = 4;  // Cambiado de 3 a 4 para coincidir con QuizGavilan
    private bool act = true;
    
    public static DateTime inicio;
    
    // Flag para verificar si LogrosGlobales está inicializado
    private bool logrosInicializados = false;

    private void Update()
    {
        // Verificar si LogrosGlobales está inicializado antes de usarlo
        if (!logrosInicializados && LogroSist != null)
        {
            LogrosGlobales logrosGlobales = LogroSist.GetComponent<LogrosGlobales>();
            if (logrosGlobales != null && logrosGlobales.logros != null && logrosGlobales.logros.Count > 2 && logrosGlobales.misiones != null && logrosGlobales.misiones.Count > 2)
            {
                logrosInicializados = true;
                Debug.Log("[ChallengePass4] LogrosGlobales inicializado correctamente");
            }
            else
            {
                // Aún no está inicializado, esperar al siguiente frame
                return;
            }
        }
        
        // SISTEMA NUEVO: Quiz del Gavilán
        if (usarNuevoSistema)
        {
            // Verificar si el quiz fue completado
            if (quizGavilan != null && quizGavilan.EstaCompletado() && act && logrosInicializados)
            {
                act = false; // Marcar inmediatamente para evitar múltiples ejecuciones
                
                Debug.Log("[ChallengePass4] Quiz del Gavilán completado, ejecutando lógica de ChallengePass");
                
                // NOTA: QuizGavilan ya maneja:
                // - Destrucción de cage
                // - Actualización de Player.instance.playerData.misiones[2]
                // - Actualización de Player.instance.playerData.logros[2]
                // - gainEXP(4)
                // - ProgresarLogro(2) y ProgresarMision(2)
                // - Peticiones al servidor (registerPlayerMission, registerFinishMission, registerPlayerPrize)
                // - CreateStadistics()
                // - Inicio de ChallengePass5
                
                // Por lo tanto, solo actualizamos los elementos visuales que no están en QuizGavilan
                if (dialogoDesafioPendiente != null)
                {
                    dialogoDesafioPendiente.SetActive(false);
                    Debug.Log("[ChallengePass4] Diálogo pendiente desactivado");
                }
                
                if (dialogoDesafioCompleto != null)
                {
                    dialogoDesafioCompleto.SetActive(true);
                    Debug.Log("[ChallengePass4] Diálogo completado activado");
                }
                
                if (recordatorio != null)
                {
                    recordatorio.SetActive(false);
                    Debug.Log("[ChallengePass4] Recordatorio desactivado");
                }
                
                // Reproducir audio de completado
                if (audioVocals != null)
                {
                    audioVocals.reproducirAlt();
                    Debug.Log("[ChallengePass4] Audio de completado reproducido");
                }
                
                // Ocultar elementos del sistema antiguo si aún están visibles
                if (haloPlato != null)
                {
                    haloPlato.SetActive(false);
                }
                
                Debug.Log("[ChallengePass4] ✅ Desafío 4 completado exitosamente");
            }
        }
        // SISTEMA ANTIGUO: Recolectar comida para el gavilán
        else
        {
            if (plateFood != null && plateFood.lleno && act && logrosInicializados)
            {
                act = false;
                
                Debug.Log("[ChallengePass4] Sistema antiguo: Plato lleno, completando misión");
                
                fpscontroller.GetComponent<Player>().gainEXP(4);
                LogroSist.GetComponent<LogrosGlobales>().ProgresarLogro(2);
                LogroSist.GetComponent<LogrosGlobales>().ProgresarMision(2, "Alimentar al Gavilan");
                dialogoDesafioPendiente.SetActive(false);
                dialogoDesafioCompleto.SetActive(true);
                Player.instance.playerData.misiones[2] = true;
                ChallengePass5.inicio = DateTime.Now;
                Mision mision = (LogroSist.GetComponent<LogrosGlobales>()).misiones[2];
                
                if (!GameManager.OfflineMode)
                {
                    Debug.Log("el level id es ----------------- " + this.levelId);
                    Peticiones.instance.registerPlayerMission(mision.nombre, Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    Peticiones.instance.registerFinishMission(Player.instance.playerData, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), this.levelId);
                }
                else
                {
                    ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                    if (ac != null)
                    {
                        ac.actionLogger.online = false;
                        ac.actionLogger.agregarPeticion("mision", mision.nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                        ac.actionLogger.agregarPeticion("finish mision", "" + this.levelId, Player.instance.playerData.Token, null, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                        try
                        {
                            ac.GetComponent<ActionLogger>().actionLogger.online = false;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("act logger component not found");
                        }
                    }
                }

                Player.instance.playerData.logros[2] = DateTime.Now.ToString();
                
                if (!GameManager.OfflineMode)
                {
                    Peticiones.instance.registerPlayerPrize((LogroSist.GetComponent<LogrosGlobales>()).logros[2].nombre, Player.instance.playerData);
                }
                else
                {
                    ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                    if (ac != null)
                    {
                        ac.actionLogger.online = false;
                        ac.actionLogger.agregarPeticion("prize", (LogroSist.GetComponent<LogrosGlobales>()).logros[2].nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                        try
                        {
                            ac.GetComponent<ActionLogger>().actionLogger.online = false;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("act logger component not found");
                        }
                    }
                }

                if (food != null) food.SetActive(true);
                if (recordatorio != null) recordatorio.SetActive(false);
                if (audioVocals != null) audioVocals.reproducirAlt();
                
                if (GameObject.FindGameObjectWithTag("Bag") != null)
                {
                    var inventory = GameObject.FindGameObjectWithTag("Bag").GetComponent<Inventory>();
                    if (inventory != null)
                    {
                        inventory.TestRemoveF(1);
                        inventory.TestRemoveF(1);
                        inventory.TestRemoveF(2);
                    }
                }
                
                CreateStadistics();
                
                if (cage != null) Destroy(cage);
            }
        }
    }

    public void sendStartReq()
    {
        inicio = DateTime.Now;
        
        // Determinar el nombre de la misión según el sistema
        string missionName = usarNuevoSistema ? "Bosque-Estación 4" : "Bosque-Estación 3";
        
        Debug.Log("[ChallengePass4] Iniciando misión: " + missionName);
        
        try
        {
            if (!GameManager.OfflineMode)
            {
                JObject res = Peticiones.instance.registerStartMission(missionName, Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"));
                if (res != null && res["payload"] != null && res["payload"]["GameLevelInstanceId"] != null)
                {
                    levelId = (int)res["payload"]["GameLevelInstanceId"];
                    Debug.Log("[ChallengePass4] Level ID obtenido: " + levelId);
                }
            }
            else
            {
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                if (ac != null)
                {
                    ac.actionLogger.online = false;
                    ac.actionLogger.agregarPeticion("start mision", missionName, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), null);
                    try
                    {
                        ac.GetComponent<ActionLogger>().actionLogger.online = false;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("act logger component not found");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("[ChallengePass4] Error al registrar inicio de nivel: " + e.Message);
        }
    }

    public void CreateStadistics()
    {
        try
        {
            if (LogroSist == null)
            {
                Debug.LogWarning("[ChallengePass4] LogroSist es null, no se pueden crear estadísticas");
                return;
            }
            
            LogrosGlobales logrosGlobales = LogroSist.GetComponent<LogrosGlobales>();
            if (logrosGlobales == null || logrosGlobales.misiones == null || logrosGlobales.misiones.Count <= 2)
            {
                Debug.LogWarning("[ChallengePass4] LogrosGlobales no está inicializado correctamente");
                return;
            }
            
            StadisticsData.Stadistics tmp1 = new StadisticsData.Stadistics("mission_data");
            string name = logrosGlobales.misiones[2].nombre;
            StadisticsData.DataMission dat1 = new StadisticsData.DataMission(inicio, name);
            tmp1.data = dat1;
            string json = JsonConvert.SerializeObject(tmp1, Formatting.Indented);
            GameManager.instance.CallEnumerator(json);
            GameManager.estas.lista.Add(tmp1);
            
            StadisticsData.Stadistics tmp2 = new StadisticsData.Stadistics("experiencie_data");
            StadisticsData.DataExperiencie dat2 = new StadisticsData.DataExperiencie(4);
            tmp2.data = dat2;
            string json2 = JsonConvert.SerializeObject(tmp2, Formatting.Indented);
            GameManager.instance.CallEnumerator(json2);
            GameManager.estas.lista.Add(tmp2);
            
            if (logrosGlobales.logros != null && logrosGlobales.logros.Count > 2)
            {
                StadisticsData.Stadistics tmp3 = new StadisticsData.Stadistics("prize_data");
                string prize = logrosGlobales.logros[2].nombre;
                StadisticsData.DataPrize dat3 = new StadisticsData.DataPrize(prize);
                tmp3.data = dat3;
                string json3 = JsonConvert.SerializeObject(tmp3, Formatting.Indented);
                GameManager.instance.CallEnumerator(json3);
                GameManager.estas.lista.Add(tmp3);
            }
            
            Debug.Log("[ChallengePass4] Estadísticas creadas exitosamente");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ChallengePass4] Error al crear estadísticas: " + e.Message);
        }
    }
}
