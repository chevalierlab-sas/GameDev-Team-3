using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Parkour Stats")]
    public int level;
    public int experience;
    private int health;
    public int maxHealth;
    public float moveSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public int jumpCount;


    private int jumpCountLeft;
    private bool readyToJump;
    private bool isSprinting;
    private bool wasSprinting;
    private bool wasJumping;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("UI")]
    public GameObject deathUI;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsVoid;


    [Header("Animator")]
    public Animator animator;
    public Transform orientation;

    // private variables
    private float flySpeed = 2f;
    private Vector3 cameraOffset = new Vector3(0, 5, 0);
    bool grounded;
    bool voided;
    private Vector3 lastPosition;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    RaycastHit hit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        jumpCountLeft = jumpCount;
        health = maxHealth;
    }

    private void Update()
    {
        isSprinting = Input.GetKey(sprintKey) && grounded && (horizontalInput != 0 || verticalInput != 0);

        if (animator != null)
        {
            animator.SetBool("isIdle", horizontalInput == 0 && verticalInput == 0 && grounded);
            animator.SetBool("isGrounded", grounded);
            animator.SetBool("isWalking", horizontalInput != 0 || verticalInput != 0 && !isSprinting);
            animator.SetBool("isRunning", horizontalInput != 0 || verticalInput != 0 && isSprinting);
            animator.SetBool("isJumping", !grounded);
            animator.SetBool("isFalling", rb.linearVelocity.y < -15 && !grounded);
            animator.SetBool("AbleToLand", rb.linearVelocity.y > -15 && !grounded);
            animator.SetBool("isDead", health <= 0);
        }

        if (health <= 0)
        {
            Died();
        }

        // print("isIdle : " + animator.GetBool("isIdle"));
        // print("isGrounded : " + animator.GetBool("isGrounded"));
        // print("isWalking : " + animator.GetBool("isWalking"));
        // print("isRunning : " + animator.GetBool("isRunning"));
        // print("isJumping : " + animator.GetBool("isJumping"));
        // print("isFalling : " + animator.GetBool("isFalling"));

        // ground check
        grounded = Physics.SphereCast(
            transform.position,
            0.3f,
            Vector3.down,
            out hit,
            playerHeight * 0.5f + 0.3f,
            whatIsGround
        );
        voided = Physics.SphereCast(
            transform.position,
            0.3f,
            Vector3.down,
            out hit,
            playerHeight * 0.5f + 0.3f,
            whatIsVoid
        );

        MyInput();
        SpeedControl();

        if (!grounded & voided)
        {
            TakeDamage(maxHealth); 
        }
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            lastPosition = transform.position;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
    }

    public int GetHealth()
    {
        return health;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && jumpCountLeft > 0)
        {
            if (!wasJumping)
            {
                wasJumping = true;
                StartCoroutine(WaitForGround());
                wasSprinting = isSprinting;
            }
            readyToJump = false;
            jumpCountLeft--;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        if (health <= 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isSprinting || wasSprinting)
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
    private void Died()
    {
        StartCoroutine(HandleDeath());
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        if (animator.GetBool("isFalling"))
        {
            jumpCountLeft = 0; // prevent jumping while falling
            return;
        }
        readyToJump = true;
    }

    IEnumerator WaitForGround()
    {
        yield return new WaitUntil(() => !grounded);
        bool ableToLand = animator.GetBool("AbleToLand");
        yield return new WaitUntil(() => grounded);
        wasSprinting = isSprinting;

        if (wasJumping)
        {
            wasJumping = false; // reset jumping state when grounded
            jumpCountLeft = jumpCount; // reset jump count when grounded
        }

        if (animator.GetBool("isFalling"))
        {
            TakeDamage(maxHealth); // take damage if falling
        }
        else
        {
            animator.SetBool("isLanding", true);
            yield return new WaitForSeconds(0.2f);
            animator.SetBool("isLanding", false);
        }

                print("isLanding: " + animator.GetBool("isLanding"));
        print("AbleToLand: " + animator.GetBool("AbleToLand"));
        print("isGrounded: " + animator.GetBool("isGrounded"));
    }

    IEnumerator HandleDeath()
    {
        animator.applyRootMotion = true;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Dead") && stateInfo.normalizedTime >= 0.6f)
        {
            animator.speed = 0f; // freeze animation
        }

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        col.height = 0.5f;
        col.center = new Vector3(0, 0.25f, 0);

        deathUI.SetActive(true);

        // Start camera fly
        Vector3 targetPos = transform.position + cameraOffset;
        Quaternion targetRot = Quaternion.LookRotation(transform.position - targetPos);
        float t = 0;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * flySpeed;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, t);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRot, t);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(3f);
        animator.speed = 1f;
        transform.position = lastPosition;
        health = maxHealth;

        col.height = 2f;
        col.center = new Vector3(0, 0, 0);
        
        deathUI.SetActive(false);
    }
}