using UnityEngine;
using System.Collections;

public abstract class BaseStormtrooper : MonoBehaviour
{
    public enum EnemyState { Patrol, Alert, Attack, Lost }

    [Header("Состояния и ИИ")]
    public EnemyState currentState = EnemyState.Patrol;
    public float detectionRadius = 8f;
    public float fieldOfViewAngle = 120f;
    public float alertDuration = 0.6f; 
    public float lostTargetMemoryTime = 3f; 

    [Header("Патрулирование")]
    public float patrolSpeed = 2f;
    public float patrolWaitTime = 2f;
    public float patrolRadius = 4f;

    [Header("Движение при атаке (Стрейф)")]
    public bool canStrafeDuringAttack = true; 
    public float strafeSpeed = 1.8f;          
    public float strafeChangeInterval = 1.5f; 

    [Header("Базовые Префабы")]
    public GameObject blasterBoltPrefab;
    public Transform firePoint;
    public GameObject deathEffectPrefab;

    [Header("Базовые Настройки Стрельбы")]
    public float minFireRate = 1f;
    public float maxFireRate = 3f;
    public float fireRateMultiplier = 1.5f;

    protected float nextFireTime;
    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    
    protected Vector2 lastKnownPosition;
    protected Vector2 patrolTarget;
    protected float arrowInputTimer = 0f;
    
    protected int strafeDirection = 1; 
    private float strafeDirectionTimer = 0f;
    private bool isWaitingAtPatrol = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        SetNextFireTime();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        patrolTarget = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
        strafeDirection = Random.value > 0.5f ? 1 : -1;
    }

    protected virtual void Update()
    {
        if (player == null) return;

        HandleDetection();

        arrowInputTimer += Time.deltaTime; 

        if (currentState == EnemyState.Patrol)
        {
            if (Vector2.Distance(transform.position, patrolTarget) < 0.3f && !isWaitingAtPatrol)
            {
                StartCoroutine(WaitAndPickNewPatrolPoint());
            }
        }

        if (currentState == EnemyState.Attack && canStrafeDuringAttack)
        {
            strafeDirectionTimer += Time.deltaTime;
            if (strafeDirectionTimer >= strafeChangeInterval)
            {
                strafeDirectionTimer = 0f;
                float roll = Random.value;
                if (roll < 0.7f)
                {
                    strafeDirection = (strafeDirection == 0) ? (Random.value > 0.5f ? 1 : -1) : -strafeDirection;
                }
                else
                {
                    strafeDirection = 0; 
                }
            }
        }

        if (currentState == EnemyState.Lost)
        {
            if (Vector2.Distance(transform.position, lastKnownPosition) < 0.5f || arrowInputTimer >= lostTargetMemoryTime)
            {
                TransitionToState(EnemyState.Patrol);
            }
        }

        if (currentState == EnemyState.Attack && Time.time >= nextFireTime)
        {
            ExecuteShooting();
            SetNextFireTime();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (player == null) return;

        switch (currentState)
        {
            case EnemyState.Patrol: HandlePatrolPhysics(); break;
            case EnemyState.Alert:  HandleAlertPhysics();  break;
            case EnemyState.Attack: HandleAttackPhysics(); break;
            case EnemyState.Lost:   HandleLostPhysics();   break;
        }
    }

    private IEnumerator WaitAndPickNewPatrolPoint()
    {
        isWaitingAtPatrol = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(patrolWaitTime);
        
        patrolTarget = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
        isWaitingAtPatrol = false;
    }

    private void HandleDetection()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        bool inView = false;

        if (distance <= detectionRadius)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            
            // ИСПРАВЛЕНО: Спрайты смотрят вниз (Y-), поэтому используем -transform.up в качестве лица
            float angle = Vector2.Angle(-transform.up, dirToPlayer);

            if (angle < fieldOfViewAngle * 0.5f || distance < 2f)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dirToPlayer, detectionRadius);
                foreach (var hit in hits)
                {
                    if (hit.collider.isTrigger || hit.collider.CompareTag("Enemy")) continue;

                    if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Perimeter"))
                    {
                        break; 
                    }

                    if (hit.collider.CompareTag("Player"))
                    {
                        inView = true;
                        lastKnownPosition = player.position;
                        break;
                    }
                }
            }
        }

        if (inView)
        {
            if (currentState == EnemyState.Patrol || currentState == EnemyState.Lost)
                TransitionToState(EnemyState.Alert);
            else if (currentState == EnemyState.Alert && arrowInputTimer >= alertDuration)
                TransitionToState(EnemyState.Attack);
        }
        else if (currentState == EnemyState.Attack)
        {
            TransitionToState(EnemyState.Lost);
        }
    }

    protected virtual void TransitionToState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        arrowInputTimer = 0; 
        strafeDirectionTimer = 0f;

        if (sr != null)
        {
            if (newState == EnemyState.Alert) sr.color = Color.yellow;
            else if (newState == EnemyState.Attack) sr.color = Color.white;
            else if (newState == EnemyState.Lost) sr.color = Color.gray;
            else sr.color = Color.white;
        }
    }

    // --- ФИЗИЧЕСКИЕ МЕТОДЫ ДВИЖЕНИЯ С НОВЫМ УГЛОМ (+90f) ---

    protected virtual void HandlePatrolPhysics()
    {
        if (isWaitingAtPatrol)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = (patrolTarget - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * patrolSpeed * Time.fixedDeltaTime);
        
        // ИСПРАВЛЕНО: +90f для разворота лица спрайта вперед
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    protected virtual void HandleAlertPhysics()
    {
        rb.linearVelocity = Vector2.zero; 
        
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        // ИСПРАВЛЕНО: +90f
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    protected virtual void HandleAttackPhysics()
    {
        Vector2 toPlayer = ((Vector2)player.position - rb.position).normalized;

        if (canStrafeDuringAttack && strafeDirection != 0)
        {
            Vector2 strafeDir = new Vector2(-toPlayer.y, toPlayer.x) * strafeDirection;
            rb.MovePosition(rb.position + strafeDir * strafeSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        // ИСПРАВЛЕНО: +90f
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    protected virtual void HandleLostPhysics()
    {
        Vector2 dir = (lastKnownPosition - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * patrolSpeed * Time.fixedDeltaTime);

        // ИСПРАВЛЕНО: +90f
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    protected abstract void ExecuteShooting();

    protected virtual void SetNextFireTime()
    {
        float baseCooldown = Random.Range(minFireRate, maxFireRate);
        nextFireTime = Time.time + (baseCooldown / fireRateMultiplier);
    }

    public virtual void TakeDamage()
    {
        if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.08f, 0.12f);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperDeathSound);
        if (GameManager.Instance != null) GameManager.Instance.CheckEnemyCount();
        Destroy(gameObject);
    }
}