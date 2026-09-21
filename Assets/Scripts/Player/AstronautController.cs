using UnityEngine;
using AresResurgence.Audio;
using AresResurgence.Interaction;
using AresResurgence.UI;

namespace AresResurgence.Player
{
    /// <summary>
    /// First-person astronaut character controller with Martian gravity (0.38g),
    /// suit biometrics simulation, head-bobbing, footsteps, and interaction raycasting.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class AstronautController : MonoBehaviour
    {
        [Header("Movement (Martian Physics)")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float sprintSpeed = 6.0f;
        [SerializeField] private float gravity = 3.72f; // 0.38g on Mars
        [SerializeField] private float jumpHeight = 1.6f; // Floatier jump in Martian gravity

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private Transform cameraHolder;

        [Header("Head Bob & Camera")]
        [SerializeField] private float bobFrequency = 1.8f;
        [SerializeField] private float bobAmount = 0.045f;

        [Header("Biometrics & Suit Telemetry")]
        [SerializeField] private float oxygenLevel = 98.4f;
        [SerializeField] private float baseO2DepletionRate = 0.04f; // % per second
        [SerializeField] private float sprintO2Multiplier = 2.5f;
        [SerializeField] private float heartRateBPM = 72f;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 3.5f;
        [SerializeField] private LayerMask interactLayers = ~0;

        private CharacterController characterController;
        private Camera playerCamera;
        private float verticalVelocity = 0f;
        private float pitch = 0f;
        private float bobTimer = 0f;
        private Vector3 defaultCameraPos;
        private IInteractable currentInteractable;

        public float OxygenLevel => oxygenLevel;
        public float HeartRateBPM => heartRateBPM;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (cameraHolder == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    cameraHolder = playerCamera.transform;
                }
            }
            else
            {
                playerCamera = cameraHolder.GetComponentInChildren<Camera>();
            }

            if (cameraHolder != null)
            {
                defaultCameraPos = cameraHolder.localPosition;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleBiometrics();
            HandleInteractionRaycast();

            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
            }
        }

        private void HandleMouseLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (cameraHolder != null)
            {
                cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            bool isGrounded = characterController.isGrounded;
            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = (transform.right * horizontal + transform.forward * vertical).normalized;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && inputDir.magnitude > 0.1f && isGrounded;
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(2f * jumpHeight * gravity);
            }

            // Martian Gravity
            verticalVelocity -= gravity * Time.deltaTime;

            Vector3 moveVelocity = inputDir * currentSpeed + Vector3.up * verticalVelocity;
            characterController.Move(moveVelocity * Time.deltaTime);

            // Head bob and footstep trigger
            if (inputDir.magnitude > 0.1f && isGrounded)
            {
                float freq = isSprinting ? bobFrequency * 1.4f : bobFrequency;
                float prevTimer = bobTimer;
                bobTimer += Time.deltaTime * freq;

                float bobY = Mathf.Sin(bobTimer * Mathf.PI * 2f) * (isSprinting ? bobAmount * 1.5f : bobAmount);
                if (cameraHolder != null)
                {
                    cameraHolder.localPosition = defaultCameraPos + new Vector3(0f, bobY, 0f);
                }

                // Play footstep at lowest point of bob cycle
                if (Mathf.Sin(prevTimer * Mathf.PI * 2f) > 0f && Mathf.Sin(bobTimer * Mathf.PI * 2f) <= 0f)
                {
                    if (CinematicAudioDirector.Instance != null)
                    {
                        CinematicAudioDirector.Instance.PlayFootstep();
                    }
                }
            }
            else
            {
                bobTimer = 0f;
                if (cameraHolder != null)
                {
                    cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, defaultCameraPos, Time.deltaTime * 6f);
                }
            }
        }

        private void HandleBiometrics()
        {
            bool isMoving = characterController.velocity.magnitude > 0.2f;
            bool isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);

            // O2 consumption
            float consumptionRate = isSprinting ? baseO2DepletionRate * sprintO2Multiplier : (isMoving ? baseO2DepletionRate * 1.2f : baseO2DepletionRate);
            oxygenLevel = Mathf.Max(0f, oxygenLevel - consumptionRate * Time.deltaTime);

            // Heart Rate dynamics
            float targetBPM = isSprinting ? 135f : (isMoving ? 92f : 72f);
            heartRateBPM = Mathf.Lerp(heartRateBPM, targetBPM, Time.deltaTime * (isSprinting ? 0.8f : 0.25f));

            if (CinematicAudioDirector.Instance != null)
            {
                CinematicAudioDirector.Instance.SetHeartRate(heartRateBPM);
            }

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.oxygenLevel = oxygenLevel;
                HelmetHUD.Instance.heartRateBPM = heartRateBPM;
            }
        }

        private void HandleInteractionRaycast()
        {
            Ray ray = new Ray(playerCamera != null ? playerCamera.transform.position : transform.position, playerCamera != null ? playerCamera.transform.forward : transform.forward);
            currentInteractable = null;

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayers))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>() ?? hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    currentInteractable = interactable;
                }
            }

            if (HelmetHUD.Instance != null)
            {
                if (currentInteractable != null)
                {
                    HelmetHUD.Instance.SetInteractionPrompt(currentInteractable.PromptText, true);
                }
                else
                {
                    HelmetHUD.Instance.SetInteractionPrompt("", false);
                }
            }

            // Interact key: E or Left Mouse Button
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) && currentInteractable != null)
            {
                if (CinematicAudioDirector.Instance != null)
                {
                    CinematicAudioDirector.Instance.PlayTerminalInteract();
                }
                currentInteractable.Interact(gameObject);
            }
        }
    }
}
