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
    
    [Header("Sprites del Certificado según Idioma")]
    public Sprite certificadoEspanol;  // Certificado_es.png
    public Sprite certificadoPortugues; // Certificado_pt.png
    
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
            Debug.LogError("Sprite seleccionado es null. Verifica que ambos sprites estén asignados en el Inspector");
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