using UnityEngine;

public class StackManager : MonoBehaviour
{
    [Header("Cubos")]
    [SerializeField] private CubeInteractable[] cubes;

    [Header("Zona de apilamiento")]
    [SerializeField] private Transform stackTarget;

    [SerializeField] private float positionTolerance = 0.25f;

    [Header("Altura entre cubos")]
    [SerializeField] private float verticalSpacing = 1f;

    [SerializeField] private float verticalTolerance = 0.25f;

    [Header("Victoria")]
    [SerializeField] private GameTimer gameTimer;

    [SerializeField] private GameObject victoryPanel;

    private bool gameWon = false;

    private void Start()
    {
        // Ocultar el panel de victoria al comenzar
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameWon)
            return;

        CheckStack();
    }

    private void CheckStack()
    {
        // -------------------------------------------------
        // VERIFICAR QUE EXISTAN LOS 4 CUBOS
        // -------------------------------------------------

        if (cubes == null || cubes.Length != 4)
            return;

        // -------------------------------------------------
        // VERIFICAR ZONA DE APILAMIENTO
        // -------------------------------------------------

        if (stackTarget == null)
            return;

        // -------------------------------------------------
        // ORDENAR LOS CUBOS POR ALTURA
        // -------------------------------------------------

        CubeInteractable[] sortedCubes =
            new CubeInteractable[cubes.Length];

        cubes.CopyTo(sortedCubes, 0);

        System.Array.Sort(
            sortedCubes,
            (a, b) =>
                a.transform.position.y.CompareTo(
                    b.transform.position.y
                )
        );

        // -------------------------------------------------
        // COMPROBAR X Y Z
        // -------------------------------------------------

        for (int i = 0; i < sortedCubes.Length; i++)
        {
            Vector3 position =
                sortedCubes[i].transform.position;

            // X
            if (Mathf.Abs(
                position.x -
                stackTarget.position.x
            ) > positionTolerance)
            {
                return;
            }

            // Z
            if (Mathf.Abs(
                position.z -
                stackTarget.position.z
            ) > positionTolerance)
            {
                return;
            }
        }

        // -------------------------------------------------
        // COMPROBAR ALTURA
        // -------------------------------------------------

        float baseY =
            sortedCubes[0].transform.position.y;

        for (int i = 1; i < sortedCubes.Length; i++)
        {
            float expectedY =
                baseY +
                verticalSpacing * i;

            float currentY =
                sortedCubes[i].transform.position.y;

            if (Mathf.Abs(
                currentY - expectedY
            ) > verticalTolerance)
            {
                return;
            }
        }

        // -------------------------------------------------
        // LOS 4 CUBOS ESTÁN CORRECTAMENTE APILADOS
        // -------------------------------------------------

        WinGame();
    }

    private void WinGame()
    {
        gameWon = true;

        // -------------------------------------------------
        // DETENER CRONÓMETRO
        // -------------------------------------------------

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }

        // -------------------------------------------------
        // MOSTRAR IMAGEN / PANEL DE VICTORIA
        // -------------------------------------------------

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Debug.Log(
            "¡GANASTE! Los 4 cubos están correctamente apilados."
        );
    }
}