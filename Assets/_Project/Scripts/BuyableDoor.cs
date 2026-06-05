using System.Collections; // OBLIGATORIO para usar Corrutinas
using System.Collections.Generic;
using UnityEngine;

public class BuyableDoor : MonoBehaviour, IInteractable
{
    // --- SECCIÓN: CONFIGURACIÓN Y ESTADO ---
    [Header("Configuración de Seguridad")]
    [SerializeField] private bool isLocked = true; // Define si la puerta inicia bloqueada
    [SerializeField] private int pointsRequired = 1500; // Puntos necesarios para desbloquear la puerta

    [Header("Configuración de Movimiento (Escombros)")]
    [SerializeField] private float moveDistance = 5f;    // Cuántos metros va a subir la puerta hacia arriba
    [SerializeField] private float moveSpeed = 3f;       // Velocidad a la que se va a mover
    [SerializeField] private float delayBeforeDestroy = 1f; // Tiempo que espera arriba antes de desaparecer

    [Header("Prompts (Textos de Interfaz)")]
    [SerializeField] private string openPrompt = "Abrir Puerta por X puntos"; //

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; //
    [SerializeField] private AudioClip openSound; //

    private bool _isOpen = false;        // Estado actual de la puerta
    private Collider doorCollider;       // Referencia al colisionador para apagarlo inmediatamente

    private void Awake()
    {
        openPrompt = $"Abrir Puerta por {pointsRequired} puntos"; //
        doorCollider = GetComponent<Collider>(); // Guardamos el collider del objeto
    }

    // --- PROPIEDADES ---
    public string InteractionPrompt
    {
        get
        {
            // Si la puerta ya se está abriendo o está abierta, no mostramos texto
            if (_isOpen || !isLocked) return "";
            return openPrompt; //
        }
    }

    /// <summary>
    /// Método principal que se ejecuta cuando el jugador presiona la tecla de interacción.
    /// </summary>
    public void Interact()
    {
        // Si ya está abierta o abriéndose, ignoramos interacciones repetidas
        if (_isOpen) return;

        // Verificación de Seguridad (Bloqueo)
        if (isLocked) //
        {
            if (PointsSystem.Instance.GetCurrentPoints() >= pointsRequired) //
            {
                // Si el jugador tiene suficientes puntos, desbloqueamos la puerta
                isLocked = false; //
                _isOpen = true;

                PointsSystem.Instance.RemovePoints(pointsRequired); // Restamos los puntos al jugador

                // Desactivamos el Collider al instante para que el jugador pueda pasar corriendo
                // mientras la puerta sube visualmente
                if (doorCollider != null)
                {
                    doorCollider.enabled = false;
                }

                // Reproducir el sonido de apertura
                if (audioSource != null && openSound != null)
                {
                    audioSource.PlayOneShot(openSound);
                }

                // Iniciamos la animación de movimiento y destrucción
                StartCoroutine(OpenDoorRoutine());
            }
            else
            {
                Debug.Log("No tienes suficientes puntos para abrir esta puerta."); //
                return; //
            }
        }
    }

    /// <summary>
    /// Corrutina que desplaza la puerta hacia arriba frame a frame y luego la destruye.
    /// </summary>
    private IEnumerator OpenDoorRoutine()
    {
        // Calculamos la posición exacta a la que debe llegar (Posición actual + metros hacia arriba en el eje Y)
        Vector3 targetPosition = transform.position + (Vector3.up * moveDistance);

        // Bucle que mueve el objeto frame a frame hasta que esté muy cerca del objetivo
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // MoveTowards desplaza la posición de forma lineal y constante sin pasarse del objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Esperamos al siguiente frame de Unity para continuar el bucle
            yield return null;
        }

        // Nos aseguramos de clavar la posición final exacta por estética
        transform.position = targetPosition;

        // Una vez arriba, esperamos el segundo de cortesía configurado
        yield return new WaitForSeconds(delayBeforeDestroy);

        // Desaparece por completo de la escena de forma limpia
        Destroy(gameObject);
    }
}