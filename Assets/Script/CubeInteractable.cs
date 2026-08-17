using UnityEngine;

public class CubeInteractable : MonoBehaviour
{
    // =====================================================
    // CONFIGURACIÓN DEL RESALTADO
    // =====================================================

    [Header("Resaltado de selección")]

    [Tooltip("Color hacia el que parpadea el cerdito.")]
    [SerializeField]
    private Color highlightColor =
        new Color(1f, 0.85f, 0.25f, 1f);

    [Tooltip("Velocidad del parpadeo.")]
    [SerializeField]
    private float blinkSpeed = 4f;

    [Tooltip("Intensidad del cambio de color.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float highlightStrength = 0.45f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private bool isSelected = false;

    private Renderer[] childRenderers;

    private Material[][] materials;

    private Color[][] originalColors;

    // =====================================================
    // PROPIEDAD PÚBLICA
    // =====================================================

    public bool IsSelected
    {
        get { return isSelected; }
    }

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        // Buscar todos los Renderer del modelo 3D
        // dentro de este objeto y sus hijos.
        childRenderers =
            GetComponentsInChildren<Renderer>(true);

        materials =
            new Material[childRenderers.Length][];

        originalColors =
            new Color[childRenderers.Length][];

        for (int i = 0;
             i < childRenderers.Length;
             i++)
        {
            // .materials crea materiales independientes
            // para este cerdito.
            materials[i] =
                childRenderers[i].materials;

            originalColors[i] =
                new Color[materials[i].Length];

            for (int j = 0;
                 j < materials[i].Length;
                 j++)
            {
                Material material =
                    materials[i][j];

                if (material == null)
                    continue;

                // -----------------------------------------
                // GUARDAR COLOR ORIGINAL
                // -----------------------------------------

                if (material.HasProperty("_BaseColor"))
                {
                    originalColors[i][j] =
                        material.GetColor("_BaseColor");
                }
                else if (material.HasProperty("_Color"))
                {
                    originalColors[i][j] =
                        material.GetColor("_Color");
                }
                else
                {
                    originalColors[i][j] =
                        Color.white;
                }
            }
        }

        Debug.Log(
            name +
            ": se encontraron " +
            childRenderers.Length +
            " Renderer para el resaltado."
        );
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!isSelected)
            return;

        // Valor que oscila entre 0 y 1.
        float pulse =
            (Mathf.Sin(
                Time.time * blinkSpeed
            ) + 1f) * 0.5f;

        // Controlar cuánto cambia realmente el color.
        float intensity =
            pulse * highlightStrength;

        ApplyHighlight(intensity);
    }

    // =====================================================
    // SELECCIONAR / DESELECCIONAR
    // =====================================================

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            Debug.Log(
                "Cerdito seleccionado: " +
                name
            );
        }
        else
        {
            RestoreOriginalColors();

            Debug.Log(
                "Cerdito deseleccionado: " +
                name
            );
        }
    }

    // =====================================================
    // APLICAR RESALTADO
    // =====================================================

    private void ApplyHighlight(float intensity)
    {
        if (materials == null)
            return;

        for (int i = 0;
             i < materials.Length;
             i++)
        {
            for (int j = 0;
                 j < materials[i].Length;
                 j++)
            {
                Material material =
                    materials[i][j];

                if (material == null)
                    continue;

                Color originalColor =
                    originalColors[i][j];

                Color newColor =
                    Color.Lerp(
                        originalColor,
                        highlightColor,
                        intensity
                    );

                SetMaterialColor(
                    material,
                    newColor
                );
            }
        }
    }

    // =====================================================
    // RESTAURAR COLORES
    // =====================================================

    private void RestoreOriginalColors()
    {
        if (materials == null)
            return;

        for (int i = 0;
             i < materials.Length;
             i++)
        {
            for (int j = 0;
                 j < materials[i].Length;
                 j++)
            {
                Material material =
                    materials[i][j];

                if (material == null)
                    continue;

                SetMaterialColor(
                    material,
                    originalColors[i][j]
                );
            }
        }
    }

    // =====================================================
    // CAMBIAR COLOR DEL MATERIAL
    // =====================================================

    private void SetMaterialColor(
        Material material,
        Color color)
    {
        // URP/Lit normalmente utiliza _BaseColor.
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                color
            );

            return;
        }

        // Compatibilidad con otros shaders.
        if (material.HasProperty("_Color"))
        {
            material.SetColor(
                "_Color",
                color
            );
        }
    }

    // =====================================================
    // LIMPIEZA
    // =====================================================

    private void OnDestroy()
    {
        if (materials == null)
            return;

        for (int i = 0;
             i < materials.Length;
             i++)
        {
            for (int j = 0;
                 j < materials[i].Length;
                 j++)
            {
                if (materials[i][j] != null)
                {
                    Destroy(
                        materials[i][j]
                    );
                }
            }
        }
    }
}