using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("UI Sounds")]
    public AudioClip clickSound;

    [Header("Game State Sounds")]
    public AudioClip winSound;
    public AudioClip gameOverSound;

    [Header("Jedi Sounds")]
    public AudioClip crystalTakenSound;
    public AudioClip jediHitSound;
    public AudioClip jediDeathSound;

    [Header("Enemy Sounds")]
    public AudioClip stormtrooperShootSound;
    public AudioClip stormtrooperDeathSound;
    public AudioClip sandwormRoarSound;

    void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Создаем AudioSource, если он не прикреплен
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Универсальный метод для проигрывания SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Быстрый метод для клика
    public void PlayClick()
    {
        PlaySFX(clickSound);
    }
}