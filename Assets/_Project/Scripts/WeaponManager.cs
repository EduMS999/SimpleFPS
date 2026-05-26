using UnityEngine;
using UnityEngine.InputSystem; // Necesario para detectar las teclas con el nuevo sistema

public class WeaponManager : MonoBehaviour
{
    [Header("Configuración de Armas")]
    [Tooltip("Arrastra aquí los GameObjects de tus dos armas (hijos del jugador).")]
    [SerializeField] private GameObject[] weapons;
    [Header("Referencias de UI")]
    [Tooltip("Arrastramos aqu� el objeto que tiene el script AmmoDisplay")]
    [SerializeField] private AmmoDisplay ammoDisplay;

    private int currentWeaponIndex = 0;

    void Awake()
    {
        InitializeWeapons();
    }
    private void Start()
    {
        // Al empezar, nos aseguramos de que la UI sepa qu� arma tenemos
        if (ammoDisplay != null)
        {
            ammoDisplay.UpdateSubscription();
        }
    }

    void Update()
    {
        // Verificamos que el teclado esté conectado
        if (Keyboard.current == null) return;

        // Detectamos si se pulsó la tecla 1
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectWeapon(0);
        }
        // Detectamos si se pulsó la tecla 2
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectWeapon(1);
        }
    }

    private void InitializeWeapons()
    {
        if (weapons == null || weapons.Length == 0) return;

        // Al iniciar, activamos solo el arma por defecto y desactivamos las demás
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].SetActive(i == currentWeaponIndex);
            }
        }
    }

    public void SelectWeapon(int index)
    {
        // Validaciones: que el índice sea correcto y no sea el arma que ya tenemos en la mano
        if (weapons == null || index < 0 || index >= weapons.Length || index == currentWeaponIndex) return;
        if (weapons[index] == null) return;

        // Desactivamos el arma actual
        if (weapons[currentWeaponIndex] != null)
        {
            weapons[currentWeaponIndex].SetActive(false);
        }

        // Activamos la nueva arma
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].SetActive(true);

        // --- CAMBIO CLAVE ---
        // Avisamos a la UI de que ahora tiene que escuchar a un arma diferente
        if (ammoDisplay != null)
        {
            ammoDisplay.UpdateSubscription();
        }
        // -----------------------

        Debug.Log($"Arma cambiada a: {weapons[currentWeaponIndex].name}");
    }

    /// <summary>
    /// Devuelve el WeaponController del arma que el jugador tiene activa en este momento.
    /// Esto nos servirá más adelante para los pickups de balas.
    /// </summary>
    public WeaponController GetActiveWeapon()
    {
        if (weapons == null || weapons.Length == 0) return null;
        if (weapons[currentWeaponIndex] == null) return null;

        return weapons[currentWeaponIndex].GetComponent<WeaponController>();
    }

    // --- METODO PARA ACCEDER A TODO EL ARSENAL ---
    public WeaponController[] GetAllWeapons()
    {
        WeaponController[] controllers = new WeaponController[weapons.Length];
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                controllers[i] = weapons[i].GetComponent<WeaponController>();
            }
        }
        return controllers;
    }
}