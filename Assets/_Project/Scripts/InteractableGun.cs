using UnityEngine;

/// <summary>
/// Clase que gestiona el comportamiento de compra de pared.
/// Implementa IInteractable para que el sistema de interacción del jugador la reconozca.
/// </summary>
public class InteractableGun : MonoBehaviour, IInteractable
{
    // --- SECCIÓN: CONFIGURACIÓN Y ESTADO ---
    [Header("Configuración de Seguridad")]
    private bool isBought = false; // Define si el arma esta comprada o no
    [SerializeField] private int gunCost; // Numero de puntos que vale el arma
    [SerializeField] private int ammoCost; // Numero de puntos que vale comprar su munición
    [SerializeField] private WeaponData weaponData; // Información del arma
    private string gunName; // Nombre del arma para el prompt

    [Header("Prompts (Textos de Interfaz)")]
    [SerializeField] private string buyPrompt = $"Comprar NOMBREARMA por COSTEARMA puntos";
    [SerializeField] private string ammoPrompt = "Comprar munición por COSTEMUNICION";

    [Header("Visual Feedback")]
    [SerializeField] private Material unlockedMaterial; // Material que se aplica al desbloquearse

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buySound;

    public string InteractionPrompt
    {
        get
        {
            if (isBought) return ammoPrompt; // Si está comprada, sugiere "Comprar munición"
            else return buyPrompt;    // Si no lo esta, sugiere que la compres  
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (weaponData != null)
        {
            gunName = weaponData.name;
            buyPrompt = $"Comprar {gunName} por {gunCost} puntos";
            ammoPrompt = $"Comprar municion por {ammoCost} puntos";
        }
        else
            Debug.LogWarning("Weapon Controller no encontrado");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        if(!isBought)
        {
            WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>(); // Sola hay un WeaponManager

            if(weaponManager != null)
            {
                WeaponController[] totalArmas = weaponManager.GetAllWeapons();
                
                if(totalArmas.Length > 1) // Tiene dos armas
                {
                    int currentWeaponIndex = weaponManager.currentWeaponIndex;
                    Destroy(weaponManager.weapons[currentWeaponIndex]); // Borramos nuestro arma actual
                    GameObject newGun =  weaponManager.weapons[currentWeaponIndex] = GameObject.Find(weaponData.name);
                }
            }
        }
    }
}
