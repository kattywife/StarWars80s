using UnityEngine;

public class Lightsaber : MonoBehaviour
{
    private JediController jedi;

    void Start()
    {
        // Ищем скрипт джедая в родительском объекте
        jedi = GetComponentInParent<JediController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Убиваем врага ТОЛЬКО если джедай сейчас в состоянии прокрутки (isSpinning)
        if (other.CompareTag("Enemy"))
        {
            if (jedi != null && jedi.isSpinning)
            {
                other.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Враг убит активным взмахом!");
            }
        }
        
        // Отражение пуль по-прежнему работает само по себе в скрипте пули, 
        // так что джедай может защищаться пассивно.
    }
}