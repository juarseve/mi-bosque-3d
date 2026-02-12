using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    [SerializeField] public bool isDialogueActive, typedSentence, startTyping;
    public bool removeDT, isPreguntas;
    public GameObject entendidoBtn;
    public Text titleText;
    public Text dialogueText;
    private FirstPersonController firstPersonController;
    private MouseController mouseController;
    [Header("Imagen de reaccion de los personajes (0:Triste, 1:Feliz, 2:Normal")]
    public Image characterImage;
    public GameObject[] characterImages;
    public Sprite[] personaje_expresiones;
    [Header("============================")]
    public Animator animatorDesafio;
    private Queue<string> sentences;
    private string sentence;
    private GameObject currentDTGO;
    private Dialogue currentD;
    public RawImage imagen;
    public GameObject leaves;
    public Image m_ImagenDialogo;
    public int index_dialogue_img;

    public GameObject CanvasPlayerGUI;

    [Tooltip("Name of the current event to trigger")] public string currentEventToTrigger;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        sentences = new Queue<string>();
        firstPersonController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        mouseController = GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>();
        index_dialogue_img = 0;
    }


    public void StartDialogue(DialogueTrigger dialogueTrigger, string eventToTrigger, bool remove, int index_img = 0)
    {
        LocalizeDialogue(dialogueTrigger.dialogue);

#if UNITY_ANDROID || UNITY_IOS
        CanvasPlayerGUI.SetActive(false);
        GameObject.Find("FPSController").GetComponent<JoystickController>().enabled = false;
#endif
        currentD = dialogueTrigger.dialogue;
        currentDTGO = dialogueTrigger.gameObject;
        removeDT = remove;
        isPreguntas = false;
        isDialogueActive = true;
        if (characterImages != null && characterImages.Length != 0)
        {
            foreach (GameObject img in characterImages)
            {
                img.SetActive(false);
            }
            characterImages[index_img].SetActive(true);
        }
        else
        {
            characterImage.sprite = personaje_expresiones[index_img];
        }
        entendidoBtn.SetActive(true);
        animatorDesafio.SetBool("IsOpen", true);
        currentEventToTrigger = eventToTrigger;
        MenuPausa.instance.Pausar();
        mouseController.enabled = false;
        sentences.Clear();
        foreach (string sentence in dialogueTrigger.dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void StartDialogue(Dialogue dialogue, string eventToTrigger, GameObject go = null, int index_img = 0, bool remove = false, bool pregunta = false)
    {
        LocalizeDialogue(dialogue);
        
        currentD = dialogue;
        currentDTGO = go;
        isPreguntas = pregunta;
        if (isPreguntas)
        {
            imagen.enabled = true;
            leaves.SetActive(true);
        }
        removeDT = remove;
        isDialogueActive = true;
        
        foreach(GameObject img in characterImages)
        {
            img.SetActive(false);
        }
        characterImages[index_img].SetActive(true);
        //characterImage.sprite = personaje_expresiones[index_img];
        entendidoBtn.SetActive(true);
        animatorDesafio.SetBool("IsOpen", true);
        currentEventToTrigger = eventToTrigger;
        MenuPausa.instance.Pausar();
        mouseController.enabled = false;
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }
    IEnumerator TypeSentences(string texto)
    {
        startTyping = true;
        typedSentence = false;
        dialogueText.text = string.Empty;
        foreach (char letter in texto.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
        startTyping = false;
        typedSentence = true;
    }

    public void DisplayNextSentence()
    {
        Debug.Log("Maybe is here: ");
        if (sentence != "")
        {
            if (startTyping && !typedSentence)
            {
                StopAllCoroutines();
                dialogueText.text = sentence;
                startTyping = false;
                typedSentence = true;
                return;
            }
        }
        if (sentences.Count == 0)
        {
            EventManager.eventManager.TriggerEvent("DesctivarFotosMision1");
            EventManager.StopListening("DesctivarFotosMision1");
            EndDialogue();
            return;
        }
        else
        {
            try
            {
                titleText.text=currentD.title[index_dialogue_img];

            if (titleText.text == "Conoce nuestras especies")
            {
                EventManager.eventManager.TriggerEvent("ActivarFotosMision1");
                EventManager.StopListening("ActivarFotosMision1");
            }
            
                
            


                if (currentD.sprites[index_dialogue_img] != null)
            {
                m_ImagenDialogo.sprite = currentD.sprites[index_dialogue_img];
                m_ImagenDialogo.enabled = true;
            }
            else
            {
                m_ImagenDialogo.enabled = false;
            }
            index_dialogue_img++;
            }
            catch (Exception e)
            {
                Debug.Log("error del cartel");
                EndDialogue();
                return;
            }
        }
        sentence = sentences.Dequeue();
        StartCoroutine(TypeSentences(sentence));
    }

    public void CloseDialogue()
    {
        if (sentences.Count == 0 && typedSentence)
        {
            EndDialogue();
        }
    }
    void EndDialogue()
    {
#if UNITY_ANDROID || UNITY_IOS
        CanvasPlayerGUI.SetActive(true);
        GameObject.Find("FPSController").GetComponent<JoystickController>().enabled = true;
        GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>().handle.anchoredPosition = Vector2.zero;
        GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>().input = Vector2.zero;
#endif
        animatorDesafio.SetBool("IsOpen", false);
        MenuPausa.instance.Reanudar();
        mouseController.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        firstPersonController.enabled = true;
        isDialogueActive = false;
        typedSentence = false;
        index_dialogue_img = 0;
        m_ImagenDialogo.enabled = false;

        if (currentEventToTrigger != "")
        {
            Debug.Log(currentEventToTrigger);
            EventManager.eventManager.TriggerEvent(currentEventToTrigger);
        }
        EventManager.StopListening(currentEventToTrigger);
        if (removeDT)
        {
            Destroy(currentDTGO);
        }

        //TriggerTutorial();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (isPreguntas)
        {
            imagen.enabled = false;
            leaves.SetActive(false);
        }
        entendidoBtn.SetActive(false);
        foreach (GameObject img in characterImages)
        {
            img.SetActive(false);
        }
        characterImages[2].SetActive(true);
        //characterImage.sprite = personaje_expresiones[2];
    }

    public void TriggerTutorial()
    {
        // Disable the player movement (first-person controller) during the tutorial
        firstPersonController.enabled = false;

        // Start the tutorial after the dialogue ends
        TargetManager.instance.StartTutorial();

        // Log to track when the tutorial starts
        Debug.Log("Starting tutorial after dialogue...");
    }

    // busca dialogos en el JSON, reemplaza las claves por los dialogos
    private void LocalizeDialogue(Dialogue dialogue)
    {
        // Traducir titles
        for (int i = 0; i < dialogue.title.Length; i++)
        {
            // Comprobar si la instancia de LanguageManager existe antes de continuar.
            if (LanguageManager.Instancia == null)
            {
                Debug.LogWarning("No se encontro una instancia de LanguageManager. Inicializando...");
                GameObject lm = new GameObject("LanguageManager");
                lm.AddComponent<LanguageManager>();
            }

            if (!string.IsNullOrEmpty(dialogue.title[i]))
            {
                string clave = dialogue.title[i];
                string traducido = LanguageManager.Instancia.ObtenerTexto(clave);

                if (!string.IsNullOrEmpty(traducido))
                {
                    traducido = traducido.Trim('[', ']');
                    dialogue.title[i] = traducido;
                }
                else
                    Debug.LogWarning($"No se encontro traducción para la clave de título: {clave}");            }
        }

        // Traducir sentences
        for (int i = 0; i < dialogue.sentences.Length; i++)
        {
            if (!string.IsNullOrEmpty(dialogue.sentences[i]))
            {
                string clave = dialogue.sentences[i];
                string traducido = LanguageManager.Instancia.ObtenerTexto(clave);
                traducido = traducido.Trim('[', ']');

                if (!string.IsNullOrEmpty(traducido))
                    dialogue.sentences[i] = traducido;
                else
                    Debug.LogWarning($"No se encontro traducción para la clave de frase: {clave}");            }
        }
    }

}