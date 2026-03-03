using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pausa_script : MonoBehaviour
{
    public GameObject btnGuardarDiploma;
    // Start is called before the first frame update
    void Start()
    {
        // Diploma option removed from pause menu
        if (btnGuardarDiploma != null)
        {
            Destroy(btnGuardarDiploma);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
