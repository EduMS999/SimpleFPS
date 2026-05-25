using UnityEngine;

public class PW_Instakill : MonoBehaviour
{
    [SerializeField] private float duration = 30f; // Duración del efecto en segundos
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo toca el jugador
        if (other.CompareTag("Player"))
        {
            // Activamos el efecto a través del GameManager global
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivateInstakill(duration);
            }

            // Efectos visuales/sonoros de recolección
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destruimos el item del mapa
            Destroy(gameObject);
        }
    }
}
