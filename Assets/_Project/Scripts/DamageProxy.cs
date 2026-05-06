using UnityEngine;

public class DamageProxy : MonoBehaviour
{
    [SerializeField] private HealthSystem mainHealthSystem;

    private void Start()
    {
        // Si no lo asignamos en el inspector, lo busca en el objeto padre automáticamente
        if (mainHealthSystem == null)
        {
            mainHealthSystem = GetComponentInParent<HealthSystem>();
        }
    }

    // Este método lo llamará el Raycast de tu arma al impactar
    public void TakeDamage(float amount, DamageType type = null)
    {
        if (mainHealthSystem != null)
        {
            // Le enviamos el daño al HealthSystem del padre
            mainHealthSystem.TakeDamage(amount, type);
        }
    }
}
