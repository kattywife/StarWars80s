using UnityEngine;
using System.Collections;

public class JediController : MonoBehaviour
{
    [Header("Основные настройки")]
    public float moveSpeed = 5f;
    public int health = 3;

    [Header("Настройки Силы (Ульта)")]
    public float ultimateRadius = 5f;
    public GameObject forceSparksPrefab;

    [Header("Настройки Ближнего Боя")]
    public GameObject saberBlade;      
    public float attackDuration = 0.2f; 
    public float attackCooldown = 0.5f; 
    private bool isAttacking = false;
    private float lastAttackTime;
    public bool isSpinning = false; // Для скрипта меча

    [Header("Управление Стрелками (Keyboard Rotation)")]
    public float arrowMaxSpeed = 360f;               // Максимальная скорость вращения
    public AnimationCurve arrowAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // График разгона
    public float arrowAccelerationDuration = 0.25f;  // Время разгона до макс. скорости (в секундах)
    public float arrowDecelerationDuration = 0.15f;  // Время затухания вращения после отпускания клавиш

    [Header("Управление Мышью (Mouse Rotation)")]
    public float mouseSmoothTime = 0.05f;             // Время сглаживания (чем меньше, тем быстрее и резче поворот за мышью)
    private float mouseRotationVelocity;              // Внутренняя переменная для расчетов физики вращения

    // Внутренние переменные разгона клавиатуры
    private float arrowInputTimer = 0f;
    private float arrowCurrentSpeed = 0f;
    private float arrowLastDirection = 0f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera mainCamera;
    private Vector2 lastMousePos; // Для отслеживания движения мыши

    [Header("Настройки отдачи (Knockback)")]
    public float knockbackDecay = 8f; // Как быстро затухает отдача (чем выше, тем быстрее остановка)
    private Vector2 knockbackVelocity; // Текущая скорость отдачи

    [Header("Визуальные эффекты меча")]
    public TrailRenderer saberTrail; // Перетащи сюда компонент Trail Renderer с лезвия меча

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
        if (isAttacking) return;

        // 1. Движение (WASD)
        // movement.x = Input.GetAxisRaw("Horizontal");
        // movement.y = Input.GetAxisRaw("Vertical");

        
        movement = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) movement.y = 1;
        if (Input.GetKey(KeyCode.S)) movement.y = -1;
        if (Input.GetKey(KeyCode.A)) movement.x = -1;
        if (Input.GetKey(KeyCode.D)) movement.x = 1;

        // 2. Поворот (Мышь ИЛИ Стрелочки)
        HandleRotation();

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
        // ОСТАНОВКА ФИЗИЧЕСКОГО ВРАЩЕНИЯ
        if (!isAttacking)
        {
            rb.angularVelocity = 0f; 
        }

        // Вычисляем обычное движение (WASD) только если не атакуем
        Vector2 currentMove = Vector2.zero;
        if (!isAttacking)
        {
            currentMove = movement.normalized * moveSpeed;
        }

        // Объединяем скорость WASD движения и скорость отдачи
        Vector2 finalVelocity = currentMove + knockbackVelocity;

        // Двигаем тело через MovePosition (работает стабильно для любого Rigidbody Body Type)
        rb.MovePosition(rb.position + finalVelocity * Time.fixedDeltaTime);

        // Постепенно гасим отдачу до нуля, создавая плавное скольжение назад
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.fixedDeltaTime * knockbackDecay);
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        // Задаем начальную скорость отдачи в указанном направлении
        knockbackVelocity = direction.normalized * force;
    }

    [Header("Настройки вращения")]
    public float rotationSpeed = 300f; // Скорость вращения кнопками

    // Замени метод HandleRotation и RotateTowardsMouse на эти:

    // В начало Update или FixedUpdate добавь это:

private void HandleRotation()
{
    // 1. ПРОВЕРКА ВВОДА КЛАВИАТУРЫ
    float inputDirection = 0f;
    if (Input.GetKey(KeyCode.LeftArrow)) inputDirection = 1f;
    else if (Input.GetKey(KeyCode.RightArrow)) inputDirection = -1f;

    // 2. ЕСЛИ НАЖАТЫ СТРЕЛКИ (Разгон по графику)
    if (inputDirection != 0f)
    {
        // Если резко сменили направление, сбрасываем таймер разгона
        if (inputDirection != arrowLastDirection)
        {
            arrowInputTimer = 0f;
        }
        arrowLastDirection = inputDirection;

        // Рассчитываем прогресс разгона
        arrowInputTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(arrowInputTimer / arrowAccelerationDuration);
        
        // Оцениваем скорость по кастомной кривой (графику) из Инспектора
        float speedMultiplier = arrowAccelerationCurve.Evaluate(progress);
        
        arrowCurrentSpeed = inputDirection * arrowMaxSpeed * speedMultiplier;

        // Поворачиваем джедая
        rb.MoveRotation(rb.rotation + arrowCurrentSpeed * Time.deltaTime);
        return; // Блокируем поворот за мышью, пока держим стрелки
    }

    // 3. ЕСЛИ ОТПУСТИЛИ СТРЕЛКИ (Плавное затухание/Инерция)
    if (arrowCurrentSpeed != 0f)
    {
        arrowInputTimer = 0f;
        
        // Линейно гасим скорость вращения до нуля
        float decayStep = (arrowMaxSpeed / arrowDecelerationDuration) * Time.deltaTime;
        arrowCurrentSpeed = Mathf.MoveTowards(arrowCurrentSpeed, 0f, decayStep);

        rb.MoveRotation(rb.rotation + arrowCurrentSpeed * Time.deltaTime);
        return;
    }

    // 4. ЕСЛИ КЛАВИАТУРА МОЛЧИТ — ВРАЩЕНИЕ МЫШЬЮ (Шелковистое слежение)
    Vector2 currentMousePos = Input.mousePosition;
    if (Vector2.Distance(currentMousePos, lastMousePos) > 1f)
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(currentMousePos.x, currentMousePos.y, 10f));
        Vector2 lookDir = (Vector2)mouseWorldPos - rb.position;

        if (lookDir.sqrMagnitude > 0.5f) 
        {
            float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            
            // Физически сглаживаем поворот, симулируя массу меча
            float smoothedAngle = Mathf.SmoothDampAngle(rb.rotation, targetAngle, ref mouseRotationVelocity, mouseSmoothTime);
            rb.MoveRotation(smoothedAngle);
        }
        lastMousePos = currentMousePos;
    }
}


    private void RotateTowardsMouse()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mouseWorldPos - rb.position;

        // ПРОВЕРКА 2: Не слишком ли близко мышь к игроку?
        // Если расстояние меньше 0.5 единиц, не поворачиваемся (чтобы не крутиться на месте)
        if (lookDir.sqrMagnitude > 0.2f) 
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f; 
        }
    }

    private IEnumerator PerformSpinAttack()
    {
        isAttacking = true;
        isSpinning = true;
        lastAttackTime = Time.time;
        
        // Включаем след от меча при замахе
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

        // Выключаем след (он плавно сойдет на нет сам)
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

    // Находим метод TakeDamage и Die и добавляем туда вызовы звуков
    public void TakeDamage(string source = "Лазерный луч")
    {
        health--;
        
        // ЗВУК: Получение урона
        if (health > 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jediHitSound);
        }

        if (UIManager.Instance != null) UIManager.Instance.UpdateHearts(health);
        if (health <= 0) Die(source);
    }

    private void Die(string source)
    {
        // ЗВУК: Смерть джедая
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
            // Теперь проверяем ТРИ условия: враги, черви и пули
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