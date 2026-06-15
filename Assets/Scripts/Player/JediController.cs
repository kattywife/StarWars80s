using UnityEngine;
using System.Collections;

public class JediController : MonoBehaviour
{
    [Header("Основные настройки")]
    public int health = 3;

    [Header("Настройки Движения (Keyboard WASD)")]
    public float maxMoveSpeed = 6f;                  // Максимальная скорость бега
    public AnimationCurve moveAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // График разгона
    public float moveAccelerationDuration = 0.2f;    // Время разгона до макс. скорости (в секундах)
    public float moveDecelerationDuration = 0.15f;   // Время затухания движения (скольжение после отпускания кнопок)
    
    private Vector2 activeMoveVelocity;              // Текущая сглаженная скорость движения
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
    public float knockbackDecay = 8f;                 // Скорость затухания отдачи
    private Vector2 knockbackVelocity;                // Текущая скорость отдачи

    [Header("Настройки Рывка (Dash Settings)")]
    public KeyCode dashKey = KeyCode.LeftShift;       // Клавиша рывка
    public float dashDistance = 4f;                   // Дистанция рывка в метрах (плитках)
    public float dashDuration = 0.18f;                 // Длительность рывка (в секундах)
    public float dashCooldown = 0.8f;                 // Перезарядка рывка
    private bool isDashing = false;
    private float lastDashTime;

    // Публичное свойство неуязвимости (i-frames) для проверки другими скриптами
    public bool IsInvincible => isDashing;

    [Header("Настройки Силы (Ульта)")]
    public float ultimateRadius = 5f;
    public GameObject forceSparksPrefab;

    [Header("Настройки Ближнего Боя")]
    public GameObject saberBlade;      
    public float attackDuration = 0.2f; 
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

    // Внутренние переменные разгона клавиатуры
    private float arrowInputTimer = 0f;
    private float arrowCurrentSpeed = 0f;
    private float arrowLastDirection = 0f;

    private void OnEnable()
    {
        // Сбрасываем все флаги состояний при воскрешении/рестарте
        isDashing = false;
        isAttacking = false;
        isSpinning = false;

        // Сбрасываем физику движения и отдачи
        movement = Vector2.zero;
        activeMoveVelocity = Vector2.zero;
        knockbackVelocity = Vector2.zero;
        moveInputTimer = 0f;

        // Сбрасываем таймеры разгона клавиатуры
        arrowInputTimer = 0f;
        arrowCurrentSpeed = 0f;
        arrowLastDirection = 0f;

        // Принудительно выключаем след меча, если он завис включенным
        if (saberTrail != null)
        {
            saberTrail.emitting = false;
        }

        // Возвращаем игроку полную видимость
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
    }

    void Update()
    {
        // Не принимаем обычные вводы во время рывка
        if (isDashing) return;

        // Поворот разрешен всегда, кроме режима атаки
        if (!isAttacking)
        {
            HandleRotation();
        }

        if (isAttacking) return;

        // 1. Движение (WASD)
        movement = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) movement.y = 1;
        if (Input.GetKey(KeyCode.S)) movement.y = -1;
        if (Input.GetKey(KeyCode.A)) movement.x = -1;
        if (Input.GetKey(KeyCode.D)) movement.x = 1;

        // 2. Логика Рывка (Dash)
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDash());
        }

        // 3. Ульта (Пробел)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance != null && GameManager.Instance.TryUseUltimate())
            {
                UseForce();
            }
        }

        // 4. Ближний бой (E)
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformSpinAttack());
        }
    }

    void FixedUpdate()
    {
        // Предотвращаем физическое закручивание
        if (!isAttacking)
        {
            rb.angularVelocity = 0f; 
        }

        // Постепенно гасим отдачу/слайд в любом состоянии, чтобы она затухала плавно
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.fixedDeltaTime * knockbackDecay);

        // Блокируем WASD физику во время атаки или рывка
        if (isAttacking || isDashing) return;

        // Вычисляем направление ввода движения
        Vector2 inputDir = movement.normalized;

        if (inputDir.sqrMagnitude > 0f)
        {
            // Если игрок резко изменил направление движения больше чем на 90 градусов (например, бежал Вправо и нажал Влево),
            // мы сбрасываем таймер разгона, чтобы сымитировать инерцию торможения и разгона
            if (Vector2.Dot(inputDir, lastMoveDirection) < 0.3f)
            {
                moveInputTimer = 0f;
            }
            lastMoveDirection = inputDir;

            // Накапливаем время ввода для разгона по графику
            moveInputTimer += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(moveInputTimer / moveAccelerationDuration);
            float speedMultiplier = moveAccelerationCurve.Evaluate(progress);

            // Целевая скорость
            Vector2 targetVelocity = inputDir * maxMoveSpeed * speedMultiplier;

            // Сглаженно стремимся к целевой скорости
            activeMoveVelocity = Vector2.MoveTowards(activeMoveVelocity, targetVelocity, (maxMoveSpeed / moveAccelerationDuration) * Time.fixedDeltaTime);
        }
        else
        {
            // Если кнопки WASD отпущены — плавно тормозим/скользим до полной остановки
            moveInputTimer = 0f;
            float stopStep = (maxMoveSpeed / moveDecelerationDuration) * Time.fixedDeltaTime;
            activeMoveVelocity = Vector2.MoveTowards(activeMoveVelocity, Vector2.zero, stopStep);
        }

        // Объединяем скорость движения и отдачу
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

        // Вычисляем направление рывка: по нажатым WASD кнопкам, либо вперед (по направлению взгляда), если стоим
        Vector2 dashDirection = movement.normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = transform.up;
        }

        // Динамически вычисляем скорость по формуле: Скорость = Дистанция / Время
        float calculatedDashSpeed = dashDistance / dashDuration;

        // Визуальный эффект: Джедай становится полупрозрачным при рывке
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
        }

        // Включаем след от меча для динамичности рывка
        if (saberTrail != null) saberTrail.emitting = true;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            
            // Двигаем игрока со строго вычисленной скоростью
            rb.MovePosition(rb.position + dashDirection * calculatedDashSpeed * Time.deltaTime);
            yield return null;
        }

        // Возвращаем исходную непрозрачность
        if (sr != null) sr.color = originalColor;
        
        // Отключаем след от меча (если джедай в этот момент не атакует)
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

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            rb.rotation += (360f / attackDuration) * Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        isSpinning = false;

        if (saberTrail != null) saberTrail.emitting = false;
    }

    private IEnumerator FlashSaber()
    {
        SpriteRenderer saberSr = saberBlade.GetComponent<SpriteRenderer>();
        if (saberSr == null) yield break;
        Color originalColor = saberSr.color;
        while (isSpinning)
        {
            saberSr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            saberSr.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }
        saberSr.color = originalColor;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        // Задаем начальную скорость отдачи в указанном направлении
        knockbackVelocity = direction.normalized * force;
    }

    public void TakeDamage(string source = "Лазерный луч")
    {
        if (isDashing) return;

        health--;
        
        if (health > 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jediHitSound);
        }

        if (UIManager.Instance != null) UIManager.Instance.UpdateHearts(health);
        if (health <= 0) Die(source);
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