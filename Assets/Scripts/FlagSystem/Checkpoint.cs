using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public string checkpointID;

    private void Start()
    {
        if (PlayerPrefs.GetInt(checkpointID, 0) == 1)
        {
            
        }
    }
}