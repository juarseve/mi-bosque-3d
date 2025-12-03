using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        public bool m_IsRunning;
        [SerializeField] public float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] [Range(0f, 1f)] private float m_WalktepLenghten;

        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;

        // [TPS] MouseLook desactivado para evitar el conflicto de rotación
        [SerializeField] public MouseLook m_MouseLook = null;

        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;
        [SerializeField] private AudioClip[] m_RunningstepSounds;

        [SerializeField] private AudioClip m_JumpSound;
        [SerializeField] private AudioClip m_LandSound;

        // --- [TPS] VARIABLES DE ROTACIÓN MANUAL ---
        [Header("Configuración TPS")]
        public Transform cameraPivot;
        public float mouseSensitivity = 2f; // Velocidad del mouse
        private float rotX = 0f; // Acumulador rotación vertical
        private float rotY = 0f; // Acumulador rotación horizontal
        // -------------------------------------------

        private Camera m_Camera;
        public bool m_Jump;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        public CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        [HideInInspector] public Vector2 RunAxis;
        [HideInInspector] public bool JumpAxis;
        [HideInInspector] public int doubleJumpCount = 1;

        public GameObject msgUI;
        public GameObject joystick;

        public bool canRotate = true;
        public bool canMove = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Recolectables")) msgUI.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Recolectables")) msgUI.SetActive(false);
        }


        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();

            // --- [TPS] INICIALIZAR ÁNGULOS ---
            if (cameraPivot == null)
            {
                Debug.LogError("¡ERROR! Asigna el 'Camera Pivot' en el Inspector.");
                cameraPivot = transform;
            }

            // Tomamos la rotación inicial para que no salte de golpe
            Vector3 rot = cameraPivot.eulerAngles;
            rotY = rot.y;
            rotX = rot.x;

            // Ocultar cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // -------------------------------------------

#if UNITY_ANDROID || UNITY_IOS
            joystick.SetActive(true);
            GameObject.Find("GuiaPausar").SetActive(false);
            if (GameObject.Find("FirstChallenge-2") != null) GameObject.Find("FirstChallenge-2").SetActive(false);
            if (GameObject.Find("TutorialCorrer") != null) GameObject.Find("TutorialCorrer").SetActive(false);
            if (GameObject.Find("ConseguirLibro") != null) GameObject.Find("ConseguirLibro").SetActive(true);
#endif   
        }


        // Usamos LateUpdate para la cámara para evitar vibraciones (Jitter)
        private void LateUpdate()
        {
            if (canRotate)
            {
                RotateCameraManual();
            }
        }

        private void Update()
        {
            // RotateView se movió a LateUpdate con lógica nueva

#if UNITY_ANDROID || UNITY_IOS
            if (JumpAxis && !m_Jump && doubleJumpCount < 2) {
                m_Jump = true;
                doubleJumpCount++;
            } else {
                doubleJumpCount = 1;
                m_Jump = false;
            }
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
#endif

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }
            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);

            // --- [TPS] CÁLCULO DE MOVIMIENTO RELATIVO A CÁMARA ---
            // Usamos la forward de la cámara
            Vector3 camForward = m_Camera.transform.forward;
            Vector3 camRight = m_Camera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 desiredMove = camForward * m_Input.y + camRight * m_Input.x;

            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            if (canMove)
            {
                m_MoveDir.x = desiredMove.x * speed;
                m_MoveDir.z = desiredMove.z * speed;
            }

            // --- [TPS] ROTACIÓN DEL CUERPO ---
            // Solo rotamos el cuerpo si nos movemos. 
            // ESTO YA NO AFECTARÁ A LA CÁMARA PORQUE LA CÁMARA USA ROTACIÓN GLOBAL EN LATEUPDATE
            if (desiredMove.sqrMagnitude > 0.01f && canMove)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredMove);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            }
            // ------------------------------------

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;
                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);
        }

        // --- [TPS] NUEVA LÓGICA DE ROTACIÓN ---
        private void RotateCameraManual()
        {
            // Leer Inputs
            float mouseX = CrossPlatformInputManager.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = CrossPlatformInputManager.GetAxis("Mouse Y") * mouseSensitivity;

            // Acumular valores (Esto guarda la posición "real" deseada)
            rotY += mouseX;
            rotX -= mouseY;

            // Limitar la vista arriba/abajo
            rotX = Mathf.Clamp(rotX, -60f, 60f);

            // APLICAR ROTACIÓN GLOBAL (WORLD SPACE)
            // Quaternion.Euler crea una rotación absoluta en el mundo.
            // Al asignar esto a .rotation (y no .localRotation), ignoramos 
            // completamente si el padre (body) está girando como loco.
            cameraPivot.rotation = Quaternion.Euler(rotX, rotY, 0);
        }
        // ----------------------------------------

        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }

        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? m_WalktepLenghten : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep)) return;

            m_NextStep = m_StepCycle + m_StepInterval;
            PlayFootStepAudio();
        }

        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded) return;
            AudioClip[] sounds = m_IsRunning ? m_RunningstepSounds : m_FootstepSounds;
            int n = Random.Range(1, sounds.Length);
            m_AudioSource.clip = sounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            sounds[n] = sounds[0];
            sounds[0] = m_AudioSource.clip;
        }

        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob) return;

            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }

        private void GetInput(out float speed)
        {
#if UNITY_ANDROID || UNITY_IOS
            float horizontal = RunAxis.x;
            float vertical = RunAxis.y;
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");
#endif
            bool waswalking = m_IsWalking;
#if !MOBILE_INPUT
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);
            m_IsRunning = (!m_IsWalking && (horizontal != 0 || vertical != 0)) ? true : false;
            if (m_Input.sqrMagnitude > 1) m_Input.Normalize();

            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (m_CollisionFlags == CollisionFlags.Below) return;
            if (body == null || body.isKinematic) return;
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}