using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Configuración del Nivel")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delayBeforeNextLevel = 3f;

    [Header("Referencias de UI")]
    [SerializeField] private GameObject victoryCanvas;

    [NonSerialized] public List<HealthSystem> activeEnemies = new List<HealthSystem>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (victoryCanvas != null) victoryCanvas.SetActive(false);
    }

    public void RegisterSpawnedEnemy(HealthSystem enemyHealth)
    {
        if (enemyHealth != null && !activeEnemies.Contains(enemyHealth))
        {
            activeEnemies.Add(enemyHealth);
            enemyHealth.OnDeath += OnEnemyDefeated;
        }
    }

    private void OnEnemyDefeated()
    {
        HealthSystem deadEnemy = null;
        foreach (HealthSystem enemy in activeEnemies)
        {
            if (enemy.currentHealth <= 0)
            {
                deadEnemy = enemy;
                break;
            }
        }

        if (deadEnemy != null)
        {
            deadEnemy.OnDeath -= OnEnemyDefeated;
            activeEnemies.Remove(deadEnemy);
        }

        // Ya NO llamamos automáticamente a WinLevel() aquí.
        // El SpawnSystem se encargará de validar cuándo es el final real.
    }

    /// <summary>
    /// Método público para que el SpawnSystem active la victoria.
    /// </summary>
    public void WinLevel()
    {
        Debug.Log("¡Nivel completado con éxito!");

        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(true);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(LoadNextLevelRoutine());
        }
        else
        {
            Debug.LogWarning("No se ha asignado el nombre de la siguiente escena.");
        }
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        yield return new WaitForSeconds(delayBeforeNextLevel);
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Detona la Nuke eliminando a todos los enemigos registrados actualmente.
    /// </summary>
    public void TriggerNukeDetonation()
    {
        Debug.Log($"[LevelManager] Detonando Nuke. Enemigos en rango: {activeEnemies.Count}");

        // Recorremos la lista al revés para evitar que los índices se descoloquen.(desde el último índice hasta el 0).
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
            {
                // Aplicamos un daño letal instantáneo
                activeEnemies[i].TakeDamage(99999f);
            }
        }
    }

}