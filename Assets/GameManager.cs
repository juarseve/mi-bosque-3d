using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Audio;
using System.IO;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    //Esta clase controlará donde aparecerá el jugador en el bosque
    public static GameManager instance = null;
    public MapManager mapManager;
    public GameObject player;
    public PlayerData playerData;
    [HideInInspector] public Inventory mochila;
    public GameObject[] spawnArray;
    public int currentStation;
    public string gameId = "001";
    public int scene;
    public bool paused;

    public bool updateMode = false;
    public static bool OfflineMode = false; /*  Nuevo   */
    public static bool ZenMode = false;
    [HideInInspector] public static string authToken;
    //[HideInInspector] public static StadisticsData.StadisticsList estadisticas;
    [HideInInspector] public string url = SystemVariables.url_puerto + "/api/authentication/token";

    [SerializeField] private AudioMixer audioMixerMusic;

    [SerializeField] private string volumeAMM;

    [SerializeField] private string volumeAMA;
    [SerializeField] private float duration;
    [SerializeField][Range(0,1)] private float targetVolumeUpMusic;
    [SerializeField][Range(0,1)] private float targetVolumeDownMusic;
    [SerializeField][Range(0,1)] private float targetVolumeUpAmbient;
    [SerializeField][Range(0,1)] private float targetVolumeDownAmbient;
    public static StadisticsData estas;
    private AudioSource audioSource;
    private DateTime startPlaying;
    private string dataList;
    public SpecieObjectList test;

    void Start()
    {

        /*if (File.Exists(Application.dataPath + "/GreenForest/Data/estadisticas.json"))
        {
            string tmp = File.ReadAllText(Application.dataPath + "/GreenForest/Data/estadisticas.json");
            estas = JsonUtility.FromJson<StadisticsData>(tmp);
            Debug.Log("Lista cargada");
        }
        else
        {
            estas = new StadisticsData()
            {
                lista = new List<StadisticsData.Stadistics>()
            };
            Debug.Log("Nueva lista creada");
        }*/
        
    }

    //void Update()
    //{
    //    if (authToken != null && !updateMode)
    //    {
    //        StartCoroutine(SaveAllSpecies());
    //        //StartCoroutine(SaveList());
    //        updateMode = true;
    //    }
    //}
    public void Zen()
    {
        ZenMode = true;
        SceneManager.LoadScene("Bosque");

    }
    void comprobacion(){
        foreach(SpecieObject tmp in test.species){
            foreach (Gallery gtmp in tmp.Gallery){
                if(File.Exists(Application.dataPath + SystemVariables.image_url + gtmp.Id + ".jpg")){
                    Debug.Log("Archivo " + gtmp.Id + ".jpg existe");
                }else{
                    Debug.Log("Archivo " + gtmp.Id + ".jpg no existe");
                }
            }
        }
    }

    public void NextScene(string name)
    {
        SceneManager.LoadScene(name);

    }

    void Awake()
    {
        Debug.Log("GameManager Awake");
        Debug.Log(Application.persistentDataPath);
;        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (spawnArray.Length == 0)
        {
            spawnArray = GameObject.FindGameObjectsWithTag("Spawn");
        }
        playerData.isDiscovered = new bool[17];
        playerData.eventosEstaciones = new bool[7][];
        audioSource=GetComponent<AudioSource>();

        //Parte del Start
        startPlaying = DateTime.Now;
        estas = new StadisticsData()
        {
            lista = new List<StadisticsData.Stadistics>()
        };

        string path = "Specie/Description/species";
        string idiomaPref = PlayerPrefs.GetString("idioma");
        if (idiomaPref == "textos_english")
            path = "Specie/Description/speciesEnglish";
        else if (idiomaPref == "textos_portugues")
            path = "Specie/Description/speciesPortugues";
        dataList = Resources.Load<TextAsset>(path).text;
        Debug.Log("Contenido datalist: " + dataList);

        try
        {
            test = JsonConvert.DeserializeObject<SpecieObjectList>(dataList);
        }
        catch (System.Exception)
        {
            Debug.Log("No hay datos de este árbol");
        }
        Debug.Log("test especies:" + test.species[0].Id);
        //
        //StartCoroutine(getAuthToken());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        LanguageEvents.OnLanguageChanged += RecargarEspecies;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        LanguageEvents.OnLanguageChanged -= RecargarEspecies;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        spawnArray = GameObject.FindGameObjectsWithTag("Spawn");
        if (scene.name == "Bosque"  )
        {
            player.GetComponent<Player>().playerData = playerData;
            player.GetComponent<Player>().ActualizarUI();
            player.GetComponent<BookPages>().isDiscovered = playerData.isDiscovered;
            player.GetComponent<ShowBook>().enabled = playerData.libroDesbloqueado;
            player.GetComponentInChildren<ShowMochila>().enabled = playerData.mochilaDesbloqueada;
            mochila = GameObject.FindGameObjectWithTag("Bag").GetComponent<Inventory>();
            if (playerData.inventoryWrapper.slotInfoList.Count > 0)
            {
                mochila.LoadSavedInventory(playerData.inventoryWrapper, playerData.accesoryWrapper);
            }
            else
            {
                mochila.LoadEmptyInventory();
            }
            //Fade Ambient to 0
            StartCoroutine(StartFade(audioMixerMusic, volumeAMA, duration, targetVolumeUpAmbient));
            //Fade Music to 1
            StartCoroutine(StartFade(audioMixerMusic, volumeAMM, duration, targetVolumeDownMusic));

        }else if (scene.name == "Lobby")
        {
            player.GetComponent<Player>().playerData = playerData;
            player.GetComponentInChildren<ShowMochila>().enabled = playerData.mochilaDesbloqueada;
            mochila = GameObject.FindGameObjectWithTag("Bag").GetComponent<Inventory>();
            if (playerData.inventoryWrapper.slotInfoList.Count > 0)
            {
                mochila.LoadSavedInventory(playerData.inventoryWrapper, playerData.accesoryWrapper);
            }
            else
            {
                mochila.LoadEmptyInventory();
            }
            //Fade Ambient to 0
            StartCoroutine(StartFade(audioMixerMusic, volumeAMA, duration, targetVolumeUpAmbient));
            //Fade Music to 1
            StartCoroutine(StartFade(audioMixerMusic, volumeAMM, duration, targetVolumeDownMusic));

        }
        else if (scene.name == "EscenaDeVideo"){
            //Fade Music n Ambient to 0
            StartCoroutine(StartFade(audioMixerMusic, volumeAMA, duration, targetVolumeDownAmbient));
            StartCoroutine(StartFade(audioMixerMusic, volumeAMM, duration, targetVolumeDownMusic));
        }
        else if(scene.name == "Tutorial"){
            StartCoroutine(StartFade(audioMixerMusic, volumeAMA, duration, targetVolumeDownAmbient));
            StartCoroutine(StartFade(audioMixerMusic, volumeAMM, duration, targetVolumeDownMusic));
        }
        else{
            //Fade Music to 0
            StartCoroutine(StartFade(audioMixerMusic, volumeAMM, duration, targetVolumeUpMusic));
            //Fade Ambiente to 1
            StartCoroutine(StartFade(audioMixerMusic, volumeAMA, duration, targetVolumeDownAmbient));

        }



        try
        {
            mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        }
        catch (System.Exception)
        {
            Debug.Log("MapManager not present in this scene.");
        }

        if (player != null && spawnArray.Length > 0)
        {
            for (int i = 0; i < spawnArray.Length; i++)
            {
                if (spawnArray[i].GetComponent<Estacion>().ID == currentStation)
                {
                    GameObject spawn = spawnArray[i];
                    if (this.scene == 1)
                    {
                        GameObject specificSpawn = spawn.GetComponent<Estacion>().spawn;
                        player.transform.position = specificSpawn.transform.position;
                        player.transform.rotation = specificSpawn.transform.rotation;
                        //player.transform.position = spawn.transform.position;
                        //player.transform.rotation = spawn.transform.rotation;
                    }
                    else if (this.scene == 0)
                    {
                        mapManager.PinInicio = spawn.GetComponent<Pin>();
                    }
                }
            }
        }

    }

    void OnApplicationQuit()
    {
        StadisticsData.Stadistics tmp = new StadisticsData.Stadistics("game_data");
        int duration = DateTime.Now.Minute - startPlaying.Minute;
        StadisticsData.DataGame dat = new StadisticsData.DataGame(startPlaying,duration,instance.playerData.experiencia,Galery.numImages);
        tmp.data = dat;
        estas.lista.Add(tmp);
        if(authToken != null){
            string json = JsonConvert.SerializeObject(tmp,Formatting.Indented); 
            CallEnumerator(json);
        }else{
            var list = JsonConvert.SerializeObject(estas, Formatting.Indented);
            File.WriteAllText(Application.dataPath + "/GreenForest/Data/estadisticas.json", list);
        }
    }

    public void SetCurrentStation(int station)
    {
        currentStation = station;
    }

    /*
    public static IEnumerator getAuthToken()
    {

        UserTest test = new UserTest("rfcx-admin", "admin1902");
        string json = JsonUtility.ToJson(test);
        UnityWebRequest request = UnityWebRequest.Put(GameManager.instance.url, json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
            Debug.Log("No obtuvo respuesta"); 
            OfflineMode = true;                 
        }
        else
        {
            Debug.Log("Obtuvo respuesta");
            string data = request.downloadHandler.text;
            string data2 = (data.Replace("\\", "")).Trim('\"');
            try
            {
                //token = JsonUtility.FromJson<tokenClass>(data2);

                string[] lista = data2.Split(':');
                string tokenxd = lista[1];
                GameManager.authToken = tokenxd;

            }
            catch (System.Exception excep)
            {
                Debug.Log(excep);
                Debug.Log("No hay datos de este token.");
            }
        }


    }*/

    /*public void SetPlayerData(string nombre, int edad, string unidadEdu, string personajeSeleccionado)
    {
        playerData.nombre = nombre;
        playerData.edad = edad;
        playerData.personajeSeleccionado = personajeSeleccionado;
        playerData.unidadEducativa = unidadEdu;
    }*/

    public void SetPlayerData(PlayerData playerData)
    {
        this.playerData = playerData;
    }


    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    //private IEnumerator SaveAllSpecies()
    //{
    //    UnityWebRequest www = UnityWebRequest.Get(SystemVariables.list_species);
    //    www.SetRequestHeader("Authorization", GameManager.authToken);

    //    yield return www.SendWebRequest();

    //    if (www.responseCode.Equals(401))
    //    {
    //        //StartCoroutine(GameManager.getAuthToken());
    //        www.SetRequestHeader("Authorization", GameManager.authToken);
    //        yield return www.SendWebRequest();
    //    }

    //    if (www.isNetworkError || www.isHttpError)
    //    {
    //        Debug.Log(www.error);
    //    }
    //    else
    //    {
    //        string data = "{\"species\":" + www.downloadHandler.text + "}";
    //        File.WriteAllText(Application.dataPath + SystemVariables.info_url + "species.json", data);
    //    }
    //    Debug.Log("Se han guardado todas las descripciones");
    //    StartCoroutine(SaveAllAudios());
    //}

    private IEnumerator SaveAllAudios()
    {
        foreach(SpecieObject tmp in test.species){
            foreach (Gallery gtmp in tmp.Gallery){
                if(!File.Exists(Application.dataPath + SystemVariables.audio_url + gtmp.audioname + ".wav")){
                    UnityWebRequest w = UnityWebRequestMultimedia.GetAudioClip(SystemVariables.WebImaAud + tmp.Id + "/" + gtmp.audioname + ".ogg", AudioType.OGGVORBIS);
                    w.SetRequestHeader("Authorization", GameManager.authToken);
                    yield return w.SendWebRequest();

                    if (w.responseCode.Equals(401))
                    {
                        //StartCoroutine(GameManager.getAuthToken());
                        w.SetRequestHeader("Authorization", GameManager.authToken);
                        yield return w.SendWebRequest();
                    }

                    if (w.isNetworkError || w.isHttpError)
                    {
                        Debug.Log(w.error);
                    }
                    else
                    {
                        AudioClip audio1 = DownloadHandlerAudioClip.GetContent(w);
                        SavWav.Save(gtmp.audioname, audio1);
                    }
                }
            }
        }
        Debug.Log("Se guardaron todos los audios");
        StartCoroutine(SaveQuestions());
    }

    private IEnumerator SaveAllImages()
    {
        foreach(SpecieObject tmp in test.species){
            foreach (Gallery gtmp in tmp.Gallery){
                string filename = gtmp.Id + ".jpg";
                if(!File.Exists(Application.dataPath + SystemVariables.image_url + filename)){
                    string ruta = SystemVariables.WebImaAud + tmp.Id + "/" + gtmp.imagename + ".jpg";
                    Debug.Log(ruta);
                    UnityWebRequest w = UnityWebRequestTexture.GetTexture(ruta);
                    w.SetRequestHeader("Authorization", GameManager.authToken);
                    yield return w.SendWebRequest();

                    if (w.responseCode.Equals(401))
                    {
                        //StartCoroutine(GameManager.getAuthToken());
                        w.SetRequestHeader("Authorization", GameManager.authToken);
                        yield return w.SendWebRequest();
                    }
                    if (w.isNetworkError || w.isHttpError)
                    {
                        Debug.Log(w.error);
                    }
                    else
                    {
                        Texture2D imgToShare = DownloadHandlerTexture.GetContent(w);
                        SaveTextureToFile(imgToShare,filename);
                        Debug.Log(filename);
                    }
                }
            }
        }
        Debug.Log("Se descargaron todas las imagenes");
    }

    private IEnumerator SaveQuestions(){
        using (UnityWebRequest www = UnityWebRequest.Get(SystemVariables.url_puerto + "/api/bpv/question"))
        {

            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                if(www.isDone)
                {
                    string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                    File.WriteAllText(Application.dataPath + SystemVariables.question_url + "questions.json", jsonResult);
                    Debug.Log("Preguntas guardadas");
                }

            }
        }
        StartCoroutine(SaveAllImages());
    }

    void SaveTextureToFile(Texture2D texture, string filename)
    {
        string filepath = Application.dataPath + SystemVariables.image_url + filename;

        byte[] bytes;
        bytes = texture.EncodeToJPG();

        if (!(System.IO.File.Exists(filepath)))
        {
            System.IO.FileStream fileSave;
            fileSave = new FileStream(filepath, FileMode.Create);

            System.IO.BinaryWriter binary;
            binary = new BinaryWriter(fileSave);
            binary.Write(bytes);
            fileSave.Close();
        }
    }

    IEnumerator SendStadistics(string data){
        UnityWebRequest request = UnityWebRequest.Put(SystemVariables.url_puerto+"/api/escuela/create", data);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else{
            Debug.Log("Form Upload correctly");
        }
    }

    public void CallEnumerator(string data){
        StartCoroutine("SendStadistics",data);
    }
    public void TeletransportarJugador()
    {
        GameObject spawn = GameObject.Find("SpawnContrarreloj");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (spawn != null && player != null)
        {
            player.transform.position = spawn.transform.position;
            player.transform.rotation = spawn.transform.rotation; // opcional
            Debug.Log("Jugador teletransportado a spawn 1");
        }
        else
        {
            Debug.LogWarning("No se encontró el spawn o el player");
        }
    }


    private void RecargarEspecies(string idiomaPref)
    {
        string path = "Specie/Description/species";
        if (idiomaPref == "textos_english")
            path = "Specie/Description/speciesEnglish";
        else if (idiomaPref == "textos_portugues")
            path = "Specie/Description/speciesPortugues";

        dataList = Resources.Load<TextAsset>(path).text;

        try
        {
            test = JsonConvert.DeserializeObject<SpecieObjectList>(dataList);
            Debug.Log("✅ Especies recargadas para: " + idiomaPref);
        }
        catch
        {
            Debug.Log("❌ No se pudieron recargar especies.");
        }
    }

}