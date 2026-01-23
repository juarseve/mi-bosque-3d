using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MinimizarVideos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject videoCanvas;    
    [SerializeField] private VideoPlayer videoPlayer;   
    [SerializeField] private AudioSource audioSource;   
    [SerializeField] private GameObject videoCanvas2;    
    [SerializeField] private VideoPlayer videoPlayer2;   
    [SerializeField] private AudioSource audioSource2;   
    private bool VideoUnoMinimizado;

    private void Start()
    {
        VideoUnoMinimizado = false;
    }
    
    public void OnMinimizar()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop(); 
            Debug.Log("[MinimizarVideos] VideoPlayer 1 detenido");
        }

        if (audioSource != null)
        {
            audioSource.Stop(); 
            audioSource.mute = true;
            Debug.Log("[MinimizarVideos] AudioSource 1 detenido y muteado");
        }
        
        // Si el VideoPlayer tiene AudioSource asociado, detenerlo también
        if (videoPlayer != null)
        {
            foreach (AudioSource audio in videoPlayer.GetComponents<AudioSource>())
            {
                audio.Stop();
                audio.mute = true;
                Debug.Log("[MinimizarVideos] AudioSource asociado al VideoPlayer 1 detenido");
            }
        }

        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false); 
            Debug.Log("[MinimizarVideos] Canvas 1 ocultado");
        }

        // BÚSQUEDA AGRESIVA: Detener TODOS los AudioSources activos en la escena
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"[MinimizarVideos] Encontrados {allAudioSources.Length} AudioSources en total");
        foreach (AudioSource audio in allAudioSources)
        {
            if (audio.isPlaying)
            {
                Debug.Log($"[MinimizarVideos] Deteniendo AudioSource: {audio.gameObject.name}");
                audio.Stop();
                audio.mute = true;
            }
        }

        Debug.Log("[MinimizarVideos] ✓ Video 1 completamente detenido y minimizado.");
        VideoUnoMinimizado = true;
    }

    public void OnMinimizarVideo2()
    {
        if (videoPlayer2 != null)
        {
            videoPlayer2.Stop(); 
            Debug.Log("[MinimizarVideos] VideoPlayer 2 detenido");
        }

        if (audioSource2 != null)
        {
            audioSource2.Stop();
            audioSource2.mute = true;
            Debug.Log("[MinimizarVideos] AudioSource 2 detenido y muteado");
        }
        
        // Si el VideoPlayer tiene AudioSource asociado, detenerlo también
        if (videoPlayer2 != null)
        {
            foreach (AudioSource audio in videoPlayer2.GetComponents<AudioSource>())
            {
                audio.Stop();
                audio.mute = true;
                Debug.Log("[MinimizarVideos] AudioSource asociado al VideoPlayer 2 detenido");
            }
        }

        if (videoCanvas2 != null)
        {
            videoCanvas2.SetActive(false); 
            Debug.Log("[MinimizarVideos] Canvas 2 ocultado");
        }

        // BÚSQUEDA AGRESIVA: Detener TODOS los AudioSources activos en la escena
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"[MinimizarVideos] Encontrados {allAudioSources.Length} AudioSources en total");
        foreach (AudioSource audio in allAudioSources)
        {
            if (audio.isPlaying)
            {
                Debug.Log($"[MinimizarVideos] Deteniendo AudioSource: {audio.gameObject.name}");
                audio.Stop();
                audio.mute = true;
            }
        }

        Debug.Log("[MinimizarVideos] ✓ Video 2 completamente detenido y minimizado.");
    }
    public  void minimizadorVideos()
    {
        if (!VideoUnoMinimizado)
        {
            OnMinimizar();
        }
        else
        {
            OnMinimizarVideo2();
        }
    }
}

