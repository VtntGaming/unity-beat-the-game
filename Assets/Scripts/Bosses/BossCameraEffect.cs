using System;
using UnityEngine;

public class BossCameraEffect : MonoBehaviour
{
    Transform player;
    Transform BossZoomCamera;
    Camera mainCamera;
    float defaultSize = 4;
    float maxSize = 8;
    float animateTime = 2f;
    float checkingDistance = 30f; // Khi gần hơn 30m, bắt đầu zoom out
    float t = 0;
    void Start()
    {
        player = GameObject.Find("Player").transform;
        BossZoomCamera = GameObject.Find("BossZoomCamera").transform;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    float getDistance(Vector3 v1, Vector3 v2)
    {
        Vector2 diff = new Vector2(v1.x - v2.x, v1.y - v2.y);
        return diff.magnitude;
    }
    public static float EaseSineInOut(float t)
    {
        return (float)(-0.5f * (Math.Cos(Math.PI * t) - 1));
    }

    void Update()
    {
        float current = mainCamera.orthographicSize;
        float distance = getDistance(player.position, BossZoomCamera.position);
        float change = Time.deltaTime / animateTime;
        if (distance > 30)
        {
            t -= change;
        }
        else
            t += change;


        t = Mathf.Clamp(t, 0, 1);
        float t2 = EaseSineInOut(t);
        mainCamera.orthographicSize = Mathf.Lerp(defaultSize, maxSize, t2);
    }
}
