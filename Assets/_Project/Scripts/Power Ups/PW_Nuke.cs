using UnityEngine;

public class PW_Nuke : MonoBehaviour
{
    [SerializeField] private GameObject mushroomCloud;
    private Vector3 position;
    [SerializeField] private AudioClip pickupSound;

    private void Start()
    {
        position = gameObject.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo toca el jugador
        if (other.CompareTag("Player"))
        {
            // Activamos el efecto a través del PowerUpManager global
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivateNuke(position, mushroomCloud);
            }

            // Efectos sonoros de recolección
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destruimos el item del mapa
            Destroy(gameObject);
        }

    }
}
