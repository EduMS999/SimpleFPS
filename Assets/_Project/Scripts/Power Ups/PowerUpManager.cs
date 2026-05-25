using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // Propiedad pública para que cualquier script sepa si el Instakill está activo
    public bool IsInstakillActive { get; private set; } = false;
    public bool IsDoublePointsActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Método que activará el Power-Up Instakill
    public void ActivateInstakill(float duration)
    {
        StartCoroutine(InstakillRoutine(duration));
    }   

    // Método que activará el Power-Up DoublePoints
    public void ActivateDoublePoints(float duration)
    {
        StartCoroutine(DoublePointsRoutine(duration));
    }

    public void ActivateNuke(Vector3 position, GameObject mushroomCloud)
    {
        int enemiesAlive = LevelManager.Instance.activeEnemies.Count;

        // Instanciar el hongo nuclear
        if (mushroomCloud != null)
        {
            GameObject cloud = Instantiate(mushroomCloud, position - new Vector3(0, 1.050225f, 0), Quaternion.identity); // Se resta el vector para compensar la altura del powerup respecto al suelo
            Destroy(cloud, 4f); // Limpieza pasados 4 segundos
        }

        // Ejecutar la detonación en el LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.TriggerNukeDetonation();
        }

        // Otorgar los 400 puntos extras al jugador
        if (PointsSystem.Instance != null)
        {
            PointsSystem.Instance.AddPoints(400);
            int pointsToSubstract = PointsSystem.Instance.pointsPerDeath * enemiesAlive;
            //Debug.Log(pointsToSubstract + " Puntos que debo restar");
            //Debug.Log(enemiesAlive + " Enemigos vivos");
            //Debug.Log(PointsSystem.Instance.pointsPerDeath + " Puntos por muerte");
            PointsSystem.Instance.RemovePoints(pointsToSubstract); // Se hace para evitar muchisimos puntos por matar a todos a la vez
        }
    }

    private IEnumerator DoublePointsRoutine(float duration)
    {
        IsDoublePointsActive = true;
        Debug.Log("PUNTOS DOBLES ACTIVADO!");

        yield return new WaitForSeconds(duration);

        IsDoublePointsActive = false;
        Debug.Log("Puntos dobles terminado.");
    }

    private IEnumerator InstakillRoutine(float duration)
    {
        IsInstakillActive = true;
        Debug.Log("¡INSTAKILL ACTIVADO!");

        yield return new WaitForSeconds(duration);

        IsInstakillActive = false;
        Debug.Log("Instakill terminado.");
    }
}
