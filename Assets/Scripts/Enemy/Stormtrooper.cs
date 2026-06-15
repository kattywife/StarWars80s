using UnityEngine;

// Наследуется от BaseStormtrooper вместо MonoBehaviour
public class Stormtrooper : BaseStormtrooper
{
    [Header("Специфичные Настройки (Standard)")]
    public float spreadAngle = 40f; 

    protected override void ExecuteShooting()
    {
        if (player == null || firePoint == null) return;

        Vector2 direction = player.position - firePoint.position;
        float angleToPlayer = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float baseAngle = angleToPlayer - 90f;
        float randomSpread = Random.Range(-spreadAngle, spreadAngle);
        float finalAngle = baseAngle + randomSpread;

        firePoint.rotation = Quaternion.Euler(0, 0, finalAngle);
        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);
        }
    }
}