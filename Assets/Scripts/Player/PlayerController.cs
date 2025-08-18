using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private CheckpointCollect checkpointCollect;
    private GemCollect gemCollect;

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

    [Header("SFX")]
    public AudioSource step;
    public AudioSource run;
    public AudioSource respawn;
    public AudioSource fall;
    public AudioSource climb;
    public AudioSource dead;
    public AudioSource item;

    [Header("UI")]
    public GameObject deathUI;
    public GameObject healthUI;
    public Image damagedUI;
    public TextMeshProUGUI gemCollected;
    public TextMeshProUGUI flagCollected;
    private GameObject healthIndicator;
    private TextMeshProUGUI healthText;
    private GameObject warningFallDamage;
    private GameObject warningFallDamageText;

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

    int fallDamageThreshold = 15;
    int fallDamage = 0;

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

        healthIndicator = healthUI.transform.Find("Indicator").gameObject;
        healthText = healthUI.transform.Find("HPLeft").GetComponent<TextMeshProUGUI>();
        warningFallDamage = healthUI.transform.Find("WarningFallDamage").gameObject;
        warningFallDamageText = healthUI.transform.Find("WarningFallDamageText").gameObject;
        
        checkpointCollect = GetComponent<CheckpointCollect>();
        gemCollect = GetComponent<GemCollect>();
    }

    private float stepInterval = 0.4f;
    private float stepTimer = 0f;

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
            animator.SetBool("isMinFalling", rb.linearVelocity.y < -10 && !grounded);
            animator.SetBool("AbleToLand", rb.linearVelocity.y > -15 && !grounded);
            animator.SetBool("isDead", health <= 0);

            if (animator.GetBool("isMinFalling"))
            {
                fallDamage = Mathf.Max(0, fallDamageThreshold - (int)(rb.linearVelocity.y / 10));
            }
            else
            {
                fallDamage = 0;
            }
        }

        // Update health UI
        healthText.text = health.ToString() + "%";
        healthIndicator.transform.localScale = new Vector3(health / (float)maxHealth, 1, 1);
        warningFallDamage.SetActive(fallDamage > 0);
        warningFallDamageText.SetActive(fallDamage > 0);

        // Update gem and flag collected UI
        if (gemCollect != null)
        {
            gemCollected.text = "Gems Collected : " + gemCollect.gemCount;
        }

        if (checkpointCollect != null)
        {
            flagCollected.text = "Flags Collected : " + checkpointCollect.checkpointCount;
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

        if (grounded)
        {
            if (moveDirection.magnitude > 0.1f)
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    if (isSprinting)
                        playSFX("run");
                    else
                        playSFX("step");

                    stepTimer = stepInterval; // reset timer
                }
            }
            else
            {
                stepTimer = 0f;
                stopSFX("step");
                stopSFX("run");
            }
        }
        else
        {
            stepTimer = 0f;
            stopSFX("step");
            stopSFX("run");
        }

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

    public void playSFX(string soundName)
    {
        AudioSource audioSource = null;

        switch (soundName)
        {
            case "step":
                audioSource = step;
                break;
            case "run":
                audioSource = run;
                break;
            case "respawn":
                audioSource = respawn;
                break;
            case "fall":
                audioSource = fall;
                break;
            case "climb":
                audioSource = climb;
                break;
            case "dead":
                audioSource = dead;
                break;
            case "item":
                audioSource = item;
                break;
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void stopSFX(string soundName)
    {
        AudioSource audioSource = null;

        switch (soundName)
        {
            case "step":
                audioSource = step;
                break;
            case "run":
                audioSource = run;
                break;
            case "respawn":
                audioSource = respawn;
                break;
            case "fall":
                audioSource = fall;
                break;
            case "climb":
                audioSource = climb;
                break;
            case "dead":
                audioSource = dead;
                break;
            case "item":
                audioSource = item;
                break;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public float flashDuration = 0.5f;
    private Coroutine flashCoroutine;

    public void TakeDamage(int amount, bool flash = false)
    {
        if (health <= 0) return;
        health -= amount;
        if (health <= 0)
        {
            Died();
            health = 0;
            playSFX("dead");
        }

        playSFX("fall");

        if (flash)
        {
            FlashDamage();
        }
    }

    public void FlashDamage()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        Color color = damagedUI.color;

        float tempSprintSpeed = sprintSpeed;
        float tempMoveSpeed = moveSpeed;

        sprintSpeed = 0f;
        moveSpeed = 0f;

        float startAlpha = 0.7f;
        damagedUI.color = new Color(color.r, color.g, color.b, startAlpha);

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / flashDuration);
            damagedUI.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        damagedUI.color = new Color(color.r, color.g, color.b, 0f);

        sprintSpeed = tempSprintSpeed;
        moveSpeed = tempMoveSpeed;
    }

    public int GetHealth()
    {
        return health;
    }

    public Animator GetAnimator()
    {
        return animator;
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

    public bool IsGrounded()
    {
        return grounded;
    }

    private void MovePlayer()
    {
        if (health <= 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 rawDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (rawDirection.sqrMagnitude > 0.01f)
        {
            RaycastHit wallHit;

            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(rayOrigin, rawDirection.normalized, out wallHit, 0.6f, whatIsGround))
            {

                rawDirection = Vector3.ProjectOnPlane(rawDirection, wallHit.normal);
            }
        }

        moveDirection = rawDirection.normalized;

        float currentSpeed = (isSprinting || wasSprinting) ? sprintSpeed : moveSpeed;

        if (grounded)
        {
            rb.linearDamping = 5f;
            rb.AddForce(moveDirection * currentSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.linearDamping = 0f;
            rb.AddForce(moveDirection * currentSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
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

    public void ResetJumpCount()
    {
        jumpCountLeft = jumpCount;
        wasJumping = false;
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
            if (fallDamage > 0)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    animator.SetBool("isLanding", true);
                    animator.Play("Landing", 0, 0f);

                    while (animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                    {
                        yield return null;
                    }

                    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                        yield return null;

                    animator.SetBool("isLanding", false);

                }
                else
                {
                    TakeDamage(fallDamage, true);
                }
                fallDamage = 0;
            }
        }

        //         print("isLanding: " + animator.GetBool("isLanding"));
        // print("AbleToLand: " + animator.GetBool("AbleToLand"));
        // print("isGrounded: " + animator.GetBool("isGrounded"));
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

        while (t < 1.5f)
        {
            t += Time.unscaledDeltaTime * flySpeed;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, t);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRot, t);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(3f);
        playSFX("respawn");

        animator.speed = 1f;
        transform.position = lastPosition;
        health = maxHealth;

        col.height = 2f;
        col.center = new Vector3(0, 0, 0);

        deathUI.SetActive(false);
    }
}