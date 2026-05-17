using UnityEngine;
using System.Collections.Generic;

public class JokeManager : MonoBehaviour
{
    private List<string> jokes = new List<string>();

    void Start()
    {
        TextAsset txt = Resources.Load<TextAsset>("Jokes");
        if (txt != null)
        {
            string[] lines = txt.text.Split('\n');
            foreach (string line in lines) if (!string.IsNullOrWhiteSpace(line)) jokes.Add(line);
        }
    }

    public string GetRandomJoke()
    {
        if (jokes.Count == 0) return "Сюжет засекречен...";
        return jokes[Random.Range(0, jokes.Count)];
    }
}