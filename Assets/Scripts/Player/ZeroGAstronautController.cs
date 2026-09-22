using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AresResurgence.Player
{
    /// <summary>
    /// 6-DOF Zero-Gravity Astronaut Controller.
    /// Simulates true microgravity physics governed by Newton's Laws of Motion:
    /// - 1st Law: An object in motion remains in motion at constant velocity unless acted on by force.
    /// - 3rd Law: Thruster firing produces an equal and opposite reaction force (F_thrust = -F_reaction).
    /// Supports both New Input System and Legacy Input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ZeroGAstronautController : MonoBehaviour
    {
        [Header("Zero-G Thruster Dynamics")]
        public float thrustForce = 4.5f;
        public float boostMultiplier = 2.0f;
        public float rotationTorque = 2.5f;
        public float linearDampingNormal = 0.05f;
        public float linearDampingBrake = 1.8f;
        public float angularDamping = 0.8f;

        [Header("Camera & View")]
        public Transform cameraTransform;
        public float mouseSensitivity = 1.6f;
        public float maxPitchAngle = 85f;

        [Header("Audio")]
        public AudioSource rcsAudioSource;
        public AudioClip rcsBurstClip;

        [Header("State Telemetry")]
        public Vector3 currentVelocity;
        public float speedMetersPerSec;
        public bool isBraking = false;
        public bool isThrusterFiring = false;
        public Vector3 lastThrustVector;
        public float rcsPropellant = 100f; // Percentage

        private Rigidbody rb;
        private float pitch = 0f;
        private float yaw = 0f;
        private float roll = 0f;
        private float rcsSoundTimer = 0f;

        public bool controlsEnabled = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = linearDampingNormal;
            rb.angularDamping = angularDamping;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }

            if (rcsAudioSource == null)
            {
                rcsAudioSource = gameObject.AddComponent<AudioSource>();
                rcsAudioSource.playOnAwake = false;
                rcsAudioSource.spatialBlend = 0f; // 2D crisp visor sound
                rcsAudioSource.volume = 0.65f;
            }

            if (rcsBurstClip == null)
            {
                rcsBurstClip = Resources.Load<AudioClip>("Audio/RCS_Thruster_Burst");
            }
        }

        private void Start()
        {
            Vector3 euler = transform.eulerAngles;
            pitch = euler.x;
            yaw = euler.y;
            roll = euler.z;
        }

        private void Update()
        {
            if (!controlsEnabled) return;

            HandleMouseLook();
            HandleBrakingInput();

            currentVelocity = rb.linearVelocity;
            speedMetersPerSec = currentVelocity.magnitude;

            if (isThrusterFiring && rcsPropellant > 0f)
            {
                rcsPropellant = Mathf.Max(0f, rcsPropellant - Time.deltaTime * 1.5f);
            }
        }

        private void FixedUpdate()
        {
            if (!controlsEnabled) return;

            Apply6DOFThrusters();
        }

        private void HandleMouseLook()
        {
            float mouseX = 0f;
            float mouseY = 0f;

            if (Mouse.current != null)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                mouseX = delta.x * 0.1f * mouseSensitivity;
                mouseY = delta.y * 0.1f * mouseSensitivity;
            }
            else
            {
                mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            }

            // Optional Roll keys (Q / E)
            float rollInput = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed) rollInput += 1f;
                if (Keyboard.current.eKey.isPressed) rollInput -= 1f;
            }
            else
            {
                if (Input.GetKey(KeyCode.Q)) rollInput += 1f;
                if (Input.GetKey(KeyCode.E)) rollInput -= 1f;
            }

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
            roll += rollInput * rotationTorque * 25f * Time.deltaTime;

            transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        }

        private void HandleBrakingInput()
        {
            bool brakePressed = false;
            if (Keyboard.current != null)
            {
                brakePressed = Keyboard.current.xKey.isPressed;
            }
            else
            {
                brakePressed = Input.GetKey(KeyCode.X);
            }

            isBraking = brakePressed;
            rb.linearDamping = isBraking ? linearDampingBrake : linearDampingNormal;
            rb.angularDamping = isBraking ? linearDampingBrake * 1.5f : angularDamping;
        }

        private void Apply6DOFThrusters()
        {
            Vector3 thrustInput = Vector3.zero;

            // Forward / Backward (W / S)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) thrustInput += Vector3.forward;
                if (Keyboard.current.sKey.isPressed) thrustInput += Vector3.back;
                if (Keyboard.current.dKey.isPressed) thrustInput += Vector3.right;
                if (Keyboard.current.aKey.isPressed) thrustInput += Vector3.left;
                if (Keyboard.current.spaceKey.isPressed) thrustInput += Vector3.up;
                if (Keyboard.current.leftShiftKey.isPressed) thrustInput += Vector3.down;
            }
            else
            {
                if (Input.GetKey(KeyCode.W)) thrustInput += Vector3.forward;
                if (Input.GetKey(KeyCode.S)) thrustInput += Vector3.back;
                if (Input.GetKey(KeyCode.D)) thrustInput += Vector3.right;
                if (Input.GetKey(KeyCode.A)) thrustInput += Vector3.left;
                if (Input.GetKey(KeyCode.Space)) thrustInput += Vector3.up;
                if (Input.GetKey(KeyCode.LeftShift)) thrustInput += Vector3.down;
            }

            if (thrustInput.sqrMagnitude > 0.01f && rcsPropellant > 0f)
            {
                thrustInput.Normalize();
                Vector3 worldThrust = transform.TransformDirection(thrustInput);
                float force = thrustForce;

                // Newton's 2nd & 3rd Laws: apply force in requested direction
                rb.AddForce(worldThrust * force, ForceMode.Force);
                lastThrustVector = worldThrust;
                isThrusterFiring = true;

                // Play burst sound
                rcsSoundTimer -= Time.fixedDeltaTime;
                if (rcsSoundTimer <= 0f)
                {
                    if (rcsAudioSource != null && rcsBurstClip != null)
                    {
                        rcsAudioSource.pitch = Random.Range(0.95f, 1.05f);
                        rcsAudioSource.PlayOneShot(rcsBurstClip, 0.45f);
                    }
                    rcsSoundTimer = 0.85f;
                }
            }
            else
            {
                isThrusterFiring = false;
            }
        }

        public void StopAllVelocity()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
