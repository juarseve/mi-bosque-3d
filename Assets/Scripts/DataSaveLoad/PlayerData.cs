using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using Inventory.InventoryWrapper;

[System.Serializable]
public class PlayerData
{
    public string nombre;
    public int edad;
    public string genero;
    public string personajeSeleccionado;
    public int numHojas;
    public int numEstacion=1;
    public int numDesafiosCompletados;
    public int maxStation;
    public InventoryWrapper inventoryWrapper;
    public InventoryWrapper accesoryWrapper;
    public bool[] isDiscovered;
    public bool[][] eventosEstaciones;
    public bool libroDesbloqueado;
    public bool mochilaDesbloqueada;
    public bool finishedGame = false;
    public string unidadEducativa;
    public string playerID;
    public int experiencia=0;
    public int nivel=0;
    public int[] limites = {0, 5, 10, 20, 35, 50, 80};

    public bool[] misiones = {false,false, false, false, false, false, false, false };
    public string[] logros = { "", "", "", "", "", "", "", "" };

    public DateTime gameStart;

    //API
    private int estudianteId;
    public int EstudianteID
    {
        get { return estudianteId; }
        set { estudianteId = value; }
    }

    private string userName;
    public string UserName
    {
        get { return userName; }
        set { userName = value; }
    }

    private string passWord;
    public string PassWord
    {
        get { return passWord; }
        set { passWord = value; }
    }

    private string token;
    public string Token
    {
        get { return token; }
        set { token = value; }
    }

    public bool isRegistered;

    private Dictionary<int, bool> preguntas = new Dictionary<int, bool>();

    public void SetPlayerData(string nombre, string genero, int edad, string unidadEducativa, Sprite personajeSeleccionado)
    {
        SetPlayerData(nombre, genero, edad, unidadEducativa, personajeSeleccionado.name);
    }



    public void SetPlayerData(string nombre, string genero, int edad, string unidadEducativa, string personajeSeleccionado){
        this.nombre=nombre;
        this.genero = genero;
        this.edad=edad;
        this.personajeSeleccionado = personajeSeleccionado;
        Debug.Log(personajeSeleccionado);
        this.unidadEducativa = unidadEducativa;
        this.isDiscovered = new bool[GameManager.instance.test.species.Count];
        this.eventosEstaciones = new bool[7][];
        this.inventoryWrapper=new InventoryWrapper();
        this.inventoryWrapper.slotInfoList=new List<SlotInfo>();

        this.accesoryWrapper = new InventoryWrapper();
        this.accesoryWrapper.slotInfoList = new List<SlotInfo>();

        this.estudianteId = 0;
        this.userName = "";
        this.passWord = "";
        this.isRegistered = false;
        gameStart = DateTime.Now;

    }

    /*public void SetPlayerData(string nombre, int edad, string personajeSeleccionado, string unidadEducativa,string playerID)
    {
        this.nombre = nombre;
        this.edad = edad;
        //this.personajeSeleccionado = personajeSeleccionado;
        this.unidadEducativa = unidadEducativa;
        this.isDiscovered = new bool[17];
        this.eventosEstaciones = new bool[7][];
        this.inventoryWrapper = new InventoryWrapper();
        this.inventoryWrapper.slotInfoList = new List<SlotInfo>();
        this.playerID = playerID;
    }*/

    public void setResponse(Newtonsoft.Json.Linq.JObject response)
    {
        if (response.HasValues)
        {

            int idEstudiante = response["payload"]["id"].ToObject<int>();
            string userName = response["payload"]["username"].ToObject<string>();
            string passWord = response["payload"]["password"].ToObject<string>();
            this.EstudianteID = idEstudiante;
            this.UserName = userName;
            this.PassWord = passWord;
            this.isRegistered = true;
        }
    }

    /*public void SetPlayerData(string unidadEducativa, string playerID)
    {
        this.unidadEducativa = unidadEducativa;
        this.playerID = playerID;
    }*/

    public int earnEXP(int gain)
    {
        experiencia += gain;
        return checkLVL();
    }

    public int checkLVL()
    {
        int[] limites = { 0, 5, 10, 20, 35, 50, 80 };
        Debug.Log("limites" + limites.Length);
        Debug.Log("nivel" + nivel);
        try
        {
            if (limites[nivel] <= experiencia)
            {
                nivel += 1;
            }
        }
        catch (Exception e)
        {
            Debug.Log("error en exp gain");
            return 0;
        }
        
        return nivel;
    }

    public void setIdPlayer(string idPlayer) {
        this.playerID = idPlayer;
    }

    public void addPregunta(int id, bool correcto)
    {

        if (preguntas.ContainsKey(id))
        {
            preguntas[id] = correcto;
        }
        else
        {
            preguntas.Add(id, correcto);
        }


    }

    public Dictionary<int, bool> getPreguntasDict()
    {
        return preguntas;
    }

}
