using UnityEngine;

/// <summary>
/// Este componente aplica daño periódico a cualquier objeto con un HealthSystem 
/// que entre en su área (Trigger) y pertenezca a una capa específica.
/// </summary>
public class HazardZone : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private DamageType damageType;      // El tipo de daño (ej. Fuego, Veneno) para efectos o resistencias.
    [SerializeField] private float damageAmount = 10f;    // Cuánta vida restamos en cada "tick".
    [SerializeField] private float damageInterval = 1f;  // Tiempo de espera entre cada golpe de daño (en segundos).

    [Header("Detección")]
    [SerializeField] private LayerMask playerLayer;      // Filtro para que el área solo afecte a objetos en capas específicas (ej. "Player").

    private float nextDamageTime; // Variable interna para controlar el temporizador de daño.

    /// <summary>
    /// OnTriggerStay se ejecuta en cada frame de las físicas mientras un Collider esté dentro del Trigger.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // 1. FILTRADO POR CAPA (Layer Mask)
        // Convertimos la capa del objeto que entró (other.layer) en un bitmask usando desplazamiento de bits (1 << layer).
        // Luego usamos el operador AND (&) para ver si esa capa está "encendida" en nuestra máscara playerLayer.
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // 2. OBTENCIÓN DEL COMPONENTE (HealthSystem)
            // TryGetComponent es más eficiente que GetComponent porque evita la asignación de memoria si el componente no existe.
            if (other.TryGetComponent<HealthSystem>(out HealthSystem health))
            {
                // 3. CONTROL DEL TIEMPO (Cooldown de Daño)
                // Comprobamos si el tiempo actual del juego ha alcanzado o superado el momento del siguiente daño.
                if (Time.time >= nextDamageTime)
                {
                    // 4. APLICACIÓN DEL DAÑO
                    health.TakeDamage(damageAmount, damageType);

                    // Programamos cuándo será el próximo daño sumando el intervalo al tiempo actual.
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}

/* Conceptos:
La Magia de las LayerMask (Operaciones de Bits)
Este es un punto que suele confundir. Explícales que Unity guarda las capas como una lista de 32 interruptores (bits).
(1 << other.gameObject.layer) es como decir: "Crea un interruptor que solo tenga encendida la posición de la capa de este objeto".
Al compararlo con playerLayer.value usando el símbolo &, el resultado solo será distinto de 0 si ambos tienen ese mismo interruptor 
encendido. Es una forma extremadamente rápida (a nivel de procesador) de filtrar colisiones.
*/