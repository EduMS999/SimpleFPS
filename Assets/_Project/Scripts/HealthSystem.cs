using UnityEngine;
using System;
using System.Collections; // Necesario para usar 'Action', que son nuestros eventos

public class HealthSystem : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f; // La vida máxima que puede tener el personaje
    public float currentHealth;    // La vida que tiene en el momento exacto

    [Header("Regeneración de Salud")]
    [SerializeField] private bool canRegenerate = true;        // ¿Este personaje puede regenerar vida?
    [SerializeField] private float healthPerSecond = 10f;      // Cuánta vida recupera por segundo
    [SerializeField] private float delayBeforeRegen = 4f;      // Cuántos segundos espera tras recibir daño para empezar a curar
    [SerializeField] private float regenTickRate = 0.2f;       // Cada cuánto tiempo (en segundos) aplica la curación (menor número = más fluido)

    private Coroutine regenCoroutine; // Guarda la referencia de la corrutina activa

    [Header("Audio")]
    // Referencia al componente que reproduce sonido y la lista de clips disponibles
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] damageSounds;

    // --- EVENTOS (La clave del desacoplamiento) ---
    // OnHealthChanged: Avisa a otros (como la barra de vida) cuánto cambió la salud.
    // OnDeath: Avisa a otros (como el Game Over) que el personaje murió.
    public event Action<float, float> OnHealthChanged; // Envía (vidaActual, vidaMáxima)
    public event Action OnDeath;
    public event Action<Transform> OnPowerUp;
        
    private void Awake()
    {
        // Al iniciar, nos aseguramos de que el personaje empiece con la vida llena.
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Función principal para recibir daño.
    /// </summary>
    /// <param name="amount">Cantidad base de daño.</param>
    /// <param name="type">Opcional: Tipo de daño para aplicar multiplicadores.</param>
    public void TakeDamage(float amount, DamageType type = null)
    {
        // Si ya está muerto, no procesamos más daño.
        if (currentHealth <= 0) return;

        // Lógica de Multiplicador: Si el daño viene con un "tipo" (ej. fuego, explosión),
        // multiplicamos el daño base. Si no, usamos el valor original.
        float finalDamage = type != null ? amount * type.damageMultiplier : amount;

        // Restamos la vida y usamos Clamp para que nunca sea menor a 0 ni mayor al máximo.
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // DISPARAR EVENTO: "?." significa "si alguien está escuchando, avísales".
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Efecto visual/sonoro inmediato
        PlayDamageSound();

        if(!canRegenerate)
            PointsSystem.Instance.AddPointsPerBullet(); // Solo sumamos puntos por cada bala que impacta en el enemigo, evitando sumar si golpean al jugador

        // --- LÓGICA DE REGENERACIÓN AL RECIBIR DAÑO ---
        if (currentHealth > 0 && canRegenerate)
        {
            // Si ya se estaba regenerando o esperando para curarse, cancelamos esa cuenta atrás
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
            }
            // Iniciamos una nueva cuenta atrás de regeneración limpia
            regenCoroutine = StartCoroutine(RegenerateHealthRoutine());
        }

        // Verificamos si la vida llegó a cero para ejecutar la muerte.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Corrutina que gestiona la espera y la curación constante en el tiempo.
    /// </summary>
    private IEnumerator RegenerateHealthRoutine()
    {
        // Esperamos el tiempo de seguridad configurado tras recibir el último golpe
        yield return new WaitForSeconds(delayBeforeRegen);

        // Mientras la vida no esté al máximo, curamos poco a poco
        while (currentHealth < maxHealth)
        {
            // Calculamos la curación proporcional al tick rate (ej: 10 de vida * 0.2 segundos = 2 de vida por tick)
            currentHealth += healthPerSecond * regenTickRate;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // Avisamos a la interfaz (barra de vida) de que la salud está subiendo
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Esperamos una fracción de segundo antes del siguiente incremento
            yield return new WaitForSeconds(regenTickRate);
        }

        // Al terminar de curarse del todo, vaciamos la variable de control
        regenCoroutine = null;
    }

    /// <summary>
    /// Función para curar al personaje (usada por los Pickups).
    /// </summary>
    public void Heal(float amount)
    {
        // MoveTowards incrementa el valor asegurándose de no pasarse del máximo.
        currentHealth = Mathf.MoveTowards(currentHealth, maxHealth, amount);

        // Avisamos a la UI que la vida subió.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // Log para depuración en la consola de Unity.
        Debug.Log($"{gameObject.name} ha muerto.");

        PointsSystem.Instance.AddPointsPerDeath();

        // Avisamos a todos los sistemas interesados (Cámara, UI, GameManager) que morimos.
        OnDeath?.Invoke();
        OnPowerUp?.Invoke(gameObject.transform); // Evento necesario para dictaminar si el zombie suelta o no powerUp
    }

    private void PlayDamageSound()
    {
        // Verificación de seguridad: ¿Tenemos un AudioSource y sonidos asignados?
        if (audioSource != null && damageSounds.Length > 0)
        {
            // Seleccionamos un sonido aleatorio del array para que no sea repetitivo.
            AudioClip clip = damageSounds[UnityEngine.Random.Range(0, damageSounds.Length)];

            // Variación de tono (Pitch): Hace que el mismo sonido suene ligeramente 
            // más grave o agudo cada vez, dando realismo.
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

            // PlayOneShot permite que el sonido suene sin interrumpir otros sonidos previos.
            audioSource.PlayOneShot(clip);
        }
    }
}
