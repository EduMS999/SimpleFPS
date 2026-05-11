using TMPro;
using UnityEngine;

public class PointsSystem : MonoBehaviour
{
    // Instancia estática que será accesible globalmente
    public static PointsSystem Instance { get; private set; }

    private int points = 0;
    [Header("Ajustes")]
    public int pointsPerBullet = 10;
    public int pointsPerDeath = 90;

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI pointsText;

    private void Awake()
    {
        // Lógica para asegurar que solo exista una instancia (Singleton)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Si ya hay uno, destruimos el duplicado
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPointsPerBullet()
    {
        points += pointsPerBullet;
        //Debug.Log($"Puntos actuales: {points}");
        pointsText.text = points.ToString();
    }

    public void AddPointsPerDeath()
    {
        points += pointsPerDeath;
        //Debug.Log($"Puntos actuales: {points}");
        pointsText.text = points.ToString();
    }


    public void RemovePoints(int pointsToRemove)
    {
        points -= pointsToRemove;
        pointsText.text = points.ToString();
    }

    // Método útil para que otros scripts lean el puntaje
    public int GetCurrentPoints() => points;
}
