using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    public GameObject flashlightObject;
    private bool isOn = false; // Queremos que empiece apagada

    private PlayerInput playerInput;
    private InputAction flashlightAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        flashlightAction = playerInput.actions.FindAction("Flashlight");

        // FORZAMOS el estado inicial al empezar el juego
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(isOn);
        }
    }

    void OnEnable()
    {
        flashlightAction.performed += ToggleFlashlight;
        flashlightAction.Enable();
    }

    void OnDisable()
    {
        flashlightAction.performed -= ToggleFlashlight;
        flashlightAction.Disable();
    }

    private void ToggleFlashlight(InputAction.CallbackContext context)
    {
        isOn = !isOn;
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(isOn);
        }
    }
}