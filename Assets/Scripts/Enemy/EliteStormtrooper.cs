using UnityEngine;

public class EliteStormtrooper : BaseStormtrooper
{
    protected override void ExecuteShooting()
    {
        if (player == null || firePoint == null) return;

        Vector2 direction = (player.position - firePoint.position).normalized;
        float angleToPlayer = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        firePoint.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90f);
        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);
    }
}