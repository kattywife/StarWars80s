using UnityEngine;

public class SuperStormtrooper : MonoBehaviour
{
    [Header("Стрельба")]
    public GameObject blasterBoltPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;

    [Header("Движение и Зрение")]
    public float moveSpeed = 3f;
    public float visionRadius = 10f; // Как далеко он видит
    
    private float nextFireTime;
    private Transform player;
    private Rigidbody2D rb;
    private bool canSeePlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextFireTime = Time.time + fireRate;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        CheckLineOfSight();

        if (canSeePlayer)
        {
            // Бежим к игроку
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            // Поворачиваем самого штурмовика к игроку
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f;

            // Стреляем по таймеру
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void CheckLineOfSight()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > visionRadius)
        {
            canSeePlayer = false; // Слишком далеко
            return;
        }

        Vector2 direction = player.position - transform.position;
        
        // Пускаем луч. RaycastAll вернет всё, через что прошел луч
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, visionRadius);

        canSeePlayer = false;

        foreach (var hit in hits)
        {
            // Игнорируем триггеры (пули, кристаллы, радиус ульты) и других врагов
            if (hit.collider.isTrigger || hit.collider.CompareTag("Enemy")) continue;

            // Если луч наткнулся на стену ДО игрока, значит мы его не видим
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Perimeter"))
            {
                break; 
            }

            // Если луч наткнулся на игрока - мы его видим!
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                break;
            }
        }
    }

    private void Shoot()
    {
        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);
    }

    public void TakeDamage()
    {
        Destroy(gameObject);
    }
}