using UnityEngine;

public class CheckpointCollect : MonoBehaviour
{
    public GameObject flagGreenPrefab; // Prefab for the flag checkpointed
    private PlayerController playerController;
    public int checkpointCount = 0;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        checkpointCount = PlayerPrefs.GetInt("checkpointCount", 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint checkpointScript = other.GetComponent<Checkpoint>();
            if (checkpointScript != null)
            {
                PlayerPrefs.SetInt(checkpointScript.checkpointID, 1);
                PlayerPrefs.SetInt("checkpointCount", checkpointCount + 1);
                PlayerPrefs.Save();

                checkpointCount++;
                Debug.Log("Checkpoints collected: " + checkpointCount);

                playerController.playSFX("item");
                Destroy(other.gameObject);

                // Instantiate the checkpoint flag
                GameObject flag = Instantiate(flagGreenPrefab, other.transform.position, Quaternion.identity);
                flag.transform.rotation = other.transform.rotation;
            }
        }
    }
}