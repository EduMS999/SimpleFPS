using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class AmmoDisplay : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private WeaponManager weaponManager;
    private WeaponController currentSubscribedWeapon;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        /*
        // En lugar de buscar un arma fija, buscamos el gestor de armas del jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            weaponManager = player.GetComponent<WeaponManager>();
        }
        */
    }

    // El arma llama aqui a traves del evento
    private void UpdateAmmoUI()
    {
        if (currentSubscribedWeapon == null) return;

        if (currentSubscribedWeapon.GetIsReloading())
        {
            textMesh.text = "RECARGANDO...";
        }
        else
        {
            textMesh.text = $"{currentSubscribedWeapon.GetCurrentAmmo()} / {currentSubscribedWeapon.GetCurrentReserveAmmo()}";
        }
    }

    public void UpdateSubscription()
    {
        if (weaponManager == null)
            weaponManager = GameObject.FindWithTag("Player")?.GetComponent<WeaponManager>();

        if (weaponManager == null) return;

        if (currentSubscribedWeapon != null)
            currentSubscribedWeapon.OnAmmoChanged -= UpdateAmmoUI;

        currentSubscribedWeapon = weaponManager.GetActiveWeapon();

        if (currentSubscribedWeapon != null)
        {
            currentSubscribedWeapon.OnAmmoChanged += UpdateAmmoUI;
            UpdateAmmoUI();
        }
    }


    private void OnDestroy()
    {
        // Limpieza para evitar errores al cerrar el juego
        if (currentSubscribedWeapon != null)
            currentSubscribedWeapon.OnAmmoChanged -= UpdateAmmoUI;
    }
}
