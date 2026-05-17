using UnityEngine;

public class Stormtrooper : MonoBehaviour
{
    public GameObject blasterBoltPrefab;
    public Transform firePoint;
    public float minFireRate = 1f;
    public float maxFireRate = 3f;

    private float nextFireTime;
    
    void Start()
    {
        SetNextFireTime();
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            ShootRandomly();
            SetNextFireTime();
        }
    }

    private void SetNextFireTime()
    {
        nextFireTime = Time.time + Random.Range(minFireRate, maxFireRate);
    }

    private void ShootRandomly()
    {
        float randomAngle = Random.Range(0f, 360f);
        firePoint.rotation = Quaternion.Euler(0, 0, randomAngle);
        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);

        // ЗВУК: Выстрел штурмовика
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);
        }
    }

    public void TakeDamage()
    {
        Debug.Log("Штурмовик повержен!");

        // ЗВУК: Смерть штурмовика
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperDeathSound);
        }

        Destroy(gameObject);
    }
}
