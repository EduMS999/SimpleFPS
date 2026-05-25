using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // Propiedad pública para que cualquier script sepa si el Instakill está activo
    public bool IsInstakillActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Método que activará el Power-Up
    public void ActivateInstakill(float duration)
    {
        StartCoroutine(InstakillRoutine(duration));
    }   

    private IEnumerator InstakillRoutine(float duration)
    {
        IsInstakillActive = true;
        Debug.Log("¡INSTAKILL ACTIVADO!");

        yield return new WaitForSeconds(duration);

        IsInstakillActive = false;
        Debug.Log("Instakill terminado.");
    }
}
