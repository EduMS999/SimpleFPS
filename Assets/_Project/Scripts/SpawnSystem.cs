using System;
using System.Collections;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [Header("Configuración de Oleadas")]
    [SerializeField] private GameObject enemyPrefab;
    public int enemyCount;
    public float spawnRate;
    [SerializeField] private float timeBetweenWaves = 4f;
    //private int currentWaveIndex = 0;
    private int currentWave = 0;

    [Header("Escalado de Dificultad")]
    [SerializeField] private float healthMultiplierPerWave = 0.1f; // Incrementa un 10% la vida base por cada ronda pasada

    [Header("Puntos de Spawn")]
    [SerializeField] private Transform[] spawnPoints;
    private int lastSpawnPointIndex = -1; // Para evitar repetir el mismo punto

    [Header("Patrulla de Enemigos")]
    [SerializeField] private Transform[] enemyPatrolPoints;

    private int enemiesAliveInWave;
    private bool isSpawningWave = false;

    public static event Action<int> OnWaveChanged;

    private void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnSystem] ¡No hay puntos de spawn asignados!");
            return;
        }

        StartCoroutine(StartNextWaveRoutine());
    }

    private IEnumerator StartNextWaveRoutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        ++currentWave;
        Debug.Log($"[SpawnSystem] --- INICIANDO: Ronda {currentWave} ---");
        OnWaveChanged?.Invoke(currentWave); // Se dispara el evento para comunicar al GameManager
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawningWave = true;
        enemiesAliveInWave = enemyCount;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(enemyPrefab);
            // Esperamos entre enemigos para que el generador aleatorio cambie de valor
            yield return new WaitForSeconds(spawnRate);
        }

        isSpawningWave = false;
        Debug.Log($"[SpawnSystem] Fin del spawn de la oleada {currentWave}. Esperando a que mueran los enemigos.");
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        // --- GUARDAS DE SEGURIDAD ---
        if (enemyPrefab == null)
        {
            Debug.LogError("[SpawnSystem] ¡No has asignado el prefab del enemigo en la oleada!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnSystem] ¡No hay puntos de spawn asignados en el Inspector!");
            return;
        }

        // Evitamos repetir el mismo punto de spawn de forma segura
        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

        // Solo intentamos buscar otro punto si tenemos más de uno disponible
        if (spawnPoints.Length > 1)
        {
            int intentos = 0; // Seguridad extra para evitar bucles infinitos
            while (randomIndex == lastSpawnPointIndex && intentos < 10)
            {
                randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
                intentos++;
            }
        }
        lastSpawnPointIndex = randomIndex;

        // Verificación de seguridad del punto elegido
        Transform spawnPoint = spawnPoints[randomIndex];
        if (spawnPoint == null)
        {
            Debug.LogError($"[SpawnSystem] El punto de spawn en el índice {randomIndex} está vacío (Missing).");
            return;
        }

        // Instanciamos al enemigo
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Obtenemos los componentes ANTES de hacer nada más
        // Cambiamos GetComponent por GetComponentInChildren para máxima seguridad
        SimpleEnemyAI enemyAI = spawnedEnemy.GetComponentInChildren<SimpleEnemyAI>();
        HealthSystem enemyHealth = spawnedEnemy.GetComponentInChildren<HealthSystem>();

        // Suscribimos el evento de muerte inmediatamente
        if (enemyHealth != null)
        {
            // --- ESCALADO DE VIDA ---
            // Calculamos la nueva vida máxima: VidaBase + (VidaBase * Multiplicador * (Ronda - 1))
            // En la Ronda 1: multiplicador es 0 (Mantiene vida base)
            // En la Ronda 2: aumenta un 10%, en la Ronda 3 un 20%...
            float extraHealthBonus = enemyHealth.maxHealth * healthMultiplierPerWave * (currentWave - 1);
            enemyHealth.maxHealth += extraHealthBonus;
            enemyHealth.currentHealth = enemyHealth.maxHealth; // Curamos al enemigo para que aparezca con la barra llena

            enemyHealth.OnDeath += OnEnemyDied;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RegisterSpawnedEnemy(enemyHealth);
            }
            Debug.Log($"[SpawnSystem] Enemigo spawneado en {spawnPoint.name} y registrado correctamente.");
        }
        else
        {
            Debug.LogError($"[SpawnSystem] ¡El prefab de enemigo {enemyPrefab.name} NO tiene un HealthSystem!");
        }

        // 5. Por último, le damos los puntos de patrulla para que empiece a moverse
        /*if (enemyAI != null)
        {
            if (enemyPatrolPoints != null && enemyPatrolPoints.Length > 0)
            {
                enemyAI.SetPatrolPoints(enemyPatrolPoints);
            }
            else
            {
                Debug.LogWarning($"[SpawnSystem] No hay 'enemyPatrolPoints' asignados en el Spawner para {spawnedEnemy.name}.");
            }
        }*/
    }

    private void OnEnemyDied()
    {
        enemiesAliveInWave--;
        Debug.Log($"[SpawnSystem] Enemigo eliminado. Quedan vivos en esta oleada: {enemiesAliveInWave}");

        // Si la oleada terminó de spawnear y ya no quedan enemigos vivos
        if (enemiesAliveInWave <= 0 && !isSpawningWave)
        {
            //// ¿Era esta la última oleada?
            //if (currentWaveIndex + 1 >= waves.Length)
            //{
            //    Debug.Log("[SpawnSystem] ¡Todas las oleadas completadas y todos los enemigos eliminados!");

            //    // Avisamos al LevelManager de que el jugador ha ganado el nivel
            //    if (LevelManager.Instance != null)
            //    {
            //        LevelManager.Instance.WinLevel();
            //    }
            //}
            
            
            // Aún quedan más oleadas, avanzamos a la siguiente
            Debug.Log($"[SpawnSystem] ¡Oleada completada! Avanzando a la siguiente.");
            StartCoroutine(StartNextWaveRoutine());
           
        }
    }
}