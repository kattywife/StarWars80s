using UnityEngine;

public class IntroCrawl : MonoBehaviour
{
    public RectTransform textTransform;
    public float scrollSpeed = 60f; // 60-80 обычно лучше для чтения
    public float exitPositionY = 2500f; 

    private bool isSkipping = false;
    private float timer = 0f;
    private float canSkipTime = 1.0f; 

    void OnEnable()
    {
        // СТАРТ: 0 означает, что текст начнет появляться СРАЗУ.
        // Если хочешь маленькую паузу, поставь -100.
        textTransform.anchoredPosition = new Vector2(0, 0f);
        
        isSkipping = false;
        timer = 0f;
        
        // АВТО-ПОРОГ: если высота текста 2000, то он должен пролететь 
        // свою высоту + высоту экрана (примерно 1000).
        // Это избавит тебя от лишнего ожидания в конце.
        exitPositionY = textTransform.rect.height + 1200f;
        
        Debug.Log("ИНТРО: Начали! Текст появится сразу.");
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        // Движение
        textTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;

        // Пропуск
        if (timer > canSkipTime && !isSkipping)
        {
            // if (Input.anyKeyDown || Input.GetMouseButtonDown(0)) SkipIntro()
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) SkipIntro();
        }
        

            // if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) SkipIntro();
        // Завершение
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