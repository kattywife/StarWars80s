using UnityEngine;

public class SuperStormtrooper : BaseStormtrooper
{
    [Header("Настройки Преследования")]
    public float chaseSpeed = 3.5f;

    protected override void HandleAttackPhysics()
    {
        if (player == null) return;

        Vector2 chaseDir = ((Vector2)player.position - rb.position).normalized;
        Vector2 finalMovementDir = chaseDir;

        if (canStrafeDuringAttack && strafeDirection != 0)
        {
            Vector2 strafeDir = new Vector2(-chaseDir.y, chaseDir.x) * strafeDirection;
            finalMovementDir = (chaseDir + strafeDir * 0.4f).normalized;
        }

        rb.MovePosition(rb.position + finalMovementDir * chaseSpeed * Time.fixedDeltaTime);

        // ИСПРАВЛЕНО: +90f для корректного разворота лица во время бега зигзагом
        float angle = Mathf.Atan2(chaseDir.y, chaseDir.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    protected override void ExecuteShooting()
    {
        if (player == null || firePoint == null) return;

        Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);
    }
}