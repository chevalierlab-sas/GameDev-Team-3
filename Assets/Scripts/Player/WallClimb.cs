using UnityEngine;

public class WallClimb : MonoBehaviour
{
    private PlayerController playerController;

    [Header("References")]
    private Rigidbody rb;
    public Transform orientation; // Where the player is facing
    public Transform head; // Position of your camera or top of head

    [Header("Climb Settings")]
    public float climbSpeed = 3f;
    public float climbCheckDistance = 1f;
    public float climbUpCheckHeight = 1.5f;
    public LayerMask climbableLayer;
    public int maxClimb = 2;

    [Header("Mantle Settings")]
    public float mantleUpForce = 5f;
    public float mantleForwardForce = 2f;

    private bool isClimbing;
    private bool readyToMantle;

    private float climbStartY;
    private float maxClimbHeight = 5f;
    private int climbCount;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (playerController.GetAnimator().GetBool("isFalling") || playerController.GetAnimator().GetBool("isDead"))
        {
            isClimbing = false; 
            return;
        }

        CheckForClimb();

        if (isClimbing && climbCount > 0)
        {
            // If we've climbed too far, stop climbing
            if (Mathf.Abs(head.position.y - climbStartY) >= maxClimbHeight)
            {
                ExitClimb();
                return;
            }

            // Climb only while holding Space
            if (Input.GetKey(KeyCode.Space))
            {
                Climb();
            }
            else
            {
                ExitClimb();
            }
        }

        if (IsGrounded())
        {
            climbCount = maxClimb; // Reset climb count when grounded
        }
    }

    private void Climb()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
        rb.AddForce(orientation.forward * climbSpeed, ForceMode.VelocityChange);
        climbCount--;

        isClimbing = false; // Reset climbing state to allow for next climb
        print("Climbing... Remaining Climb Count: " + climbCount);
        playerController.playSFX("climb");
    }

    public void ResetClimbCount()
    {
        climbCount = maxClimb;
    }

    bool IsGrounded()
    {
        bool grounded = playerController.IsGrounded();
        return grounded;
    }

    private void CheckForClimb()
    {
        Vector3 chestPos = transform.position + Vector3.up * 0.9f;
        Debug.DrawRay(chestPos, orientation.forward * climbCheckDistance, Color.red);

        if (Physics.Raycast(chestPos, orientation.forward, out RaycastHit hit, climbCheckDistance, climbableLayer))
        {
            // Only start climb if pressing Space
            if (Input.GetKeyDown(KeyCode.Space) && !isClimbing)
            {
                // Check for top clearance
                if (!Physics.Raycast(head.position + orientation.forward * 0.3f, Vector3.up, 1f, climbableLayer))
                {
                    readyToMantle = true;
                }
                else
                {
                    readyToMantle = false;
                }

                StartClimb();
            }
        }
        else
        {
            ExitClimb();
        }
    }

    private void StartClimb()
    {
        isClimbing = true;
        climbStartY = head.position.y;
    }

    private void ExitClimb()
    {
        if (isClimbing)
        {
            isClimbing = false;
            rb.useGravity = true;
        }
    }

    private void MantleUp()
    {
        ExitClimb();
        rb.AddForce(Vector3.up * mantleUpForce, ForceMode.VelocityChange);
        rb.AddForce(orientation.forward * mantleForwardForce, ForceMode.VelocityChange);
    }
}
