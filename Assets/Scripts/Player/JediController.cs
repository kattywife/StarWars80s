using UnityEngine;
using System.Collections;

public class JediController : MonoBehaviour
{
    [Header("Основные настройки")]
    public int health = 3;

    [Header("Настройки Движения (Keyboard WASD)")]
    public float maxMoveSpeed = 6f;                  
    public AnimationCurve moveAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); 
    public float moveAccelerationDuration = 0.2f;    
    public float moveDecelerationDuration = 0.15f;   
    
    private Vector2 activeMoveVelocity;              
    private float moveInputTimer = 0f;
    private Vector2 lastMoveDirection = Vector2.zero;

    [Header("Управление Стрелками (Keyboard Rotation)")]
    public float arrowMaxSpeed = 360f;               
    public AnimationCurve arrowAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); 
    public float arrowAccelerationDuration = 0.25f;  
    public float arrowDecelerationDuration = 0.15f;  

    [Header("Управление Мышью (Mouse Rotation)")]
    public float mouseSmoothTime = 0.05f;             
    private float mouseRotationVelocity;              

    [Header("Настройки Отдачи (Knockback)")]
    public float knockbackDecay = 8f;                 
    private Vector2 knockbackVelocity;                

    [Header("Настройки Рывка (Dash Settings)")]
    public KeyCode dashKey = KeyCode.LeftShift;       
    public float dashDistance = 4f;                   
    public float dashDuration = 0.18f;                 
    public float dashCooldown = 0.8f;                 
    private bool isDashing = false;
    private float lastDashTime;

    [Header("Настройки Урона и Сока (Damage Juice)")]
    public float damageKnockbackForce = 12f;          // Сила отдачи при получении урона
    public float damageSlowMultiplier = 0.5f;         // Замедление (0.5 = на 50% медленнее во время i-frames)
    public float damageRecoveryDuration = 1.5f;       // Длительность неуязвимости и замедления (сек)
    public float flickerInterval = 0.08f;             // Скорость мерцания спрайта

    [Header("Настройки Выпадения Кристаллов")]
    public bool dropCrystalsOnDamage = true;          // Должны ли выпадать кристаллы при уроне
    public int crystalsToDrop = 1;                    // Сколько кристаллов выпадает за раз
    public GameObject bouncingCrystalPrefab;          // Префаб кристалла со скриптом BouncingCrystal

    private bool isRecovering = false;                // Находится ли игрок во фреймах неуязвимости
    private float currentSpeedMultiplier = 1f;       // Динамический множитель скорости (для замедления)

    // Публичное свойство неуязвимости (i-frames) для проверки пулями
    public bool IsInvincible => isDashing || isRecovering;

    [Header("Настройки Силы (Ульта)")]
    public float ultimateRadius = 5f;
    public GameObject forceSparksPrefab;

    [Header("Настройки Ближнего Боя (Melee Combat)")]
    public GameObject lightsaberObject; 
    public float saberDistance = 2f;                  
    public float attackDuration = 0.35f;              
    public float spinDegrees = 360f;                  
    public float attackCooldown = 0.5f; 
    private bool isAttacking = false;
    private float lastAttackTime;
    public bool isSpinning = false; 

    [Header("Визуальные эффекты меча")]
    public TrailRenderer saberTrail; 

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera mainCamera;
    private Vector2 lastMousePos; 

    private float arrowInputTimer = 0f;
    private float arrowCurrentSpeed = 0f;
    private float arrowLastDirection = 0f;

    private void OnValidate()
    {
        if (lightsaberObject != null)
        {
            lightsaberObject.transform.localPosition = new Vector3(saberDistance, 0f, 0f);
        }
    }

    private void OnEnable()
    {
        isDashing = false;
        isAttacking = false;
        isSpinning = false;
        isRecovering = false;

        movement = Vector2.zero;
        activeMoveVelocity = Vector2.zero;
        knockbackVelocity = Vector2.zero;
        moveInputTimer = 0f;
        currentSpeedMultiplier = 1f;

        arrowInputTimer = 0f;
        arrowCurrentSpeed = 0f;
        arrowLastDirection = 0f;

        if (saberTrail != null)
        {
            saberTrail.emitting = false;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        lastMousePos = Input.mousePosition;

        if (GameManager.Instance != null)
        {
            health = GameManager.Instance.playerHealth;
        }

        if (lightsaberObject != null)
        {
            lightsaberObject.transform.localPosition = new Vector3(saberDistance, 0f, 0f);
        }
    }

    void Update()
    {
        if (isDashing) return;

        if (!isAttacking)
        {
            HandleRotation();
        }

        // Движение разрешено во время атаки
        movement = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) movement.y = 1;
        if (Input.GetKey(KeyCode.S)) movement.y = -1;
        if (Input.GetKey(KeyCode.A)) movement.x = -1;
        if (Input.GetKey(KeyCode.D)) movement.x = 1;

        if (isAttacking) return;

        // Логика Рывка (Dash)
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDash());
        }

        // Ульта (Пробел)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance != null && GameManager.Instance.TryUseUltimate())
            {
                UseForce();
            }
        }

        // Ближний бой (E)
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformSpinAttack());
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.angularVelocity = 0f; 
        }

        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.fixedDeltaTime * knockbackDecay);

        if (isDashing) return;

        Vector2 inputDir = movement.normalized;

        if (inputDir.sqrMagnitude > 0f)
        {
            if (Vector2.Dot(inputDir, lastMoveDirection) < 0.3f)
            {
                moveInputTimer = 0f;
            }
            lastMoveDirection = inputDir;

            moveInputTimer += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(moveInputTimer / moveAccelerationDuration);
            float speedMultiplier = moveAccelerationCurve.Evaluate(progress);

            // Применяем currentSpeedMultiplier к скорости движения (для замедления при уроне)
            Vector2 targetVelocity = inputDir * (maxMoveSpeed * currentSpeedMultiplier) * speedMultiplier;

            activeMoveVelocity = Vector2.MoveTowards(activeMoveVelocity, targetVelocity, (maxMoveSpeed / moveAccelerationDuration) * Time.fixedDeltaTime);
        }
        else
        {
            moveInputTimer = 0f;
            float stopStep = (maxMoveSpeed / moveDecelerationDuration) * Time.fixedDeltaTime;
            activeMoveVelocity = Vector2.MoveTowards(activeMoveVelocity, Vector2.zero, stopStep);
        }

        Vector2 finalVelocity = activeMoveVelocity + knockbackVelocity;
        
        rb.MovePosition(rb.position + finalVelocity * Time.fixedDeltaTime);
    }

    private void HandleRotation()
    {
        float inputDirection = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) inputDirection = 1f;
        else if (Input.GetKey(KeyCode.RightArrow)) inputDirection = -1f;

        if (inputDirection != 0f)
        {
            if (inputDirection != arrowLastDirection) arrowInputTimer = 0f;
            arrowLastDirection = inputDirection;

            arrowInputTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(arrowInputTimer / arrowAccelerationDuration);
            float speedMultiplier = arrowAccelerationCurve.Evaluate(progress);
            arrowCurrentSpeed = inputDirection * arrowMaxSpeed * speedMultiplier;

            rb.MoveRotation(rb.rotation + arrowCurrentSpeed * Time.deltaTime);
            return; 
        }

        if (arrowCurrentSpeed != 0f)
        {
            arrowInputTimer = 0f;
            float decayStep = (arrowMaxSpeed / arrowDecelerationDuration) * Time.deltaTime;
            arrowCurrentSpeed = Mathf.MoveTowards(arrowCurrentSpeed, 0f, decayStep);
            rb.MoveRotation(rb.rotation + arrowCurrentSpeed * Time.deltaTime);
            return;
        }

        Vector2 currentMousePos = Input.mousePosition;
        if (Vector2.Distance(currentMousePos, lastMousePos) > 1f)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(currentMousePos.x, currentMousePos.y, 10f));
            Vector2 lookDir = (Vector2)mouseWorldPos - rb.position;

            if (lookDir.sqrMagnitude > 0.5f) 
            {
                float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
                float smoothedAngle = Mathf.SmoothDampAngle(rb.rotation, targetAngle, ref mouseRotationVelocity, mouseSmoothTime);
                rb.MoveRotation(smoothedAngle);
            }
            lastMousePos = currentMousePos;
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector2 dashDirection = movement.normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = transform.up;
        }

        float calculatedDashSpeed = dashDistance / dashDuration;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
        }

        if (saberTrail != null) saberTrail.emitting = true;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(rb.position + dashDirection * calculatedDashSpeed * Time.deltaTime);
            yield return null;
        }

        if (sr != null) sr.color = originalColor;
        if (saberTrail != null && !isSpinning) saberTrail.emitting = false;

        isDashing = false;
    }

    private IEnumerator PerformSpinAttack()
    {
        isAttacking = true;
        isSpinning = true;
        lastAttackTime = Time.time;
        
        if (saberTrail != null) saberTrail.emitting = true;

        StartCoroutine(FlashSaber());

        float startAngle = rb.rotation;
        float targetAngle = startAngle + spinDegrees;
        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / attackDuration;
            
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, progress);
            rb.rotation = currentAngle; 
            
            yield return null;
        }

        rb.rotation = targetAngle;

        isAttacking = false;
        isSpinning = false;

        if (saberTrail != null) saberTrail.emitting = false;
    }

    private IEnumerator FlashSaber()
    {
        SpriteRenderer sr = lightsaberObject.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color originalColor = sr.color;
        while (isSpinning)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }
        sr.color = originalColor;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
    }

    // ИСПРАВЛЕННЫЙ МЕТОД: поддерживает адаптивное количество урона damageAmount (по умолчанию 1)
    public void TakeDamage(string source = "Лазерный луч", Vector2 attackerPosition = default, int damageAmount = 1)
    {
        // Не получаем урон во время рывка или неуязвимости
        if (isDashing || isRecovering) return;

        health -= damageAmount; // Отнимаем указанное количество урона

        // 1. Рассчитываем точную отдачу (Knockback) в сторону от источника урона
        Vector2 pushDir = Vector2.zero;
        if (attackerPosition != default)
        {
            pushDir = ((Vector2)transform.position - attackerPosition).normalized;
        }
        else
        {
            pushDir = -transform.up; // Откат назад, если источник неизвестен
        }
        ApplyKnockback(pushDir, damageKnockbackForce);

        // 2. Логика выпадения кристаллов на землю
        int currentCrystals = GameManager.Instance != null ? GameManager.Instance.crystals : 0;
        if (dropCrystalsOnDamage && bouncingCrystalPrefab != null && currentCrystals > 0)
        {
            int actualDropCount = Mathf.Min(currentCrystals, crystalsToDrop);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.crystals -= actualDropCount;
                if (UIManager.Instance != null) UIManager.Instance.UpdateCrystals(GameManager.Instance.crystals);
            }

            for (int i = 0; i < actualDropCount; i++)
            {
                Instantiate(bouncingCrystalPrefab, transform.position, Quaternion.identity);
            }
        }

        // 3. Звук и UI сердечек
        if (health > 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jediHitSound);
        }

        if (UIManager.Instance != null) UIManager.Instance.UpdateHearts(health);
        
        if (health <= 0)
        {
            Die(source);
            return;
        }

        // 4. Запускаем Корутину мерцания и замедления
        StartCoroutine(DamageRecoveryRoutine());
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        isRecovering = true;
        currentSpeedMultiplier = damageSlowMultiplier;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        float elapsed = 0f;
        bool isVisible = true;

        while (elapsed < damageRecoveryDuration)
        {
            elapsed += flickerInterval;
            isVisible = !isVisible;

            // Мерцание прозрачностью спрайта
            if (sr != null)
            {
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, isVisible ? 1f : 0.2f);
            }

            yield return new WaitForSecondsRealtime(flickerInterval);
        }

        // Восстанавливаем дефолтное состояние по завершении i-frames
        if (sr != null) sr.color = originalColor;
        currentSpeedMultiplier = 1f;
        isRecovering = false;
    }

    private void Die(string source)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jediDeathSound);
        }

        gameObject.SetActive(false); 
        if (GameManager.Instance != null) GameManager.Instance.GameOver(source);
    }

    private void UseForce()
    {
        if (forceSparksPrefab != null) Instantiate(forceSparksPrefab, transform.position, Quaternion.identity);
        
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, ultimateRadius);
        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("Worm")) 
            {
                hit.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
            }
            else if (hit.GetComponent<BlasterBolt>() != null) 
            {
                Destroy(hit.gameObject);
            }
        }
    }
}
