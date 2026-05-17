using UnityEngine;

public class BlasterBolt : MonoBehaviour
{
    public float speed = 10f;
    private bool isDeflected = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Используем Awake вместо Start, так как Awake срабатывает мгновенно при создании объекта
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Запускаем пулю вперед при создании
        if (rb != null)
        {
            rb.linearVelocity = transform.up * speed;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // Изначально красная
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Проверяем теги
        if (hitInfo.CompareTag("Wall") || hitInfo.CompareTag("Perimeter"))
        {
            Destroy(gameObject);
        }
        else if (hitInfo.CompareTag("Lightsaber") && !isDeflected)
        {
            Deflect(hitInfo.transform);
        }
        else if (hitInfo.CompareTag("Player") && !isDeflected)
        {
            // Урон игроку
            JediController jedi = hitInfo.GetComponent<JediController>();
            if (jedi != null) jedi.TakeDamage("Штурмовик попал в цель впервые в истории Галактики");
            Destroy(gameObject);
        }
        else if (hitInfo.CompareTag("Enemy") && isDeflected)
        {
            // Урон врагу через SendMessage (универсально для всех типов штурмовиков)
            hitInfo.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }

    public void Deflect(Transform saberTransform)
    {
        // Если компоненты почему-то не нашлись в Awake, ищем их еще раз
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        isDeflected = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green; // Меняем цвет на зеленый
        }

        if (rb != null)
        {
            // Отражаем в сторону, куда смотрит меч, и ускоряем
            rb.linearVelocity = saberTransform.up * (speed * 1.5f);
            transform.up = saberTransform.up; 
        }
    }
}