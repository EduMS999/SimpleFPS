using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using System;

public class WeaponController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletHoleDecalPrefab;

    [Header("Referencias de UI")]
    [SerializeField] private HitmarkerUI hitmarkerUI;
    [SerializeField] private TextMeshProUGUI reloadText;

    // --- EVENTO PARA LA UI ---
    // Este evento es el que busca AmmoDisplay.cs
    public event Action OnAmmoChanged;

    private int currentReserveAmmo; // Balas disponibles en la reserva actual

    private float nextFireTime;
    private bool isAttacking = false;
    private int currentAmmo;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = weaponData.maxAmmo;
        currentReserveAmmo = weaponData.maxReserveAmmo;

        // Notificamos el estado inicial
        OnAmmoChanged?.Invoke();
    }


    // Método público para que la UI pueda leer la munición
    public int GetCurrentAmmo() => currentAmmo;
    //public int GetCurrentAmmo() { return currentAmmo; }
    public bool GetIsReloading() => isReloading;
    public int GetMaxAmmo() => weaponData.maxAmmo;
    // --- MÉTODOS PARA LA RESERVA ---
    public int GetCurrentReserveAmmo() => currentReserveAmmo;
    public int GetMaxReserveAmmo() => weaponData.maxReserveAmmo;

    // Se ejecuta automáticamente si la acción en el Input Action Asset se llama "Attack"
    // y el Player Input está en modo "Send Messages"
    private void OnAttack(InputValue value)
    {
        // Esta es la forma más segura de leer un botón en el nuevo Input System
        // .isPressed devuelve true mientras el botón esté abajo y false cuando se suelte
        isAttacking = value.isPressed;

        // --- LÓGICA PARA ARMAS SEMIAUTOMÁTICAS ---
        // Si el arma NO es automática, y el jugador acaba de presionar el botón (isPressed es true),
        // disparamos inmediatamente una sola bala, respetando la cadencia.
        if (!weaponData.isAutomatic && isAttacking && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + weaponData.fireRate;
        }
    }

    void Update()
    {
        // --- LÓGICA PARA ARMAS AUTOMÁTICAS ---
        // El Update solo se encarga de disparar repetidamente si el arma ES automática.
        if (weaponData.isAutomatic && isAttacking && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + weaponData.fireRate;
        }

        
    }

    private void Shoot()
    {
        // Evitamos que el jugador dispare mientras está en la secuencia de recarga
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            //Debug.Log("¡Sin munición! Necesitas recargar.");
            return;
        }

        if (currentAmmo <= weaponData.maxAmmo / 3.5f)
            ReloadText(true);

        

        currentAmmo--; // Restamos una bala
        //Debug.Log($"Munición restante: {currentAmmo}/{weaponData.maxAmmo}");

        // --- ACTUALIZAR UI AL DISPARAR ---
        OnAmmoChanged?.Invoke();

        // --- LÓGICA DE EFECTOS ---

        // 1. Aparecer el destello en la punta del cañón
        if (weaponData.muzzleFlashPrefab != null && firePoint != null)
        {
            // Creamos el efecto y lo destruimos tras 0.1 segundos para que no llene la jerarquía
            GameObject flash = Instantiate(weaponData.muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }

        // 2. Reproducir el sonido en la posición del arma
        if (weaponData.shootSound != null)
        {
            AudioSource.PlayClipAtPoint(weaponData.shootSound, transform.position);
        }

        // --- LÓGICA DE FÍSICA  ---
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.yellow, 0.5f);

        if (Physics.Raycast(ray, out hit, weaponData.range))
        {
            HandleHit(hit, ray);
        }
    }

    private void HandleHit(RaycastHit hit, Ray ray)
    {
        // Impacto Visual
        if (weaponData.hitEffectPrefab != null)
        {
            GameObject impactVFX = Instantiate(weaponData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactVFX, 2f);
        }

        // Decals
        if (bulletHoleDecalPrefab != null)
        {
            Vector3 posicionConOffset = hit.point - (hit.normal * 0.01f);
            GameObject decal = Instantiate(bulletHoleDecalPrefab, posicionConOffset, Quaternion.LookRotation(hit.normal));
            decal.transform.SetParent(hit.transform);
            decal.transform.localScale = Vector3.one;
            Destroy(decal, 30f);
        }

        // Fuerza Física
        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForceAtPosition(ray.direction * weaponData.impactForce, hit.point, ForceMode.Impulse);
        }

        // --- GESTIÓN DE DAÑO EN EL IMPACTO ---
        bool damageApplied = false;

        // Calculamos el daño final. Si Instakill está activo, infligimos un daño masivo.
        float finalDamage = weaponData.damage;
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsInstakillActive)
        {
            finalDamage = 99999f; // O cualquier número absurdamente alto para garantizar la muerte
        }

        // Opción 1: Verificamos si el objeto impactado tiene directamente el HealthSystem
        HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(finalDamage);
            damageApplied = true;
        }
        else
        {
            // Opción 2: Si no tiene HealthSystem directo, miramos si es un DamageProxy (ej. el TargetPoint o la cápsula visual)
            DamageProxy proxy = hit.collider.GetComponent<DamageProxy>();
            if (proxy != null)
            {
                proxy.TakeDamage(finalDamage);
                damageApplied = true;
            }
        }

        if (damageApplied && hitmarkerUI != null)
        {
            hitmarkerUI.ShowHitmarker();
        }
    }

    // Se ejecuta al pulsar "R"
    private void OnReload(InputValue value)
    {
        // REGLAS DE RECARGA: 
        // Solo recargamos si no estamos recargando ya, si el cargador no está lleno 
        // y si nos quedan balas en la reserva.
        if (!isReloading && currentAmmo < weaponData.maxAmmo && currentReserveAmmo > 0)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnAmmoChanged?.Invoke(); // Notificamos que ahora el estado es "RECARGANDO..."
        ReloadText(false);

        yield return new WaitForSeconds(weaponData.reloadTime);

        // Calculamos cuántas balas necesitamos para llenar el cargador
        int ammoNeeded = weaponData.maxAmmo - currentAmmo;

        // Caso A: Tenemos suficiente munición en reserva para llenar el cargador completo
        if (currentReserveAmmo >= ammoNeeded)
        {
            currentAmmo += ammoNeeded;
            currentReserveAmmo -= ammoNeeded;
        }
        // Caso B: Nos queda menos en la reserva de lo que el cargador necesita
        else
        {
            currentAmmo += currentReserveAmmo;
            currentReserveAmmo = 0;
        }

        isReloading = false;
        OnAmmoChanged?.Invoke(); // Notificamos que la recarga terminó
    }

    // --- MÉTODO: Usado por el AmmoPickup.cs ---
    /// <summary>
    /// Intenta añadir munición a la reserva si el calibre coincide.
    /// </summary>
    public bool AddAmmo(AmmoType typeOfAmmo, int amount)
    {
        // Si el calibre de la caja no es el que usa esta arma, lo ignoramos
        if (weaponData.ammoType != typeOfAmmo) return false;

        // Si ya tenemos la reserva llena, no la recogemos
        if (currentReserveAmmo >= weaponData.maxReserveAmmo) return false;

        // Añadimos las balas limitando al máximo
        currentReserveAmmo = Mathf.Clamp(currentReserveAmmo + amount, 0, weaponData.maxReserveAmmo);
        // --- ACTUALIZAR UI AL RECOGER BALAS ---
        OnAmmoChanged?.Invoke();

        return true; // Éxito: Munición aceptada
    }
    // --- MÉTODO: Usado por el PW_MaxAmmo.cs ---
    /// <summary>
    /// Intenta añadir munición a la reserva si el calibre coincide.
    /// </summary>
    public bool AddMaxAmmo()
    {
        int amount = weaponData.maxReserveAmmo;
        // Añadimos las balas limitando al máximo
        currentReserveAmmo = amount;
        // --- ACTUALIZAR UI AL RECOGER BALAS ---
        OnAmmoChanged?.Invoke();

        return true; // Éxito: Munición aceptada
    }

    private void OnDisable()
    {
        // Si el objeto se desactiva (o morimos), forzamos el cese del disparo
        isAttacking = false;

        // Se oculta el texto de recarga al desactivar el arma
        if (reloadText != null)
        {
            ReloadText(false);
        }
    }

    private void OnEnable()
    {
        // Al activar el objeto, verificamos si la munición es baja para mostrar el texto
        // Usamos la misma lógica que en el método Shoot()
        if (currentAmmo <= weaponData.maxAmmo / 3.5f && !isReloading)
        {
            ReloadText(true);
        }
    }

    private void ReloadText(bool s)
    {
        // Se encarga de desactivar o activar el texto de recargar en pantalla

        if (!reloadText.IsActive() && s)
            reloadText.gameObject.SetActive(true);
        if(reloadText.IsActive() && !s)
            reloadText.gameObject.SetActive(false);
            
    }
}