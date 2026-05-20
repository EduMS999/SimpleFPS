using UnityEngine;

public class PowerUpDropper : MonoBehaviour
{
    [Header("Power Ups")]
    public GameObject[] powerUps;
    [Range(0, 100)]
    public int powerUpSpawnProbability = 10;

    private HealthSystem healthSystem; // Guardamos la referencia para desuscribirnos luego

    void Awake()
    {
        healthSystem = gameObject.GetComponentInChildren<HealthSystem>();
    }

    private void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnPowerUp += SpawnPowerUp;
        }
    }

    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnPowerUp -= SpawnPowerUp;
        }
    }

    void SpawnPowerUp(Transform zombiePosition)
    {
        int randomNumber = Random.Range(0, 100);

        if (randomNumber <= powerUpSpawnProbability)
        {
            int randomIndex = Random.Range(0, powerUps.Length);
            Instantiate(powerUps[randomIndex], zombiePosition.position, powerUps[randomIndex].transform.rotation);
        }
    }
}
