using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class dialogo_puente : MonoBehaviour
{
    public string titulostr;
    public string bodystr;

    public GameObject btnSig;
    public GameObject cuadrodiag;

    public GameObject titulo;
    public GameObject body;

    public GameObject fps;

    private TextMeshProUGUI tit;
    private TextMeshProUGUI bdy;
    //private FirstPersonController fpContoller;

    private void Awake()
    {
        tit = titulo.GetComponent<TextMeshProUGUI>();
        bdy = body.GetComponent<TextMeshProUGUI>();
        //fpContoller = fps.GetComponent<FirstPersonController>();
    }

    public virtual void TriggerDialogue()
    {
        tit.text = titulostr;
        bdy.text = bodystr;
        //fpContoller.LockCursor = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        btnSig.SetActive(true);
        //fps.GetComponent<FirstPersonController>().enabled = false;
        cuadrodiag.SetActive(true);

    }

    public void terminarDialogo()
    {
        btnSig.SetActive(false);
        cuadrodiag.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("eeee");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
}