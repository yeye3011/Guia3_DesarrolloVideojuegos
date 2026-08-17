using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    private float elapsedTime = 0f;
    private bool isRunning = false;

    public float ElapsedTime => elapsedTime;
    public bool IsRunning => isRunning;

    private void Start()
    {
        elapsedTime = 0f;
        isRunning = false;

        UpdateTimerText();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        elapsedTime += Time.deltaTime;

        UpdateTimerText();
    }

    public void StartTimer()
    {
        if (isRunning)
            return;

        isRunning = true;

        Debug.Log("Cronómetro iniciado.");
    }

    public void StopTimer()
    {
        if (!isRunning)
            return;

        isRunning = false;

        UpdateTimerText();

        Debug.Log(
            "Cronómetro detenido: " +
            FormatTime(elapsedTime)
        );
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        timerText.text = FormatTime(elapsedTime);
    }

    public string FormatTime(float time)
    {
        int minutes =
            Mathf.FloorToInt(time / 60f);

        int seconds =
            Mathf.FloorToInt(time % 60f);

        int milliseconds =
            Mathf.FloorToInt(
                (time * 1000f) % 1000f
            );

        return string.Format(
            "{0:00}:{1:00}.{2:000}",
            minutes,
            seconds,
            milliseconds
        );
    }
}