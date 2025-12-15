using UnityEngine;

public class FlashHolder : MonoBehaviour
{
    [SerializeField] private Transform player;  // Vị trí của nhân vật (hoặc đối tượng cần theo dõi)
    [SerializeField] private Transform flashPoint;  // Vị trí của flashPoint (hoặc đối tượng tấn công)

    private void Update()
    {
        if (player == null)
        {
            // Nếu nv bị thay thế, tìm cách check lại nv
            Debug.Log("NV bị thay thế");

            GameObject newPlayer = GameObject.Find("Player");

            if (newPlayer != null)
            {

                Debug.Log("Đã bind nv mới");
                player = newPlayer.transform;
                flashPoint = player.transform.Find("flashPoint");
            }
            else return; // Nếu không tìm được object, huỷ lần update này
        }

        // Không cần gán scale trực tiếp cho FlashHolder nữa
        // Điều này sẽ giúp tránh việc scale của flashPoint bị ảnh hưởng không mong muốn
        flashPoint.localScale = new Vector3(player.localScale.x, flashPoint.localScale.y, flashPoint.localScale.z);
    }
}
