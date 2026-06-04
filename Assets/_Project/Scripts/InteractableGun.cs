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
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager == null) return;

        int currentIndex = weaponManager.currentWeaponIndex;

        // Variables para comprobar el estado del inventario
        int slotDeEstaArma = -1;
        int slotVacio = -1;

        // Analizamos las 2 ranuras del WeaponManager
        for (int i = 0; i < weaponManager.weapons.Length; i++)
        {
            GameObject armaEnSlot = weaponManager.weapons[i];

            if (armaEnSlot != null)
            {
                WeaponController controller = armaEnSlot.GetComponent<WeaponController>();
                // Comprobamos si el arma de este slot es la misma que vende la pared
                if (controller != null && controller.GetMaxAmmo() == weaponData.maxAmmo && controller.GetMaxReserveAmmo() == weaponData.maxReserveAmmo)
                {
                    slotDeEstaArma = i; // El jugador ya tiene esta arma en este slot
                }
            }
            else
            {
                slotVacio = i; // Encontramos una ranura libre
            }
        }

        // CASO A: EL JUGADOR YA TIENE ESTA ARMA
        if (slotDeEstaArma != -1)
        {
            // Si el jugador la tiene guardada pero no en la mano, podemos hacer que cambie a ella,
            // pero lo estándar es que si interactúa con la pared de esa misma arma, compre munición.
            GameObject armaObject = weaponManager.weapons[slotDeEstaArma];
            WeaponController currentWeapon = armaObject.GetComponent<WeaponController>();

            if (currentWeapon != null && PointsSystem.Instance.GetCurrentPoints() >= ammoCost)
            {
                currentWeapon.AddAmmo(weaponData.ammoType, weaponData.maxReserveAmmo);
                PointsSystem.Instance.RemovePoints(ammoCost);
                Debug.Log($"Munición comprada para {weaponData.name}.");
            }
            return;
        }

        // CASO B: EL JUGADOR NO TIENE EL ARMA (COMPRA)
        if (PointsSystem.Instance.GetCurrentPoints() < gunCost)
        {
            Debug.Log("No tienes suficientes puntos.");
            return;
        }

        // Buscamos el GameObject de la jerarquía de la cámara que corresponde a esta arma de la pared.
        Transform camTransform = GameObject.FindGameObjectWithTag("MainCamera").transform; 
        GameObject armaEnCamara = null;

        // Buscamos entre todos los hijos (activos o inactivos) de la cámara el que coincida con el nombre
        foreach (Transform hijo in camTransform)
        {
            if (hijo.name == weaponData.name)
            {
                armaEnCamara = hijo.gameObject;
                break;
            }
        }

        if (armaEnCamara == null)
        {
            Debug.LogError($"[InteractableGun] No se encontró el objeto {weaponData.name} bajo la cámara del jugador.");
            return;
        }

        // Determinar en qué ranura se va a guardar
        int slotDestino = -1;

        if (slotVacio != -1)
        {
            // Si hay espacio libre (tiene solo 1 arma), se guarda en el slot vacío
            slotDestino = slotVacio;
        }
        else
        {
            // Si el inventario está lleno (tiene 2 armas), reemplazamos la que tiene en la mano actualmente.
            // Desactivamos por completo el arma actual del inventario
            if (weaponManager.weapons[currentIndex] != null)
            {
                weaponManager.weapons[currentIndex].SetActive(false);
            }
            slotDestino = currentIndex;
        }

        // Colocamos la nueva arma en el slot de armas utilizables del mánager
        weaponManager.weapons[slotDestino] = armaEnCamara;

        // La activamos físicamente para que el jugador la vea y la use
        armaEnCamara.SetActive(true);

        isBought = true; // Marcamos que esta arma ya ha sido comprada para mostrar el prompt de munición

        // Le decimos al mánager que ahora esta es su arma activa
        weaponManager.currentWeaponIndex = slotDestino;

        // Cobramos los puntos
        PointsSystem.Instance.RemovePoints(gunCost);

        if (audioSource != null && buySound != null)
        {
            audioSource.PlayOneShot(buySound);
        }

        Debug.Log($"Arma {weaponData.name} añadida al slot {slotDestino} del inventario.");
    }
}
