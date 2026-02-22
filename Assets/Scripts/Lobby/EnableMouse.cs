using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class EnableMouse : MonoBehaviour
{
    public GameObject controlerObj;
    public bool mochila = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (mochila)
            {
                controlerObj.GetComponent<ShowMochila>().mochilaInt(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (mochila)
            {
                controlerObj.GetComponent<ShowMochila>().mochilaInt(false);
            }
        }
    }
}