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
        BoxCollider boxCollider;
        Rigidbody rb;
        CapsuleCollider capsuleCollider;
        //limitesEstacion limites;
        for (int i = 0; i < listaInstanciar.Length; i++)
        {
            try
            {
               objeto= Instantiate(listaInstanciar[i], desface, rotacion, this.gameObject.transform);
               
                // **NUEVO**: Asegurar que el objeto raíz tenga un BoxCollider con isTrigger = false
                AsegurarBoxCollider(objeto);
                
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
                    // **IMPORTANTE**: Lista para almacenar objetos a destruir (no se puede destruir en foreach)
                    List<Transform> objetosADestruir = new List<Transform>();
                    
                    foreach (Transform child in objeto.transform)
                    {
                        // **VALIDACIÓN CRÍTICA**: Destruir Iguana si NO estamos en Manejador 1
                        if (child.name == "Iguana" && idManejador != 1)
                        {
                            Debug.Log("[ManejadorEstacion] ❌ Iguana detectada en Manejador " + idManejador + " - SERÁ DESTRUIDA (solo debe existir en Manejador 1)");
                            objetosADestruir.Add(child);
                            continue; // Saltar al siguiente hijo, no procesar esta Iguana
                        }
                        
                        // **NUEVO**: Asegurar que cada hijo de Especies tenga un BoxCollider con isTrigger = false
                        AsegurarBoxCollider(child.gameObject);
                        
                        // **NUEVO**: Configurar layer "Collectables" (layer 10) para todas las especies
                        child.gameObject.layer = 10;
                        Debug.Log("[ManejadorEstacion] " + child.name + " configurado en layer 10 (Collectables)");
                        
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
                                
                                // **FIX COMPLETO PARA IGUANA**: Configurar transform, rigidbody y colliders
                                // ⚠️ SOLO EN MANEJADOR 1 (Estación 1)
                                if (idManejador == 1)
                                {
                                    ConfigurarIguana(child);
                                }
                                else
                                {
                                    // Este código NO debería ejecutarse nunca porque destruimos la Iguana arriba
                                    Debug.LogWarning("[ManejadorEstacion] ⚠️ Este código no debería ejecutarse - Iguana en Manejador " + idManejador);
                                }
                            }
                            if (child.name == "Pechiche")
                            {
                                clic.CuadroChallengeDos = pechichecaja;
                            }
                            
                            // **NUEVO: CONFIGURAR OSO PEREZOSO**
                            if (child.name.Contains("oso perezoso") || child.name.Contains("Sloth") || child.name.Contains("perezoso"))
                            {
                                Debug.Log("[ManejadorEstacion] 🦥 Oso Perezoso detectado en Manejador " + idManejador);
                                ConfigurarOsoPerezoso(child);
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
                    
                    // **DESTRUIR OBJETOS MARCADOS**: Ahora que terminó el foreach, destruir objetos
                    foreach (Transform objetoADestruir in objetosADestruir)
                    {
                        Debug.Log("[ManejadorEstacion] 🗑️ Destruyendo: " + objetoADestruir.name + " del Manejador " + idManejador);
                        Destroy(objetoADestruir.gameObject);
                    }
                    
                    if (objetosADestruir.Count > 0)
                    {
                        Debug.Log("[ManejadorEstacion] ✓ Total de objetos destruidos en Manejador " + idManejador + ": " + objetosADestruir.Count);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("error instanciando estacion: " + e.Message);
            }
        }
        
    }
    
    /// <summary>
    /// Configura completamente la Iguana con transform, rigidbody y colliders según especificaciones
    /// ⚠️ SOLO SE DEBE LLAMAR EN MANEJADOR 1 (Estación 1)
    /// </summary>
    /// <param name="iguanaTransform">Transform de la Iguana</param>
    private void ConfigurarIguana(Transform iguanaTransform)
    {
        GameObject iguana = iguanaTransform.gameObject;
        
        Debug.Log("[ManejadorEstacion] ✓ Configurando Iguana en Manejador " + idManejador + " (Estación 1)");
        
        // **1. CONFIGURAR TRANSFORM**
        // Position: (-4.838098, -1.466103, 6.076778)
        iguana.transform.localPosition = new Vector3(-4.838098f, -1.466103f, 6.076778f);
        
        // Rotation: (1.002, -114.228, 13.302)
        iguana.transform.localRotation = Quaternion.Euler(1.002f, -114.228f, 13.302f);
        
        // Scale: (2, 2, 2)
        iguana.transform.localScale = new Vector3(2f, 2f, 2f);
        
        Debug.Log("[ManejadorEstacion] Iguana Transform configurado - Pos: " + iguana.transform.localPosition + 
                  ", Rot: " + iguana.transform.localRotation.eulerAngles + ", Scale: " + iguana.transform.localScale);
        
        // **2. CONFIGURAR RIGIDBODY**
        Rigidbody rb = iguana.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 1f;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            
            // Freeze Position en todos los ejes (X, Y, Z)
            // Freeze Rotation en todos los ejes (X, Y, Z)
            rb.constraints = RigidbodyConstraints.FreezePositionX | 
                            RigidbodyConstraints.FreezePositionY | 
                            RigidbodyConstraints.FreezePositionZ |
                            RigidbodyConstraints.FreezeRotationX | 
                            RigidbodyConstraints.FreezeRotationY | 
                            RigidbodyConstraints.FreezeRotationZ;
            
            Debug.Log("[ManejadorEstacion] Iguana Rigidbody configurado con freeze position y rotation en todos los ejes");
        }
        else
        {
            Debug.LogWarning("[ManejadorEstacion] Iguana no tiene Rigidbody en " + iguana.name);
        }
        
        // **3. CONFIGURAR CAPSULE COLLIDER**
        CapsuleCollider capsuleCollider = iguana.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null; // SmallFriction (si tienes el material, asignarlo aquí)
            capsuleCollider.center = new Vector3(0f, 0.05f, -0.11f);
            capsuleCollider.radius = 0.1f;
            capsuleCollider.height = 0.8f;
            capsuleCollider.direction = 2; // Z-Axis
            
            Debug.Log("[ManejadorEstacion] Iguana CapsuleCollider configurado - Center: " + capsuleCollider.center + 
                      ", Radius: " + capsuleCollider.radius + ", Height: " + capsuleCollider.height);
        }
        else
        {
            // Si no existe, crear uno
            capsuleCollider = iguana.AddComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.center = new Vector3(0f, 0.05f, -0.11f);
            capsuleCollider.radius = 0.1f;
            capsuleCollider.height = 0.8f;
            capsuleCollider.direction = 2; // Z-Axis
            
            Debug.Log("[ManejadorEstacion] Iguana CapsuleCollider CREADO y configurado");
        }
        
        // **4. CONFIGURAR BOX COLLIDER**
        BoxCollider boxCollider = iguana.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
            boxCollider.material = null; // None (Physic Material)
            boxCollider.center = new Vector3(0f, 0.04f, 0f);
            boxCollider.size = new Vector3(0.23f, 0.09f, 0.61f);
            
            Debug.Log("[ManejadorEstacion] Iguana BoxCollider configurado - Center: " + boxCollider.center + 
                      ", Size: " + boxCollider.size);
        }
        else
        {
            // Si no existe, crear uno
            boxCollider = iguana.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false;
            boxCollider.center = new Vector3(0f, 0.04f, 0f);
            boxCollider.size = new Vector3(0.23f, 0.09f, 0.61f);
            
            Debug.Log("[ManejadorEstacion] Iguana BoxCollider CREADO y configurado");
        }
        
        Debug.Log("[ManejadorEstacion] ✓✓✓ Iguana completamente configurada en Manejador 1 ✓✓✓");
    }
    
    /// <summary>
    /// Asegura que un GameObject tenga al menos un BoxCollider con isTrigger = false
    /// Si no tiene BoxCollider, lo crea. Si tiene, configura isTrigger = false en todos.
    /// </summary>
    /// <param name="obj">GameObject a verificar/modificar</param>
    private void AsegurarBoxCollider(GameObject obj)
    {
        if (obj == null) return;
        
        // Obtener todos los BoxColliders del objeto (sin incluir hijos)
        BoxCollider[] boxColliders = obj.GetComponents<BoxCollider>();
        
        if (boxColliders == null || boxColliders.Length == 0)
        {
            // No tiene BoxCollider, crear uno nuevo
            BoxCollider nuevoBoxCollider = obj.AddComponent<BoxCollider>();
            nuevoBoxCollider.isTrigger = false;
            Debug.Log("[ManejadorEstacion] BoxCollider creado en " + obj.name + " con isTrigger = false");
        }
        else
        {
            // Ya tiene BoxColliders, asegurar que todos tengan isTrigger = false
            foreach (BoxCollider bc in boxColliders)
            {
                if (bc.isTrigger)
                {
                    bc.isTrigger = false;
                    Debug.Log("[ManejadorEstacion] BoxCollider en " + obj.name + " configurado a isTrigger = false");
                }
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

    /// <summary>
    /// Configura completamente el Oso Perezoso con transform, rigidbody y colliders según las coordenadas especificadas
    /// </summary>
    /// <param name="osoTransform">Transform del Oso Perezoso</param>
    private void ConfigurarOsoPerezoso(Transform osoTransform)
    {
        GameObject oso = osoTransform.gameObject;
        
        Debug.Log("[ManejadorEstacion] 🦥 ✓ Configurando Oso Perezoso en Manejador " + idManejador);
        
        // **1. CONFIGURAR TRANSFORM SEGÚN INSPECTOR**
        // Position: X: -12.98, Y: 2.719, Z: 12.9
        oso.transform.localPosition = new Vector3(-12.98f, 2.719f, 12.9f);
        
        // Rotation: X: 0, Y: -272.1, Z: 0
        oso.transform.localRotation = Quaternion.Euler(0f, -272.1f, 0f);
        
        // Scale: X: 0.4457071, Y: 0.4457071, Z: 0.4457071
        oso.transform.localScale = new Vector3(0.4457071f, 0.4457071f, 0.4457071f);
        
        Debug.Log("[ManejadorEstacion] 🦥 Oso Perezoso Transform configurado:");
        Debug.Log($"[ManejadorEstacion]    - Position: {oso.transform.localPosition}");
        Debug.Log($"[ManejadorEstacion]    - Rotation: {oso.transform.localRotation.eulerAngles}");
        Debug.Log($"[ManejadorEstacion]    - Scale: {oso.transform.localScale}");
        
        // **2. CONFIGURAR RIGIDBODY (si existe)**
        Rigidbody rb = oso.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Configurar como kinematic para que no se mueva con física
            rb.isKinematic = true;
            rb.useGravity = false;
            
            Debug.Log("[ManejadorEstacion] 🦥 Oso Perezoso Rigidbody configurado como kinematic");
        }
        else
        {
            Debug.Log("[ManejadorEstacion] 🦥 Oso Perezoso no tiene Rigidbody (normal para objetos estáticos)");
        }
        
        // **3. ASEGURAR COLLIDER PARA INTERACCIÓN**
        AsegurarBoxCollider(oso);
        
        // **4. CONFIGURAR LAYER**
        oso.layer = 10; // Layer "Collectables"
        Debug.Log("[ManejadorEstacion] 🦥 Oso Perezoso configurado en layer 10 (Collectables)");
        
        Debug.Log("[ManejadorEstacion] 🦥 ✓✓✓ Oso Perezoso completamente configurado ✓✓✓");
    }
}
