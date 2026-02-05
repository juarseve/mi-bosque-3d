using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

public class ChallengePass3 : MonoBehaviour
{
    public GameObject dialogoDesafioCompleto,dialogoDesafioPendiente;
    public GameObject haloNest;
    public GameObject recordatorio;
    public GameObject LogroSist;
    public AudioVocals audioVocals;
    public GameObject fpscontroller;
    private int levelId = 2;
    bool act=true;

    public static DateTime inicio;
    
    // Flag para verificar si LogrosGlobales está inicializado
    private bool logrosInicializados = false;

    void Update()
    {
        // Verificar si LogrosGlobales está inicializado antes de usarlo
        if (!logrosInicializados && LogroSist != null)
        {
            LogrosGlobales logrosGlobales = LogroSist.GetComponent<LogrosGlobales>();
            if (logrosGlobales != null && logrosGlobales.logros != null && logrosGlobales.logros.Count > 1 && logrosGlobales.misiones != null && logrosGlobales.misiones.Count > 1)
            {
                logrosInicializados = true;
                Debug.Log("[ChallengePass3] LogrosGlobales inicializado correctamente");
            }
            else
            {
                // Aún no está inicializado, esperar al siguiente frame
                return;
            }
        }
        
        if (Nest.home && act && logrosInicializados)
        {
            act = false; // Marcar inmediatamente para evitar múltiples ejecuciones
            
            Debug.Log("[ChallengePass3] Completando misión del conejo");
            
            // Seguir el mismo patrón que ChallengePass4 y ChallengePass5
            fpscontroller.GetComponent<Player>().gainEXP(3);
            LogroSist.GetComponent<LogrosGlobales>().ProgresarLogro(1);
            LogroSist.GetComponent<LogrosGlobales>().ProgresarMision(1, "Devolver el conejo a su madriguera");
            
            dialogoDesafioPendiente.SetActive(false);
            dialogoDesafioCompleto.SetActive(true);
            
            Player.instance.playerData.misiones[1] = true;
            Mision mision = (LogroSist.GetComponent<LogrosGlobales>()).misiones[1];
            
            ChallengePass4.inicio = DateTime.Now;
            
            if (!GameManager.OfflineMode)
            {
                Debug.Log("el level id es ----------------- " + this.levelId);
                Peticiones.instance.registerPlayerMission(mision.nombre, Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                Peticiones.instance.registerFinishMission(Player.instance.playerData, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), this.levelId);
            }
            else
            {
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                if (!GameManager.OfflineMode)
                {
                    ac.actionLogger.agregarAccion("Settings", "Offline");
                }

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

            Player.instance.playerData.logros[1] = DateTime.Now.ToString();
            
            if (!GameManager.OfflineMode)
            {
                Peticiones.instance.registerPlayerPrize((LogroSist.GetComponent<LogrosGlobales>()).logros[1].nombre, Player.instance.playerData);
            }
            else
            {
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                if (!GameManager.OfflineMode)
                {
                    ac.actionLogger.agregarAccion("Settings", "Offline");
                }

                ac.actionLogger.online = false;
                ac.actionLogger.agregarPeticion("prize", (LogroSist.GetComponent<LogrosGlobales>()).logros[1].nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                try
                {
                    ac.GetComponent<ActionLogger>().actionLogger.online = false;
                }
                catch (Exception e)
                {
                    Debug.Log("act logger component not found");
                }
            }
            
            haloNest.SetActive(false);
            recordatorio.SetActive(false);
            audioVocals.reproducirAlt();
            CreateStadistics();
        }
    }

    public void sendStartReq()
    {
        inicio = DateTime.Now;
        Debug.Log("Offline Mode: " + GameManager.OfflineMode);
        try
        {
            if (!GameManager.OfflineMode)
            {
                Debug.Log("Intento con online1");
                JObject res = Peticiones.instance.registerStartMission("Bosque-Estación 2", Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"));
                if (res["payload"]["GameLevelInstanceId"] != null)
                {
                    levelId = (int)res["payload"]["GameLevelInstanceId"];
                }
            }
            else
            {
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                if (!GameManager.OfflineMode)
                {
                    ac.actionLogger.agregarAccion("Settings", "Offline");
                }

                ac.actionLogger.online = false;
                ac.actionLogger.agregarPeticion("start mision", "Bosque-Estación 2", Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), null);
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
        catch
        {
            Debug.Log("Error al registrar inico de nivel.");
        }
    }

    public void CreateStadistics()
    {
        StadisticsData.Stadistics tmp1 = new StadisticsData.Stadistics("mission_data");
        string name = LogroSist.GetComponent<LogrosGlobales>().misiones[1].nombre;
        StadisticsData.DataMission dat1 = new StadisticsData.DataMission(inicio,name);
        tmp1.data = dat1;
        string json = JsonConvert.SerializeObject(tmp1,Formatting.Indented);
        GameManager.instance.CallEnumerator(json);
        GameManager.estas.lista.Add(tmp1);
        //
        StadisticsData.Stadistics tmp2 = new StadisticsData.Stadistics("experiencie_data");
        StadisticsData.DataExperiencie dat2 = new StadisticsData.DataExperiencie(5);
        tmp2.data = dat2;
        string json2 = JsonConvert.SerializeObject(tmp2,Formatting.Indented);
        GameManager.instance.CallEnumerator(json2);
        GameManager.estas.lista.Add(tmp2);
        //
        StadisticsData.Stadistics tmp3 = new StadisticsData.Stadistics("prize_data");
        string prize = LogroSist.GetComponent<LogrosGlobales>().logros[1].nombre;
        StadisticsData.DataPrize dat3 = new StadisticsData.DataPrize(prize);
        tmp3.data = dat3;
        string json3 = JsonConvert.SerializeObject(tmp3,Formatting.Indented);
        GameManager.instance.CallEnumerator(json3);
        GameManager.estas.lista.Add(tmp3);
    }
}
