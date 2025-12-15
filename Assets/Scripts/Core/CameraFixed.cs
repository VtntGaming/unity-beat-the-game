using UnityEngine;

public class CameraFixed : MonoBehaviour
{
    public Transform player;  // Reference to the player
    public float smoothSpeed = 5f; // Smooth follow speed
    public float minZoom = 3f;  // Minimum zoom level
    public float maxZoom = 10f; // Maximum zoom level
    public float zoomSpeed = 2f; // Speed of zoom in/out
    private Camera cam;
    void BindPlayer()
    {
        if (player == null)
        {
            Debug.Log("Bind nv mới vào camera");
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player == null)
            {
                Debug.LogError("Player is not assigned or missing the 'Player' tag!");
            }
        }
    }

    void Start()
    {
        cam = GetComponent<Camera>();

        // Automatically find player if not assigned
        BindPlayer();
    }

    private void Update()
    {
        BindPlayer();
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Smoothly follow the player
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        // Zoom in/out with '-' and '+'
        if (Input.GetKey(KeyCode.Minus))
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }
        if (Input.GetKey(KeyCode.Equals)) // '+' key
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }
    }
}
