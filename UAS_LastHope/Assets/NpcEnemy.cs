using UnityEngine;

public class NpcEnemy : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 2f;
    private Transform player;
    private bool shouldChase = false;

    public void StartChase(Transform playerTransform)
    {
        player = playerTransform;
        shouldChase = true;
        Debug.Log("NpcEnemy mulai mengejar player");
    }

    public void GetSledged(Transform player, float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        Vector3 throwDir = (transform.position - player.position).normalized + Vector3.up * 0.5f;
        rb.AddForce(throwDir * force, ForceMode.Impulse);

        shouldChase = false;
        this.enabled = false;
    }

    void Update()
    {
        if (shouldChase && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
            else
            {
                Controller playerController = player.GetComponent<Controller>();
                if (playerController != null)
                {
                    playerController.SetStoppedByGuard(true, this);
                }
            }
        }
    }
} 