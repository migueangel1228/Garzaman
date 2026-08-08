using UnityEngine;

public class movementsPlayerAnimations : MonoBehaviour
{
    public Animator animator;   // referencia al Animator del personaje

    void Update()
    {
        // Detectar movimiento con WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float speed = new Vector3(horizontal, 0, vertical).magnitude;

        // Pasar velocidad al Animator
        animator.SetFloat("Speed", speed);

        // Shift activa correr
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && speed > 0.1f;
        animator.SetBool("IsRunning", isRunning);

        // Salto
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("IsJumping");
        }
    }
}
