using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AmmoPickup : MonoBehaviour
{
    [Header("Configuración de Munición")]
    [Tooltip("El tipo de munición que contiene esta caja.")]
    [SerializeField] private AmmoType ammoType;

    [Tooltip("Cantidad de balas que esta caja añade a la reserva.")]
    [SerializeField] private int ammoAmount = 30;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponent<WeaponManager>();

            if (weaponManager != null)
            {
                WeaponController activeWeapon = weaponManager.GetActiveWeapon();

                // Intentamos darle la munición al arma activa
                if (activeWeapon != null)
                {
                    // Si el arma acepta la munición (mismo calibre y tiene espacio), se destruye la caja
                    if (activeWeapon.AddAmmo(ammoType, ammoAmount))
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        Debug.Log("No puedes recoger esta munición: Calibre incorrecto o reserva llena.");
                    }
                }
            }
        }
    }
}