using UnityEngine;

public class Lightsaber : MonoBehaviour
{
    private JediController jedi;

    void Start()
    {
        // Находим скрипт джедая на родительском объекте
        jedi = GetComponentInParent<JediController>();
    }

    // Срабатывает в момент первого касания
    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageEnemy(other);
    }

    // Срабатывает каждый кадр, пока коллайдеры продолжают соприкасаться
    void OnTriggerStay2D(Collider2D other)
    {
        TryDamageEnemy(other);
    }

    private void TryDamageEnemy(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Наносим урон только если джедай сейчас крутится в атаке
            if (jedi != null && jedi.isSpinning)
            {
                other.gameObject.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Враг получил урон от светового меча!");
            }
        }
    }
}