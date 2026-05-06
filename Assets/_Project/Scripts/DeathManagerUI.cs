using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena
using UnityEngine.UI; // Necesario para interactuar con Componentes de UI (Button)
using System.Collections; // Necesario para usar Corrutinas (IEnumerator)

/// <summary>
/// Este script gestiona la pantalla de "Game Over".
/// Se encarga de escuchar cuando el jugador muere y mostrar el menú tras una breve espera.
/// </summary>
public class DeathManagerUI : MonoBehaviour
{
    [Header("Configuración de UI")]
    [Tooltip("El panel que contiene los botones y textos de muerte")]
    [SerializeField] private GameObject deathScreenPanel;

    [Tooltip("Botón que el jugador presionará para reintentar")]
    [SerializeField] private Button restartButton;

    [Tooltip("Segundos que esperamos antes de mostrar la UI (para ver la animación de caída)")]
    [SerializeField] private float delayBeforeShow = 3.0f;

    // Referencia al sistema de salud para saber cuándo muere el jugador
    private HealthSystem playerHealth;

    private void Start()
    {
        SetupPlayerConnection();

        // Programamos el botón: "Cuando hagan clic, ejecuta la función RestartGame"
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        // Nos aseguramos de que la pantalla de muerte esté oculta al empezar
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);
    }

    /// <summary>
    /// Busca al jugador en la escena y se suscribe a su evento de muerte.
    /// </summary>
    private void SetupPlayerConnection()
    {
        // Buscamos el objeto con el Tag "Player" (Desacoplamiento)
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();

            if (playerHealth != null)
            {
                // SUSCRIPCIÓN AL EVENTO: 
                // "Oye HealthSystem, cuando lances OnDeath, avísame ejecutando HandleDeath"
                playerHealth.OnDeath += HandleDeath;
            }
        }
    }

    /// <summary>
    /// Este método se dispara automáticamente cuando el evento OnDeath ocurre.
    /// </summary>
    private void HandleDeath()
    {
        // Usamos una Corrutina para que la espera no bloquee el resto del juego
        StartCoroutine(DeathSequenceRoutine());
    }

    /// <summary>
    /// Secuencia temporal: Espera -> Muestra UI -> Libera el Ratón
    /// </summary>
    private IEnumerator DeathSequenceRoutine()
    {
        // 1. Pausa la ejecución de esta función por 'delayBeforeShow' segundos
        yield return new WaitForSeconds(delayBeforeShow);

        // 2. Activamos visualmente el panel de muerte
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(true);

        // 3. Liberamos el cursor para que el alumno pueda hacer clic en el botón
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Función para el botón de reinicio.
    /// </summary>
    public void RestartGame()
    {
        // Si el juego se pausó, lo devolvemos a velocidad normal
        Time.timeScale = 1f;

        // Cargamos de nuevo la escena actual (resetea todo el nivel)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        // LIMPIEZA: Es vital desuscribirse de los eventos al destruir el objeto
        // para evitar errores de "Memory Leaks" o referencias nulas.
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandleDeath;
        }
    }
}
/* Conceptos:
Gestión del Cursor:
En los FPS, el cursor suele estar bloqueado al centro (Locked). Es fundamental recordarles que al morir, 
debemos "liberar" el ratón para que el usuario pueda interactuar con los botones de la interfaz.
*/