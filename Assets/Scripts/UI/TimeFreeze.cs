using UnityEngine;
using System.Collections;

public class TimeFreeze : MonoBehaviour
{
    public static TimeFreeze Instance;
    private bool isFreezing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Freeze(float duration)
    {
        if (isFreezing) return;
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        isFreezing = true;
        Time.timeScale = 0f;
        
        // Wait using real-world seconds, ignoring the frozen time scale
        yield return new WaitForSecondsRealtime(duration);
        
        Time.timeScale = 1f;
        isFreezing = false;
    }
}