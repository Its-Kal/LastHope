using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Untuk reload scene saat game over

public class RockFollow : MonoBehaviour
{
    public Transform player; // Drag player ke sini di Inspector
    public float speed = 5f;

    void Update()
    {
        if (player != null)
        {
            // Gerak ke arah player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Game Over: reload scene, atau bisa ganti dengan logic lain
            Debug.Log("Game Over!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (other.CompareTag("Peti"))
        {
            Collider[] allColliders = other.GetComponentsInChildren<Collider>();
            foreach (var col in allColliders)
            {
                col.enabled = false;
            }
        }
    }
}
