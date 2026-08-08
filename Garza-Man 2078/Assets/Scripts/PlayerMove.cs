using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float runSpeed = 7;
    public float rotationSpeed = 250;
    public Animator animator;

    private Vector2 moveInput;
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Player.Enable();

        // Captura el vector de movimiento (x,y)
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        float x = moveInput.x; // izquierda/derecha
        float y = moveInput.y; // adelante/atrás

        // Movimiento físico
        transform.Rotate(0, x * Time.deltaTime * rotationSpeed, 0);
        transform.Translate(0, 0, y * Time.deltaTime * runSpeed);

        // Parámetros para el Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", x);        // adelante/atrás
            animator.SetFloat("Horizontal", y);  // izquierda/derecha
        }
    }
}
