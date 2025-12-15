using UnityEngine;

public class SelfDestroyTimer : MonoBehaviour
{
    [SerializeField] private float duration = 30f;   // Total lifetime in seconds
    [SerializeField] private float flashDuration = 10f; // Start flashing when time left <= this

    private float timer;
    private SpriteRenderer spriteRenderer;
    private bool isFlashing = false;
    private float flashInterval = 0.2f;
    private float nextFlashTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SelfDestroyTimer requires a SpriteRenderer to flash!");
        }
    }

    private void OnEnable()
    {
        timer = duration;
        isFlashing = false;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= flashDuration && !isFlashing)
        {
            StartFlashing();
        }

        if (isFlashing && spriteRenderer != null)
        {
            if (Time.time >= nextFlashTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled; // toggle visibility
                nextFlashTime = Time.time + flashInterval;
            }
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void StartFlashing()
    {
        isFlashing = true;
        nextFlashTime = Time.time + flashInterval;
    }
}
