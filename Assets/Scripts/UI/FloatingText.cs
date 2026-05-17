using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 2f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    public float moveSpeed = 1f;

    private TextMeshPro textMesh;
    private Color textColor;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        textColor = textMesh.color;
        // Уничтожаем через пару секунд
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Плавно плывем вверх
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Плавно исчезаем (Fade out)
        float alpha = Mathf.Lerp(textColor.a, 0, (Time.deltaTime * (1f / destroyTime)) * 5f);
        textColor.a -= Time.deltaTime / destroyTime;
        textMesh.color = textColor;
    }
}