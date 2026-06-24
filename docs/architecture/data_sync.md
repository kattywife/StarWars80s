# Синхронизация данных и межскриптовое взаимодействие

В игре «Стар80рс» важную роль играет обмен данными между долгоживущими глобальными менеджерами (которые не уничтожаются при перезапуске уровней) и динамически генерируемыми объектами сцены (игрок, штурмовики, кристаллы, элементы окружения).

---

## 1. Концепция и Смысл синхронизации данных

Для обеспечения стабильного игрового процесса данные разделены по степени их долговечности:
*   **Персистентные данные (Постоянные):** Рекорды игрока (максимальный пройденный уровень). Должны сохраняться даже после закрытия приложения.
*   **Сессионные данные (Временные в рамках игры):** Текущий уровень, состояние собранных Ключей Холлидея, количество кристаллов Силы. Они сбрасываются при начале новой игры.
*   **Динамические данные уровня (Локальные):** Текущее здоровье джедая на уровне (всегда сбрасывается до 3 при переходе на новую карту), текущее количество врагов на сцене.

Эффективная передача этих данных исключает утечки памяти и рассинхронизацию интерфейса.

---

## 2. Визуальное и звуковое отображение изменений

Каждое изменение данных мгновенно транслируется в визуальные и звуковые отклики:
*   **Получение ключа:** При вызове метода записи ключа синглтон `GameManager` обновляет данные, `UIManager` делает иконку ключа непрозрачной и белой, а `AudioManager` проигрывает `winSound`.
*   **Сбор кристалла:** Приращение счетчика кристаллов сопровождается звуком `crystalTakenSound` и заполнением ячеек HUD спрайтом `fullCrystal`.

---

## 3. Техническая реализация (Код)

### 3.1. Постоянное сохранение данных (`PlayerPrefs`)

Для сохранения рекордов используется встроенный класс Unity `PlayerPrefs`. Запись происходит в `GameManager.cs` при завершении уровня или смерти игрока:

```csharp
private void SaveMyRecord()
{
    // Считываем прошлый рекорд (0 по умолчанию)
    int lastRecord = PlayerPrefs.GetInt("Record", 0);
    
    // Если текущий уровень больше сохраненного рекорда, перезаписываем его
    if (currentLevel > lastRecord)
    {
        PlayerPrefs.SetInt("Record", currentLevel);
        PlayerPrefs.Save(); // Принудительная запись на диск
    }
}
```

Чтение рекорда происходит при каждом включении панели главного меню в `MainMenuPanel.cs`:

```csharp
void OnEnable()
{
    int record = PlayerPrefs.GetInt("Record", 0);
    recordText.text = "РЕКОРД: " + record + " УРОВНЕЙ";
}
```

### 3.2. Межскриптовое взаимодействие (Сбор ресурсов и урон)

#### А. Сбор кристаллов (`Crystal.cs` -> `GameManager.cs` -> `UIManager.cs`)
При вхождении игрока в триггер кристалла происходит цепочка вызовов:

1.  Компонент кристалла отслеживает игрока:
    ```csharp
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCrystal(); // Передача вызова в ядро
            Destroy(gameObject);
        }
    }
    ```
2.  `GameManager` увеличивает счетчик (до лимита в 3 единицы) и дает команду интерфейсу:
    ```csharp
    public void AddCrystal()
    {
        if (crystals < 3)
        {
            crystals++;
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.crystalTakenSound);
            
            if (UIManager.Instance != null) 
                UIManager.Instance.UpdateCrystals(crystals);
        }
    }
    ```

#### Б. Расчет направления отдачи при уроне (`BlasterBolt.cs` -> `JediController.cs`)
При попадании снаряда штурмовика в джедая, скрипт пули передает свои пространственные координаты контроллеру игрока, чтобы тот мог рассчитать правильный вектор отскока назад (Knockback):

```csharp
// Внутри BlasterBolt.OnTriggerEnter2D:
JediController jedi = hitInfo.GetComponent<JediController>();
if (jedi != null && !jedi.IsInvincible)
{
    // Передаем координаты пули (transform.position)
    jedi.TakeDamage("Штурмовик попал в цель впервые в истории Галактики", transform.position);
    Destroy(gameObject);
}
```

Контроллер игрока принимает позицию обидчика и рассчитывает вектор отдачи:

```csharp
// Внутри JediController.TakeDamage:
Vector2 pushDir = Vector2.zero;
if (attackerPosition != default)
{
    // Вектор от штурмовика/пули к джедаю
    pushDir = ((Vector2)transform.position - attackerPosition).normalized;
}
else
{
    pushDir = -transform.up; // Откат назад по умолчанию
}
ApplyKnockback(pushDir, damageKnockbackForce);
```

### 3.3. Событийная модель (Делегаты)

Для отпугивания Космического Червя используется слабосвязанная событийная модель на основе делегатов C#. Это исключает жесткую связь между скриптом червя `SpaceWorm.cs` и игроком.

1.  В `GameManager.cs` объявлен статический делегат и событие использования ульты:
    ```csharp
    public delegate void UltimateAction();
    public static event UltimateAction OnUltimateUsed;
    ```
2.  При успешном расходовании 3-х кристаллов в `GameManager.TryUseUltimate()` вызывается это событие:
    ```csharp
    if (OnUltimateUsed != null) OnUltimateUsed();
    ```
3.  Каждый активный на сцене червь `SpaceWorm.cs` при появлении подписывается на это событие, а при уничтожении — отписывается во избежание утечек памяти:
    ```csharp
    void Start()
    {
        // Подписка на событие ульты
        GameManager.OnUltimateUsed += ScareAway;
    }

    void OnDestroy()
    {
        // Обязательная отписка
        GameManager.OnUltimateUsed -= ScareAway;
    }

    private void ScareAway()
    {
        // Логика бегства червя за пределы карты
        isRetreating = true;
    }
    ```

---

## 🛠️ Рекомендации по оптимизации обмена данными

*   **Избегайте `FindGameObjectsWithTag`:** В коде `LevelGenerator` очистка сцены выполнялась через поиск объектов по тегам. Это медленная операция. Рекомендуется использовать встроенный в генератор список `spawnedEntities` для точечного удаления сгенерированных объектов, как это успешно реализовано в `GridLevelGenerator.cs`.
*   **Используйте события вместо `SendMessage`:** В скриптах `Lightsaber.cs` и `BlasterBolt.cs` для нанесения урона используется вызов `other.gameObject.SendMessage("TakeDamage", ...)`. Метод `SendMessage` работает через рефлексию Unity, что является ресурсоемкой операцией и не проверяется на этапе компиляции. Рекомендуется заменить его на получение прямого интерфейса или базового класса через `GetComponent<BaseStormtrooper>()` и вызов метода `TakeDamage()` напрямую.
