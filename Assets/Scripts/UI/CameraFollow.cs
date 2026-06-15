using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;

    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 shakeOffset = Vector3.zero;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        FindPlayerTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindPlayerTarget();
            return;
        }

        // Объединяем целевую позицию слежения и текущее смещение от тряски
        Vector3 desiredPosition = target.position + offset + shakeOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            transform.position = target.position + offset;
        }
    }

    // Вызываем этот метод из BlasterBolt для тряски камеры
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}