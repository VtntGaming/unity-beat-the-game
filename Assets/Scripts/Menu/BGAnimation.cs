using UnityEngine;
using UnityEngine.UI;

public class BGAnimation : MonoBehaviour
{
    private Vector2 baseMove = new Vector2(64, 36);
    private Vector2 next = new Vector2 (0, 0);
    private Vector2 previous = new Vector2(0, 0);
    private float progress = 0;
    private float moveTime = 5f;
    RectTransform rect;

    void nextTransition()
    {
        float rndX = Random.Range(-1f, 1f);
        float rndY = Random.Range(-1f, 1f);

        previous = next;
        next = baseMove * new Vector2(rndX, rndY);
        progress = 0;
        Debug.Log(next);
    }
    void Start()
    {
        rect = transform.GetComponent<RectTransform>();
        nextTransition();
    }

    float sineInOut(float t)
    {
        return (float)(-0.5 * (Mathf.Cos(Mathf.PI * t) - 1));
    }
    void Update()
    {
        progress += Time.deltaTime / moveTime;
        if (progress >= 1)
        {
            nextTransition();
        }
        else
        {
            Vector2 current = previous + (next - previous) * sineInOut(progress);
            rect.localPosition = current;
        }
    }
}
