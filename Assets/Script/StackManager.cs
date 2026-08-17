using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StackManager : MonoBehaviour
{
    // =====================================================
    // CONFIGURACIÓN
    // =====================================================

    [Header("Configuración")]

    [SerializeField]
    private int requiredPigs = 4;

    [SerializeField]
    private float horizontalToleranceX = 0.8f;

    [SerializeField]
    private float horizontalToleranceZ = 0.8f;

    [SerializeField]
    private float minimumVerticalDifference = 0.3f;

    // =====================================================
    // VICTORIA
    // =====================================================

    [Header("Victoria")]

    [SerializeField]
    private GameTimer gameTimer;

    [SerializeField]
    private GameObject winPanel;

    [SerializeField]
    private TMP_Text finalTimeText;

    [Tooltip("Segundos que espera antes de mostrar el panel de victoria.")]
    [SerializeField]
    private float winPanelDelay = 3f;

    // =====================================================
    // CERDITOS EN ZONA B
    // =====================================================

    private readonly List<CubeInteractable> pigsInZone =
        new List<CubeInteractable>();

    private bool gameWon = false;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // El panel de victoria empieza oculto.
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    // =====================================================
    // ENTRAR A ZONA B
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        CubeInteractable pig =
            other.GetComponent<CubeInteractable>();

        if (pig == null)
        {
            pig =
                other.GetComponentInParent<
                    CubeInteractable
                >();
        }

        if (pig == null)
            return;

        if (!pigsInZone.Contains(pig))
        {
            pigsInZone.Add(pig);

            Debug.Log(
                "Cerdito entró en Zona B: " +
                pig.name +
                ". Total: " +
                pigsInZone.Count
            );
        }

        CheckWinCondition();
    }

    // =====================================================
    // SALIR DE ZONA B
    // =====================================================

    private void OnTriggerExit(Collider other)
    {
        CubeInteractable pig =
            other.GetComponent<CubeInteractable>();

        if (pig == null)
        {
            pig =
                other.GetComponentInParent<
                    CubeInteractable
                >();
        }

        if (pig == null)
            return;

        if (pigsInZone.Contains(pig))
        {
            pigsInZone.Remove(pig);

            Debug.Log(
                "Cerdito salió de Zona B: " +
                pig.name +
                ". Total: " +
                pigsInZone.Count
            );
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (gameWon)
            return;

        if (pigsInZone.Count >= requiredPigs)
        {
            CheckWinCondition();
        }
    }

    // =====================================================
    // COMPROBAR VICTORIA
    // =====================================================

    private void CheckWinCondition()
    {
        if (gameWon)
            return;

        if (pigsInZone.Count < requiredPigs)
            return;

        // -------------------------------------------------
        // ORDENAR POR ALTURA
        // -------------------------------------------------

        List<CubeInteractable> orderedPigs =
            new List<CubeInteractable>(
                pigsInZone
            );

        orderedPigs.Sort(
            (a, b) =>
                a.transform.position.y.CompareTo(
                    b.transform.position.y
                )
        );

        // -------------------------------------------------
        // CERDITO MÁS BAJO COMO REFERENCIA
        // -------------------------------------------------

        Vector3 basePosition =
            orderedPigs[0]
                .transform.position;

        // =================================================
        // COMPROBAR X / Z
        // =================================================

        foreach (
            CubeInteractable pig
            in orderedPigs)
        {
            Vector3 position =
                pig.transform.position;

            float differenceX =
                Mathf.Abs(
                    position.x -
                    basePosition.x
                );

            float differenceZ =
                Mathf.Abs(
                    position.z -
                    basePosition.z
                );

            if (differenceX >
                    horizontalToleranceX ||
                differenceZ >
                    horizontalToleranceZ)
            {
                return;
            }
        }

        // =================================================
        // COMPROBAR ALTURAS
        // =================================================

        for (int i = 1;
             i < orderedPigs.Count;
             i++)
        {
            float previousY =
                orderedPigs[i - 1]
                    .transform.position.y;

            float currentY =
                orderedPigs[i]
                    .transform.position.y;

            float verticalDifference =
                currentY -
                previousY;

            if (verticalDifference <
                minimumVerticalDifference)
            {
                return;
            }
        }

        // =================================================
        // VICTORIA
        // =================================================

        Win();
    }

    // =====================================================
    // GANAR
    // =====================================================

    private void Win()
    {
        if (gameWon)
            return;

        gameWon = true;

        Debug.Log(
            "¡VICTORIA! Los 4 cerditos están apilados."
        );

        // -------------------------------------------------
        // DETENER CRONÓMETRO INMEDIATAMENTE
        // -------------------------------------------------

        if (gameTimer != null)
        {
            gameTimer.StopTimer();

            if (finalTimeText != null)
            {
                finalTimeText.text =
                    gameTimer.FormatTime(
                        gameTimer.ElapsedTime
                    );
            }
        }
        else
        {
            Debug.LogWarning(
                "GameTimer no está asignado en StackManager."
            );
        }

        // -------------------------------------------------
        // ESPERAR ANTES DE MOSTRAR PANEL
        // -------------------------------------------------

        StartCoroutine(
            ShowWinPanelAfterDelay()
        );
    }

    // =====================================================
    // MOSTRAR PANEL DESPUÉS DE UNOS SEGUNDOS
    // =====================================================

    private IEnumerator ShowWinPanelAfterDelay()
    {
        Debug.Log(
            "Esperando " +
            winPanelDelay +
            " segundos para mostrar WinPanel."
        );

        yield return new WaitForSeconds(
            winPanelDelay
        );

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            Debug.Log(
                "WinPanel mostrado."
            );
        }
        else
        {
            Debug.LogWarning(
                "WinPanel no está asignado."
            );
        }
    }
}