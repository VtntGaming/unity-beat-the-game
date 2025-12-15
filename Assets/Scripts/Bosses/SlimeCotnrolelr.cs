using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public GameObject gapTrigger;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        gapTrigger.SetActive(false); // Ẩn ban đầu
    }

    public void EnableGapTrigger()
    {
        gapTrigger.SetActive(true);
    }

    public void DisableGapTrigger()
    {
        gapTrigger.SetActive(false);
    }
}
