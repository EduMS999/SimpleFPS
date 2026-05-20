using UnityEngine;

public class PW_Points : MonoBehaviour
{
    public int pointsToAdd = 500;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PointsSystem.Instance.AddPoints(pointsToAdd);   // Añade los puntos al coger el powerup
            Debug.Log($"{pointsToAdd} Puntos adquiridos");
            Destroy(gameObject); // Se destruye para que no moleste mas
        }
       
    }
}
