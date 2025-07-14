using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public int jumpCount = 1;
    private int jumpCountLeft;
    private bool readyToJump;
    private bool isSprinting;
    

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsVoid;
    bool grounded;

    [Header("Animator")]
    public Animator animator;
    public Transform orientation;

    // private variables
    private Vector3 lastPosition;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        jumpCountLeft = jumpCount;
    }

    private void Update()
    {
        isSprinting = Input.GetKey(sprintKey);
        if (animator != null) {
            animator.SetBool("isIdle", horizontalInput == 0 && verticalInput == 0 && grounded);
            animator.SetBool("isGrounded", grounded);
            animator.SetBool("isWalking", horizontalInput != 0 || verticalInput != 0 && !isSprinting);
            animator.SetBool("isRunning", horizontalInput != 0 || verticalInput != 0 && isSprinting);
            animator.SetBool("isJumping", !grounded);
        }

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();

        if (!grounded & Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsVoid))
        {
            transform.position = lastPosition;
            rb.linearVelocity = Vector3.zero;
        }
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            lastPosition = transform.position;
            jumpCountLeft = jumpCount; // reset jump count when grounded
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded && jumpCountLeft > 0)
        {
            readyToJump = false;
            jumpCountLeft--;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isSprinting)
        {
            rb.AddForce(moveDirection.normalized * sprintSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
}