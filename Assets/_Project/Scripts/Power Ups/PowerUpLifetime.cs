using UnityEngine;

public class PowerUpLifetime : MonoBehaviour
{
    [Header("Tiempo de Vida")]
    [SerializeField] private float lifetime = 15f; //
    [SerializeField, Range(0f, 1f)] private float startBlinkingPercentage = 0.3f; // Empieza a parpadear al quedar el 30% del tiempo

    [Header("Frecuencia de Parpadeo")]
    [SerializeField] private float initialBlinkDelay = 0.4f; // Parpadeo lento al principio
    [SerializeField] private float minBlinkDelay = 0.05f;    // Parpadeo ultra rápido al final

    private Renderer objectRenderer;
    private float timeRemaining;
    private float blinkTimer;
    private bool isRendererEnabled = true;

    void Start()
    {
        timeRemaining = lifetime;

        // Buscamos el renderizador visual del objeto (puede estar en el padre o en los hijos)
        objectRenderer = GetComponentInChildren<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogWarning($"[PowerUpLifetime] No se encontró un Renderer en {gameObject.name}. El parpadeo visual no funcionará.");
        }
    }

    void Update()
    {
        // Consumimos el tiempo frame a frame
        timeRemaining -= Time.deltaTime;

        // Si se acaba el tiempo, destruimos el power-up
        if (timeRemaining <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // Calculamos qué porcentaje de tiempo queda (de 1.0 a 0.0)
        float currentPercentage = timeRemaining / lifetime;

        // Si el tiempo restante es menor que el porcentaje configurado, empieza el parpadeo
        if (currentPercentage <= startBlinkingPercentage && objectRenderer != null)
        {
            ProcessBlinking(currentPercentage);
        }
    }

    private void ProcessBlinking(float currentPercentage)
    {
        blinkTimer -= Time.deltaTime;

        if (blinkTimer <= 0)
        {
            // Invertimos el estado de visibilidad del objeto
            isRendererEnabled = !isRendererEnabled;
            objectRenderer.enabled = isRendererEnabled;

            // A menos porcentaje de tiempo, más nos acercamos a 'minBlinkDelay'.
            float t = currentPercentage / startBlinkingPercentage; // Va de 1 (inicio del parpadeo) a 0 (destrucción)
            blinkTimer = Mathf.Lerp(minBlinkDelay, initialBlinkDelay, t);
        }
    }
}