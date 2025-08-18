using UnityEngine;
using System.Collections.Generic;

public class GemInstantiate : MonoBehaviour
{
    public GameObject gemsPrefab; // Prefab for the gem
    public List<GameObject> listofGemsPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject gemPosition in listofGemsPosition)
        {
            GameObject gem = Instantiate(gemsPrefab, gemPosition.transform.position, Quaternion.identity);
            gem.transform.rotation = gemPosition.transform.rotation;
            
            Gem gemScript = gem.GetComponent<Gem>();
            if (gemScript != null)
            {
                gemScript.gemID = gemPosition.name;
                if (PlayerPrefs.GetInt(gemScript.gemID, 0) == 1)
                {
                    Destroy(gem);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
