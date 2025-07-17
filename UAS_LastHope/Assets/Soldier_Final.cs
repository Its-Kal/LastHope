using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier_Final : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 2f; // Jarak minimal ke player
    private Transform player;
    private bool shouldChase = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Dipanggil oleh player saat trigger TrigGuaard disentuh
    public void StartChase(Transform playerTransform)
    {
        player = playerTransform;
        shouldChase = true;
        Debug.Log("Soldier_Final mulai mengejar player!");
    }

    void Update()
    {
        if (shouldChase && player != null)
        {
            Debug.Log($"Soldier_Final Update: shouldChase={shouldChase}, player={player.name}, posNPC={transform.position}, posPlayer={player.position}");
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
                if (animator != null)
                    animator.SetBool("isRunning", true); // Pastikan parameter ini ada di Animator
            }
            else
            {
                if (animator != null)
                    animator.SetBool("isRunning", false);
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("isRunning", false);
        }
    }
}
