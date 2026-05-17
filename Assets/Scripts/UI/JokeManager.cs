using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class JokeData
{
    public List<string> jokes;
    public List<string> hints;
}

public class JokeManager : MonoBehaviour
{
    private JokeData jokeData;

    void Awake()
    {
        LoadJokes();
    }

    void LoadJokes()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Jokes");
        
        if (jsonFile == null)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: Файл Jokes.json не найден в папке Resources!");
            return;
        }

        string rawText = jsonFile.text;

        // Убираем возможный BOM (невидимый символ в начале файла)
        if (rawText.StartsWith("\uFEFF"))
        {
            rawText = rawText.Substring(1);
        }

        try 
        {
            jokeData = JsonUtility.FromJson<JokeData>(rawText);
            Debug.Log("<color=green>Jokes.json успешно загружен!</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError("ОШИБКА ПАРСИНГА JSON: " + e.Message);
            Debug.Log("Содержимое файла, которое пришло в скрипт: " + rawText);
        }
    }

    public string GetRandomJoke()
    {
        if (jokeData == null || jokeData.jokes == null || jokeData.jokes.Count == 0) return "Система OASIS подгружается...";
        return jokeData.jokes[Random.Range(0, jokeData.jokes.Count)];
    }

    public string GetRandomHint()
    {
        if (jokeData == null || jokeData.hints == null || jokeData.hints.Count == 0) return "Имперские базы данных недоступны...";
        return jokeData.hints[Random.Range(0, jokeData.hints.Count)];
    }

    public string GetAnything()
    {
        return (Random.value > 0.5f) ? GetRandomJoke() : GetRandomHint();
    }
}