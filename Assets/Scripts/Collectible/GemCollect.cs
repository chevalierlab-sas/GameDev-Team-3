using UnityEngine;

public class GemCollect : MonoBehaviour
{
    private PlayerController playerController;
    public int gemCount = 0;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        gemCount = PlayerPrefs.GetInt("gemCount", 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gems"))
        {
            Gem gemScript = other.GetComponent<Gem>();
            if (gemScript != null)
            {
                PlayerPrefs.SetInt(gemScript.gemID, 1);
                PlayerPrefs.SetInt("gemCount", gemCount + 1);
                PlayerPrefs.Save();

                gemCount++;
                Debug.Log("Gems collected: " + gemCount);

                playerController.playSFX("item");
                Destroy(other.gameObject);
            }
        }
    }
}