using UnityEngine;

public class SecretDiskette : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что дискету подобрал именно игрок
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                // Вызываем получение 3-го ключа
                GameManager.Instance.CollectKey(3, "Хрустальный ключ (Дискета Холлидея)");
            }

            // Уничтожаем дискету после подбора
            Destroy(gameObject);
        }
    }
}