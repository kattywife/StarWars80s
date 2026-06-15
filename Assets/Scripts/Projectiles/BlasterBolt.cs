using UnityEngine;

public class BlasterBolt : MonoBehaviour
{
    public float speed = 10f;
    private bool isDeflected = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [Header("Настройки Сока (Juice Settings)")]
    public GameObject deflectSparkPrefab; // Префаб искр при отражении (можно использовать уменьшенные искры ульты)
    public float knockbackStrength = 8f;   // Сила отдачи джедая назад
    public float freezeDuration = 0.04f;   // Продолжительность микро-паузы (40 миллисекунд)
    public float shakeDuration = 0.1f;     // Длина тряски камеры
    public float shakeMagnitude = 0.15f;   // Сила тряски камеры

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.linearVelocity = transform.up * speed;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; 
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Wall") || hitInfo.CompareTag("Perimeter"))
        {
            Destroy(gameObject);
        }
        else if (hitInfo.CompareTag("Lightsaber") && !isDeflected)
        {
            Deflect(hitInfo.transform, hitInfo);
        }
        else if (hitInfo.CompareTag("Player") && !isDeflected)
        {
            JediController jedi = hitInfo.GetComponent<JediController>();
            if (jedi != null) 
            {
                // Если джедай находится в рывке — пуля пролетает насквозь!
                if (jedi.IsInvincible) return; 

                jedi.TakeDamage("Штурмовик попал в цель впервые в истории Галактики");
                Destroy(gameObject);
            }
        }
        else if (hitInfo.CompareTag("Enemy") && isDeflected)
        {
            hitInfo.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }

    public void Deflect(Transform saberTransform, Collider2D saberCollider)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        isDeflected = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green; 
        }

        // ==========================================================
        // 1. ФИЗИЧЕСКАЯ ОТДАЧА (KNOCKBACK)
        // ==========================================================
        JediController jedi = saberTransform.GetComponentInParent<JediController>();
        if (jedi != null)
        {
            // Толкаем джедая назад, противоположно направлению меча
            Vector2 pushDirection = -saberTransform.up;
            jedi.ApplyKnockback(pushDirection, knockbackStrength);
        }

        // ==========================================================
        // 2. СПАВН ИСКР В ТОЧКЕ КОНТАКТА
        // ==========================================================
        if (deflectSparkPrefab != null && saberCollider != null)
        {
            // Находим ближайшую точку на коллайдере меча к пуле
            Vector3 contactPoint = saberCollider.bounds.ClosestPoint(transform.position);
            Instantiate(deflectSparkPrefab, contactPoint, Quaternion.identity);
        }

        // ==========================================================
        // 3. МИКРО-СТОП ВРЕМЕНИ (TIME FREEZE)
        // ==========================================================
        if (TimeFreeze.Instance != null)
        {
            TimeFreeze.Instance.Freeze(freezeDuration);
        }

        // ==========================================================
        // 4. ТРЯСКА КАМЕРЫ (CAMERA SHAKE)
        // ==========================================================
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(shakeDuration, shakeMagnitude);
        }

        // Отражаем саму пулю
        if (rb != null)
        {
            rb.linearVelocity = saberTransform.up * (speed * 1.5f);
            transform.up = saberTransform.up; 
        }
    }
}