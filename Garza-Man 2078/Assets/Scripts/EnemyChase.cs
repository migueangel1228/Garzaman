using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Start Chase")]
    [SerializeField] private float startDistanceBehindPlayer = 1.5f;
    [SerializeField] private float startHeightOffset = 0f;
    [SerializeField] private float initialSpeed = 8f;
    [SerializeField] private float initialAcceleration = 20f;
    [SerializeField] private float initialStoppingDistance = 0.5f;
    [SerializeField] private float highPressureDuration = 3f;

    [Header("After Pressure")]
    [SerializeField] private float chaseSpeed = 6.3f;
    [SerializeField] private float catchUpSpeed = 7.4f;
    [SerializeField] private float chaseAcceleration = 8f;
    [SerializeField] private float chaseStoppingDistance = 1.8f;
    [SerializeField] private float maxVisibleDistance = 8f;

    private NavMeshAgent agent;
    private bool pressureFinished;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PositionEnemyNearPlayer();
        ApplyInitialChaseValues();
        StartCoroutine(ReducePressureAfterDelay());
    }

    private void Update()
    {
        if (player == null || agent == null)
        {
            return;
        }

        agent.SetDestination(player.position);
        UpdateChaseSpeed();
    }

    // Coloca al enemigo un poco detrás del jugador para que la persecución arranque cerca.
    private void PositionEnemyNearPlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 startPosition = player.position - (player.forward * startDistanceBehindPlayer);
        startPosition.y += startHeightOffset;

        if (agent.enabled)
        {
            agent.Warp(startPosition);
            return;
        }

        transform.position = startPosition;
    }

    // Durante los primeros segundos el enemigo corre más fuerte y se mantiene muy cerca.
    private void ApplyInitialChaseValues()
    {
        agent.speed = initialSpeed;
        agent.acceleration = initialAcceleration;
        agent.stoppingDistance = initialStoppingDistance;
    }

    // Después de la presión inicial, el enemigo baja un poco el ritmo, pero no desaparece de cámara.
    private IEnumerator ReducePressureAfterDelay()
    {
        yield return new WaitForSeconds(highPressureDuration);

        if (agent == null)
        {
            yield break;
        }

        agent.speed = chaseSpeed;
        agent.acceleration = chaseAcceleration;
        agent.stoppingDistance = chaseStoppingDistance;
        pressureFinished = true;
    }

    // Si el jugador se aleja demasiado, el enemigo acelera un poco para volver a sentirse presente.
    private void UpdateChaseSpeed()
    {
        if (!pressureFinished)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        agent.speed = distanceToPlayer > maxVisibleDistance ? catchUpSpeed : chaseSpeed; 
    }
}
