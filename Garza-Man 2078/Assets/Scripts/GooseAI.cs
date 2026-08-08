using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class GooseAI : MonoBehaviour
{
    public enum State { Patrol, Suspicious, Investigate, Chase, Return }
    public State currentState = State.Patrol;

    [Header("Movement")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6.5f;
    public float acceleration = 8f;

    [Header("Detection")]
    public float visionRange = 15f;
    public float visionAngle = 60f;
    public float baseHearingRadius = 5f;
    public float catchRadius = 1.4f;
    public LayerMask detectionLayer;
    public float detectionTickInterval = 0.2f;

    [Header("Suspicion")]
    public float suspicionToChase = 100f;
    public float visionSuspicionRate = 55f;
    public float hearingSuspicionRate = 35f;
    public float suspicionDecayRate = 18f;
    public float chaseMemoryTime = 2f;
    [Range(0f, 100f)] public float currentSuspicion = 0f;

    [Header("Chase Recovery")]
    public float maxChaseTimeWithoutSight = 4f;
    public float maxUnreachableChaseTime = 2.5f;
    public float maxStuckTime = 2f;
    public float stuckSpeedThreshold = 0.15f;

    [Header("Patrol")]
    public List<Transform> patrolPoints;
    private int currentPatrolIndex = -1;

    [Header("Investigate")]
    public float investigationWaitTime = 3f;
    private Vector3 investigationPoint;

    [Header("Growl Audio")]
    public AudioSource growlAudioSource;
    public AudioClip[] growlClips;
    public float patrolGrowlMinDelay = 8f;
    public float patrolGrowlMaxDelay = 18f;
    public float chaseGrowlMinDelay = 3f;
    public float chaseGrowlMaxDelay = 7f;
    public float growlVolume = 0.8f;

    [Header("Footstep Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;
    public float patrolStepInterval = 0.6f;
    public float chaseStepInterval = 0.35f;
    public float footstepVolume = 0.8f;
    public float minimumMoveSpeedForFootsteps = 0.15f;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerController playerController;
    private bool isPlayerSafe = false;
    private bool isWaitingAtInvestigationPoint = false;
    private float timeSincePlayerSeen = float.MaxValue;
    private float unreachableChaseTimer = 0f;
    private float stuckTimer = 0f;
    private float nextGrowlTime;
    private float footstepTimer;
    private Vector3 lastKnownPlayerPosition;
    private Animator anim;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            player = GameObject.Find("PlayerCapsule")?.transform;

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            lastKnownPlayerPosition = player.position;
        }

        agent.speed = patrolSpeed;
        agent.acceleration = acceleration;

        anim = GetComponentInChildren<Animator>();

        if (growlAudioSource == null)
            growlAudioSource = GetComponent<AudioSource>();

        if (footstepAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length > 1)
                footstepAudioSource = audioSources[1];
        }

        ScheduleNextGrowl();
        ChooseNextPatrolPoint();
        StartCoroutine(AILoop());
    }

    private void Update()
    {
        if (anim != null && agent != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", currentSpeed);
        }

        HandleGrowls();
        HandleFootsteps();
    }

    private IEnumerator AILoop()
    {
        while (true)
        {
            CheckForPlayer();

            switch (currentState)
            {
                case State.Patrol:
                    Patrol();
                    break;
                case State.Suspicious:
                    Suspicious();
                    break;
                case State.Investigate:
                    Investigate();
                    break;
                case State.Chase:
                    Chase();
                    break;
                case State.Return:
                    ReturnToPatrol();
                    break;
            }

            yield return new WaitForSeconds(detectionTickInterval);
        }
    }

    private void HandleGrowls()
    {
        if (growlAudioSource == null) return;
        if (growlClips == null || growlClips.Length == 0) return;
        if (Time.time < nextGrowlTime) return;
        if (growlAudioSource.isPlaying) return;

        AudioClip clip = growlClips[Random.Range(0, growlClips.Length)];

        if (clip != null)
            growlAudioSource.PlayOneShot(clip, growlVolume);

        ScheduleNextGrowl();
    }

    private void HandleFootsteps()
    {
        if (footstepAudioSource == null) return;
        if (footstepClips == null || footstepClips.Length == 0) return;
        if (agent == null) return;

        bool isMoving = agent.velocity.magnitude > minimumMoveSpeedForFootsteps && currentState != State.Investigate;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        float interval = currentState == State.Chase ? chaseStepInterval : patrolStepInterval;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

            if (clip != null)
                footstepAudioSource.PlayOneShot(clip, footstepVolume);

            footstepTimer = interval;
        }
    }

    private void ScheduleNextGrowl()
    {
        float minDelay = currentState == State.Chase ? chaseGrowlMinDelay : patrolGrowlMinDelay;
        float maxDelay = currentState == State.Chase ? chaseGrowlMaxDelay : patrolGrowlMaxDelay;

        nextGrowlTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    private void CheckForPlayer()
    {
        if (player == null)
            return;

        if (isPlayerSafe)
        {
            currentSuspicion = 0f;
            if (currentState == State.Chase || currentState == State.Suspicious)
                StartInvestigation(lastKnownPlayerPosition);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (currentState == State.Chase && distanceToPlayer < catchRadius)
        {
            GameOver();
            return;
        }

        bool canSeePlayer = CanSeePlayer();
        bool canHearPlayer = CanHearPlayer(distanceToPlayer);

        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            timeSincePlayerSeen = 0f;
            currentSuspicion += GetVisionSuspicionRate(distanceToPlayer) * detectionTickInterval;
        }
        else
        {
            timeSincePlayerSeen += detectionTickInterval;
        }

        if (canHearPlayer)
        {
            lastKnownPlayerPosition = player.position;
            currentSuspicion += GetHearingSuspicionRate(distanceToPlayer) * detectionTickInterval;
        }

        if (currentSuspicion >= suspicionToChase && currentState != State.Chase)
        {
            currentSuspicion = suspicionToChase;
            StartChase();
            return;
        }

        if (canSeePlayer || canHearPlayer)
        {
            if (currentState == State.Patrol || currentState == State.Return || currentState == State.Investigate)
            {
                currentState = State.Suspicious;
                isWaitingAtInvestigationPoint = false;
            }

            return;
        }

        currentSuspicion = Mathf.Max(0f, currentSuspicion - suspicionDecayRate * detectionTickInterval);

        if (currentState == State.Chase && timeSincePlayerSeen >= maxChaseTimeWithoutSight)
        {
            StartInvestigation(lastKnownPlayerPosition);
        }
        else if (currentState == State.Suspicious && currentSuspicion <= 0f)
        {
            StartInvestigation(lastKnownPlayerPosition);
        }
    }

    private bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > GetEffectiveVisionRange())
            return false;

        Vector3 eyePosition = transform.position + Vector3.up * 1.4f;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > visionAngle / 2f)
            return false;

        Vector3[] targetOffsets = { Vector3.up * 0.2f, Vector3.up, Vector3.up * 1.7f };

        foreach (Vector3 offset in targetOffsets)
        {
            Vector3 targetPosition = player.position + offset;
            Vector3 rayDirection = (targetPosition - eyePosition).normalized;

            if (Physics.Raycast(eyePosition, rayDirection, out RaycastHit hit, visionRange, detectionLayer))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                    return true;
            }
        }

        return false;
    }

    private bool CanHearPlayer(float distanceToPlayer)
    {
        return distanceToPlayer <= GetEffectiveHearingRadius();
    }

    private float GetEffectiveVisionRange()
    {
        float range = visionRange;

        if (playerController != null)
        {
            if (playerController.IsCrouching)
                range *= 0.65f;
            else if (playerController.IsSprinting)
                range *= 1.15f;
        }

        return range;
    }

    private float GetEffectiveHearingRadius()
    {
        if (playerController == null)
            return baseHearingRadius;

        if (playerController.IsCrouching)
            return baseHearingRadius * 0.4f;

        if (playerController.IsSprinting)
            return baseHearingRadius * 2f;

        if (playerController.CurrentSpeed < 0.5f)
            return baseHearingRadius * 0.3f;

        return baseHearingRadius;
    }

    private float GetVisionSuspicionRate(float distanceToPlayer)
    {
        float proximityMultiplier = Mathf.Lerp(1.4f, 0.75f, distanceToPlayer / Mathf.Max(visionRange, 0.01f));
        float postureMultiplier = playerController != null && playerController.IsCrouching ? 0.65f : 1f;
        return visionSuspicionRate * proximityMultiplier * postureMultiplier;
    }

    private float GetHearingSuspicionRate(float distanceToPlayer)
    {
        float hearingRadius = Mathf.Max(GetEffectiveHearingRadius(), 0.01f);
        float proximityMultiplier = Mathf.Lerp(1.25f, 0.6f, distanceToPlayer / hearingRadius);
        return hearingSuspicionRate * proximityMultiplier;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 1f)
            ChooseNextPatrolPoint();
    }

    private void Suspicious()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(lastKnownPlayerPosition);

        Vector3 direction = lastKnownPlayerPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, detectionTickInterval * 4f);
        }
    }

    private void Investigate()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(investigationPoint);

        if (!agent.pathPending && agent.remainingDistance < 1f && !isWaitingAtInvestigationPoint)
            StartCoroutine(WaitThenReturn());
    }

    private IEnumerator WaitThenReturn()
    {
        isWaitingAtInvestigationPoint = true;
        yield return new WaitForSeconds(investigationWaitTime);

        if (currentState == State.Investigate)
        {
            currentSuspicion = 0f;
            currentState = State.Return;
        }

        isWaitingAtInvestigationPoint = false;
    }

    private void Chase()
    {
        if (player == null) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (timeSincePlayerSeen >= maxChaseTimeWithoutSight)
        {
            StartInvestigation(lastKnownPlayerPosition);
            return;
        }

        if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
            unreachableChaseTimer += detectionTickInterval;
        else
            unreachableChaseTimer = 0f;

        if (agent.hasPath && agent.velocity.magnitude <= stuckSpeedThreshold)
            stuckTimer += detectionTickInterval;
        else
            stuckTimer = 0f;

        if (unreachableChaseTimer >= maxUnreachableChaseTime || stuckTimer >= maxStuckTime)
            StartInvestigation(GetReachableInvestigationPoint());
    }

    private void ReturnToPatrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            currentState = State.Patrol;
            return;
        }

        if (currentPatrolIndex < 0 || currentPatrolIndex >= patrolPoints.Count)
            ChooseNextPatrolPoint();
        else
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (!agent.pathPending && agent.remainingDistance < 1f)
            currentState = State.Patrol;
    }

    private void ChooseNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        int nextIndex = Random.Range(0, patrolPoints.Count);

        if (patrolPoints.Count > 1)
        {
            int guard = 0;
            while (nextIndex == currentPatrolIndex && guard < 8)
            {
                nextIndex = Random.Range(0, patrolPoints.Count);
                guard++;
            }
        }

        currentPatrolIndex = nextIndex;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void StartInvestigation(Vector3 point)
    {
        investigationPoint = point;
        currentState = State.Investigate;
        isWaitingAtInvestigationPoint = false;
        ResetChaseRecoveryTimers();
        ScheduleNextGrowl();
    }

    private void StartChase()
    {
        currentState = State.Chase;
        isWaitingAtInvestigationPoint = false;
        ResetChaseRecoveryTimers();
        ScheduleNextGrowl();
    }

    private void ResetChaseRecoveryTimers()
    {
        unreachableChaseTimer = 0f;
        stuckTimer = 0f;
    }

    private Vector3 GetReachableInvestigationPoint()
    {
        if (NavMesh.SamplePosition(lastKnownPlayerPosition, out NavMeshHit hit, 3f, agent.areaMask))
            return hit.position;

        return transform.position;
    }

    public void SetPlayerSafe(bool safe)
    {
        isPlayerSafe = safe;

        if (player != null)
            lastKnownPlayerPosition = player.position;

        if (safe && (currentState == State.Chase || currentState == State.Suspicious))
            StartInvestigation(lastKnownPlayerPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isPlayerSafe && collision.gameObject.CompareTag("Player"))
            GameOver();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPlayerSafe && other.CompareTag("Player"))
            GameOver();
    }

    private void GameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GAME OVER - Puchi was caught by the goose! (GameManager missing)");
            Time.timeScale = 0;
        }
    }
}