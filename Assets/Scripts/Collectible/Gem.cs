using UnityEngine;

public class Gem : MonoBehaviour
{
    public string gemID;

    private void Start()
    {
        if (PlayerPrefs.GetInt(gemID, 0) == 1)
        {
            Destroy(gameObject);
        }
    }
}