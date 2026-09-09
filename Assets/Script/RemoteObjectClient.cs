using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen; 


public class RemoteObjectClient : MonoBehaviour
{
    [Header("Network UI")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject connectionMenuPanel;

    [Header("Input Setup")]
    [SerializeField] private OnScreenStick joystick;

    private TcpClient client;
    private StreamWriter writer;
    private bool isConnected;

    private float yawInput;
    private bool grabRequested;
    private bool releaseRequested;

    public void ConnectToServer()
    {
        if (isConnected) return;

        string ip = ipInputField != null ? ipInputField.text : "127.0.0.1";
        int port = 7777;

        if (portInputField != null && !int.TryParse(portInputField.text, out port))
        {
            port = 7777;
        }

        try
        {
            client = new TcpClient(ip, port);
            writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false))
            {
                AutoFlush = true
            };

            isConnected = true;
            WriteStatus("¡Conectado al servidor!");
        }
        catch (Exception ex)
        {
            WriteStatus("Error de conexión: " + ex.Message);
        }
        isConnected = true;
        WriteStatus("¡Conectado al servidor!");

        // Ocultar la interfaz del menú inicial al conectar con éxito
        if (connectionMenuPanel != null)
        {
            connectionMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isConnected || writer == null) return;

        // Leer ejes X y Z usando el Input System (teclado o joystick)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        RemoteControlMessage msg = new RemoteControlMessage
        {
            x = moveX,
            z = moveZ,
            yaw = yawInput,
            grab = grabRequested,
            release = releaseRequested
        };

        grabRequested = false;
        releaseRequested = false;

        try
        {
            string json = JsonUtility.ToJson(msg);
            writer.WriteLine(json);
        }
        catch (Exception ex)
        {
            WriteStatus("Error enviando datos: " + ex.Message);
            Disconnect();
        }
    }

    public void StartRotateLeft() => yawInput = -1f;
    public void StartRotateRight() => yawInput = 1f;
    public void StopRotate() => yawInput = 0f;

    public void TriggerGrab() => grabRequested = true;
    public void TriggerRelease() => releaseRequested = true;

    private void Disconnect()
    {
        isConnected = false;
        writer?.Close();
        client?.Close();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void WriteStatus(string text)
    {
        Debug.Log(text);
        if (statusText != null) statusText.text = text;
    }
}