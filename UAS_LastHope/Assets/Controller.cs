using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float forwardSpeed = 5f; // Kecepatan maju
    public float horizontalSpeed = 5f; // Kecepatan kiri/kanan
    public float jumpForce = 8f; // Kekuatan lompat, lebih tinggi agar lompat lebih jauh
    public float crouchHeight = 0.5f; // Tinggi saat jongkok
    public float mouseSensitivity = 100f; // Sensitivitas mouse
    public AudioClip jumpSound; // Sound lompat
    public AudioClip runSound; // Sound lari
    private AudioSource audioSource;
    private bool isRunningSoundPlaying = false;
    private bool wasGroundedLastFrame = true;
    private float originalHeight; // Tinggi asli karakter
    private bool isGrounded = true; // Apakah karakter di tanah
    private bool isCrouching = false;
    private CharacterController characterController;
    private float yRotation = 0f;
    public float crouchSpeedMultiplier = 0.5f; // Atur di Inspector sesuai kebutuhan
    public float jumpSlowMultiplier = 0.5f; // Kecepatan setelah lompat (misal 0.5 = 50%)
    public float jumpSlowDuration = 1.5f;   // Lama efek slow setelah lompat (detik)
    private bool isJumpSlowed = false;
    private bool isFinished = false; // Tambahan: status finish

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            originalHeight = characterController.height;
            characterController.stepOffset = 0.7f; // Lebih tinggi agar bisa naik permukaan lebih kasar
            characterController.slopeLimit = 50f;  // Biar bisa naik tanjakan lebih curam
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
        // Jika sudah finish, hentikan semua kontrol
        if (isFinished)
            return;

        RaycastHit hit; // Deklarasi satu kali di awal fungsi

        // Tidak ada rotasi sama sekali
        float speedMultiplier = 1f;
        if (isCrouching)
            speedMultiplier *= crouchSpeedMultiplier;
        if (isJumpSlowed)
            speedMultiplier *= jumpSlowMultiplier;

        // Geser kiri/kanan dengan A/D
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;

        // Cek tabrakan depan dengan Raycast
        float zSpeed = forwardSpeed * speedMultiplier;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.forward, out hit, 0.6f))
        {
            zSpeed = 0f; // Stop maju jika ada dinding di depan
        }

        // Maju lurus ke Z+, geser di X
        Vector3 move = new Vector3(horizontalInput * horizontalSpeed * speedMultiplier, 0, zSpeed);

        // Cek grounded
        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;
        }
        else
        {
            isGrounded = transform.position.y <= 0.51f;
        }

        // Lompat
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
            // Mulai efek slow setelah lompat
            if (!isJumpSlowed)
                StartCoroutine(JumpSlowEffect());
        }

        // Gravity
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = 0f;
        }

        move.y = verticalVelocity;

        // Jongkok
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

        // Gerakkan karakter
        if (characterController != null)
        {
            characterController.Move(move * Time.deltaTime);

            // Auto step up: cek ganjalan kecil di depan
            if (isGrounded)
            {
                Vector3 origin = transform.position + Vector3.up * 0.1f;
                float stepCheckDistance = 0.6f; // Jarak cek ke depan
                float maxStepHeight = 0.7f;     // Maksimal tinggi step up (sama dengan stepOffset)
                RaycastHit stepHit;
                // Cek ada ganjalan di depan pada ketinggian kaki
                if (Physics.Raycast(origin, Vector3.forward, out stepHit, stepCheckDistance))
                {
                    // Cek permukaan di atas ganjalan (cek apakah bisa naik)
                    Vector3 upperOrigin = origin + Vector3.up * maxStepHeight;
                    if (!Physics.Raycast(upperOrigin, Vector3.forward, stepCheckDistance))
                    {
                        // Cek permukaan di atas ganjalan
                        RaycastHit upperHit;
                        if (Physics.Raycast(stepHit.point + Vector3.up * (maxStepHeight + 0.1f), Vector3.down, out upperHit, maxStepHeight + 0.2f))
                        {
                            float stepHeight = upperHit.point.y - transform.position.y;
                            if (stepHeight > 0.01f && stepHeight < maxStepHeight)
                            {
                                // Naikkan posisi karakter ke atas ganjalan
                                Vector3 pos = transform.position;
                                pos.y += stepHeight + 0.02f;
                                transform.position = pos;
                            }
                        }
                    }
                }
            }

            // Raycast snap ke permukaan ramp agar karakter menempel, hanya saat tidak grounded
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

        // Kontrol suara run/jump
        if (isGrounded && !wasGroundedLastFrame)
        {
            // Baru mendarat, play run sound jika belum
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
            // Baru lompat, stop run sound
            if (audioSource != null && isRunningSoundPlaying)
            {
                audioSource.Stop();
                isRunningSoundPlaying = false;
            }
        }
        wasGroundedLastFrame = isGrounded;

        // Kunci rotasi karakter setiap frame agar tidak bisa berotasi walau tabrakan
        transform.rotation = Quaternion.identity;
    }

    IEnumerator JumpSlowEffect()
    {
        isJumpSlowed = true;
        yield return new WaitForSeconds(jumpSlowDuration);
        isJumpSlowed = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            isFinished = true;
            forwardSpeed = 0f;
            horizontalSpeed = 0f;
            jumpForce = 0f;
            // Tambahkan efek lain jika perlu
        }
    }
}
