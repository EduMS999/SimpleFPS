using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "FPS Lab/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identificación")]
    public string weaponName;

    [Header("Estadísticas de Disparo")]
    public float damage = 10f;
    public float range = 50f;
    public float fireRate = 0.2f; // Tiempo entre disparos

    // --- NUEVO CAMPO PARA EL MODO DE DISPARO ---
    [Tooltip("Si está marcado, el arma dispara continuamente al mantener pulsado. Si no, requiere un clic por bala.")]
    public bool isAutomatic = true;

    // --- NUEVA VARIABLE PARA FUERZA FÍSICA ---
    [Tooltip("Fuerza de impacto aplicada a objetos con Rigidbody")]
    public float impactForce = 150f;

    [Header("Munición")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Tooltip("Munición máxima que el jugador puede llevar en reserva para este arma.")]
    public int maxReserveAmmo = 30;
    [Tooltip("El tipo de munición que utiliza este arma.")]
    public AmmoType ammoType;

    [Header("Efectos Visuales")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public AudioClip shootSound;
}
