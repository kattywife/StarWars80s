using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    public float typingSpeed = 0.05f; // —корость печати (секунд на букву)
    
    private Coroutine typingCoroutine;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void StartTyping(string fullText)
    {
        // ≈сли уже что-то печатаетс€ Ч останавливаем
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(fullText));
    }

    private IEnumerator TypeRoutine(string fullText)
    {
        textMesh.text = ""; // ќчищаем текст в начале
        
        // »спользуем WaitForSecondsRealtime, так как во врем€ меню врем€ в игре может быть остановлено (Time.timeScale = 0)
        foreach (char letter in fullText.ToCharArray())
        {
            textMesh.text += letter;
            
            // ≈сли буква - знак препинани€, делаем паузу чуть дольше дл€ естественности
            if (letter == '.' || letter == '!' || letter == '?')
                yield return new WaitForSecondsRealtime(typingSpeed * 3);
            else
                yield return new WaitForSecondsRealtime(typingSpeed);
        }

        typingCoroutine = null;
    }
    
    // ћетод дл€ мгновенного завершени€ (если игрок нажал "ѕропустить")
    public void Skip(string fullText)
    {
        StopAllCoroutines();
        textMesh.text = fullText;
        typingCoroutine = null;
    }
}