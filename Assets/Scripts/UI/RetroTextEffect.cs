using UnityEngine;
using TMPro;

public class RetroTextEffect : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    public bool pulseScale = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;

    public bool flickerAlpha = true;

    private Vector3 originalScale;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 1. Ёффект пульсации (легкое увеличение/уменьшение)
        if (pulseScale)
        {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * scale;
        }

        // 2. Ёффект мерцани€ (имитаци€ старого монитора)
        if (flickerAlpha)
        {
            if (Random.value > 0.98f) // Ўанс на короткое мерцание
            {
                StartCoroutine(FlickerRoutine());
            }
        }
    }

    System.Collections.IEnumerator FlickerRoutine()
    {
        Color c = textMesh.color;
        textMesh.color = new Color(c.r, c.g, c.b, 0.7f); // —легка тускнеет
        yield return new WaitForSeconds(0.05f);
        textMesh.color = new Color(c.r, c.g, c.b, 1f); // ¬озвращаетс€ в норму
    }
}