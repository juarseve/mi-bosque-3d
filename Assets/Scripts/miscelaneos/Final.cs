using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;


public class Final : MonoBehaviour
{

    public Estacion station;
    public GameObject stationScreen, panelPersonaje, canvasDialogo, Panel, mira, certificadoCanvas, Contenido;
    public Text dialogoPersonaje;
    public Sprite medalla;
    private string texto, nestrellas, ndesafios, nestaciones;
    private string final, final2, final3, final4;
    public AudioClip audioClip;
    public AudioSource audioSource;
    public GameObject CanvasJoysticks;



    void DestroyScriptInstance()
    {
        // Removes this script instance from the game object
        Destroy(this);
    }

    public void GuardarFinal()
    {
        GameManager.instance.SetPlayerData(Player.instance.playerData);

        SaveProfile.instance.SaveGame();
    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.gameObject.tag == "Player")
        {
            Preguntas();
            gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    private void Preguntas()
    {
#if UNITY_ANDROID || UNITY_IOS
        CanvasJoysticks.SetActive(false);
#endif

        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        //Panel.SetActive(false);
        mira.SetActive(false);
        MenuPausa.instance.Pausar();
        Time.timeScale = 1f;
        audioSource.clip = audioClip;
        audioSource.Play();
        texto = "¡Has completado el recorrido con éxito !!";
        nestrellas = "\n- Conseguiste " + Player.instance.playerData.numHojas.ToString() + " hojas en los desafíos";
        ndesafios = "\n- Misiones completados correctamente " + Player.instance.playerData.numDesafiosCompletados.ToString() + " de 7";
        //nestaciones = "\n \n - Estaciones visitadas " + stations.ToString() + " de 6";
        final = texto + nestrellas + ndesafios + nestaciones;
        final2 = "Durante toda tu aventura:\nDescubriste especies.\nAyudaste al conejo bebé llegar a su madriguera.\nCompletaste el Test de Sabiduría sobre Cadenas Tróficas.\nApagaste un incendio.";
        final3 = "Reciclaste basura de nuestro bosque.\nNos ayudaste a reforestar.\nPor esto y mucho más...";
        final4 = Player.instance.playerData.nombre + " eres acreedor de la medalla de protector del Bosque La Prosperina.\nRecuerda siempre cuidar de los diversos ecosistemas del planeta.\nSigue así, campeón.";
        Dialogue dialogue = new Dialogue();
        dialogue.title = new string[] { "¡FELICIDADES!", "Tus Logros", "Tus Logros", "!Nuevo Guardian¡" };
        dialogue.sprites = new Sprite[] { null, null, null, medalla };
        dialogue.sentences = new string[] { final, final2, final3, final4 };
        DialogueManager.instance.StartDialogue(dialogue, "ActivarCertificado", null, 2, true);
        //StartCoroutine(Dialogo(canvasDialogo, dialogoPersonaje, final));
        /*yield return new WaitForSeconds(20.0f);
        canvasDialogo.SetActive(false);
        Continuar();*/

    }

    public void activarCertificadoCanvas()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        //Panel.SetActive(false);
        mira.SetActive(false);
        MenuPausa.instance.Pausar();
        Cursor.visible = true;
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        certificadoCanvas.SetActive(true);
        string codigo = "NA019";
        int number1 = Random.Range(0, 10);
        int number2 = Random.Range(0, 10);
        int number3 = Random.Range(0, 10);
        try
        {
            string char1 = Player.instance.playerData.nombre[0].ToString().ToUpper();
            string char2 = Player.instance.playerData.nombre[1].ToString().ToUpper();
            codigo = char1 + char2 + number1.ToString()+ number2.ToString() + number3.ToString();
        }
        catch (System.Exception e)
        {
            codigo = "NA019";
        }
        

        Contenido.GetComponent<TMP_Text>().text=Contenido.GetComponent<TMP_Text>().text.Replace("Placeholder", Player.instance.playerData.nombre).Replace("fecha", System.DateTime.Now.ToString("dd-MM-yyyy")).Replace("NA019", codigo);
        
    }

    public void desactivarCertificadoCanvas()
    {
        certificadoCanvas.SetActive(false);

        MenuPausa.instance.Reanudar();
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventManager.eventManager.TriggerEvent("RegresarMapa");
        EventManager.StopListening("RegresarMapa");

    }

}
