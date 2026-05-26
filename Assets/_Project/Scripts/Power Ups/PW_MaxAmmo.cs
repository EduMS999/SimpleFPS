using UnityEngine;

public class PW_MaxAmmo : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager wm = other.GetComponent<WeaponManager>();
            if (wm != null)
            {
                WeaponController[] inv = wm.GetAllWeapons();
                bool success = false;

                foreach (WeaponController weapon in inv)
                {
                    if (weapon != null)
                    {
                        // Si AddAmmo devuelve true, es que el arma acepta las balas
                        if (weapon.AddMaxAmmo())
                        {
                            success = true;
                        }
                    }
                }

                if (success)
                {
                    Debug.Log("Munici�n recogida con �xito.");
                    Destroy(gameObject);
                }
            }
        }

        // Efectos sonoros de recolección
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}
