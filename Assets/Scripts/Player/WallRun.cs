using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class WallRun : MonoBehaviour
{
    private PlayerController playerController;
    private WallClimb wallClimb;

    [Header("General")]
    public LayerMask wallMask;
    public Transform cam;

    [Header("Detection")]
    public float wallCheckDistance = 0.8f;
    public float minWallRunHeight = 3f; // min height above ground to allow wallrun

    [Header("Movement")]
    public float wallRunSpeed = 8f;
    public float wallStickForce = 15f;   
    public float wallDownSpeed = 1.2f;    
    public int maxWallRun = 3;

    [Header("UI")]
    public GameObject WallRunUI;
    private GameObject Indicator;
    private TextMeshProUGUI WallRunLeft;

    [Header("Jump")]
    public float jumpUpForce = 6f;
    public float jumpOffForce = 6f;
    public float forwardBoost = 5f;

    [Header("Limits")]
    public float maxWallRunTime = 1.8f;

    Rigidbody rb;
    CapsuleCollider col;

    bool isWallRunning = false;
    Vector3 currentWallNormal;
    float wallRunTimer = 0f;
    bool jumpRequested = false;
    int wallRunCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        playerController = GetComponent<PlayerController>();
        wallClimb = GetComponent<WallClimb>();
        wallRunCount = maxWallRun + 1;

        Indicator = WallRunUI.transform.Find("Indicator").gameObject;
        WallRunLeft = WallRunUI.transform.Find("WallRunLeft").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // capture jump input here and handle in FixedUpdate
        if (Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;
    }

    void FixedUpdate()
    {
        AnimatorStateInfo stateInfo = playerController.GetAnimator().GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Falling") || stateInfo.IsName("Dead"))
        {
            StopWallRun();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * (col.height * 0.5f - col.radius * 0.5f);

        RaycastHit hitRight, hitLeft;
        bool right = Physics.Raycast(origin, transform.right, out hitRight, wallCheckDistance, wallMask);
        bool left = Physics.Raycast(origin, -transform.right, out hitLeft, wallCheckDistance, wallMask);

        bool canWallRun = (right || left) && !IsGrounded() && transform.position.y > minWallRunHeight;

        if (IsGrounded())
        {
            wallRunTimer = 0f;
            isWallRunning = false;
            wallRunCount = maxWallRun;

            WallRunUI.SetActive(false);
        }

        if (canWallRun && wallRunCount > 0)
        {
            currentWallNormal = right ? hitRight.normal : hitLeft.normal;
            StartWallRun();

            WallRunUI.SetActive(true);
        }
        else
        {
            StopWallRun();
        }

        WallRunLeft.text = (wallRunCount <= 0 ? 0 : wallRunCount - 1).ToString() + " Wall Runs Left";

        if (isWallRunning)
        {
            rb.AddForce(-currentWallNormal * wallStickForce, ForceMode.Acceleration);

            Vector3 wallForward = Vector3.Cross(currentWallNormal, Vector3.up).normalized;
            if (Vector3.Dot(wallForward, transform.forward) < 0) wallForward = -wallForward;

            Vector3 newVel = wallForward * wallRunSpeed;
            newVel.y = -wallDownSpeed;
            rb.linearVelocity = newVel;

            float horizontalSpeed = rb.linearVelocity.magnitude;
            Vector3 forwardDir = transform.forward * horizontalSpeed;

            if (jumpRequested)
            {
                Vector3 jumpDir = Vector3.up * jumpUpForce + currentWallNormal * jumpOffForce;
                float jumpStrength = jumpUpForce + jumpOffForce;
                rb.linearVelocity = forwardDir + jumpDir + transform.forward * forwardBoost;
                StopWallRun();
            }

            wallRunTimer += Time.fixedDeltaTime;
            Indicator.transform.localScale = new Vector3(1f - (wallRunTimer / maxWallRunTime), 1f, 1f);

            if (wallRunTimer >= maxWallRunTime)
            {
                wallRunCount = 0;
                StopWallRun();
            }
        }

        jumpRequested = false;
    }

    void StartWallRun()
    {
        if (isWallRunning) return;
        wallRunCount--;
        playerController.ResetJumpCount();
        wallClimb.ResetClimbCount();

        isWallRunning = true;
        wallRunTimer = 0f;
        rb.useGravity = false;

        Indicator.SetActive(true);
    }

    void StopWallRun()
    {
        if (!isWallRunning) return;
        isWallRunning = false;
        rb.useGravity = true;

        Indicator.SetActive(false);
    }

    bool IsGrounded()
    {
        bool grounded = playerController.IsGrounded();
        return grounded;
    }

    public bool IsWallRunning() => isWallRunning;

    void OnDrawGizmosSelected()
    {
        if (col == null) return;
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * (col.height * 0.5f - col.radius * 0.5f);
        Gizmos.DrawLine(origin, origin + transform.right * wallCheckDistance);
        Gizmos.DrawLine(origin, origin - transform.right * wallCheckDistance);
    }
}
