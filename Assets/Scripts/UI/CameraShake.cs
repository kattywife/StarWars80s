using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            // UnscaledDeltaTime allows the camera to shake even during a Time Freeze
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}