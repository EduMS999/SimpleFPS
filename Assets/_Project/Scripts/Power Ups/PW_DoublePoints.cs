using UnityEngine;

public class PW_DoublePoints : MonoBehaviour
{
    [SerializeField] private float duration = 30f; // Duración del efecto en segundos
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo toca el jugador
        if (other.CompareTag("Player"))
        {
            // Activamos el efecto a través del PowerUpManager global
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivateDoublePoints(duration);
            }

            // Efectos sonoros de recolección
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destruimos el item del mapa
            Destroy(gameObject);
        }

        // Lógica de los puntos dobles en la funciones de PointsSystem.cs
    }
}
