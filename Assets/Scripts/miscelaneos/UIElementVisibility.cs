using UnityEngine;

public class UIElementVisibility : MonoBehaviour
{
    [Header("En qué estación debe estar visible este elemento")]
    public int requiredStation = 1;

    private void Start()
    {
        VerificarVisibilidad();
    }

    private void OnEnable()
    {
        VerificarVisibilidad();
    }

    private void VerificarVisibilidad()
    {
        if (GameManager.instance != null)
        {
            // Si no estás en la estación requerida, desactiva el elemento
            if (GameManager.instance.currentStation != requiredStation)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
