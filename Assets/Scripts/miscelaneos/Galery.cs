using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Video;

public class Galery : MonoBehaviour
{

    public string info_url;
    public string image_url;
    public int treeID;
    public TextMeshProUGUI titulo;
    public TextMeshProUGUI cuerpo;
    public Sprite[] imagenes;
    public int imagenActual = 0;
    public RawImage imagen;
    public AudioSource _audio;
    public static int numImages = 0;
    public GameObject panelGaleria;
    public GameObject buttonVideo;
    public VideoPlayer videoplayer;
    public VideoClip[] clips; 
   // public GameObject videoPlayer;

    public string[] especies = new string[] { "Teca", "Ceibo", "Bototillo", "Pechiche", "Guasmo", "Fernan Sánchez", "Iguana", "Ardilla de Guayaquil", "Momoto Gritón", "Pinzón Sabanero", "Gavilán Gris", "Garrapatero Piquiestriado", "Tangara Azul y Gris", "Búho Blanquinegro", "Garcilla Estriada", "Tirano Tropical", "Mosquero Rayado", "Jacaranda", "Guayacán", "Laurel De Judea", "Oso Perezoso", "Venado Cola Blanca", "Zorra Pampera" };

    private Arbol tree = null;
    public bool visible;

    void Update()
    {
        if (visible)
        {
            LoadInfoOffline();
            visible = false;
            numImages += 1;
            if (tree.Video==null)
            {
                buttonVideo.SetActive(false);
            }
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.A))
        {
            CargarImagen(1);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.D))
        {
            CargarImagen(-1);
        }
    }

    public void CargarImagen(int num)
    {
        imagenActual += num;
        if (imagenActual >= tree.Gallery.Length)
        {
            imagenActual = 0;
        }
        else if (imagenActual < 0)
        {
            imagenActual = tree.Gallery.Length - 1;
        }

        cuerpo.text = tree.Gallery[imagenActual].Description;
        Debug.Log(cuerpo.text);
        LoadImageOffLine(tree.Gallery[imagenActual].Id);
        LoadAudioOffline();
        createSpecieStadistic();
    }

    /*Modo offline*/
    public void LoadInfoOffline()
    {
        foreach(SpecieObject specie in GameManager.instance.test.species){
            if(specie.Name == name){
                Debug.Log("Especie encontrada");
                tree = new Arbol();
                tree.SpecieId = specie.SpecieId;
                tree.Id = specie.Id;
                tree.Name = specie.Name;
                tree.NameView = specie.NameView;
                tree.Family = specie.Family;
                tree.Gallery = specie.Gallery;
                tree.Video = specie.Video;
            }
        }
        titulo.text = tree.NameView; 
        String urlPrefix = "file://" + Application.streamingAssetsPath + "/"+  tree.Video;
        videoplayer.url = urlPrefix;
        CargarImagen(0);
    }

    public void LoadImageOffLine(int id)
    {
        imagen.texture = Resources.Load<Texture2D>("Specie/Images/"+id);
    }

    private void LoadAudioOffline()
    {
        _audio.clip = Resources.Load<AudioClip>("Specie/Audio/" + tree.Gallery[imagenActual].audioname);
        Debug.Log("Audio cargado");
        _audio.Play();
    }

    /*---- end ----*/

    public void createSpecieStadistic()
    {
        StadisticsData.Stadistics tmp = new StadisticsData.Stadistics("specie_data");
        StadisticsData.DataSpecie dat = new StadisticsData.DataSpecie(tree.Id.ToString());
        tmp.data = dat;
        GameManager.estas.lista.Add(tmp);
        if(GameManager.authToken != null){
            string json = JsonConvert.SerializeObject(tmp,Formatting.Indented);
            GameManager.instance.CallEnumerator(json);
        }
    }

    public void Limpiar()
    {
        titulo.text = String.Empty;
        cuerpo.text = String.Empty;
        buttonVideo.SetActive(true);
        imagen.texture = null;
        tree = null;
        panelGaleria.SetActive(false);
        imagenActual = 0;
    }
}

[Serializable]
public class Arbol
{
    public string SpecieId;
    public int Id;
    public string Name;
    public string NameView;
    public string Family;
    public string Video;
    public Gallery[] Gallery;
}

[Serializable]
public class ListSpecie
{
    public List<Arbol> lista;
}

