using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias UI")]
    public TextMeshProUGUI waveNumberText;

    [Header("Power Ups")]
    public 

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
        SpawnSystem.OnWaveChanged += UpdateWaveNumber;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateWaveNumber(int waveNumber)
    {
        if(waveNumberText != null)
            waveNumberText.text = waveNumber.ToString();
    }
}
