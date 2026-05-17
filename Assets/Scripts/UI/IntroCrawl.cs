using UnityEngine;

public class IntroCrawl : MonoBehaviour
{
    public RectTransform textTransform;
    public float scrollSpeed = 50f;
    public float exitPositionY = 4000f; // Увеличили порог!

    private bool isSkipping = false;
    private float timer = 0f;
    private float canSkipTime = 1.0f; 

    void OnEnable()
    {
        // СТАРТ: Ставим текст глубоко внизу. 
        // -500 значит, что ВЕРХ текста (Pivot Y=1) спрятан на 500 пикселей под нижним краем.
        textTransform.anchoredPosition = new Vector2(0, -500f);
        
        isSkipping = false;
        timer = 0f;
        Debug.Log("ИНТРО: Поехали!");
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        // Движение вверх
        textTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;

        // ПРОВЕРКА ПРОПУСКА
        if (timer > canSkipTime && !isSkipping)
        {
            if (Input.anyKeyDown) SkipIntro();
        }

        // ПРОВЕРКА ЗАВЕРШЕНИЯ
        // Теперь мы ждем, пока текст улетит на 4000 пикселей вверх
        if (textTransform.anchoredPosition.y > exitPositionY && !isSkipping)
        {
            SkipIntro();
        }
    }

    void SkipIntro()
    {
        if (isSkipping) return;
        isSkipping = true;
        this.gameObject.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.StartLevel();
    }
}