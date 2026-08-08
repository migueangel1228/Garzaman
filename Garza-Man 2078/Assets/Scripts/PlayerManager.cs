using UnityEngine; // Permite usar clases de Unity como Vector3, Mathf, MonoBehaviour y Time.
using UnityEngine.Serialization; // Permite conservar valores antiguos del Inspector si una variable cambio de nombre.

public class PlayerManager : MonoBehaviour // Script principal que controla al jugador.
{
    [Header("Movement")] // Crea un titulo en el Inspector para ordenar las variables de movimiento.
    [FormerlySerializedAs("runSpeed")] // Si antes existia runSpeed en el Inspector, Unity pasa ese valor a forwardSpeed.
    [SerializeField] private float forwardSpeed = 7f; // Velocidad constante hacia adelante, sobre el eje Z.
    [SerializeField] private float laneChangeSpeed = 10f; // Velocidad con la que el jugador se mueve de un carril a otro.
    [SerializeField] private float laneDistance = 1.1f; // Distancia horizontal entre el centro de un carril y el siguiente.
    [SerializeField] private int laneCount = 5; // Cantidad total de carriles disponibles.
    [SerializeField] private float inputDeadZone = 0.5f; // Valor minimo para aceptar el input horizontal.

    private InputManager inputManager; // Referencia al script que lee los inputs del jugador.
    private Animator animator; // Referencia al Animator para controlar las animaciones del jugador.
    private Vector3 startPosition; // Posicion inicial del jugador; se usa como centro para calcular los carriles.
    private int currentLane; // Carril actual del jugador. Ejemplo con 5 carriles: 0, 1, 2, 3, 4.
    private bool horizontalInputConsumed; // Evita que mantener A o D presionado mueva varios carriles de una vez.
    private bool isRolling; // Indica si el jugador esta en medio de una animacion de roll, para evitar que se pueda iniciar otro roll o cambiar de carril durante el roll.
    private bool isJumping; // Indica si el jugador esta en medio de una animacion de salto, para evitar que se pueda iniciar otro salto o cambiar de carril durante el salto.
    private void Start() // Se ejecuta una vez al iniciar el juego.
    {
        inputManager = GetComponent<InputManager>(); // Busca el InputManager en el mismo GameObject del jugador.
        animator = GetComponentInChildren<Animator>(); // Busca el Animator en el jugador o en sus hijos.
        laneCount = Mathf.Max(1, laneCount); // Evita que laneCount sea 0 o negativo.
        startPosition = transform.position; // Guarda la posicion inicial como referencia central.
        currentLane = Mathf.Clamp(laneCount / 2, 0, laneCount - 1); // Empieza en el carril del centro.
        isRolling = false; // El jugador empieza sin estar en medio de un roll.
        isJumping = false; // El jugador empieza sin estar en medio de un salto.
    }

    private void Update() // Se ejecuta una vez por frame.
    {
        if (inputManager == null) // Si no existe InputManager, no se puede leer input.
        {
            return; // Sale del Update para evitar errores.
        }

        HandleLaneInput(); // Revisa si el jugador presiono izquierda o derecha.
        MoveForward(); // Hace que el jugador avance siempre hacia adelante.
        MoveSideways(); // Mueve suavemente al jugador hacia el carril objetivo.
        CheckForRoll(); // Revisa si el jugador presiono el boton de roll.
        CheckForJump(); // Revisa si el jugador presiono el boton de salto.
    }

    private void HandleLaneInput() // Decide si el jugador debe cambiar de carril.
    {
        float horizontalInput = inputManager.horizontalMovement.ReadValue<float>(); // Lee A/D o stick horizontal.

        if (Mathf.Abs(horizontalInput) < inputDeadZone) // Si el input esta casi en 0, no cuenta como movimiento.
        {
            horizontalInputConsumed = false; // Permite que la proxima pulsacion vuelva a cambiar de carril.
            return; // Sale porque no hay movimiento lateral valido.
        }

        if (horizontalInputConsumed) // Si ya usamos esta pulsacion, no repetimos el cambio.
        {
            return; // Sale para no saltar varios carriles manteniendo la tecla.
        }

        int direction = horizontalInput > 0f ? 1 : -1; // D o derecha da 1; A o izquierda da -1.
        currentLane = Mathf.Clamp(currentLane + direction, 0, laneCount - 1); // Cambia el carril sin salirse de los limites.
        horizontalInputConsumed = true; // Marca que esta pulsacion ya fue usada.
    }

    private void MoveForward() // Aplica el avance automatico del runner.
    {
        transform.position += Vector3.forward * (forwardSpeed * Time.deltaTime); // Mueve al jugador en Z cada frame.
    }

    private void CheckForRoll()
    {
        if (IsBusy()) // Si el jugador esta ocupado haciendo un roll, no puede iniciar otro roll.
        {
            return; // Sale para evitar iniciar un nuevo roll.
        }
        if (inputManager.roll.WasPressedThisFrame()) // Si se detecta que el jugador presiono el boton de roll.
        {
            animator.SetTrigger("Roll"); // Activa la animacion de roll.
            isRolling = true; // Marca que el jugador esta haciendo un roll para evitar otras acciones.
        }
    }

    private void CheckForJump()
    {
        if (IsBusy()) // Si el jugador esta ocupado haciendo un salto o roll, no puede iniciar otro salto.
        {
            return; // Sale para evitar iniciar un nuevo salto.
        }
        if (inputManager.jump.WasPressedThisFrame()) // Si se detecta que el jugador presiono el boton de salto.
        {
            animator.SetTrigger("Jump"); // Activa la animacion de salto.
            isJumping = true; // Marca que el jugador esta haciendo un salto para evitar otras acciones.
        }
    }

    private bool IsBusy()
    {
        return isRolling || isJumping; // El jugador esta ocupado si esta haciendo un roll o un salto.
    }

    public void EndRoll()
    {
        isRolling = false; // Este metodo se llama desde un evento en la animacion de roll para marcar que el roll termino.
    }

    public void EndJump()
    {
        isJumping = false; // Este metodo se llama desde un evento en la animacion de salto para marcar que el salto termino.
    }

    private void MoveSideways() // Calcula y aplica el movimiento lateral hacia el carril actual.
    {
        float centeredLane = currentLane - ((laneCount - 1) * 0.5f); // Convierte el indice del carril en offset centrado.
        float laneOffset = centeredLane * laneDistance; // Convierte ese offset en unidades reales del mundo.
        float targetX = startPosition.x + laneOffset; // Calcula la X exacta donde debe quedar el jugador.
        Vector3 position = transform.position; // Copia la posicion actual para modificar solo X.

        position.x = Mathf.MoveTowards(position.x, targetX, laneChangeSpeed * Time.deltaTime); // Acerca X al carril objetivo.
        transform.position = position; // Aplica la nueva posicion al jugador.
    }
}
