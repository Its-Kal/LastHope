using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Drag Player ke sini di Inspector
    public Vector3 offset = new Vector3(0, 5, -10); // Offset kamera (atur sesuai kebutuhan)

    void LateUpdate()
    {
        if (target == null) return;
        // Kamera mengikuti X dan Z player, tetap menghadap ke depan
        Vector3 newPos = target.position + offset;
        newPos.x = target.position.x; // Ikuti X player
        newPos.z = target.position.z + offset.z; // Selalu di belakang player
        transform.position = newPos;
        transform.rotation = Quaternion.Euler(20, 0, 0); // Sudut pandang tetap ke depan
    }
} 