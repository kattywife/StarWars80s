using UnityEngine;

public class SuperStormtrooper : BaseStormtrooper
{
    [Header("Специфичные Настройки (Super)")]
    public float moveSpeed = 3f;
    public float visionRadius = 10f;

    private Rigidbody2D rb;
    private bool canSeePlayer = false;

    protected override void Start()
    {
        base.Start(); // Вызывает Start() родительского класса, чтобы найти игрока
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        CheckLineOfSight();

        if (canSeePlayer)
        {
            // Движение к джедаю
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            // Поворот штурмовика в сторону бега
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f;
        }
    }

    private void CheckLineOfSight()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > visionRadius)
        {
            canSeePlayer = false; 
            return;
        }

        Vector2 direction = player.position - transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, visionRadius);

        canSeePlayer = false;

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger || hit.collider.CompareTag("Enemy")) continue;

            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Perimeter"))
            {
                break; 
            }

            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                break;
            }
        }
    }

    protected override void ExecuteShooting()
    {
        // Супер-штурмовик стреляет по таймеру только тогда, когда видит игрока
        if (canSeePlayer && firePoint != null)
        {
            Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);
            }
        }
    }
}