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
    
    // Referencia directa a Panel3 para poder reactivarlo correctamente
    [HideInInspector]
    public GameObject panel3Ref;
    
    // Referencias a las cajas de objetivos para reactivarlas si no han sido completadas
    [HideInInspector]
    public GameObject ardillaCajaRef;
    [HideInInspector]
    public GameObject iguanaCajaRef;
    [HideInInspector]
    public GameObject pechicheCajaRef;

    private void Awake()
    {
        Debug.Log("[Galery] Awake ejecutado para: " + gameObject.name);
        this.enabled = true;  // Asegurar que está habilitado
    }

    void Update()
    {
        if (visible)
        {
            Debug.Log("[Galery] Update - visible=true, name='" + name + "'");
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
        LoadImageOffLine(tree.Gallery[imagenActual].Id);
        LoadAudioOffline();
        createSpecieStadistic();
    }

    /*Modo offline*/
    public void LoadInfoOffline()
    {
        Debug.Log("[Galery] LoadInfoOffline - buscando especie: '" + name + "'");
        Debug.Log("[Galery] Total especies en DB: " + (GameManager.instance.test.species != null ? GameManager.instance.test.species.Count : 0));
        
        foreach(SpecieObject specie in GameManager.instance.test.species){
            Debug.Log("[Galery] Comparando: '" + specie.Name + "' == '" + name + "' ? " + (specie.Name == name));
            if(specie.Name == name){
                Debug.Log("[Galery] Especie encontrada: " + specie.Name);
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
        
        if (tree != null)
        {
            Debug.Log("[Galery] Configurando UI - titulo: " + tree.NameView);
            titulo.text = tree.NameView; 
            String urlPrefix = "file://" + Application.streamingAssetsPath + "/"+  tree.Video;
            videoplayer.url = urlPrefix;
            CargarImagen(0);
        }
        else
        {
            Debug.LogError("[Galery] *** tree es NULL - especie no encontrada ***");
        }
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
        Debug.Log("[Galery] Limpiar() iniciado");
        try
        {
            titulo.text = String.Empty;
            cuerpo.text = String.Empty;
            buttonVideo.SetActive(true);
            imagen.texture = null;
            tree = null;
            panelGaleria.SetActive(false);
            imagenActual = 0;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Galery] Error en Limpiar antes de RestaurarPanel3: " + e.Message);
        }
        finally
        {
            // SIEMPRE restaurar Panel3, incluso si algo falla arriba
            RestaurarPanel3();
            // Resetear IsGalery como respaldo (Continuar del BotonCerrar puede fallar
            // si el Teca de referencia está inactivo)
            ClickMouse.IsGalery = false;
            Debug.Log("[Galery] Limpiar() completado, IsGalery=false");
        }
    }
    
    /// <summary>
    /// Restaura Panel3 usando múltiples estrategias de fallback.
    /// </summary>
    private void RestaurarPanel3()
    {
        if (panel3Ref != null)
        {
            panel3Ref.SetActive(true);
            Debug.Log("[Galery] Panel3 reactivado usando referencia directa");
            return;
        }
        
        if (ClickMouse.Panel3Static != null)
        {
            ClickMouse.Panel3Static.SetActive(true);
            panel3Ref = ClickMouse.Panel3Static;
            Debug.Log("[Galery] Panel3 reactivado usando ClickMouse.Panel3Static");
            return;
        }
        
        // Último recurso: buscar entre todos los RectTransform (incluye inactivos)
        Debug.LogWarning("[Galery] panel3Ref es null, buscando Panel3 con FindObjectsOfTypeAll...");
        RectTransform[] allRects = Resources.FindObjectsOfTypeAll<RectTransform>();
        foreach (RectTransform rt in allRects)
        {
            if (rt.gameObject.name == "Panel Controles")
            {
                rt.gameObject.SetActive(true);
                panel3Ref = rt.gameObject;
                Debug.Log("[Galery] Panel3 encontrado y reactivado: " + rt.gameObject.name);
                return;
            }
        }
        
        Debug.LogError("[Galery] No se pudo encontrar Panel3 para reactivarlo");
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

