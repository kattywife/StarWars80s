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

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera mainCamera;
    private Vector2 lastMousePos; // Для отслеживания движения мыши

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
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

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
        // Это не дает врагам или стенам закрутить джедая
        if (!isAttacking)
        {
            rb.angularVelocity = 0f; 
        }

        if (isAttacking) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    [Header("Настройки вращения")]
    public float rotationSpeed = 300f; // Скорость вращения кнопками

    // Замени метод HandleRotation и RotateTowardsMouse на эти:

    // В начало Update или FixedUpdate добавь это:

private void HandleRotation()
{
    bool keysPressed = false;
    float targetAngle = rb.rotation;

    // Вращение КНОПКАМИ (Стрелки)
    if (Input.GetKey(KeyCode.LeftArrow))
    {
        targetAngle += rotationSpeed * Time.deltaTime;
        keysPressed = true;
    }
    else if (Input.GetKey(KeyCode.RightArrow))
    {
        targetAngle -= rotationSpeed * Time.deltaTime;
        keysPressed = true;
    }

    if (keysPressed)
    {
        rb.MoveRotation(targetAngle);
        return; 
    }

    // Вращение МЫШЬЮ
    Vector2 currentMousePos = Input.mousePosition;
    if (Vector2.Distance(currentMousePos, lastMousePos) > 1f)
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(currentMousePos.x, currentMousePos.y, 10f));
        Vector2 lookDir = (Vector2)mouseWorldPos - rb.position;

        // "Мертвая зона" стала больше для стабильности
        if (lookDir.sqrMagnitude > 0.5f) 
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle - 90f);
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
            if (hit.CompareTag("Enemy")) hit.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
            else if (hit.GetComponent<BlasterBolt>() != null) Destroy(hit.gameObject);
        }
    }
}