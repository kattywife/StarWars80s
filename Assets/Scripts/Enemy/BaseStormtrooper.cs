using UnityEngine;

public abstract class BaseStormtrooper : MonoBehaviour
{
    [Header("Базовые Префабы")]
    public GameObject blasterBoltPrefab;
    public Transform firePoint;
    public GameObject deathEffectPrefab;

    [Header("Базовые Настройки Стрельбы")]
    public float minFireRate = 1f;
    public float maxFireRate = 3f;
    public float fireRateMultiplier = 1.5f;

    protected float nextFireTime;
    protected Transform player;

    protected virtual void Start()
    {
        SetNextFireTime();

        // Все штурмовики автоматически находят джедая на старте
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    protected virtual void Update()
    {
        if (Time.time >= nextFireTime)
        {
            ExecuteShooting();
            SetNextFireTime();
        }
    }

    // Каждый подкласс должен реализовать свою уникальную логику стрельбы
    protected abstract void ExecuteShooting();

    protected virtual void SetNextFireTime()
    {
        float baseCooldown = Random.Range(minFireRate, maxFireRate);
        float actualCooldown = baseCooldown / fireRateMultiplier;
        nextFireTime = Time.time + actualCooldown;
    }

    // Все типы штурмовиков теперь умирают абсолютно одинаково
    public virtual void TakeDamage()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.08f, 0.12f); 
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperDeathSound);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckEnemyCount();
        }

        Destroy(gameObject);
    }
}