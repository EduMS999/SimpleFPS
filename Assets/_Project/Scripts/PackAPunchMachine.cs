using UnityEngine;

/// <summary>
/// Gestiona la mejora de armas mediante el sistema de activación/desactivación de la cámara.
/// </summary>
public class PackAPunchMachine : MonoBehaviour, IInteractable
{
    [Header("Configuración de Coste")]
    [SerializeField] private int upgradeCost = 5000;

    [Header("Prompts de Interfaz")]
    [SerializeField] private string upgradePrompt = "Mejorar arma por 5000 puntos";
    [SerializeField] private string weaponAlreadyUpgradedPrompt = "Este arma ya está al máximo";
    [SerializeField] private string noWeaponPrompt = "Necesitas un arma en la mano";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip upgradeSound;

    // Propiedad dinámica para mostrar el texto correcto en la UI
    public string InteractionPrompt
    {
        get
        {
            WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
            if (weaponManager != null) return upgradePrompt;

            int currentIndex = weaponManager.currentWeaponIndex;
            GameObject activeWeaponObject = weaponManager.weapons[currentIndex];

            if (activeWeaponObject == null) return noWeaponPrompt;

            // Intentamos buscar si la que tenemos en la mano tiene el camuflaje dinámico
            // o si ya es una versión que no se puede mejorar más
            AnimatedCamouflage camo = activeWeaponObject.GetComponent<AnimatedCamouflage>();
            if (camo != null)
            {
                return weaponAlreadyUpgradedPrompt;
            }

            return upgradePrompt;
        }
    }

    public void Interact()
    {
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager == null) return;

        // Validar puntos del jugador
        if (PointsSystem.Instance.GetCurrentPoints() < upgradeCost)
        {
            Debug.Log("No tienes suficientes puntos para mejorar.");
            return;
        }

        // Obtener el arma actual del jugador
        int currentIndex = weaponManager.currentWeaponIndex;
        GameObject currentWeaponObject = weaponManager.weapons[currentIndex];

        if (currentWeaponObject == null)
        {
            Debug.LogWarning("No tienes ningún arma equipada para mejorar.");
            return;
        }

        // Comprobar si el arma ya está mejorada (si ya tiene el script de camuflaje animado)
        if (currentWeaponObject.GetComponent<AnimatedCamouflage>() != null)
        {
            Debug.Log("Este arma ya ha sido mejorada.");
            return;
        }

        // Buscar el equivalente mejorado bajo la cámara del jugador.
        // Convención de nombres: Si el arma se llama "Pistola", su versión mejorada en la jerarquía debe llamarse "PistolaPAP
        string upgradedWeaponName = currentWeaponObject.name + "PAP";
        Transform camTransform = GameObject.FindGameObjectWithTag("MainCamera").transform; // Contenedor de armas (Cámara)
        GameObject upgradedWeaponObject = null;

        foreach (Transform hijo in camTransform)
        {
            if (hijo.name == upgradedWeaponName)
            {
                upgradedWeaponObject = hijo.gameObject;
                break;
            }
        }

        // Intercambio de armas mediante estados activos/inactivos
        if (upgradedWeaponObject != null)
        {
            // Apagamos el arma normal y la dejamos inactiva bajo la cámara
            currentWeaponObject.SetActive(false);

            // Encendemos el nuevo arma mejorada
            upgradedWeaponObject.SetActive(true);

            // Reemplazamos la referencia en el slot del inventario del mánager
            weaponManager.weapons[currentIndex] = upgradedWeaponObject;

            // Cobramos los puntos correspondientes
            PointsSystem.Instance.RemovePoints(upgradeCost);

            // Efecto de sonido
            if (audioSource != null && upgradeSound != null)
            {
                audioSource.PlayOneShot(upgradeSound);
            }

            Debug.Log($"¡{currentWeaponObject.name} ha sido evolucionada a {upgradedWeaponName}!");
        }
        else
        {
            Debug.LogError($"[PackAPunchMachine] No se encontró el objeto '{upgradedWeaponName}' desactivado bajo la cámara.");
        }
    }
}