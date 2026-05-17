using UnityEngine;
using System.Collections;

public class SpaceWorm : MonoBehaviour
{
    public float speed = 15f;
    private float currentRetreatSpeed; // Скорость уползания
    public Color flashColor = Color.red;

    private Transform player;
    private bool isRetreating = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        // Подписываемся на ульту
        GameManager.OnUltimateUsed += ScareAway;
    }

    void OnDestroy()
    {
        GameManager.OnUltimateUsed -= ScareAway;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 direction;
        if (!isRetreating)
        {
            // Летим К игроку
            direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
        else
        {
            // Уползаем ОТ игрока
            direction = (transform.position - player.position).normalized;
            transform.position += (Vector3)direction * currentRetreatSpeed * Time.deltaTime;
            
            // Если уполз далеко — удаляем
            if (Vector2.Distance(transform.position, player.position) > 40f) 
                Destroy(gameObject);
        }

        // Поворот: голова всегда по направлению движения
        // Если летит боком, замени 'transform.right' на 'transform.up'
        transform.right = direction; 
    }

    // Это срабатывает, если джедай нажал ПРОБЕЛ
    private void ScareAway()
    {
        if (isRetreating) return;
        
        // Улетает быстро (в 1.5 раза быстрее чем обычно), испугавшись силы
        currentRetreatSpeed = speed * 1.5f; 
        isRetreating = true;
        StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(0.5f);
        sr.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Если коснулся джедая и еще не уползает
        if (other.CompareTag("Player") && !isRetreating)
        {
            JediController jedi = other.GetComponent<JediController>();
            if (jedi != null)
            {
                // Отнимаем все жизни
                for (int i = 0; i < 3; i++) jedi.TakeDamage("Космический червь славно поужинал"); 
            }

            Debug.Log("Червь съел джедая и сытый уползает...");
            
            // ПЕРЕХОДИМ В РЕЖИМ ОТСТУПЛЕНИЯ ПОСЛЕ ЕДЫ
            isRetreating = true;
            // Уползает в 2 раза медленнее, чем летел до этого
            currentRetreatSpeed = speed / 2f; 
        }
    }
}