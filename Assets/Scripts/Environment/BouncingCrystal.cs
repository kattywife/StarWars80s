using UnityEngine;

public class BouncingCrystal : MonoBehaviour
{
    [Header("Настройки физики")]
    public float throwForce = 6f;       // Сила вылета кристалла
    public float pickupDelay = 0.8f;    // Задержка перед тем, как игрок сможет подобрать его обратно

    private Rigidbody2D rb;
    private Collider2D col;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        
        rb.gravityScale = 0f;
        
        // Используем 'drag' вместо 'linearDamp' для обратной совместимости
        rb.linearDamping = 3.5f; 

        // Выталкиваем кристалл в случайном направлении при спавне
        // Используем 'velocity' вместо 'linearVelocity'
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 throwDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        rb.linearVelocity = throwDirection * throwForce;

        spawnTime = Time.time;

        if (col != null) col.enabled = false; 
    }

    void Update()
    {
        if (col != null && !col.enabled && Time.time >= spawnTime + pickupDelay)
        {
            col.enabled = true;
        }
    }
}