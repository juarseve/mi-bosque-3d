using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Squirrel : MonoBehaviour, IInteractable
{
    public Transform target; // La madriguera (Nest)
    float Speed = 10f;
    float RotationalDamp = 0.5f;
    private bool inside;
    private Animator animator;
    
    // Estados estáticos para comunicación con Nest y ChallengePass3
    public static bool caught = false;  // El conejo está siendo llevado
    public static bool activate = false; // El desafío está activo
    
    public GameObject img;
    public Text timeText;
    public float _timer = 10.0f;
    private float timer;
    public GameObject skin;
    public AudioScript clockSound;
    public GameObject recordatorio;
    public int iteracion = 0;
    public float incremento = 5;
    
    // Referencia al puntero para feedback visual
    private GameObject puntero;
    
    // Flag para evitar doble interacción
    private bool isInteracting = false;
    
    // Flag para saber si el conejo llegó a la madriguera
    private bool reachedNest = false;
    
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        timer = _timer;
        puntero = GameObject.Find("Crosshair/Image");
        
        // Resetear estados al iniciar
        caught = false;
        // No resetear activate aquí porque lo controla el trigger del desafío
    }

    private void Update()
    {
        // Si la misión ya está completada, no hacer nada
        if (Nest.home)
        {
            return;
        }
        
        if (!inside)
        {
            // El jugador no está cerca
            transform.LookAt(this.transform);
            if (animator != null)
            {
                animator.SetBool("Run", false);
            }
        }
        else
        {
            // El jugador está cerca
            if (!caught)
            {
                // El conejo huye del jugador
                if (animator != null)
                {
                    animator.SetBool("Run", true);
                }
                Turn();
                Move();
            }
            else
            {
                // El conejo fue capturado, va hacia la madriguera
                if (animator != null)
                {
                    animator.SetTrigger("Caught");
                }
                TakeHome();
                
                // Desactivar el collider bloqueador mientras carga el conejo
                GameObject rabbitBlocker = GameObject.FindGameObjectWithTag("Rabbit");
                if (rabbitBlocker != null)
                {
                    CapsuleCollider capsule = rabbitBlocker.GetComponent<CapsuleCollider>();
                    if (capsule != null)
                    {
                        capsule.enabled = false;
                    }
                }
                
                img.SetActive(true);
                skin.SetActive(false);
                
                // Timer para soltar el conejo si no llega a tiempo
                timer -= Time.deltaTime;
                timeText.text = "" + timer.ToString("f0");
                
                // Verificar si llegó a la madriguera
                if (target != null)
                {
                    float distanceToNest = Vector3.Distance(transform.position, target.position);
                    if (distanceToNest < 1f && !reachedNest)
                    {
                        reachedNest = true;
                        Debug.Log("[Squirrel] Conejo llegó a la madriguera");
                    }
                }
                
                // Si el tiempo se acaba, soltar el conejo
                if (timer <= -1)
                {
                    SoltarConejo();
                }
                
                // Si el desafío se desactiva (misión completada), desaparecer
                if (!activate)
                {
                    this.gameObject.SetActive(false);
                    img.SetActive(false);
                    clockSound.detener();
                }
            }
        }
    }
    
    /// <summary>
    /// Suelta el conejo (se escapó o se completó la misión)
    /// </summary>
    private void SoltarConejo()
    {
        clockSound.detener();
        caught = false;
        reachedNest = false;
        
        // Reactivar el collider bloqueador
        GameObject rabbitBlocker = GameObject.FindGameObjectWithTag("Rabbit");
        if (rabbitBlocker != null)
        {
            CapsuleCollider capsule = rabbitBlocker.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                capsule.enabled = true;
            }
        }
        
        img.SetActive(false);
        iteracion++;
        timer = _timer + iteracion * incremento;
        skin.SetActive(true);
        recordatorio.SetActive(false);
        
        Debug.Log("[Squirrel] Conejo soltado. Próximo intento tendrá " + timer + " segundos");
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting) return;
        
        Debug.Log("[Squirrel] OnInteract llamado. activate=" + activate + ", caught=" + caught);
        
        // Solo permitir capturar si el desafío está activo y el conejo no está ya capturado
        if (activate && !caught && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            isInteracting = true;
            Debug.Log("[Squirrel] ¡Capturando conejo!");
            
            // Reproducir animación de pickup
            if (InteractionDetector.instance != null)
            {
                InteractionDetector.instance.PlayPickupAnimation();
            }
            
            // Mostrar recordatorio de llevar a la madriguera
            if (recordatorio != null)
            {
                recordatorio.SetActive(true);
            }
            
            // Iniciar captura
            StartCoroutine(TimeCapture());
            
            Invoke("ResetInteracting", 1f);
        }
        else
        {
            if (!activate)
            {
                Debug.Log("[Squirrel] El desafío no está activo");
            }
            else if (caught)
            {
                Debug.Log("[Squirrel] El conejo ya está capturado");
            }
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        // Solo mostrar puntero si el desafío está activo y el conejo no está capturado
        if (!activate || caught) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar();
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

    // ============== LÓGICA DE MOVIMIENTO ==============
    
    private void Move()
    {
        transform.position += transform.forward * Speed * Time.deltaTime;
    }

    private void Turn()
    {
        Vector3 pos = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationalDamp * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            inside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            inside = false;
        }
    }

    // Mantener OnMouseDown para compatibilidad
    private void OnMouseDown()
    {
        if (InteractionDetector.instance == null)
        {
            if (activate && !caught && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                if (recordatorio != null)
                {
                    recordatorio.SetActive(true);
                }
                StartCoroutine(TimeCapture());
            }
        }
    }

    IEnumerator TimeCapture()
    {
        caught = true;
        reachedNest = false;
        clockSound.reproducir();
        
        float captureTime = _timer + iteracion * incremento;
        Debug.Log("[Squirrel] Conejo capturado. Tienes " + captureTime + " segundos para llevarlo a la madriguera");
        
        yield return new WaitForSeconds(captureTime);
        
        // Solo soltar si todavía está capturado (la misión no se completó)
        if (caught && !Nest.home)
        {
            SoltarConejo();
        }
    }

    private void TakeHome()
    {
        transform.LookAt(target);
        transform.position = Vector3.MoveTowards(transform.position, target.position, Speed * Time.deltaTime);
    }
}
