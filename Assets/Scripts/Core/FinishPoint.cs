using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // ПРОВЕРКА: Только если вошел объект с тегом Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=green>ДЖЕДАЙ НА ФИНИШЕ!</color>");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelCompleted();
            }
        }
        // Пули и враги теперь будут игнорироваться
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null) Gizmos.DrawWireCube(transform.position, collider.size);
    }
}