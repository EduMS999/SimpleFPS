using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string firstLevelName = "Level01"; // Nombre exacto de tu primera escena

    /// <summary>
    /// Se llama al pulsar el botón de Jugar.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelName);
    }

    /// <summary>
    /// Se llama al pulsar el botón de Salir.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
