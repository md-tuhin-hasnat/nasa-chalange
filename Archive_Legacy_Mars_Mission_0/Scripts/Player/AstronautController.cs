using System;
using UnityEngine;
using UnityEngine.InputSystem;
using AresResurgence.Audio;
using AresResurgence.Interaction;
using AresResurgence.UI;

namespace AresResurgence.Player
{
    /// <summary>
    /// First-person astronaut character controller with Martian gravity (0.38g),
    /// dual input system (Unity New Input System + Legacy fallback),
    /// suit biometrics simulation, head-bobbing, footsteps, and interaction raycasting.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class AstronautController : MonoBehaviour
    {
        [Header("Movement (Martian Physics)")]
        [SerializeField] private float walkSpeed = 4.2f;
        [SerializeField] private float sprintSpeed = 7.5f;
        [SerializeField] private float gravity = 3.72f; // 0.38g on Mars
        [SerializeField] private float jumpHeight = 1.8f; // Floatier jump in Martian gravity

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 1.6f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private Transform cameraHolder;

        [Header("Head Bob & Camera")]
        [SerializeField] private float bobFrequency = 2.0f;
        [SerializeField] private float bobAmount = 0.04f;

        [Header("Biometrics & Suit Telemetry")]
        [SerializeField] private float oxygenLevel = 98.4f;
        [SerializeField] private float baseO2DepletionRate = 0.035f; // % per second
        [SerializeField] private float sprintO2Multiplier = 2.4f;
        [SerializeField] private float heartRateBPM = 72f;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 4.0f;
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
                if (playerCamera == null)
                {
                    playerCamera = Camera.main;
                }
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
            HandleCursorToggles();
        }

        private void HandleCursorToggles()
        {
            bool clickPressed = false;
            bool escapePressed = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) clickPressed = true;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) escapePressed = true;

            // Fallback
            try
            {
                if (!clickPressed && Input.GetMouseButtonDown(0)) clickPressed = true;
                if (!escapePressed && Input.GetKeyDown(KeyCode.Escape)) escapePressed = true;
            }
            catch { }

            if (clickPressed && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (escapePressed)
            {
                Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = (Cursor.lockState != CursorLockMode.Locked);
            }
        }

        private void HandleMouseLook()
        {
            float mouseX = 0f;
            float mouseY = 0f;

            if (Mouse.current != null)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                mouseX = delta.x * mouseSensitivity * 0.12f;
                mouseY = delta.y * mouseSensitivity * 0.12f;
            }
            else
            {
                try
                {
                    mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                    mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
                }
                catch { }
            }

            // Also support Arrow Keys or Numpad for turning if mouse isn't focused
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed) mouseX -= 1.8f * mouseSensitivity * Time.deltaTime * 60f;
                if (Keyboard.current.rightArrowKey.isPressed) mouseX += 1.8f * mouseSensitivity * Time.deltaTime * 60f;
                if (Keyboard.current.upArrowKey.isPressed) mouseY += 1.2f * mouseSensitivity * Time.deltaTime * 60f;
                if (Keyboard.current.downArrowKey.isPressed) mouseY -= 1.2f * mouseSensitivity * Time.deltaTime * 60f;
            }

            if (Mathf.Abs(mouseX) > 0.001f)
            {
                transform.Rotate(Vector3.up * mouseX);
            }

            if (Mathf.Abs(mouseY) > 0.001f)
            {
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

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

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current != null)
            {
                Keyboard kb = Keyboard.current;
                if (kb.wKey.isPressed) vertical += 1f;
                if (kb.sKey.isPressed) vertical -= 1f;
                if (kb.aKey.isPressed) horizontal -= 1f;
                if (kb.dKey.isPressed) horizontal += 1f;
            }
            else
            {
                try
                {
                    horizontal = Input.GetAxisRaw("Horizontal");
                    vertical = Input.GetAxisRaw("Vertical");
                }
                catch { }
            }

            Vector3 inputDir = (transform.right * horizontal + transform.forward * vertical).normalized;

            bool isSprinting = false;
            if (Keyboard.current != null)
            {
                isSprinting = (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) && inputDir.magnitude > 0.1f && isGrounded;
            }
            else
            {
                try { isSprinting = Input.GetKey(KeyCode.LeftShift) && inputDir.magnitude > 0.1f && isGrounded; } catch { }
            }

            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Jump
            bool jumpPressed = false;
            if (Keyboard.current != null)
            {
                jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            }
            else
            {
                try { jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space); } catch { }
            }

            if (jumpPressed && isGrounded)
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
                float freq = isSprinting ? bobFrequency * 1.35f : bobFrequency;
                float prevTimer = bobTimer;
                bobTimer += Time.deltaTime * freq;

                float bobY = Mathf.Sin(bobTimer * Mathf.PI * 2f) * (isSprinting ? bobAmount * 1.4f : bobAmount);
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
            bool isSprinting = false;

            if (Keyboard.current != null)
            {
                isSprinting = isMoving && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            }
            else
            {
                try { isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift); } catch { }
            }

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

            if (HollywoodVisorHUD.Instance != null)
            {
                HollywoodVisorHUD.Instance.oxygenLevel = oxygenLevel;
                HollywoodVisorHUD.Instance.heartRateBPM = heartRateBPM;
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

            if (HollywoodVisorHUD.Instance != null)
            {
                HollywoodVisorHUD.Instance.SetInteractionPrompt(currentInteractable != null ? currentInteractable.PromptText : "", currentInteractable != null);
            }

            // Interact key: E or Space or Left Mouse Click
            bool interactPressed = false;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) interactPressed = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) interactPressed = true;

            try
            {
                if (!interactPressed && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))) interactPressed = true;
            }
            catch { }

            if (interactPressed && currentInteractable != null)
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
