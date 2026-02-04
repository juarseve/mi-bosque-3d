using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB; // Requiere importar StandaloneFileBrowser

public class ControladorExportacion : MonoBehaviour
{
    [Header("Referencias UI Visible (Overlay)")]
    public Text textoNombreJugadorVisible; // El texto que ve el jugador

    [Header("Referencias UI Oculta (Photo Booth)")]
    public Camera camaraFoto;          // La cámara que creamos en Y=-5000
    public Text textoNombreJugadorFoto; // El texto en el canvas oculto
    public Image imagenDiploma;        // La imagen del diploma que cambiaremos según idioma
    
    [Header("Configuración Foto")]
    public int anchoFoto = 3840; // 4K
    public int altoFoto = 2160;

    public void IniciarExportacion()
    {
        // 1. Sincronizar datos: Copiamos lo que ve el usuario al certificado oculto
        textoNombreJugadorFoto.text = textoNombreJugadorVisible.text;

        // 2. Cambiar la imagen del diploma según el idioma
        CambiarImagenDiplomaSegunIdioma();

        // 3. Iniciar proceso
        StartCoroutine(CapturarYGuardar());
    }

    void CambiarImagenDiplomaSegunIdioma()
    {
        if (imagenDiploma == null)
        {
            Debug.LogError("Referencia a imagenDiploma no asignada en ControladorExportacion");
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

        // Cargar el sprite desde Resources o Assets
        Sprite nuevoSprite = Resources.Load<Sprite>("Sprites/" + certificateName);
        
        if (nuevoSprite != null)
        {
            imagenDiploma.sprite = nuevoSprite;
            Debug.Log("Imagen del diploma cambiada a: " + certificateName);
        }
        else
        {
            Debug.LogError("No se encontró el sprite: Sprites/" + certificateName);
            // Intentar cargar directamente desde Assets si no está en Resources
            nuevoSprite = Resources.Load<Sprite>("Assets/Sprites/" + certificateName);
            if (nuevoSprite != null)
            {
                imagenDiploma.sprite = nuevoSprite;
                Debug.Log("Imagen del diploma cambiada a: " + certificateName);
            }
        }
    }

    IEnumerator CapturarYGuardar()
    {
        // Esperamos al fin del frame para asegurar que el texto se actualizó
        yield return new WaitForEndOfFrame();

        // 3. Preparar RenderTexture de Alta Calidad
        RenderTexture rt = new RenderTexture(anchoFoto, altoFoto, 24);
        camaraFoto.targetTexture = rt;
        
        // 4. Encender cámara temporalmente y renderizar
        camaraFoto.gameObject.SetActive(true);
        camaraFoto.Render();
        camaraFoto.gameObject.SetActive(false); // Apagarla de inmediato

        // 5. Extraer píxeles
        RenderTexture.active = rt;
        Texture2D certificadoFinal = new Texture2D(anchoFoto, altoFoto, TextureFormat.RGB24, false);
        certificadoFinal.ReadPixels(new Rect(0, 0, anchoFoto, altoFoto), 0, 0);
        certificadoFinal.Apply();

        // Limpieza
        camaraFoto.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 6. Convertir a PNG
        byte[] bytes = certificadoFinal.EncodeToPNG();
        Destroy(certificadoFinal);

        // 7. Abrir ventana de "Guardar Como..."
        // Esto funciona en Windows, Mac y Linux (en el Build)
        string extensionList = "png";
        var path = StandaloneFileBrowser.SaveFilePanel("Guardar Certificado", "", "MiCertificado", extensionList);

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("Guardado en: " + path);
        }
        else 
        {
            Debug.Log("Cancelado por el usuario");
        }
    }
}