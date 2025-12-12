using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System;
using UnityEngine.Networking;

[Serializable]
public class Station
{
    public string StationId;
    public int Id;
    public string APIKey;
    public string Name;
    public int GameStation;
    public string Latitude;
    public string Longitude;
    public string AndroidVersion;
    public string ServicesVersion;
}

[Serializable]
public class Sensor
{
    public string DataId;
    public int StationId;
    public int SensorId;
    public int Id;
    public int Timestamp;
    public string Type;
    public string Value;
    public string Units;
    public string Location;
}

[Serializable]
public class Stations
{
    public List<Station> estaciones;
}

public class MapManager : MonoBehaviour {
    public Character character;
	public GameObject actionLogger;
	public Pin PinInicio;
	public Text Estacion;
	public Text Cargando;
	public GameObject Panel;
	public GameObject Canvas;
	public Text TextSensor;
	public SceneChanger sceneChanger;
	//GameStation es el ID
	public static Dictionary<int, int> diccionarioID = new Dictionary<int, int>();
	public static Dictionary<int, String> diccionarioNombre = new Dictionary<int, String>();
   	bool flagT = false;
   	bool flagH = false;
	public GameObject[] pines;

    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;
    public GameObject cn;

    [DllImport("__Internal")]
    private static extern string GetTemperature(int station_id);

    private void Start(){
		for (int i = 0; i < GameManager.instance.playerData.maxStation; i++)
		{
			pines[i].GetComponent<Pin>().desbloqueado=true;
			SpriteRenderer spriteRenderer=pines[i].GetComponent<SpriteRenderer>();
			var color=spriteRenderer.color;
			color.a=1f;
			spriteRenderer.color=color;
		}

		StartCoroutine(GetStations());
		Panel.SetActive(false);
		character.Iniciar(this, PinInicio);
		flagT = false;
		flagH = false;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE 
        up.SetActive(false);
        down.SetActive(false);
        left.SetActive(false);
        right.SetActive(false);
        cn.SetActive(false);
#endif
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        actionLogger = GameObject.Find("ActionLogger");
	}

    public void Update(){
		if (character.IsMoving){
			return;
		}
		CheckForInput();
	}
    public void enterEstacion(int estacion)
    {
        if (estacion != 0)
        {
            Panel.SetActive(true);
            if (diccionarioNombre.ContainsKey(estacion))
            {
                actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Menu Mapa-Bosque-" + estacion);
                Cargando.text = string.Format("Entrando a: " + diccionarioNombre[estacion]);
                Canvas.SetActive(false);
            }
            sceneChanger.FadeToLevel(estacion);
        }
    }

    private void CheckForInput(){
		if (Input.GetKeyUp(KeyCode.Return) && character.PinActual.estacion.ID != 0){
			Panel.SetActive(true);
			if (diccionarioNombre.ContainsKey(character.PinActual.estacion.ID))
			{
				actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Menu Mapa-Bosque-" + character.PinActual.estacion.ID);
				Cargando.text = string.Format("Entrando a: " + diccionarioNombre[character.PinActual.estacion.ID]);
				Canvas.SetActive(false);
			}
			sceneChanger.FadeToLevel(character.PinActual.estacion.ID);
		} else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)){
			character.TrySetDireccion(Direccion.Arriba);
		} else if (Input.GetKeyUp(KeyCode.DownArrow)|| Input.GetKeyUp(KeyCode.S)){
			character.TrySetDireccion(Direccion.Abajo);
		} else if (Input.GetKeyUp(KeyCode.LeftArrow)|| Input.GetKeyUp(KeyCode.A)){
			character.TrySetDireccion(Direccion.Izquierda);
		} else if (Input.GetKeyUp(KeyCode.RightArrow)|| Input.GetKeyUp(KeyCode.D)){
			character.TrySetDireccion(Direccion.Derecha);
        }
        else if (Input.GetKeyUp(KeyCode.Return) && character.PinActual.estacion.ID == 0)
        {
            GameManager.instance.scene = 2;
			actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Menu Mapa-Lobby");
			sceneChanger.FadeToLevel(character.PinActual.estacion.ID);
           
        }
#if UNITY_ANDROID || UNITY_IOS
        if (cn.GetComponent<FixedButton>().Pressed && character.PinActual.estacion.ID != 0)
        {
            Panel.SetActive(true);
            if (diccionarioNombre.ContainsKey(character.PinActual.estacion.ID))
            {
                Cargando.text = string.Format("Entrando a: " + diccionarioNombre[character.PinActual.estacion.ID]);
                Canvas.SetActive(false);
            }
			actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Menu Mapa-Bosque-"+ character.PinActual.estacion.ID);
            sceneChanger.FadeToLevel(character.PinActual.estacion.ID);
        }
        else if (up.GetComponent<FixedButton>().Pressed) { character.TrySetDireccion(Direccion.Arriba); }
        else if (down.GetComponent<FixedButton>().Pressed) { character.TrySetDireccion(Direccion.Abajo); }
        else if (right.GetComponent<FixedButton>().Pressed) { character.TrySetDireccion(Direccion.Derecha); }
        else if (left.GetComponent<FixedButton>().Pressed) { character.TrySetDireccion(Direccion.Izquierda); }
#endif
	}

	public void UpdateGUI(){
		StartCoroutine(GetTemp());
	}

	IEnumerator GetStations()
    {
		using (UnityWebRequest www = UnityWebRequest.Get(SystemVariables.url_puerto+"/api/Station"))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string descarga = www.downloadHandler.text;
                string JSONToParse = "{\"estaciones\":" + descarga + "}";
                
                Stations datos = JsonUtility.FromJson<Stations>(JSONToParse);

                foreach (var dato in datos.estaciones)
	            {
	                if (!diccionarioID.ContainsKey(dato.GameStation))
	                {
	                    diccionarioID.Add(dato.GameStation, dato.Id);
	                }

	                if (!diccionarioNombre.ContainsKey(dato.GameStation))
	                {
	                    diccionarioNombre.Add(dato.GameStation, dato.Name);
	                }
	            }
            }
        }
	}

	IEnumerator GetTemp()
    {
    	if (diccionarioID.ContainsKey(character.PinActual.estacion.ID))
    	{
    		Sensor datos1 = null;
    		Sensor datos2 = null;
			string url_temp= SystemVariables.url+"/api/station/" + diccionarioID[character.PinActual.estacion.ID] + "/sensor/2/data/lastdata";
    		//SOLO PARA TESTEO Y EVITAR ERRORES
    		//string url_temp=SystemVariables.url+"/api/station/16/sensor/2/data/lastdata";
			using (UnityWebRequest www = UnityWebRequest.Get(url_temp))
	        {
	            yield return www.SendWebRequest();

	            if (www.isNetworkError || www.isHttpError)
	            {
	                Debug.Log(www.error);
	            }
	            else
	            {
	                string descarga = www.downloadHandler.text;

	                try {
						datos1 = JsonUtility.FromJson<Sensor>(descarga);
					} catch (System.Exception){
						flagT = true;
						Debug.Log("No hay datos de Temperatura en la estacion " + diccionarioNombre[character.PinActual.estacion.ID]);
					}
	            }
	        }
			string url_hum= SystemVariables.url+"/api/station/" + diccionarioID[character.PinActual.estacion.ID] + "/sensor/1/data/lastdata";
    		//SOLO PARA TESTEO
			//string url_hum=SystemVariables.url+"/api/station/16/sensor/1/data/lastdata";
	        using (UnityWebRequest www = UnityWebRequest.Get(url_hum))
	        {
	            yield return www.SendWebRequest();

	            if (www.isNetworkError || www.isHttpError)
	            {
	                Debug.Log(www.error);
	            }
	            else
	            {
	                string descarga = www.downloadHandler.text;
	                
	                try {
	                	datos2 = JsonUtility.FromJson<Sensor>(descarga);
					} catch (System.Exception){
						flagH = true;
						Debug.Log("No hay datos de Humedad en la estacion " + diccionarioNombre[character.PinActual.estacion.ID]);
					}
	            }
	        }

	        if (flagT & flagH){
	        	TextSensor.text = string.Format("Temperatura:\nHumedad:");
	        } else if (flagT) {
	        	TextSensor.text = string.Format("Humedad: {2} {3}", datos2.Value, datos2.Units);
	        } else if (flagH) {
	        	TextSensor.text = string.Format("Temperatura: {0} {1}", datos1.Value, datos1.Units);
	        } else {
	        	TextSensor.text = string.Format("Temperatura: {0} {1}\nHumedad: {2} {3}", datos1.Value, datos1.Units, datos2.Value, datos2.Units);
	        }

			Estacion.text = string.Format("{0}", diccionarioNombre[character.PinActual.estacion.ID]);
			flagT = false;
			flagH = false;
    	}
    	else
		{
			TextSensor.text = "";
			Estacion.text = "";
		}
	}
}
