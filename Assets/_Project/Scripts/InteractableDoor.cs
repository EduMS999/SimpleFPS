using UnityEngine;
using System.Collections.Generic; // Necesario para usar List<>

/// <summary>
/// Clase que gestiona el comportamiento de una puerta interactuable.
/// Implementa IInteractable para que el sistema de interacción del jugador la reconozca.
/// </summary>
public class InteractableDoor : MonoBehaviour, IInteractable
{
    // --- SECCIÓN: CONFIGURACIÓN Y ESTADO ---
    [Header("Configuración de Seguridad")]
    [SerializeField] private bool isLocked = true; // Define si la puerta inicia bloqueada
    [SerializeField] private List<KeyType> requiredKeys; // Lista de llaves (Enum) necesarias para abrirla

    [Header("Prompts (Textos de Interfaz)")]
    [SerializeField] private string openPrompt = "Abrir Puerta";
    [SerializeField] private string closePrompt = "Cerrar Puerta";
    [SerializeField] private string lockedPrompt = "Faltan llaves: ";

    [Header("Visual Feedback")]
    [SerializeField] private MeshRenderer statusLightRenderer; // Referencia a la luz/indicador de la puerta
    [SerializeField] private Material unlockedMaterial; // Material que se aplica al desbloquearse

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    // --- PROPIEDADES ---
    // Esta propiedad devuelve el texto dinámico que debe ver el jugador al mirar la puerta
    public string InteractionPrompt
    {
        get
        {
            if (_isOpen) return closePrompt; // Si está abierta, sugiere "Cerrar"

            // Si está bloqueada, concatena el aviso con la lista de llaves pendientes
            if (isLocked) return lockedPrompt + string.Join(", ", requiredKeys);

            return openPrompt; // Si no está bloqueada ni abierta, sugiere "Abrir"
        }
    }

    // --- VARIABLES PRIVADAS DE CONTROL ---
    private bool _isOpen = false;        // Estado actual de la puerta
    private Quaternion _targetRotation;  // Hacia dónde debe rotar la puerta
    private Quaternion _closedRotation;  // La rotación original (cerrada)

    [SerializeField] private float openAngle = 90f; // Ángulo de apertura en grados
    [SerializeField] private float smoothing = 5f;  // Velocidad de la transición (Interpolación)

    void Start()
    {
        // Guardamos la rotación inicial del objeto al comenzar el juego
        _closedRotation = transform.localRotation;
        _targetRotation = _closedRotation;
    }

    void Update()
    {
        // Slerp (Spherical Linear Interpolation) suaviza el movimiento entre la rotación actual y la deseada
        // Esto evita que la puerta "teletransporte" y en su lugar rote suavemente cada frame
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            _targetRotation,
            Time.deltaTime * smoothing
        );
    }

    /// <summary>
    /// Método principal que se ejecuta cuando el jugador presiona la tecla de interacción.
    /// </summary>
    public void Interact()
    {
        // 1. Verificación de Seguridad (Bloqueo)
        if (isLocked)
        {
            // Buscamos el inventario en la escena (usando el método optimizado de Unity 6)
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

            // Si el jugador tiene todas las llaves requeridas...
            if (inventory != null && inventory.HasAllKeys(requiredKeys))
            {
                isLocked = false; // Desbloqueamos permanentemente

                // Feedback visual: cambiamos el color de la luz/panel si existe
                if (statusLightRenderer != null && unlockedMaterial != null)
                {
                    statusLightRenderer.material = unlockedMaterial;
                }
                Debug.Log("Puerta desbloqueada.");
            }
            else
            {
                // Si no tiene las llaves, suena el error y detenemos la ejecución del método
                if (audioSource && lockedSound) audioSource.PlayOneShot(lockedSound);
                return;
            }
        }

        // 2. Lógica de Movimiento (Apertura/Cierre)
        // Invertimos el estado de la variable booleana
        _isOpen = !_isOpen;

        // Feedback sonoro: elegimos el clip según si se abre o se cierra
        AudioClip clipToPlay = _isOpen ? openSound : closeSound;
        if (audioSource && clipToPlay) audioSource.PlayOneShot(clipToPlay);

        // Calculamos la nueva rotación objetivo
        // Si se abre, multiplicamos la rotación cerrada por un ángulo en el eje Y
        _targetRotation = _isOpen
            ? _closedRotation * Quaternion.Euler(0, openAngle, 0)
            : _closedRotation;
    }
}