using UnityEngine;

public class EliteStormtrooper : MonoBehaviour
{
    public GameObject blasterBoltPrefab;
    public Transform firePoint;
    public float minFireRate = 1f;
    public float maxFireRate = 2.5f; // Чуть быстрее базового

    private float nextFireTime;
    private Transform player;

    void Start()
    {
        SetNextFireTime();
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            ShootAtPlayer();
            SetNextFireTime();
        }
    }

    private void SetNextFireTime()
    {
        nextFireTime = Time.time + Random.Range(minFireRate, maxFireRate);
    }

    private void ShootAtPlayer()
    {
        if (player == null) return;

        // Вычисляем угол до игрока
        Vector2 lookDir = player.position - firePoint.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        
        // Поворачиваем точку выстрела ровно на игрока (отнимаем 90, если пуля смотрит вверх)
        firePoint.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // Стреляем
        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);
    }

    public void TakeDamage()
    {
        Destroy(gameObject);
    }
}