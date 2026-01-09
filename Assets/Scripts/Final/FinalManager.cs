using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FinalManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject camera;
    public GameObject polo;
    public GameObject NPCs;
    public GameObject ballons;
    public GameObject confetti1;
    public GameObject confetti2;
    public AudioSource audioClap;
    public Canvas canvas;
    public Transform target;

    public GameObject miniCertificado;
    public Text playerName;
    public PlayerData playerData;
    public GameObject diploma;
    public GameObject background;
    public GameObject credits;
    public GameObject exportButton;

    [Header("Configuración")]
    public int currentSpeed = 6;               // velocidad de la cámara
    public float diplomaScaleTarget = 0.5f;    // tamaño final del diploma
    public float diplomaScaleSpeed = 3f;       // rapidez del escalado
    public float pauseBeforeCredits = 1.0f;    // pausa antes de créditos
    public float fadeSpeed = 2.0f;             // velocidad de fades

    // Estados internos
    private bool onDiplome = true;
    private bool fadingToCredits = false;
    private bool creditsLaunched = false;

    private float diplomaAlpha = 1f;
    private float backgroundAlpha = 0f;

    private Image diplomaImage;
    private Image backgroundImage;

    void Start()
    {
        loadData();

        diplomaImage = diploma.GetComponent<Image>();
        backgroundImage = background.GetComponent<Image>();

        // Inicializar estados
        background.SetActive(false);
        credits.SetActive(false);

        if (exportButton != null)
            exportButton.SetActive(false);
    }

    void Update()
    {
        if (Time.time > 2)
        {
            cameraAutoMove();
            statePolo();
        }

        if (onDiplome)
            activeDiplome();

        if (fadingToCredits)
            transitionToCredits();
    }

    void cameraAutoMove()
    {
        Vector3 nextPos = Vector3.MoveTowards(camera.transform.position, target.position, currentSpeed * Time.deltaTime);
        camera.GetComponent<Rigidbody>().MovePosition(nextPos);
    }

    void statePolo()
    {
        if (!audioClap.isPlaying)
        {
            polo.GetComponent<Animator>().SetInteger("state", 1);
        }
    }

    void activeDiplome()
    {
        if (camera.transform.position.z <= target.position.z)
        {
            miniCertificado.SetActive(false);
            diploma.SetActive(true);

            RectTransform rt = diploma.GetComponent<RectTransform>();
            Vector3 targetScale = Vector3.one * diplomaScaleTarget;
            rt.localScale = Vector3.Lerp(rt.localScale, targetScale, diplomaScaleSpeed * Time.deltaTime);

            if (Vector3.Distance(rt.localScale, targetScale) < 0.01f)
            {
                onDiplome = false;

                if (!creditsLaunched)
                {
                    creditsLaunched = true;
                    StartCoroutine(StartCreditsAfter(pauseBeforeCredits));
                }
            }
        }
    }

    IEnumerator StartCreditsAfter(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        fadingToCredits = true;
        background.SetActive(true); // se prepara el fondo
    }

    void transitionToCredits()
    {
        // Parar aplausos
        audioClap.Stop();

        // Fade out diploma
        diplomaAlpha = Mathf.MoveTowards(diplomaAlpha, 0f, fadeSpeed * Time.deltaTime);
        diplomaImage.color = new Color(diplomaImage.color.r, diplomaImage.color.g, diplomaImage.color.b, diplomaAlpha);

        if (diplomaAlpha <= 0.5f)
        {
            if (diploma.transform.childCount > 0)
                diploma.transform.GetChild(0).gameObject.SetActive(false);
        }

        // Fade in background
        backgroundAlpha = Mathf.MoveTowards(backgroundAlpha, 1f, fadeSpeed * Time.deltaTime);
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, backgroundAlpha);

        // Solo lanzar créditos cuando el fondo esté completamente opaco
        if (backgroundAlpha >= 1f)
        {
            offElements();
            credits.SetActive(true);
            playCredits();
            fadingToCredits = false; // transición completada

            if (exportButton != null)
                exportButton.SetActive(true);
        }
    }

    void playCredits()
    {
        credits.GetComponent<Animator>().Play("Base Layer.Credits", 0, 0);
    }

    void offElements()
    {
        NPCs.SetActive(false);
        ballons.SetActive(false);
        confetti1.SetActive(false);
        confetti2.SetActive(false);
        polo.SetActive(false);
    }

    void loadData()
    {
        playerData = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().playerData;
        playerName.text = playerData.nombre;
    }
}
