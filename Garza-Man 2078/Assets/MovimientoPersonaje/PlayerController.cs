using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float lookSensitivity = 0.1f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    private float standingHeight;
    private Vector3 cameraStandingPos;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public float interactLookAngle = 8f;
    public LayerMask interactLayer;

    [Header("Footstep Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;
    public float walkStepInterval = 0.55f;
    public float sprintStepInterval = 0.35f;
    public float crouchStepInterval = 0.8f;
    public float footstepVolume = 0.8f;
    public bool debugFootsteps = false;

    [Header("Jump Audio")]
    public AudioClip jumpClip;
    public float jumpVolume = 0.7f;

    [Header("Land Audio")]
    public AudioClip landClip;
    public float landVolume = 0.7f;

    private CharacterController controller;
    private FeatherHUD featherHud;
    private Interactable currentInteractable;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float cameraPitch = 0f;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private float footstepTimer;

    public bool IsCrouching => isCrouching;
    public bool IsSprinting => isSprinting;
    public float CurrentSpeed => new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;

        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>()?.transform;

        if (cameraTransform != null)
            cameraStandingPos = cameraTransform.localPosition;

        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        featherHud = Object.FindAnyObjectByType<FeatherHUD>();

        GetComponent<PlayerInput>().currentActionMap.Enable();
    }

    private void Update()
    {
        UpdateSprintState();
        HandleRotation();
        HandleMovement();
        HandleLanding();
        HandleFootsteps();
        HandleCrouch();
        UpdateInteractionTarget();
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (isGrounded && value.isPressed && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (footstepAudioSource != null && jumpClip != null)
                footstepAudioSource.PlayOneShot(jumpClip, jumpVolume);
        }
    }

    public void OnSprint(InputValue value) => isSprinting = value.isPressed;

    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
            isCrouching = !isCrouching;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
            PerformInteraction();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        isGrounded = controller.isGrounded;
    }

    private void HandleLanding()
    {
        if (!wasGrounded && isGrounded)
        {
            if (footstepAudioSource != null && landClip != null)
                footstepAudioSource.PlayOneShot(landClip, landVolume);
        }

        wasGrounded = isGrounded;
    }

    private void HandleFootsteps()
    {
        if (footstepAudioSource == null)
        {
            if (debugFootsteps) Debug.Log("No hay Footstep Audio Source");
            return;
        }

        if (footstepClips == null || footstepClips.Length == 0)
        {
            if (debugFootsteps) Debug.Log("No hay clips de pasos asignados");
            return;
        }

        if (!controller.isGrounded)
            return;

        bool isMoving = moveInput.magnitude > 0.1f;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        float interval = isCrouching ? crouchStepInterval : (isSprinting ? sprintStepInterval : walkStepInterval);

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

            if (clip == null)
            {
                if (debugFootsteps) Debug.Log("Hay un espacio vacio en Footstep Clips");
                return;
            }

            footstepAudioSource.PlayOneShot(clip, footstepVolume);

            if (debugFootsteps) Debug.Log("Paso sonando: " + clip.name);

            footstepTimer = interval;
        }
    }

    private void UpdateSprintState()
    {
        if (Keyboard.current == null) return;

        isSprinting = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);

        if (cameraTransform != null)
        {
            Vector3 targetCamPos = isCrouching
                ? new Vector3(0, cameraStandingPos.y - 0.5f, 0)
                : cameraStandingPos;

            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetCamPos, Time.deltaTime * 10f);
        }
    }

    private void HandleRotation()
    {
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);
        cameraPitch -= lookInput.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void PerformInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.BaseInteract();
            currentInteractable = null;
            SetInteractionPrompt("");
        }
    }

    private void UpdateInteractionTarget()
    {
        currentInteractable = FindLookedAtInteractable();
        SetInteractionPrompt(currentInteractable != null ? currentInteractable.promptMessage : "");
    }

    private Interactable FindLookedAtInteractable()
    {
        if (cameraTransform == null) return null;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable == null) return null;

            Vector3 directionToColliderCenter = (hit.collider.bounds.center - cameraTransform.position).normalized;
            float lookAngle = Vector3.Angle(cameraTransform.forward, directionToColliderCenter);

            return lookAngle <= interactLookAngle ? interactable : null;
        }

        return null;
    }

    private void SetInteractionPrompt(string message)
    {
        if (featherHud == null)
            featherHud = Object.FindAnyObjectByType<FeatherHUD>();

        if (featherHud != null)
            featherHud.SetPrompt(message);
    }
}