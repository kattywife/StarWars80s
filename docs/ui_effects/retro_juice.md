# Ретро-эффекты и Сочность игрового процесса (Retro Juice)

Понятие «сочности» (Game Juice) описывает обилие мгновенной обратной связи от действий игрока. В игре «Стар80рс» сочность не только делает игровой процесс кинетичным, но и воссоздает атмосферу залов игровых автоматов 80-х годов и старых CRT-экранов.

---

## 1. Концепция и Смысл ретро-эффектов

Механики обратной связи решают следующие геймдизайнерские задачи:
*   **Ощущение силы удара (Impact):** Каждое успешное отражение лазера световым мечом должно ощущаться физически. Остановка времени и тряска экрана превращают обычное столкновение коллайдеров в кульминационный микро-эпизод.
*   **Имитация CRT-монитора (ЭЛТ):** Статичные плоские шрифты лишают игру ретро-шарма. Пульсация размера и случайные помехи (мерцание прозрачности) имитируют нестабильный аналоговый видеосигнал старого телевизора или терминала.
*   **Тактильный интерфейс:** UI-кнопки плавно увеличиваются при наведении курсора, давая игроку четкое интуитивное понимание интерактивности элементов меню.

---

## 2. Визуальное оформление и Аудио

*   **Тряска экрана:** Камера физически вибрирует по осям X и Y, создавая эффект отдачи от взрыва или отражения энергии.
*   **«Дышащий» текст:** Крупные заголовки и рекорды на экранах меню плавно увеличиваются и уменьшаются, создавая живую динамику интерфейса.
*   **Помехи видеосигнала (Flicker):** Случайные падения яркости текста (эффект мерцания альфа-канала) имитируют помехи видеокассеты VHS или ЭЛТ-монитора.

---

## 3. Техническая реализация (Код)

Все эффекты «сочности» написаны с использованием независимых от масштаба времени переменных (`Time.unscaledTime` и `Time.unscaledDeltaTime`). Это гарантирует, что визуальные анимации и тряска продолжают работать стабильно даже во время остановки времени.

### 3.1. Микро-пауза времени (`TimeFreeze.cs`)
Применяется при отражении лазеров. Остановка физического времени выполняется через манипуляцию `Time.timeScale`.

```csharp
public class TimeFreeze : MonoBehaviour
{
    public static TimeFreeze Instance;
    private bool isFreezing = false;

    void Awake() { if (Instance == null) Instance = this; }

    public void Freeze(float duration)
    {
        if (isFreezing) return;
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        isFreezing = true;
        Time.timeScale = 0f; // Полностью останавливаем физику и Time.deltaTime
        
        // Ожидаем в реальном физическом времени, игнорируя нулевой масштаб
        yield return new WaitForSecondsRealtime(duration); // duration = 0.04f (40 мс)
        
        Time.timeScale = 1f; // Возвращаем нормальное время
        isFreezing = false;
    }
}
```

### 3.2. Динамическая тряска камеры (`CameraFollow.cs`)
Тряска интегрирована напрямую в алгоритм следования камеры за игроком, благодаря чему смещение от вибрации суммируется с координатами слежения:

*   Метод запуска тряски из снаряда `BlasterBolt.cs`:
    ```csharp
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }
    ```
*   Корутина расчета смещения:
    ```csharp
    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            // Рассчитываем случайное смещение по осям
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0);
            
            // Используем unscaledDeltaTime, чтобы тряска работала во время заморозки времени
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }
        shakeOffset = Vector3.zero; // Сбрасываем смещение в конце
    }
    ```
*   В методе `LateUpdate()` смещение прибавляется к позиции джедая:
    ```csharp
    Vector3 desiredPosition = target.position + offset + shakeOffset;
    transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    ```

### 3.3. Пульсация и помехи текста (`RetroTextEffect.cs`)
Управляет поведением ретро-текста на экранах.

1.  **Эффект дыхания (Pulse Scale):**
    Масштабирование текста колеблется по синусоиде. Применение `Time.unscaledTime` позволяет тексту плавно пульсировать на паузе:
    ```csharp
    if (pulseScale)
    {
        float scale = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * scale; // originalScale считывается на старте
    }
    ```
2.  **Эффект помех (Flicker Alpha):**
    В методе `Update` каждый кадр с вероятностью 2% запускается корутина помех:
    ```csharp
    if (flickerAlpha && Random.value > 0.98f)
    {
        StartCoroutine(FlickerRoutine());
    }
    ```
    Корутина кратковременно приглушает прозрачность текста до `0.7f` (70%) на `0.05` секунды, имитируя мерцание экрана:
    ```csharp
    System.Collections.IEnumerator FlickerRoutine()
    {
        Color c = textMesh.color;
        textMesh.color = new Color(c.r, c.g, c.b, 0.7f);
        yield return new WaitForSeconds(0.05f);
        textMesh.color = new Color(c.r, c.g, c.b, 1f);
    }
    ```

### 3.4. Масштабирование UI-кнопок при наведении (`ButtonHoverScale.cs`)
Реализует плавное увеличение кнопок. Класс наследует интерфейсы событий Unity `IPointerEnterHandler` и `IPointerExitHandler`:

*   При наведении курсора целевой масштаб увеличивается: `targetScale = originalScale * hoverScaleMultiplier` (на 10%).
*   При уходе курсора возвращается исходный размер.
*   Плавный переход рассчитывается в `Update` через линейную интерполяцию независимого времени:
    ```csharp
    transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    ```
