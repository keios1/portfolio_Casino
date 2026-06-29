using UnityEngine;

public class CoinTossDebugStarter : MonoBehaviour
{
    public CoinTossManager toss;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (toss == null) toss = FindObjectOfType<CoinTossManager>();

            toss.Open((CoinTossManager.CoinFace face) =>
            {
                Debug.Log("COIN RESULT = " + face);
            });
        }
    }
}
