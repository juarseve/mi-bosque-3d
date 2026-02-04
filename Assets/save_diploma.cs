using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class save_diploma : MonoBehaviour
{
    public GameObject notifDiploma;
    public Text route;
    public Image imagenDiploma; // Referencia a la imagen del diploma para cambiar según idioma
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void save()
    {
        Debug.Log(Application.dataPath);
        
        // Cambiar la imagen del diploma según el idioma antes de exportar
        CambiarImagenDiplomaSegunIdioma();
        
        // Obtener el idioma actual desde PlayerPrefs
        string idioma = PlayerPrefs.GetString("idioma", "textos_espanol");
        string certificateFileName = "Certificado_es.png"; // Por defecto español
        
        // Seleccionar el certificado según el idioma
        if (idioma.Contains("portugues") || idioma.Contains("portuguese"))
        {
            certificateFileName = "Certificado_pt.png";
        }
        else // Para español e inglés usamos el español
        {
            certificateFileName = "Certificado_es.png";
        }
        
        string sourceFile = Application.dataPath + "/Sprites/" + certificateFileName;
        string localfolder = Application.dataPath;
        var array = localfolder.Split('/');
        var username = array[2];
        string destinationFile = "C:/Users/" + username + "/Downloads/Certificado" + Player.instance.playerData + ".png";
        try
        {
            File.Copy(sourceFile, destinationFile, true);
            route.text = destinationFile;
            StartCoroutine(feedback());
        }
        catch (IOException iox)
        {

            Debug.Log(iox.Message);
        }
        destinationFile = "D:/Users/" + username + "/Downloads/Certificado" + Player.instance.playerData.nombre + ".png";
        try
        {
            File.Copy(sourceFile, destinationFile, true);
            route.text = destinationFile;
            StartCoroutine(feedback());
        }
        catch (IOException iox)
        {

            Debug.Log(iox.Message);
        }
    }

    void CambiarImagenDiplomaSegunIdioma()
    {
        if (imagenDiploma == null)
        {
            Debug.LogWarning("Referencia a imagenDiploma no asignada en save_diploma");
            return;
        }

        // Obtener el idioma actual desde PlayerPrefs
        string idioma = PlayerPrefs.GetString("idioma", "textos_espanol");
        string certificateName = "Certificado_es"; // Por defecto español

        // Seleccionar el certificado según el idioma
        if (idioma.Contains("portugues") || idioma.Contains("portuguese"))
        {
            certificateName = "Certificado_pt";
        }
        else // Para español e inglés usamos el español
        {
            certificateName = "Certificado_es";
        }

        // Cargar el sprite desde Resources
        Sprite nuevoSprite = Resources.Load<Sprite>("Sprites/" + certificateName);
        
        if (nuevoSprite != null)
        {
            imagenDiploma.sprite = nuevoSprite;
            Debug.Log("Imagen del diploma cambiada a: " + certificateName);
        }
        else
        {
            Debug.LogWarning("No se encontró el sprite: Sprites/" + certificateName);
        }
    }

    IEnumerator feedback()
    {
        notifDiploma.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        notifDiploma.SetActive(false);
    }
}
