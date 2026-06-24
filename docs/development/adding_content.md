# Руководство разработчика: Добавление нового контента

Данный документ представляет собой пошаговое руководство по расширению игры «Стар80рс». Здесь описаны правила интеграции новых типов штурмовиков, звуковых эффектов и пополнения базы данных текстовых ретро-шуток.

---

## 1. Добавление нового типа штурмовиков

Вся логика врагов-стрелков базируется на абстрактном классе `BaseStormtrooper.cs`. Чтобы добавить новый тип штурмовика (например, *HeavyStormtrooper*, стреляющего очередями), выполните следующие шаги:

### Шаг 1: Написание C#-скрипта поведения
Создайте новый скрипт в папке `Assets/Scripts/Enemy/` (например, `HeavyStormtrooper.cs`), унаследуйте его от `BaseStormtrooper` и переопределите абстрактный метод `ExecuteShooting()`:

```csharp
using UnityEngine;
using System.Collections;

public class HeavyStormtrooper : BaseStormtrooper
{
    [Header("Специфичные Настройки (Heavy)")]
    public int shotsInBurst = 3;       // Количество выстрелов в очереди
    public float timeBetweenShots = 0.1f; // Задержка между выстрелами в очереди

    protected override void ExecuteShooting()
    {
        if (player == null || firePoint == null) return;
        
        // Запускаем корутину стрельбы очередью
        StartCoroutine(ShootBurstRoutine());
    }

    private IEnumerator ShootBurstRoutine()
    {
        for (int i = 0; i < shotsInBurst; i++)
        {
            if (player == null || firePoint == null) break;

            // Направляем пулю точно на игрока
            Vector2 direction = player.position - firePoint.position;
            float angleToPlayer = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90f);

            // Спавним лазер
            Instantiate(blasterBoltPrefab, firePoint.position, firePoint.rotation);

            // Воспроизводим звук выстрела
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.stormtrooperShootSound);

            yield return new WaitForSeconds(timeBetweenShots);
        }
    }
}
```

### Шаг 2: Настройка префаба в Unity
1.  Импортируйте спрайт нового штурмовика в папку `Assets/Sprites/characters/`.
2.  Создайте новый GameObject на сцене или продублируйте префаб `Stormtrooper.prefab`.
3.  Замените спрайт на компоненте `SpriteRenderer`.
4.  Убедитесь, что тег объекта установлен на `Enemy`, а слой — на `Default`.
5.  Добавьте компонент `Rigidbody2D` (тип *Dynamic*, гравитация *0*, заморозка вращения *Z*).
6.  Добавьте физический коллайдер `Collider2D` (в режиме обычного твердого тела, не триггер).
7.  Создайте дочерний пустой GameObject в передней части спрайта, назовите его `FirePoint`. Это точка, откуда будут вылетать пули.
8.  Прикрепите созданный на Шаге 1 скрипт `HeavyStormtrooper.cs`.
9.  В инспекторе перетащите ссылки:
    *   `BlasterBolt` префаб в слот `Blaster Bolt Prefab`.
    *   Дочерний `FirePoint` в слот `Fire Point`.
    *   `EnemyDeathEffect` префаб в слот `Death Effect Prefab`.
10. Сохраните объект как префаб в папку `Assets/Prefabs/Characters/HeavyStormtrooper.prefab`.

### Шаг 3: Интеграция в генератор уровней (`GridLevelGenerator.cs`)
Откройте скрипт `GridLevelGenerator.cs` и добавьте новый тип штурмовика в пул спавна.
1.  Объявите ссылку на новый префаб в начале класса:
    ```csharp
    public GameObject heavyStormtrooperPrefab;
    ```
2.  Перейдите в метод `SpawnEntities()` и настройте распределение вероятностей спавна:
    ```csharp
    // Пример: 60% — обычный, 25% — элитный, 15% — тяжелый штурмовик
    float roll = Random.Value;
    if (roll < 0.15f)
        SpawnObject(heavyStormtrooperPrefab, spawnPos);
    else if (roll < 0.40f)
        SpawnObject(eliteStormtrooperPrefab, spawnPos);
    else
        SpawnObject(stormtrooperPrefab, spawnPos);
    ```
3.  В редакторе Unity выберите игровой объект генератора и перетащите ваш новый префаб штурмовика в незаполненный слот в инспекторе.

---

## 2. Добавление новых звуковых эффектов

Звуковое сопровождение игры централизовано через синглтон `AudioManager.cs`.

### Шаг 1: Импорт аудиофайла
Поместите аудиофайл (в формате `.mp3` или `.wav`) в папку `Assets/Sounds/`.

### Шаг 2: Регистрация звука в коде
Откройте `AudioManager.cs` и объявите новое публичное поле для аудиоклипа:
```csharp
[Header("Новые звуковые эффекты")]
public AudioClip heavyShootSound; // Ссылка на новый звук
```

### Шаг 3: Настройка в инспекторе
1.  Найдите объект `AudioManager` в вашей сцене (или внутри префаба ядра).
2.  Перетащите импортированный аудиофайл из окна проекта в новое поле `Heavy Shoot Sound` в инспекторе скрипта `AudioManager`.

### Шаг 4: Вызов звука из любого скрипта
Теперь вы можете проиграть этот звук из любого места вашей программы с помощью одной строчки кода:
```csharp
if (AudioManager.Instance != null)
{
    AudioManager.Instance.PlaySFX(AudioManager.Instance.heavyShootSound);
}
```

---

## 3. Расширение базы данных шуток и подсказок

Тексты для загрузочных экранов и переходов хранятся непосредственно в скрипте `JokeManager.cs`.

### Шаг 1: Открытие базы данных
Откройте скрипт `Assets/Scripts/UI/JokeManager.cs`. Вы увидите два списка строк: `jokes` (шутки) и `hints` (подсказки).

### Шаг 2: Добавление новых строк
Просто впишите новую строку в конец соответствующего списка, соблюдая синтаксис C#:

```csharp
private readonly List<string> jokes = new List<string>
{
    // ... прошлые шутки ...
    "Это не лагающий джедай, это Сила искривляет пространство-время.",
    "Моя новая тяжелая броня весит ровно 16 байт." // Новая шутка
};

private readonly List<string> hints = new List<string>
{
    // ... прошлые подсказки ...
    "Подсказка: Тяжелые штурмовики стреляют очередями. Всегда держите меч наготове!",
    "Совет мастера: Рывок (Dash) делает вас неуязвимым на 0.18 секунд." // Новая подсказка
};
```
