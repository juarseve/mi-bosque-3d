using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Seeds : MonoBehaviour, IInteractable
{
    public int id;
    public Animator semillasAnim;
    bool entregada = false;
    public GameObject padreSemillas;
    
    // Referencias para mostrar galería (asignadas por ManejadorEstacion para árboles dinámicos)
    public GameObject galeriaPanel;
    public GameObject panelUI;
    
    // Referencia al puntero para feedback visual
    private GameObject puntero;
    
    // Referencia al ClickMouse del mismo objeto (para abrir galería)
    private ClickMouse clickMouse;
    
    // Flag para evitar doble interacción
    private bool isInteracting = false;

    private void Start()
    {
        padreSemillas = GameObject.Find("SemillasAnim");
        puntero = GameObject.Find("Crosshair/Image");
        
        // Buscar Animator para animación de recolección
        if (semillasAnim == null && padreSemillas != null)
        {
            semillasAnim = padreSemillas.GetComponent<Animator>();
            if (semillasAnim != null)
            {
                Debug.Log("[Seeds] Animator encontrado en SemillasAnim: " + semillasAnim.gameObject.name);
            }
        }
        
        if (semillasAnim == null)
        {
            // Buscar Animator en el mismo objeto o en children
            semillasAnim = GetComponent<Animator>();
            if (semillasAnim == null)
            {
                semillasAnim = GetComponentInChildren<Animator>();
                if (semillasAnim != null)
                {
                    Debug.Log("[Seeds] Animator encontrado en children de " + gameObject.name);
                }
            }
        }
        
        // Buscar ClickMouse en el mismo objeto para poder abrir la galería
        clickMouse = GetComponent<ClickMouse>();
        if (clickMouse != null)
        {
            Debug.Log("[Seeds] ClickMouse encontrado en " + gameObject.name);
        }

        // Corregir/asegurar el ID de semilla según el nombre del GameObject
        // (algunas instancias en la escena tenían el id por defecto apuntando a teca)
        string nameLower = gameObject.name.ToLower();
        if ((id == 0 || id == 3) )
        {
            if (nameLower.Contains("ceibo") && id != 4)
            {
                Debug.Log($"[Seeds] Corrigiendo id de {gameObject.name} -> Ceibo (4)");
                id = 4;
            }
            else if (nameLower.Contains("guasmo") && id != 15)
            {
                Debug.Log($"[Seeds] Corrigiendo id de {gameObject.name} -> Guasmo (15)");
                id = 15; // new unique id (5 was reserved for papel)
            }
            else if (nameLower.Contains("fernan") || nameLower.Contains("fernansanchez") || nameLower.Contains("fernan_sanchez"))
            {
                if (id != 14)
                {
                    Debug.Log($"[Seeds] Corrigiendo id de {gameObject.name} -> Fernan Sanchez (14)");
                    id = 14;
                }
            }else if (nameLower.Contains("bototillo") )
            {
                if (id != 8)
                {
                    Debug.Log($"[Seeds] Corrigiendo id de {gameObject.name} -> Bototillo (8)");
                    id = 8;
                }
            }
        }
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting) return;
        isInteracting = true;
        
        Debug.Log("[Seeds] OnInteract llamado en " + gameObject.name);
        
        try
        {
            // Primero recoger la semilla
            RecogerSemilla();
            
            // Luego abrir la galería si hay ClickMouse
            if (clickMouse != null)
            {
                Debug.Log("[Seeds] Llamando a ClickMouse.OnInteract para abrir galería");
                clickMouse.OnInteract();
            }
            else
            {
                // Si no hay ClickMouse (fue destruido), abrir galería internamente
                Debug.Log("[Seeds] No hay ClickMouse - abriendo galería autónomamente");
                ShowGalleryAutonomously();
            }
        }
        finally
        {
            // Resetear flag después de un pequeño delay
            Invoke("ResetInteracting", 0.5f);
        }
    }
    
    private void ShowGalleryAutonomously()
    {
        Debug.Log("[Seeds] ShowGalleryAutonomously - Abriendo galería para " + gameObject.name);
        Debug.Log("[Seeds] DEBUG: galeriaPanel=" + (galeriaPanel != null ? galeriaPanel.name : "NULL") + ", panelUI=" + (panelUI != null ? panelUI.name : "NULL"));
        
        try
        {
            // Usar MenuPausa singleton
            if (MenuPausa.instance != null)
            {
                MenuPausa.instance.Pausar();
                Debug.Log("[Seeds] Menú pausado");
            }
            
            // Desactivar mouse controller
            if (ConstantObjects.instance != null && ConstantObjects.instance.mouseController != null)
            {
                ConstantObjects.instance.mouseController.enabled = false;
                Debug.Log("[Seeds] Mouse controller desactivado");
            }
            
            // PASO 1: Activar Panel (UI general)
            GameObject panelToActivate = panelUI;
            if (panelToActivate == null)
            {
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    Transform panelTransform = canvas.transform.Find("Panel");
                    if (panelTransform != null)
                    {
                        panelToActivate = panelTransform.gameObject;
                    }
                }
            }
            
            if (panelToActivate != null)
            {
                panelToActivate.SetActive(true);
                Debug.Log("[Seeds] Panel UI activado: " + panelToActivate.name);
            }
            else
            {
                Debug.LogWarning("[Seeds] No se encontró Panel en Canvas");
            }
            
            // PASO 2: Activar Galería
            GameObject galeriaToActivate = galeriaPanel;
            if (galeriaToActivate == null)
            {
                Debug.LogWarning("[Seeds] galeriaPanel es null, buscando en Canvas");
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    Debug.Log("[Seeds] Canvas encontrado, buscando Galeria child");
                    Transform galeriaTransform = canvas.transform.Find("Galeria");
                    if (galeriaTransform != null)
                    {
                        galeriaToActivate = galeriaTransform.gameObject;
                        Debug.Log("[Seeds] Galeria encontrada en Canvas via Find");
                    }
                    else
                    {
                        Debug.LogError("[Seeds] Galeria NO encontrada via Find en Canvas");
                        // Buscar en todos los children recursivamente
                        Debug.Log("[Seeds] Buscando recursivamente entre " + canvas.transform.childCount + " children de Canvas");
                        foreach (Transform child in canvas.transform)
                        {
                            Debug.Log("[Seeds] Canvas child: " + child.name);
                            if (child.name == "Galeria")
                            {
                                galeriaToActivate = child.gameObject;
                                Debug.Log("[Seeds] Galeria encontrada buscando recursivamente");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("[Seeds] Canvas no encontrado");
                }
            }
            else
            {
                Debug.Log("[Seeds] galeriaPanel tiene referencia: " + galeriaPanel.name);
            }
            
            Debug.Log("[Seeds] *** ANTES DE ACTIVAR GALERIA: galeriaToActivate = " + (galeriaToActivate != null ? galeriaToActivate.name : "NULL") + " ***");
            
            if (galeriaToActivate != null)
            {
                galeriaToActivate.SetActive(true);
                Debug.Log("[Seeds] Galeria activada: " + galeriaToActivate.name);
                
                // Buscar Galery script 
                Galery galeryScript = galeriaToActivate.GetComponent<Galery>();
                if (galeryScript == null)
                {
                    galeryScript = galeriaToActivate.GetComponentInChildren<Galery>();
                }
                
                if (galeryScript == null)
                {
                    Debug.LogWarning("[Seeds] Galery script no encontrado en Galeria. Buscando en toda la escena (incluyendo inactivos)...");
                    Galery[] allGalery = Resources.FindObjectsOfTypeAll<Galery>();
                    Debug.Log("[Seeds] Total Galery scripts encontrados (incluyendo inactivos): " + allGalery.Length);
                    
                    if (allGalery.Length > 0)
                    {
                        foreach (Galery g in allGalery)
                        {
                            Debug.Log("[Seeds]   - Galery en: " + g.gameObject.name + " (active: " + g.gameObject.activeSelf + ", parent: " + (g.gameObject.transform.parent != null ? g.gameObject.transform.parent.name : "none") + ")");
                        }
                        
                        // Usar el primer Galery encontrado
                        galeryScript = allGalery[0];
                        Debug.Log("[Seeds] Usando Galery script de: " + galeryScript.gameObject.name);
                    }
                }
                
                if (galeryScript != null)
                {
                    // Asegurar que el GameObject del script y todos sus parents estén activos
                    GameObject gobj = galeryScript.gameObject;
                    Debug.Log("[Seeds] Activando jerarquía completa de: " + gobj.name);
                    
                    // Activar de abajo hacia arriba en la jerarquía
                    while (gobj != null)
                    {
                        if (!gobj.activeSelf)
                        {
                            gobj.SetActive(true);
                            Debug.Log("[Seeds] Activado: " + gobj.name);
                        }
                        gobj = gobj.transform.parent != null ? gobj.transform.parent.gameObject : null;
                    }
                    
                    // Habilitar el script también
                    if (!galeryScript.enabled)
                    {
                        galeryScript.enabled = true;
                        Debug.Log("[Seeds] Galery script habilitado");
                    }
                    
                    // Usar solo el nombre base (sin sufijos como _estacion o (Clone))
                    string speciesName = GetSpeciesName(gameObject.name);
                    galeryScript.name = speciesName;
                    galeryScript.visible = true;
                    Debug.Log("[Seeds] Galería script configurado para: " + speciesName + " (visible=" + galeryScript.visible + ")");
                    
                    // PASO 3: Ocultar Panel3 (panel de especies objetivo)
                    GameObject panel3 = GameObject.Find("Panel3");
                    if (panel3 != null)
                    {
                        panel3.SetActive(false);
                        // Asignar referencia directa a Panel3 para que Limpiar() pueda reactivarlo
                        galeryScript.panel3Ref = panel3;
                        
                        // Asignar referencias a las cajas de objetivos
                        GameObject ardillaCaja = GameObject.Find("ArdillaCaja");
                        GameObject iguanaCaja = GameObject.Find("IguanaCaja");
                        GameObject pechicheCaja = GameObject.Find("PechicheCaja");
                        if (ardillaCaja != null) galeryScript.ardillaCajaRef = ardillaCaja;
                        if (iguanaCaja != null) galeryScript.iguanaCajaRef = iguanaCaja;
                        if (pechicheCaja != null) galeryScript.pechicheCajaRef = pechicheCaja;
                        
                        Debug.Log("[Seeds] Panel3 (especies objetivo) ocultado y referencias asignadas");
                    }
                }
                else
                {
                    Debug.LogError("[Seeds] *** Galery script NUNCA ENCONTRADO en escape scene ***");
                }
            }
            else
            {
                Debug.LogError("[Seeds] *** Galeria NO SE PUDO ACTIVAR ***");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Seeds] Error en ShowGalleryAutonomously: " + e.Message + "\n" + e.StackTrace);
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (entregada) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.puntero();
            }
        }
    }
    
    public void OnLookAway()
    {
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.mira();
            }
        }
    }

    // ============== LÓGICA DE RECOLECCIÓN ==============
    
    public void RecogerSemilla()
    {
        if (entregada)
        {
            Debug.Log("[Seeds] Semilla ya entregada");
            return;
        }
        
        entregada = true;
        Debug.Log("[Seeds] Recogiendo semilla ID " + id);
        
        // Añadir a la mochila
        if (GameManager.instance != null && GameManager.instance.mochila != null)
        {
            GameManager.instance.mochila.TestAddF(id);
            Debug.Log("[Seeds] Semilla añadida a la mochila");
        }
        else
        {
            Debug.LogWarning("[Seeds] No se pudo añadir a la mochila - GameManager o mochila es null");
        }
        
        // Actualizar UI de semillas
        if (padreSemillas != null)
        {
            // Desactivar todos los indicadores
            for (int i = 0; i < padreSemillas.transform.childCount; i++)
            {
                padreSemillas.transform.GetChild(i).gameObject.SetActive(false);
            }

            // Activar el indicador correspondiente
            int childIndex = GetSemillaChildIndex(id);
            if (childIndex >= 0 && childIndex < padreSemillas.transform.childCount)
            {
                padreSemillas.transform.GetChild(childIndex).gameObject.SetActive(true);
                Debug.Log("[Seeds] UI de semilla actualizada");
            }
        }
        
        // Reproducir animación
        if (semillasAnim != null)
        {
            Debug.Log("[Seeds] Animator encontrado: " + semillasAnim.gameObject.name);
            semillasAnim.SetTrigger("NuevaSemilla");
            Debug.Log("[Seeds] Animación de nueva semilla activada con trigger 'NuevaSemilla'");
        }
        else
        {
            Debug.LogWarning("[Seeds] *** semillasAnim es NULL - no se puede reproducir animación ***");
        }
        
        Debug.Log("[Seeds] Semilla " + id + " recogida exitosamente");
    }
    
    /// <summary>
    /// Obtiene el índice del hijo para mostrar según el ID de la semilla
    /// </summary>
    private int GetSemillaChildIndex(int seedId)
    {
        switch (seedId)
        {
            case 3:  return 1;  // teca
            case 4:  return 0;  // ceibo
            case 15: return 6;  // guasmo (moved from 5 because 5 used by papel)
            case 14: return 7;  // fernan_sanchez
            case 8:  return 2;  // bototillo
            case 9:  return 4;  // judea
            case 10: return 5;  // guayacan
            case 11: return 3;  // jacaranda
            default: return -1;
        }
    }

    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo
        if (InteractionDetector.instance == null)
        {
            RecogerSemilla();
            
            // También abrir galería con mouse
            if (clickMouse != null)
            {
                clickMouse.OnInteract();
            }
        }
    }
    
    private void OnMouseEnter()
    {
        OnLookAt();
    }
    
    private void OnMouseExit()
    {
        OnLookAway();
    }
    
    /// <summary>
    /// Extrae el nombre base de la especie del nombre del GameObject
    /// Ej: "Bototillo_estacion1" -> "Bototillo", "Bototillo (Clone)" -> "Bototillo"
    /// </summary>
    private string GetSpeciesName(string gameObjectName)
    {
        // Remover sufijos comunes de GameObjects dinámicos
        string name = gameObjectName;
        
        // Remover " (Clone)"
        name = name.Replace(" (Clone)", "");
        // Remover " (1)", " (2)", etc.
        name = Regex.Replace(name, @" \(\d+\)$", "");
        // Remover números al final precedidos de guion o guion bajo
        name = Regex.Replace(name, @"[_-]\d+$", "");
        
        // Capitalizar primera letra para asegurar coincidencia con base de datos
        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name.Substring(1);
        }
        
        Debug.Log("[Seeds] GetSpeciesName: '" + gameObjectName + "' -> '" + name + "'");
        return name;
    }
}

