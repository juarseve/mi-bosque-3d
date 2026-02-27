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
        titulo.text = String.Empty;
        cuerpo.text = String.Empty;
        buttonVideo.SetActive(true);
        imagen.texture = null;
        tree = null;
        panelGaleria.SetActive(false);
        imagenActual = 0;
        
        // Volver a mostrar Panel3 (especies objetivo) usando referencia directa
        if (panel3Ref != null)
        {
            panel3Ref.SetActive(true);
            Debug.Log("[Galery] Panel3 (especies objetivo) reactivado usando referencia directa");
            
            // CRÍTICO: Reactivar las cajas de objetivos pendientes (especies no descubiertas)
            ReactivarCajasPendientes();
        }
        else
        {
            // Fallback: intentar buscar Panel3 si no hay referencia
            GameObject panel3 = GameObject.Find("Panel3");
            if (panel3 != null)
            {
                panel3.SetActive(true);
                Debug.Log("[Galery] Panel3 (especies objetivo) reactivado usando GameObject.Find (fallback)");
                ReactivarCajasPendientes();
            }
            else
            {
                Debug.LogWarning("[Galery] No se pudo encontrar Panel3 para reactivarlo");
            }
        }
    }
    
    /// <summary>
    /// Reactiva las cajas de objetivos (ardilla, iguana, pechiche) si las especies NO han sido descubiertas
    /// </summary>
    private void ReactivarCajasPendientes()
    {
        // Verificar qué especies faltan por descubrir usando BookPages.isDiscovered
        if (BookPages.instance == null || BookPages.instance.nombres == null || BookPages.instance.isDiscovered == null)
        {
            Debug.LogWarning("[Galery] No se puede verificar especies descubiertas - BookPages no disponible");
            return;
        }
        
        // Buscar índices de Ardilla, Iguana, Pechiche en el array de especies
        int ardillaIndex = System.Array.IndexOf(BookPages.instance.nombres, "Ardilla de Guayaquil");
        int iguanaIndex = System.Array.IndexOf(BookPages.instance.nombres, "Iguana");
        int pechicheIndex = System.Array.IndexOf(BookPages.instance.nombres, "Pechiche");
        
        // Reactivar caja de Ardilla si NO ha sido descubierta
        if (ardillaIndex >= 0 && ardillaIndex < BookPages.instance.isDiscovered.Length)
        {
            if (!BookPages.instance.isDiscovered[ardillaIndex] && ardillaCajaRef != null)
            {
                ardillaCajaRef.SetActive(true);
                Debug.Log("[Galery] ✓ Ardilla caja reactivada (especie no descubierta)");
            }
        }
        
        // Reactivar caja de Iguana si NO ha sido descubierta
        if (iguanaIndex >= 0 && iguanaIndex < BookPages.instance.isDiscovered.Length)
        {
            if (!BookPages.instance.isDiscovered[iguanaIndex] && iguanaCajaRef != null)
            {
                iguanaCajaRef.SetActive(true);
                Debug.Log("[Galery] ✓ Iguana caja reactivada (especie no descubierta)");
            }
        }
        
        // Reactivar caja de Pechiche si NO ha sido descubierta
        if (pechicheIndex >= 0 && pechicheIndex < BookPages.instance.isDiscovered.Length)
        {
            if (!BookPages.instance.isDiscovered[pechicheIndex] && pechicheCajaRef != null)
            {
                pechicheCajaRef.SetActive(true);
                Debug.Log("[Galery] ✓ Pechiche caja reactivada (especie no descubierta)");
            }
        }
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

