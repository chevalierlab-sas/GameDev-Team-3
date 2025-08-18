using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    private PlayerController playerController;
    public float climbSpeed = 3f;
    private bool isClimbing = false;
    private Rigidbody rb;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Optional: freeze rotation so player doesn't tumble
        rb.freezeRotation = true;

        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");

        if (isClimbing)
        {

            rb.useGravity = false;

            Vector3 climbMovement = new Vector3(0, verticalInput * climbSpeed, 0);
            rb.linearVelocity = climbMovement;

            playerController.playSFX("climb");
        }
        else
        {
            rb.useGravity = true;
            playerController.stopSFX("climb");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Entered Ladder Trigger: " + other.name);
        if (other.CompareTag("Ladder"))
        {
            isClimbing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = false;
        }
    }
}
