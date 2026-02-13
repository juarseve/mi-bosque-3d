using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManejadorEstacion : MonoBehaviour
{
    public int idManejador;
    public bool estado=false;
    [Header("Componentes de estacion a instanciar")]
    public GameObject[] listaInstanciar;
    public GameObject paredes;
    public GameObject decoracion;
    public GameObject especies;
    private GameObject[] instanciados;
    [Header("Componentes de componentes")]
    public GameObject Panel;
    public GameObject Galeria;
    public GameObject Panel3;
    public GameObject logroSist;
    public GameObject fpscontroller;
    public GameObject canvasJoy;
    public GameObject ardillacaja;
    public GameObject pechichecaja;
    public GameObject iguanacaja;
    public Animator animSemilla;
    private Galery GaleryScript;
    public GameObject controladorCalidad;

    [Header("Componentes de estacion a destruir")]
    public GameObject[] listadoDestruir;
    [Header("Especificaciones")]
    public Vector3 desface;
    public Quaternion rotacion;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Instancia de estacion " + idManejador);
        GaleryScript = Galeria.GetComponent<Galery>();
        /*if (idManejador==1)
        {
            instanciar();
        }*/
        //instanciar();
    }
    public void activar()
    {
        if (!estado)
        {
            instanciar();
        }
        else
        {
            destruir();
            instanciar();
        }
    }
    public void desactivar()
    {
        estado = true;
        destruir();
    }

  

    void instanciar()
    {
        GameObject objeto;
        ClickMouse clic;
        BoxCollider[] boxColliders;
        //limitesEstacion limites;
        for (int i = 0; i < listaInstanciar.Length; i++)
        {
            try
            {
               objeto= Instantiate(listaInstanciar[i], desface, rotacion, this.gameObject.transform);
                /*if (objeto.name.Contains("entrada-salida"))
                {
                    foreach (Transform child in objeto.transform)
                    {
                        limites = child.transform.GetComponent<limitesEstacion>();
                        limites.performanceManager = controladorCalidad;
                    }
                }*/

                if (objeto.name.Contains("Especies"))
                {
                    foreach (Transform child in objeto.transform)
                    {
                        clic = child.transform.GetComponent<ClickMouse>();
                        if (clic != null)
                        {
                            clic.Panel= Panel;
                            clic.Galeria = Galeria;
                            clic.Panel3 = Panel3;
                            clic.logroSist = logroSist;
                            clic.fpscontroller = fpscontroller;
                            clic.canvasJoy = canvasJoy;
                            //clic.GaleryScript = GaleryScript;
                            if (clic.isPlant)
                            {
                                child.transform.GetComponent<Seeds>().semillasAnim=animSemilla;
                            }
                            if (child.name== "Squirrel")
                            {
                                clic.CuadroChallengeDos = ardillacaja;
                            }
                            if (child.name == "Iguana")
                            {
                                clic.CuadroChallengeDos = iguanacaja;
                            }
                            if (child.name == "Pechiche")
                            {
                                clic.CuadroChallengeDos = pechichecaja;
                            }
                            
                            // **FIX PARA DESAFÍO 1**: Asegurar que TODOS los BoxColliders del Ceibo tengan isTrigger = false
                            if (child.name.Contains("Ceibo"))
                            {
                                // Obtener TODOS los BoxColliders (puede tener múltiples)
                                boxColliders = child.GetComponents<BoxCollider>();
                                
                                if (boxColliders != null && boxColliders.Length > 0)
                                {
                                    Debug.Log("[ManejadorEstacion] Ceibo encontrado con " + boxColliders.Length + " BoxCollider(s)");
                                    
                                    for (int j = 0; j < boxColliders.Length; j++)
                                    {
                                        boxColliders[j].isTrigger = false;
                                        Debug.Log("[ManejadorEstacion] Configurado Ceibo BoxCollider #" + (j + 1) + " isTrigger = false");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("[ManejadorEstacion] Ceibo no tiene BoxColliders en " + child.name);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("error instanciando estacion: " + e.Message);
            }
        }
        
    }

    void destruir()
    {
        
        for (int i = 0; i < listadoDestruir.Length; i++)
            {
            
            try
             {
                 if (!listadoDestruir[i].name.Contains("Manejador"))
                 {
                        Destroy(listadoDestruir[i]);
                 }
             }
             catch (Exception e)
             {
                Debug.Log("error instanciando estacion");
             }
         }
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
            

    }
    public void completar()
    {
        for (int i = 0; i < listadoDestruir.Length; i++)
        {

            try
            {
                if (!listadoDestruir[i].name.Contains("Manejador"))
                {
                    Destroy(listadoDestruir[i]);
                }
            }
            catch (Exception e)
            {
                Debug.Log("error instanciando estacion");
            }
        }
    }

    public void dormir()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

}
