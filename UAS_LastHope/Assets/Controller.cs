using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float horizontalSpeed = 5f;
    public float jumpForce = 8f;
    public float crouchHeight = 0.5f;
    public float mouseSensitivity = 100f;
    public AudioClip jumpSound;
    public AudioClip runSound;
    private AudioSource audioSource;
    private bool isRunningSoundPlaying = false;
    private bool wasGroundedLastFrame = true;
    private float originalHeight;
    private bool isGrounded = true;
    private bool isCrouching = false;
    private CharacterController characterController;
    private float yRotation = 0f;
    public float crouchSpeedMultiplier = 0.5f;
    public float jumpSlowMultiplier = 0.5f;
    public float jumpSlowDuration = 1.5f;
    private bool isJumpSlowed = false;
    private bool isFinished = false;
    private bool stoppedByGuard = false;
    private bool isSledding = false;
    public float sleddingForce = 10f;
    private NpcEnemy nearbyNpcEnemy = null;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            originalHeight = characterController.height;
            characterController.stepOffset = 0.7f;
            characterController.slopeLimit = 50f;
        }
        else
        {
            originalHeight = transform.localScale.y;
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && runSound != null)
        {
            audioSource.clip = runSound;
            audioSource.loop = true;
            audioSource.Play();
            isRunningSoundPlaying = true;
        }
    }

    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    void Update()
    {
        if (isFinished || stoppedByGuard)
        {
            // Sledding: jika player menekan Ctrl saat berhenti karena npc
            if (nearbyNpcEnemy != null && Input.GetKeyDown(KeyCode.LeftControl))
            {
                isSledding = true;
                Debug.Log("Player melakukan sledding ke NpcEnemy!");
                nearbyNpcEnemy.GetSledged(transform, sleddingForce);
                stoppedByGuard = false;
                nearbyNpcEnemy = null;
                return;
            }
            return;
        }
        else
        {
            isSledding = false;
        }

        RaycastHit hit;
        float speedMultiplier = 1f;
        if (isCrouching)
            speedMultiplier *= crouchSpeedMultiplier;
        if (isJumpSlowed)
            speedMultiplier *= jumpSlowMultiplier;

        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;

        float zSpeed = forwardSpeed * speedMultiplier;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.forward, out hit, 0.6f))
        {
            zSpeed = 0f;
        }

        Vector3 move = new Vector3(horizontalInput * horizontalSpeed * speedMultiplier, 0, zSpeed);

        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;
        }
        else
        {
            isGrounded = transform.position.y <= 0.51f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
        {
            verticalVelocity = jumpForce;
            if (audioSource != null && jumpSound != null)
            {
                if (isRunningSoundPlaying)
                {
                    audioSource.Stop();
                    isRunningSoundPlaying = false;
                }
                audioSource.PlayOneShot(jumpSound);
            }
            if (!isJumpSlowed)
                StartCoroutine(JumpSlowEffect());
        }

        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = 0f;
        }

        move.y = verticalVelocity;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                if (characterController != null)
                {
                    characterController.height = crouchHeight;
                }
                else
                {
                    Vector3 scale = transform.localScale;
                    scale.y = crouchHeight;
                    transform.localScale = scale;
                }
            }
        }
        else
        {
            if (isCrouching)
            {
                isCrouching = false;
                if (characterController != null)
                {
                    characterController.height = originalHeight;
                }
                else
                {
                    Vector3 scale = transform.localScale;
                    scale.y = originalHeight;
                    transform.localScale = scale;
                }
            }
        }

        if (characterController != null)
        {
            characterController.Move(move * Time.deltaTime);
            if (isGrounded)
            {
                Vector3 origin = transform.position + Vector3.up * 0.1f;
                float stepCheckDistance = 0.6f;
                float maxStepHeight = 0.7f;
                RaycastHit stepHit;
                if (Physics.Raycast(origin, Vector3.forward, out stepHit, stepCheckDistance))
                {
                    Vector3 upperOrigin = origin + Vector3.up * maxStepHeight;
                    if (!Physics.Raycast(upperOrigin, Vector3.forward, stepCheckDistance))
                    {
                        RaycastHit upperHit;
                        if (Physics.Raycast(stepHit.point + Vector3.up * (maxStepHeight + 0.1f), Vector3.down, out upperHit, maxStepHeight + 0.2f))
                        {
                            float stepHeight = upperHit.point.y - transform.position.y;
                            if (stepHeight > 0.01f && stepHeight < maxStepHeight)
                            {
                                Vector3 pos = transform.position;
                                pos.y += stepHeight + 0.02f;
                                transform.position = pos;
                            }
                        }
                    }
                }
            }
            if (!isGrounded)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 2f))
                {
                    Vector3 pos = transform.position;
                    pos.y = hit.point.y + characterController.height / 2f;
                    transform.position = pos;
                }
            }
        }
        else
        {
            transform.Translate(new Vector3(move.x, 0, move.z) * Time.deltaTime, Space.World);
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }

        if (isGrounded && !wasGroundedLastFrame)
        {
            if (audioSource != null && runSound != null && !isRunningSoundPlaying)
            {
                audioSource.clip = runSound;
                audioSource.loop = true;
                audioSource.Play();
                isRunningSoundPlaying = true;
            }
        }
        if (!isGrounded && wasGroundedLastFrame)
        {
            if (audioSource != null && isRunningSoundPlaying)
            {
                audioSource.Stop();
                isRunningSoundPlaying = false;
            }
        }
        wasGroundedLastFrame = isGrounded;
        transform.rotation = Quaternion.identity;
    }

    IEnumerator JumpSlowEffect()
    {
        isJumpSlowed = true;
        yield return new WaitForSeconds(jumpSlowDuration);
        isJumpSlowed = false;
    }

    public void SetStoppedByGuard(bool value, NpcEnemy npc = null)
    {
        stoppedByGuard = value;
        if (value && npc != null)
        {
            nearbyNpcEnemy = npc;
        }
        else if (!value)
        {
            nearbyNpcEnemy = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            isFinished = true;
            forwardSpeed = 0f;
            horizontalSpeed = 0f;
            jumpForce = 0f;
        }
        else if (other.CompareTag("TrigGuaard"))
        {
            Debug.Log("Player menyentuh trigger TrigGuaard");
            Guard guard = FindObjectOfType<Guard>();
            if (guard != null)
            {
                guard.StartChase(transform);
            }
        }
        else if (other.CompareTag("TrigNPC"))
        {
            Debug.Log("Player menyentuh trigger TrigNPC");
            NpcEnemy npc = FindObjectOfType<NpcEnemy>();
            if (npc != null)
            {
                npc.StartChase(transform);
            }
        }
    }
} 