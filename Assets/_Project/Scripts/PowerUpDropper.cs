using UnityEngine;

public class PowerUpDropper : MonoBehaviour
{
    [Header("Power Ups")]
    public GameObject[] powerUps;
    [Range(0, 100)]
    public int powerUpSpawnProbability = 10;

    
    void Awake()
    {
        HealthSystem healthSystem = gameObject.GetComponentInChildren<HealthSystem>();
        healthSystem.OnPowerUp += SpawnPowerUp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnPowerUp(Transform zombiePosition)
    {
        int randomNumber = Random.Range(0, 100);

        if (randomNumber > powerUpSpawnProbability)
        {
            int randomIndex = Random.Range(0, powerUps.Length);
            Instantiate(powerUps[randomIndex], zombiePosition.position, Quaternion.identity);
        }
    }
}
