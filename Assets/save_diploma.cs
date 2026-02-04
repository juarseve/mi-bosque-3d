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
    public Sprite certificadoEspanol;  // Certificado_es.png
    public Sprite certificadoPortugues; // Certificado_pt.png
    
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

        // Seleccionar el sprite según el idioma
        Sprite spriteSeleccionado = certificadoEspanol; // Por defecto español

        if (idioma.Contains("portugues") || idioma.Contains("portuguese"))
        {
            spriteSeleccionado = certificadoPortugues;
            Debug.Log("Certificado en Portugués seleccionado");
        }
        else
        {
            spriteSeleccionado = certificadoEspanol;
            Debug.Log("Certificado en Español seleccionado");
        }

        // Cambiar la imagen
        if (spriteSeleccionado != null)
        {
            imagenDiploma.sprite = spriteSeleccionado;
            Debug.Log("Imagen del diploma cambiada. Idioma: " + idioma);
        }
        else
        {
            Debug.LogWarning("Sprite seleccionado es null. Verifica que ambos sprites estén asignados en el Inspector");
        }
    }

    IEnumerator feedback()
    {
        notifDiploma.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        notifDiploma.SetActive(false);
    }
}
