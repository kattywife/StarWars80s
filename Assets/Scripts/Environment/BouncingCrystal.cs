using UnityEngine;
using System.Collections;

public class BouncingCrystal : MonoBehaviour
{
    [Header("Настройки физики")]
    public float throwForce = 6f;       
    public float pickupDelay = 0.8f;    

    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        
        rb.gravityScale = 0f;
        rb.linearDamping = 3.5f; 

        // Выталкиваем кристалл в случайном направлении при спавне
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 throwDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        rb.linearVelocity = throwDirection * throwForce;
    }

    void Start()
    {
        // Находим джедая в сцене по тегу Player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Collider2D playerCol = player.GetComponent<Collider2D>();
            
            // Если нашли игрока, временно игнорируем столкновения с ним
            if (playerCol != null && col != null)
            {
                // Это отключает коллизии и триггер сбора только для игрока, 
                // позволяя кристаллу при этом ударяться о стены!
                Physics2D.IgnoreCollision(col, playerCol, true);
                
                // Запускаем корутину восстановления сбора
                StartCoroutine(EnablePickupAfterDelay(playerCol));
            }
        }
    }

    private IEnumerator EnablePickupAfterDelay(Collider2D playerCol)
    {
        yield return new WaitForSeconds(pickupDelay);
        
        if (col != null && playerCol != null)
        {
            // Снова разрешаем коллизии и сбор кристалла игроком
            Physics2D.IgnoreCollision(col, playerCol, false);
        }
    }
}