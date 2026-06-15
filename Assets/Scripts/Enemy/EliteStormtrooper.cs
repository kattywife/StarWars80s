using UnityEngine;

public class EliteStormtrooper : BaseStormtrooper
{
    // Элитный стреляет без разброса (напрямую в игрока)
    protected override void ExecuteShooting()
    {
        if (player == null || firePoint == null) return;

        Vector2 direction = player.position - firePoint.position;
        float angleToPlayer = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        firePoint.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90f);
        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);
        }
    }
}