using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 2f; // Jarak minimal ke player
    private Transform player;
    private bool shouldChase = false;

    // Dipanggil oleh player saat trigger TrigGuaard disentuh
    public void StartChase(Transform playerTransform)
    {
        player = playerTransform;
        shouldChase = true;
        Debug.Log("Guard mulai mengejar player");
    }

    void Update()
    {
        if (shouldChase && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                // Bergerak ke arah player
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
            else
            {
                Debug.Log("Guard sudah dekat dengan player, player akan berhenti.");
                Controller playerController = player.GetComponent<Controller>();
                if (playerController != null)
                {
                    playerController.SetStoppedByGuard(true);
                }
            }
        }
    }
}
