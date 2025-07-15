using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float forwardSpeed = 5f; // Kecepatan maju
    public float horizontalSpeed = 5f; // Kecepatan kiri/kanan
    public float jumpForce = 5f; // Kekuatan lompat
    public float crouchHeight = 0.5f; // Tinggi saat jongkok
    public float mouseSensitivity = 100f; // Sensitivitas mouse
    private float originalHeight; // Tinggi asli karakter
    private bool isGrounded = true; // Apakah karakter di tanah
    private bool isCrouching = false;
    private CharacterController characterController;
    private float yRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            originalHeight = characterController.height;
        }
        else
        {
            originalHeight = transform.localScale.y;
        }
    }

    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    void Update()
    {
        // Rotasi karakter dengan mouse (kanan-kiri)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Gerak maju otomatis (mengikuti arah karakter)
        Vector3 move = transform.forward * forwardSpeed;

        // Input kiri/kanan
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }
        move += transform.right * horizontalInput * horizontalSpeed;

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
        }
        else
        {
            transform.Translate(new Vector3(move.x, 0, move.z) * Time.deltaTime, Space.World);
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }
}
