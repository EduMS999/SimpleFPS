using UnityEngine;
using System.Collections.Generic;

public class AnimatedCamouflage : MonoBehaviour
{
    [Header("Velocidad del Movimiento")]
    [SerializeField] private float speedX = 0.05f;
    [SerializeField] private float speedY = 0.05f;

    [Header("Configuración del Shader")]
    [SerializeField] private string texturePropertyName = "_BaseMap";

    private List<Material> materialsToAnimate = new List<Material>();
    private Vector2 currentOffset = Vector2.zero;

    void Start()
    {
        // Buscamos todos los Renderers en el padre y en sus hijos (cañón, mira, cargador, etc.)
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        // Guardamos las referencias de sus materiales
        foreach (Renderer rend in renderers)
        {
            // Usamos rend.materials (en plural) por si alguna pieza tiene más de un material aplicado
            if (rend != null && rend.materials != null)
            {
                foreach (Material mat in rend.materials)
                {
                    // Solo lo añadimos si el material tiene la propiedad de textura que buscamos
                    if (mat.HasProperty(texturePropertyName))
                    {
                        materialsToAnimate.Add(mat);
                    }
                }
            }
        }

        if (materialsToAnimate.Count == 0)
        {
            Debug.LogWarning($"[AnimatedCamouflage] No se encontraron materiales con la propiedad {texturePropertyName} en {gameObject.name} o sus hijos.");
        }
    }

    void Update()
    {
        if (materialsToAnimate.Count == 0) return;

        // Calculamos el nuevo desplazamiento basándonos en el tiempo transcurrido
        currentOffset.x += speedX * Time.deltaTime;
        currentOffset.y += speedY * Time.deltaTime;

        // Evitamos que el número crezca infinitamente reseteándolo al llegar a 1
        if (currentOffset.x >= 1f) currentOffset.x -= 1f;
        if (currentOffset.y >= 1f) currentOffset.y -= 1f;

        // Aplicamos el desplazamiento a todos los materiales guardados de golpe
        foreach (Material mat in materialsToAnimate)
        {
            mat.SetTextureOffset(texturePropertyName, currentOffset);
        }
    }
}