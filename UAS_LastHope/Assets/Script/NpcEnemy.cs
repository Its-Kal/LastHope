using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcEnemy : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 2f; // Jarak minimal ke player
    private Transform player;
    private bool shouldChase = false;

    // Dipanggil oleh player saat trigger TrigNPC disentuh
    public void StartChase(Transform playerTransform)
    {
        player = playerTransform;
        shouldChase = true;
        Debug.Log("NpcEnemy mulai mengejar player");
    }

    void Update()
    {
        if (shouldChase && player != null)
        {
            Debug.Log($"NpcEnemy Update: shouldChase={shouldChase}, player={player.name}, posNPC={transform.position}, posPlayer={player.position}");
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                // Bergerak ke arah player
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
            else
            {
                Debug.Log("NpcEnemy sudah dekat dengan player, player akan berhenti.");
                Controller playerController = player.GetComponent<Controller>();
                if (playerController != null)
                {
                    playerController.SetStoppedByGuard(true); // Gunakan method yang sama untuk menghentikan player
                }
            }
        }
        else
        {
            Debug.Log($"NpcEnemy Update: shouldChase={shouldChase}, player={(player != null ? player.name : "null")}");
        }
    }
}
