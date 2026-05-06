using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI waveNumberText;

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
