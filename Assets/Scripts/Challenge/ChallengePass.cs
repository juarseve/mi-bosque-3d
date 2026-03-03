using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ChallengePass : MonoBehaviour
{

    public static DateTime inicio = DateTime.Now;
    public GameObject dialogoDesafioCompleto;
    public GameObject dialogoDesafioPendiente;

    public GameObject ardilla;
    public GameObject iguana;
    public GameObject pepiche;

    public GameObject feedback;
    public Text message;
    public bool empezado;
    public AudioVocals audioVocals;
    bool restric = false;
    private bool sent=false;
    private int levelId = 1;
    public GameObject LogroSist;

    public GameObject fpscontroller;
    
    // OPTIMIZACIÓN: Flag para evitar ejecutar lógica de finalización múltiples veces
    private bool misionYaFinalizada = false;
    // Cache de LogrosGlobales para evitar GetComponent cada frame
    private LogrosGlobales _logrosGlobalesCache;

    //public GameObject actionLogger;

    void Start()
    {
        //actionLogger = GameObject.Find("ActionLogger");
        // Cachear referencia a LogrosGlobales
        if (LogroSist != null)
        {
            _logrosGlobalesCache = LogroSist.GetComponent<LogrosGlobales>();
        }
        
        // Desactivar UIElementVisibility en las cajas de objetivos para que ChallengePass
        // tenga control exclusivo de su visibilidad. UIElementVisibility las oculta cuando
        // currentStation != 1, lo cual conflictúa con ChallengePass que necesita mostrarlas
        // en cualquier estación mientras la misión esté activa.
        DesactivarUIElementVisibility(ardilla);
        DesactivarUIElementVisibility(iguana);
        DesactivarUIElementVisibility(pepiche);
    }
    
    private void DesactivarUIElementVisibility(GameObject obj)
    {
        if (obj == null) return;
        var uiVis = obj.GetComponent<UIElementVisibility>();
        if (uiVis != null)
        {
            uiVis.enabled = false;
            Debug.Log("[ChallengePass] UIElementVisibility desactivado en " + obj.name);
        }
    }

    private void Update()
    {
        // OPTIMIZACIÓN: Si la misión ya fue finalizada, solo actualizar UI y salir
        if (misionYaFinalizada)
        {
            if (dialogoDesafioPendiente != null) dialogoDesafioPendiente.SetActive(false);
            if (dialogoDesafioCompleto != null) dialogoDesafioCompleto.SetActive(true);
            return;
        }
        
        // Verificar si la misión 0 está completa usando datos del jugador (ÚNICA fuente de verdad)
        bool misionCompletada = false;
        
        if (Player.instance != null && Player.instance.playerData != null)
        {
            if (Player.instance.playerData.misiones != null && Player.instance.playerData.misiones.Length > 0)
            {
                misionCompletada = Player.instance.playerData.misiones[0];
            }
        }
        
        // SIEMPRE usar datos de misión para determinar si el desafío está completo.
        // NUNCA usar la visibilidad de las cajas de UI, porque es frágil
        // (se pueden ocultar al abrir la galería, animaciones, etc.)
        restric = !misionCompletada;
        
        // Sincronizar la visibilidad de las cajas con el estado real de la misión.
        // Esto actúa como red de seguridad: incluso si algo oculta una caja por error,
        // el siguiente frame la restaurará automáticamente.
        SincronizarCajasConMision();

        if (ardilla.activeSelf != iguana.activeSelf || iguana.activeSelf != pepiche.activeSelf ||ardilla.activeSelf != pepiche.activeSelf)
        {
            empezado = true;
        }

        if (!restric == true)
        {
            // CRÍTICO: Marcar como finalizada ANTES de ejecutar operaciones pesadas
            misionYaFinalizada = true;
            
            dialogoDesafioPendiente.SetActive(false);
            dialogoDesafioCompleto.SetActive(true);

            int[] numbers = { 0, 8 };

            foreach (int number in numbers) 
            {
                Player.instance.playerData.misiones[number] = true;
                LogrosGlobales LogrosGlobales = LogroSist.GetComponent<LogrosGlobales>();
                Mision mision = LogrosGlobales.misiones[number];
                
                Player.instance.playerData.logros[number] = DateTime.Now.ToString();
            }
            
            // OPTIMIZACIÓN: Mover StartCoroutine y peticiones FUERA del foreach
            // y solo ejecutar si empezado && !sent
            if (empezado && !sent)
            {
                Debug.Log("enviando estadísticas de final de misión...");
                LogrosGlobales LogrosGlobales = LogroSist.GetComponent<LogrosGlobales>();
                
                // Procesar misión 0
                Mision mision0 = LogrosGlobales.misiones[0];
                Debug.Log(Peticiones.instance.registerPlayerMission(mision0.nombre, Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")));
                
                if (!GameManager.OfflineMode)
                {
                    Debug.Log("Intento con online1");
                    Peticiones.instance.registerPlayerPrize(LogrosGlobales.logros[0].nombre, Player.instance.playerData);
                }
                else
                {
                    ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                    if (!GameManager.OfflineMode)
                    {
                        ac.actionLogger.agregarAccion("Settings", "Offline");
                    }

                    ac.actionLogger.online = false;
                    ac.actionLogger.agregarPeticion("prize", "" + LogrosGlobales.logros[0].nombre, Player.instance.playerData.Token, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    try
                    {
                        ac.GetComponent<ActionLogger>().actionLogger.online = false;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("act logger component not found");
                    }
                }
                
                sent = true;
                ChallengePass3.inicio = DateTime.Now;
                
                // CRÍTICO: StartCoroutine al FINAL, una sola vez
                StartCoroutine(ShowFeedback());
            }
        }
        else 
        {
            dialogoDesafioPendiente.SetActive(true);
            dialogoDesafioCompleto.SetActive(false);
        }
    }

    /// <summary>
    /// Sincroniza la visibilidad de las cajas de objetivos (ardilla, iguana, pepiche)
    /// con el estado real de la misión 0 en LogrosGlobales.
    /// Esto previene que bugs de UI (galería, animaciones) afecten los indicadores.
    /// IMPORTANTE: Respeta UIElementVisibility — las cajas solo se muestran en la estación requerida.
    /// </summary>
    private void SincronizarCajasConMision()
    {
        if (_logrosGlobalesCache == null)
        {
            if (LogroSist != null) _logrosGlobalesCache = LogroSist.GetComponent<LogrosGlobales>();
            if (_logrosGlobalesCache == null) return;
        }
        
        if (_logrosGlobalesCache.misiones == null || _logrosGlobalesCache.misiones.Count == 0) return;
        
        Mision mision0 = _logrosGlobalesCache.misiones[0];
        if (mision0 == null || mision0.requisitos == null) return;
        
        // Verificar si estamos en la estación donde las cajas deben ser visibles.
        // Las cajas tienen UIElementVisibility con requiredStation=1, pero lo desactivamos
        // en Start() para que ChallengePass controle la visibilidad directamente.
        // Las cajas se muestran en CUALQUIER estación mientras la misión esté activa.
        
        // Cada caja debe estar visible si la especie aún está pendiente (en requisitos)
        bool ardillaDebeVerse = mision0.requisitos.Contains("Ardilla de Guayaquil");
        bool iguanaDebeVerse = mision0.requisitos.Contains("Iguana");
        bool pechicheDebeVerse = mision0.requisitos.Contains("Pechiche");
        
        // Solo llamar SetActive si el estado cambió (optimización para evitar spam)
        if (ardilla != null && ardilla.activeSelf != ardillaDebeVerse)
        {
            ardilla.SetActive(ardillaDebeVerse);
        }
        if (iguana != null && iguana.activeSelf != iguanaDebeVerse)
        {
            iguana.SetActive(iguanaDebeVerse);
        }
        if (pepiche != null && pepiche.activeSelf != pechicheDebeVerse)
        {
            pepiche.SetActive(pechicheDebeVerse);
        }
        
        // RED DE SEGURIDAD: Verificar que el padre esté activo
        bool algunaDebeVerse = ardillaDebeVerse || iguanaDebeVerse || pechicheDebeVerse;
            if (algunaDebeVerse && ardilla != null)
            {
                bool padreInactivo = (ardilla.activeSelf && !ardilla.activeInHierarchy)
                                  || (iguana != null && iguana.activeSelf && !iguana.activeInHierarchy)
                                  || (pepiche != null && pepiche.activeSelf && !pepiche.activeInHierarchy);
                
                if (padreInactivo)
                {
                    Transform current = ardilla.transform.parent;
                    while (current != null)
                    {
                        if (!current.gameObject.activeSelf)
                        {
                            current.gameObject.SetActive(true);
                            Debug.Log("[ChallengePass] Padre '" + current.gameObject.name + "' reactivado (ocultaba cajas de objetivos)");
                        }
                        current = current.parent;
                    }
                }
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
                JObject res = Peticiones.instance.registerStartMission("Bosque-Estación 1", Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"));

            
            if (res["payload"]["GameLevelInstanceId"] != null)
            {
                levelId =(int) res["payload"]["GameLevelInstanceId"];
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
                ac.actionLogger.agregarPeticion("start mision", "Bosque-Estación 1", Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), null);
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

    IEnumerator ShowFeedback()
    {
        //actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Finish Bosque mision", "" + 1);
        LogroSist.GetComponent<LogrosGlobales>().ProgresarLogro(0);
        
        fpscontroller.GetComponent<Player>().gainEXP(3);
        if (!GameManager.OfflineMode)
        {
            Debug.Log("el level id es ----------------- " + this.levelId);
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
            
        //message.text = "Gran trabajo, avanza hasta el final de la estación";
        message.text = LanguageManager.Instancia.ObtenerTexto("recordatorios.feedback");
        empezado = false;
        yield return new WaitForSeconds(1.0f);
        feedback.SetActive(true);
        audioVocals.reproducirAlt();
        yield return new WaitForSeconds(2.0f);
        feedback.SetActive(false);
        CreateStadistics();
    }

    public void CreateStadistics(){
        Debug.Log("Creando estadísticas...");
        StadisticsData.Stadistics tmp1 = new StadisticsData.Stadistics("mission_data");
        string name = LogroSist.GetComponent<LogrosGlobales>().misiones[0].nombre;
        StadisticsData.DataMission dat1 = new StadisticsData.DataMission(inicio,name);
        tmp1.data = dat1;
        string json = JsonConvert.SerializeObject(tmp1,Formatting.Indented);
        GameManager.instance.CallEnumerator(json);
        GameManager.estas.lista.Add(tmp1);
        //
        StadisticsData.Stadistics tmp2 = new StadisticsData.Stadistics("experiencie_data");
        StadisticsData.DataExperiencie dat2 = new StadisticsData.DataExperiencie(3);
        tmp2.data = dat2;
        string json2 = JsonConvert.SerializeObject(tmp2,Formatting.Indented);
        GameManager.instance.CallEnumerator(json);
        GameManager.estas.lista.Add(tmp2);
        //
        StadisticsData.Stadistics tmp3 = new StadisticsData.Stadistics("prize_data");
        string prize = LogroSist.GetComponent<LogrosGlobales>().logros[0].nombre;
        StadisticsData.DataPrize dat3 = new StadisticsData.DataPrize(prize);
        tmp3.data = dat3;
        string json3 = JsonConvert.SerializeObject(tmp3,Formatting.Indented);
        GameManager.instance.CallEnumerator(json3);
        GameManager.estas.lista.Add(tmp3);
        Debug.Log("Estadisticas creadas");
    }


    public void Empezar()
    {
        empezado = true;
    }

}
