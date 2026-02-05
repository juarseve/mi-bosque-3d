using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowBadBehavior : MonoBehaviour
{
    public Text text;

    void Start()
    {
        PlayerPrefs.DeleteKey("BadBehavior");
        // StartCoroutine(FadeIn());

        bool bad = PlayerPrefs.GetInt("BadBehavior", 0) == 1;
        text.text = bad
            ? "Muy mal comportamiento."
            : "Buen comportamiento.\n¡Felicidades!";
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.05f;
        text.transform.localScale = Vector3.one * pulse;

        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayerPrefs.SetInt("BadBehavior", 1);
            Debug.Log("BadBehavior FORCED");
            Refresh();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayerPrefs.SetInt("BadBehavior", 0);
            Debug.Log("GoodBehavior FORCED");
            Refresh();
        }
    }

    void Refresh()
    {
        bool bad = PlayerPrefs.GetInt("BadBehavior", 0) == 1;

        if (bad)
        {
            text.text = "Muy mal comportamiento.";
            // text.color = Color.red;
        }
        else
        {
            text.text = "Buen comportamiento.\n¡Felicidades!";
            // text.color = Color.green;
        }
    }

    IEnumerator FadeIn()
    {
        CanvasGroup cg = text.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime;
            yield return null;
        }
    }

}
