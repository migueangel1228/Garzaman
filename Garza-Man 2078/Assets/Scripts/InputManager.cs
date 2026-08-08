using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private MyPlayerControls controls;
    public InputAction horizontalMovement;
    public InputAction roll;
    public InputAction jump;
    private void Awake()
    {
        controls = new MyPlayerControls();

        horizontalMovement = controls.InGame.HorizontalMovement;
        jump = controls.InGame.Jump;
        roll = controls.InGame.Roll;
        
        horizontalMovement.Enable();
        jump.Enable();
        roll.Enable();
    }

    private void OnDisable()
    {
        horizontalMovement.Disable();
        jump.Disable();
        roll.Disable();
    }
}
