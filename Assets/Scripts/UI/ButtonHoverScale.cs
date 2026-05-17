using UnityEngine;
using UnityEngine.EventSystems; // Нужно для обработки событий мыши

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки масштаба")]
    public float hoverScaleMultiplier = 1.1f; // На сколько увеличится кнопка (1.1 = +10%)
    public float animationSpeed = 10f;        // Скорость плавного перехода

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        // Запоминаем начальный размер кнопки
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Плавно меняем текущий размер к целевому
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    // Срабатывает, когда мышь заходит в зону кнопки
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScaleMultiplier;
    }

    // Срабатывает, когда мышь покидает зону кнопки
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    // Сбрасываем размер при выключении объекта (чтобы кнопка не осталась большой)
    void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}