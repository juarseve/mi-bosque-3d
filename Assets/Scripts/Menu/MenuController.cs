using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;
using DataApi;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using System;

public class MenuController : MonoBehaviour
{
    public GameObject actionLogger;
    public InputField nombre;
    public InputField edad;
    public InputField unidadEducativa;
    public InputField identificacion;
    public Dropdown dropDownGenero;

    public SpriteRenderer personajeIMG;
    public Sprite[] personajes;
    public string character;
    public Text errorMsj;
    public GameObject errorPanel;
    public GameObject ingresoID;
    public GameObject unidadEducativaPanel;
    private PlayerData playerData;
    private string idCollege;
    private int n_edadF;
    private bool conti = false;
    private bool conti2 = true;

    /*Data para la API de Instituciones*/
    private const string URL = "https://200.10.147.118:80/";
    string urlInstutucines = URL + "api/Institution/list";
    public List<DataInstituciones> institucionesObject;
    public InputField colegio;
    private string reqInstituciones;
    //Código de profesor
    private string unidadEducativaID;
    private string idPlayer;
    private bool continuar = false;
    private bool existeColegio = false;

    /*Data para la API de Estudiantes*/
    public List<Estudiante> estudiantesObject;
    public InputField ingresoInputID;
    private string reqEstudiantes;
    public string urlestudiantes;

    public Peticiones PeticionesHTTP;
    private JObject JObjectStudentStatus;

    public GameObject botonContinuar;

    private void Start()
    {
        Debug.Log("START DE CONTROLLER");
        Button boton = botonContinuar.GetComponent<Button>();
        Image imagen = botonContinuar.GetComponent<Image>();
        if (SaveProfile.instance.SaveFileExists())
        {
            boton.interactable = true;
            imagen.color = Color.white;
        }
        else
        {
            boton.interactable = false;
            imagen.color = Color.black;
        }
    }
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        playerData = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().playerData;

        //Define el tipo de caracteres que se pueden ingresar dentro
        nombre.contentType = InputField.ContentType.Name;
        edad.contentType = InputField.ContentType.IntegerNumber;
        actionLogger = GameObject.Find("ActionLogger");
        //actionLogger.GetComponent<ActionLogger>().actionLogger.locacion = "Menu de partida";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Zen()
    {
        GameManager.ZenMode = true;
        SceneManager.LoadScene("Bosque");

    }
    public void NextScene(string name)
    {
        SceneManager.LoadScene(name);

    }

    public void NextToLobby()
    {
        try{
            actionLogger = GameObject.Find("ActionLogger");
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Lobby");
            actionLogger.GetComponent<ActionLogger>().actionLogger.locacion = "Lobby";
        }catch(Exception e)
        {

        }

        SceneManager.LoadScene("Lobby");

    }

    public void nextToVideo()
    {
        try
        {
            actionLogger = GameObject.Find("ActionLogger");
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Lobby");
            actionLogger.GetComponent<ActionLogger>().actionLogger.locacion = "Lobby";
        }
        catch (Exception e)
        {

        }
        
        NextScene("EscenaDeVideo");
    }


    public void Exit()
    {
        Application.Quit();
    }

    public void SelectCharacter(Image i)
    {
        personajeIMG.sprite = i.sprite;
    }

    public void CrearPerfil()
    {
        int n_edad;
        if (nombre.text == string.Empty || edad.text == string.Empty)
        {
            errorPanel.SetActive(true);
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó nombre");
            errorMsj.text = "Completo los campos Nombre o Edad para continuar";
        }
        else if (!(int.TryParse(edad.text, out n_edad)))
        {
            errorPanel.SetActive(true);
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó edad");
            errorMsj.text = "La edad que has ingresado es incorrecta";
        }
        else if (obtenerGenero(dropDownGenero) == "0")
        {
            errorPanel.SetActive(true);
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó género");
            errorMsj.text = "Selecciona el género por favor";
        }
        else if (unidadEducativa.text == string.Empty)
        {
            errorPanel.SetActive(true);
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó código de unidad");
            //errorMsj.text = "Escriba el código del profesor para continuar";
            string errorText = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_3");
            errorMsj.text = errorText;
        }
        else
        {
            if (n_edad < 5)
            {
                errorPanel.SetActive(true);
                errorMsj.text = "Hmmm estás muy pequeño para jugar.\nIngresa tu edad nuevamente";
                actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Edad inferior");
            }
            else if (n_edad > 90)
            {
                errorPanel.SetActive(true);
                errorMsj.text = "WOAH! Tienes " + n_edad + "! .\nPrueba ingresando tu edad nuevamente";
                actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Edad superior");
            }

            else
            {
                continuar = true;
                n_edadF = n_edad;
                activateIngresoId();
            }
        }

    }

    public bool validar(int x)
    {
        int n_edad;
        switch (x)
        {
            case 1:
                if (nombre.text == string.Empty || edad.text == string.Empty)
                {
                    errorPanel.SetActive(true);
                    //actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó nombre");
                    string errorText = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_0");
                    //errorMsj.text = "Completo los campos Nombre o Edad para continuar";
                    errorMsj.text = errorText;
                    return false;
                }
                else if (!(int.TryParse(edad.text, out n_edad)))
                {
                    errorPanel.SetActive(true);
                    //actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó edad");
                    string errorText = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_1");
                    //errorMsj.text = "La edad que has ingresado es incorrecta";
                    errorMsj.text = errorText;
                    return false;
                }
                else if(n_edad < 5)
                {
                    errorPanel.SetActive(true);
                    string errorText = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_4");
                    //errorMsj.text = "Hmmm estás muy pequeño para jugar.\nIngresa tu edad nuevamente";
                    errorMsj.text = errorText;
                    actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Edad inferior");
                    return false;
                }
                else if (n_edad > 90)
                {
                    errorPanel.SetActive(true);
                    string errorText0 = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_5_0");
                    string errorText1 = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_5_1");
                    //errorMsj.text = "WOAH! Tienes " + n_edad + "! .\nPrueba ingresando tu edad nuevamente";
                    errorMsj.text = errorText0 + n_edad + errorText1;
                    actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Edad superior");
                    return false;
                }
                else if (obtenerGenero(dropDownGenero) == "0")
                {
                    errorPanel.SetActive(true);
                    //actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó género");
                    string errorText1 = LanguageManager.Instancia.ObtenerTexto("mensajes_error.error_2");
                    errorMsj.text = "Selecciona el género por favor";
                    return false;
                }
                return true;
                

            case 2:
                if (unidadEducativa.text == string.Empty)
                {
                    errorPanel.SetActive(true);
                    //actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "No ingresó código de unidad");
                    errorMsj.text = "Escriba el código del profesor para continuar";
                    return false;
                }
                return true;

            default:
                return false;
        }

    }

    private string obtenerGenero(Dropdown dropdownGenero)
    {
        int value = dropdownGenero.value;
        string generoSeleccionado = dropdownGenero.options[value].text;

        if (generoSeleccionado.Contains("identificas"))
        {
            return "0";
        }
        else
        {
            if (generoSeleccionado == "Niño")
            {
                return "M";
            }
            else if (generoSeleccionado == "Niña")
            {
                return "F";
            }
            else
            {
                return "NA";
            }

        }
    }

    private string obtenerNombreGenero(Dropdown dropdownGenero)
    {
        int value = dropdownGenero.value;
        string generoSeleccionado = dropdownGenero.options[value].text;
        return generoSeleccionado;

    }

    public void OcultarPanel()
    {
        errorPanel.SetActive(false);
    }

    public void CargarPartida()
    {
        Debug.Log(SaveProfile.instance.SaveFileExists());
        if (SaveProfile.instance.SaveFileExists())
        {
            
            SaveProfile.instance.LoadGame();
            //Revisar se se creó datos en el servidor
            try
            {
                JObject statusRes = PeticionesHTTP.leerStatus("/responseRegister.json");
                int pStatus = (int)statusRes["status"];
                Debug.Log("status: " + pStatus);
                if (pStatus < 300)
                {
                    SaveProfile.instance.CreatePlayerData().setResponse(PeticionesHTTP.leerStatus("/responseRegister.json"));
                    PeticionesHTTP.login(SaveProfile.instance.CreatePlayerData());
                }
                else
                {
                    Debug.Log("Error con responseRegister.json");
                    actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Error en register de JSON cargando partida");
                    throw new WebException();
                }
            }
            catch
            {
                JObjectStudentStatus = PeticionesHTTP.RegistrarEstudiante(SaveProfile.instance.CreatePlayerData());
                JToken status = JObjectStudentStatus.GetValue("status");
                int value = status.ToObject<int>();
                if (value == 201)
                {
                    Debug.Log("Registro en el servidor completo!!!");
                    try
                    {
                        SaveProfile.instance.CreatePlayerData().setResponse(PeticionesHTTP.leerStatus("/responseRegister.json"));
                        PeticionesHTTP.login(SaveProfile.instance.CreatePlayerData());
                    }
                    catch
                    {
                        Debug.Log("Error en login.");
                        actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Error en login cargando partida");
                    }
                }
                else
                {
                    Debug.Log("Registro en el servidor fallido...");
                    actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Error en registro de servidor cargando partida");
                }
            }
            Debug.Log(SaveProfile.instance.CreatePlayerData().personajeSeleccionado);
            personajeIMG.sprite = Resources.Load<Sprite>("RECURSOS GRAFICOS DEL JUEGO 08-2020/PERSONAJES NIÑOS Y NIÑAS/"+SaveProfile.instance.CreatePlayerData().personajeSeleccionado);
            Debug.Log(Resources.Load<Sprite>("RECURSOS GRAFICOS DEL JUEGO 08-2020/PERSONAJES NIÑOS Y NIÑAS/NIÑO 3-08"));
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", "Menu Partidas-Mapa");
            actionLogger.GetComponent<ActionLogger>().actionLogger.locacion = "Mapa";
            NextScene("Mapa");
        }
        else
        {
            errorMsj.text = "No hay datos guardados";
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail","No se cargo partida-datos inexistentes");
            errorPanel.SetActive(true);
        }
        Debug.Log("OfflineMOde: " + GameManager.OfflineMode);
    }

    /*CONEXION CON API DE INSTITUCIONES,ESTUDIANTES Y ESCRITURA DE JSON */
    // Use this for initialization
    public void Request()
    {
        StartCoroutine(OnResponse(urlInstutucines));


    }

    private IEnumerator OnResponse(string path)
    {

        UnityWebRequest req = new UnityWebRequest(path);
        req.downloadHandler = new DownloadHandlerBuffer();
        yield return req.SendWebRequest();

        if (req.isNetworkError)
        {
            Debug.Log(req.error);
            nextToVideo();
        }
        else
        {
            // Show results as text
            reqInstituciones = req.downloadHandler.text;
            var instituciones = JsonConvert.DeserializeObject<List<DataInstituciones>>(reqInstituciones);
            institucionesObject = instituciones;
            Debug.Log(instituciones.Count);



        }




    }

    private IEnumerator OnResponseEs(string path)
    {
        UnityWebRequest req = new UnityWebRequest(path);
        req.downloadHandler = new DownloadHandlerBuffer();
        yield return req.SendWebRequest();

        if (req.isNetworkError)
        {
            Debug.Log(req.error);
            nextToVideo();
        }
        else
        {
            //SHow results as text
            reqEstudiantes = req.downloadHandler.text;
            var estudiantes = JsonConvert.DeserializeObject<List<Estudiante>>(reqEstudiantes);
            estudiantesObject = estudiantes;
            Debug.Log(estudiantes.Count);
            conti = true;
        }
    }

    public void activateIngresoId()
    {
        string genero = obtenerGenero(dropDownGenero);
        //PlayerData playerData;
        if (conti)
        {
            bool nohayunidad = true;
            foreach (DataInstituciones institucion in institucionesObject)
            {

                if (colegio.text == institucion.name)
                {
                    unidadEducativaPanel.SetActive(false);
                    ingresoID.SetActive(true);
                    unidadEducativaID = institucion.id;
                    urlestudiantes = URL + "institution/" + unidadEducativaID + "/students";
                    StartCoroutine(OnResponseEs(urlestudiantes));
                    nohayunidad = false;
                    conti2 = true;


                }

            }

            if (nohayunidad)
            {
                //aqui
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                ac.actionLogger.nombre = nombre.text;
                ac.actionLogger.user = playerData.UserName;
                ac.actionLogger.password = playerData.PassWord;
                ac.actionLogger.token = playerData.Token;
                nextToVideo();
            }
        }
        else {
            //nextToVideo();
            playerData = new PlayerData();
            GameManager.instance.SetCurrentStation(0);
            playerData.SetPlayerData(nombre.text, genero,  n_edadF, unidadEducativa.text, personajeIMG.sprite);
            /* SaveProfile.instance.SaveGame();
             GameManager.instance.SetPlayerData(playerData);
            Debug.Log("El genero es "+genero);
            Debug.Log("SE CREO EL PERSONAJE");*/
            JObjectStudentStatus = PeticionesHTTP.RegistrarEstudiante(playerData);
            //Peticiones.instance.RegistrarEstudiante(playerData);

        }

        JToken status = JObjectStudentStatus.GetValue("status");
        Debug.Log(status);
        int value = status.ToObject<int>();
        if (value == 201)
        {
            print("Next To video:");
            Debug.Log("Registro exitoso");
            Debug.Log(JObjectStudentStatus);
            playerData.setResponse(JObjectStudentStatus);
            PeticionesHTTP.login(playerData);
            Debug.Log("El genero es: " + playerData.genero);
            Debug.Log("El Estudiante ID es: " + playerData.EstudianteID);
            Debug.Log("El UserName es: " + playerData.UserName);
            Debug.Log("El PassWord es: " + playerData.PassWord);
            GameManager.instance.SetPlayerData(playerData);
            SaveProfile.instance.SaveGame();
            
            Debug.Log("SE CREO EL PERSONAJE");
            Debug.Log("Offline mode: " + GameManager.OfflineMode);
            actionLogger.GetComponent<ActionLogger>().actionLogger.online = true;
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Settings", "Online");
            //aqui
            ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
            ac.actionLogger.nombre = nombre.text;
            ac.actionLogger.user = playerData.UserName;
            ac.actionLogger.password = playerData.PassWord;
            ac.actionLogger.token = playerData.Token;
            nextToVideo();

        }
        else if (value >= 400 && value <500 )
        {
            errorPanel.SetActive(true);
            actionLogger.GetComponent<ActionLogger>().actionLogger.online = true;
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Settings", "Online");
            string errorJson = JObjectStudentStatus["error"].ToObject<string>();
            if (errorJson.Equals("Course does not exist"))
            {
                errorMsj.text = "No existe este curso. Ingresa uno correcto.";
                actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Curso inexistente");
            }                
            else if (errorJson.Equals("Course code must be 4 digits long."))
            {
                errorMsj.text = "Código incompleto o con más de 4 dígitos.";
                actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Fail", "Curso inexistente");
            }
            else if (errorJson.Equals("Age value is required."))
                errorMsj.text = "Debes decirnos tu edad.";
            else
                errorMsj.text = "Parece que no has puesto una edad válida. Dinos tu edad real por favor.";
        }
        else
        {
            print("Otro error (PROBABLEMENTE LA CONEXION). Next To video:");
            actionLogger.GetComponent<ActionLogger>().actionLogger.online = false;
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Settings", "Offline");
            Debug.Log(JObjectStudentStatus["error"]);
            playerData.genero = obtenerNombreGenero(dropDownGenero);
            Debug.Log("El genero es: " + playerData.genero);
            GameManager.instance.SetPlayerData(playerData);
            SaveProfile.instance.SaveGame();
            PeticionesHTTP.EraseStatus("/responseRegister.json");
            
            Debug.Log("SE CREO EL PERSONAJE");
            ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
            ac.actionLogger.nombre = nombre.text;
            ac.actionLogger.user = playerData.UserName;
            ac.actionLogger.password = playerData.PassWord;
            ac.actionLogger.token = playerData.Token;
            //aqui
            nextToVideo();
        }

        /*if (continuar)
        {
            PlayerData playerData = new PlayerData();
            GameManager.instance.SetCurrentStation(0);
            playerData.SetPlayerData(nombre.text, genero, n_edadF, unidadEducativa.text, personajeIMG.sprite);
            SaveProfile.instance.SaveGame();
            GameManager.instance.SetPlayerData(playerData);
            Debug.Log("El genero es " + genero);
            Debug.Log("SE CREO EL PERSONAJE");
            PeticionesHTTP.RegistrarEstudiante(playerData);
            //Peticiones.instance.RegistrarEstudiante(playerData);

        }*/


    }


    /*public void activateJugar()
    {
        NextScene("EscenaDeVideo");

        if (conti2)
        {
            foreach (Estudiante estudiante in estudiantesObject)
            {
                if (estudiante.identification.ToString() == ingresoInputID.text)
                {
                    playerData.SetPlayerData(unidadEducativaID, estudiante.id);
                    dataEstudiante dataEs = new dataEstudiante();
                    dataEs.id = estudiante.id;
                    dataEs.idCollegue = unidadEducativaID;
                    dataEs.idPlayer = estudiante.player;
                    string jason = JsonUtility.ToJson(dataEs);
                    File.WriteAllText("dataEstudiantes.JSON", jason);
                    Debug.Log(reqEstudiantes);

                }
            }
        }
        else {
            nextToVideo();
        }
    }



    /*public void setDataPlayer(PlayerData playerData)
    {
        playerData.SetPlayerData(unidadEducativaID, idPlayer);
    }*/

    private class dataEstudiante
    {
        public string id;
        public string idPlayer;
        public string idCollegue;
    }



}