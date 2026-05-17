using UnityEngine;

public class ForceEffect : MonoBehaviour
{
    public float expandSpeed = 15f;
    public float maxScale = 10f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Увеличиваем размер
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
        
        // Плавно делаем прозрачным
        Color c = sr.color;
        c.a -= (expandSpeed / maxScale) * Time.deltaTime; 
        sr.color = c;

        // Удаляем, когда волна стала большой
        if (transform.localScale.x >= maxScale || c.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}