using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // =====================================================
    // JUGAR
    // =====================================================

    public void Jugar()
    {
        Debug.Log(
            "SE PRESIONÓ EL BOTÓN JUGAR"
        );

        SceneManager.LoadScene(
            "MainScene"
        );
    }

    // =====================================================
    // INSTRUCCIONES
    // =====================================================

    public void Instrucciones()
    {
        Debug.Log(
            "SE PRESIONÓ EL BOTÓN INSTRUCCIONES"
        );

        SceneManager.LoadScene(
            "InstruccionesScene"
        );
    }

    // =====================================================
    // IR A JUGAR DESDE INSTRUCCIONES
    // =====================================================

    public void IrAJugar()
    {
        SceneManager.LoadScene(
            "MainScene"
        );
    }

    // =====================================================
    // REPLAY
    // =====================================================

    public void Replay()
    {
        Debug.Log(
            "SE PRESIONÓ REPLAY"
        );

        SceneManager.LoadScene(
            "MainScene"
        );
    }

    // =====================================================
    // VOLVER AL MENÚ
    // =====================================================

    public void VolverAlMenu()
    {
        Debug.Log(
            "SE PRESIONÓ VOLVER AL MENÚ"
        );

        SceneManager.LoadScene(
            "MenuScene"
        );
    }

    // =====================================================
    // SALIR
    // =====================================================

    public void Salir()
    {
        Debug.Log(
            "SE PRESIONÓ EL BOTÓN SALIR"
        );

        Application.Quit();
    }
}