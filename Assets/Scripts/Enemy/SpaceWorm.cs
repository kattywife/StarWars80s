using UnityEngine;
using System.Collections;

public class SpaceWorm : MonoBehaviour
{
    public float speed = 15f;
    private float currentRetreatSpeed; // �������� ���������
    public Color flashColor = Color.red;

    private Transform player;
    private bool isRetreating = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        // ������������� �� �����
        GameManager.OnUltimateUsed += ScareAway;
    }

    void OnDestroy()
    {
        GameManager.OnUltimateUsed -= ScareAway;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 direction;
        if (!isRetreating)
        {
            // ����� � ������
            direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
        else
        {
            // �������� �� ������
            direction = (transform.position - player.position).normalized;
            transform.position += (Vector3)direction * currentRetreatSpeed * Time.deltaTime;
            
            // ���� ����� ������ � �������
            if (Vector2.Distance(transform.position, player.position) > 40f) 
                Destroy(gameObject);
        }

        // �������: ������ ������ �� ����������� ��������
        // ���� ����� �����, ������ 'transform.right' �� 'transform.up'
        transform.right = direction; 
    }

    // ��� �����������, ���� ������ ����� ������
    private void ScareAway()
    {
        if (isRetreating) return;
        
        // ������� ������ (� 1.5 ���� ������� ��� ������), ����������� ����
        currentRetreatSpeed = speed * 1.5f; 
        isRetreating = true;
        StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(0.5f);
        sr.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRetreating)
        {
            JediController jedi = other.GetComponent<JediController>();
            if (jedi != null)
            {
                // Передаем transform.position червя!
                for (int i = 0; i < 3; i++) jedi.TakeDamage("Космический червь укусил джедая", transform.position); 
            }

            Debug.Log("Червь задел джедая...");
            
            isRetreating = true;
            currentRetreatSpeed = speed / 2f; 
        }
    }

    public void TakeDamage()
    {
        // ��������� �������� � ��������� ����������� ����� (������ 2)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectKey(2, "���������� ���� (������ ������)");
        }

        // ������ ��� ������ (���� ���� ���� ��� �������, ����� �������� �����)
        Debug.Log("<color=red>����������� ����� ��������!</color>");
        
        Destroy(gameObject);
    }
}