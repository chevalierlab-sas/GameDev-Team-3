using UnityEngine;
using System.Collections.Generic;

public class CheckpointInstantiate : MonoBehaviour
{
    public GameObject flagGrayPrefab; // Prefab for the flag uncheckpointed
    public GameObject flagGreenPrefab; // Prefab for the flag checkpointed
    public List<GameObject> listofFlagsPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject flagPosition in listofFlagsPosition)
        {
            GameObject flag;
            if (PlayerPrefs.GetInt(flagPosition.name, 0) == 1)
            {
                flag = Instantiate(flagGreenPrefab, flagPosition.transform.position, Quaternion.identity);
            }
            else
            {
                flag = Instantiate(flagGrayPrefab, flagPosition.transform.position, Quaternion.identity);
            }

            flag.transform.rotation = flagPosition.transform.rotation;

            Checkpoint checkpointScript = flag.GetComponent<Checkpoint>();
            
            if (checkpointScript != null)
            {
                checkpointScript.checkpointID = flagPosition.name;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
