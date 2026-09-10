using System;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class SmartphoneSocketController : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private float sendInterval = 0.05f;

    [Header("UI Controls (Joystick)")]
    [SerializeField] private RectTransform joystickThumb;       // Objeto JoystickHandle (Círculo rojo)
    [SerializeField] private RectTransform joystickBackground;  // Objeto JoystickBackground

    private TcpClient client;
    private NetworkStream stream;
    private float nextSendTime;

    private float yawInput;
    private bool grabRequested;
    private bool releaseRequested;

    public void Connect()
    {
        try
        {
            string ip = ipField.text.Trim();
            int port = int.Parse(portField.text);

            client = new TcpClient();
            client.Connect(ip, port);
            stream = client.GetStream();

            SetStatus($"Conectado a {ip}:{port}");
        }
        catch (Exception ex)
        {
            SetStatus("Error al conectar: " + ex.Message);
        }
    }

    private void Update()
    {
        if (stream == null || Time.time < nextSendTime)
            return;

        float x = 0f;
        float z = 0f;

        if (joystickThumb != null && joystickBackground != null)
        {
            // Vector de desplazamiento
            Vector3 displacement = joystickThumb.position - joystickBackground.position;

            float radius = (joystickBackground.rect.width * joystickBackground.lossyScale.x) * 0.5f;

            if (radius > 0)
            {
                float rawX = displacement.x / radius;
                float rawZ = displacement.y / radius;

                // ZONA MUERTA (Deadzone de 0.15f / 15%): 
                // Ignora imprecisiones milimétricas cuando la palanca está en reposo
                float deadzone = 0.15f;

                x = (Mathf.Abs(rawX) > deadzone) ? Mathf.Clamp(rawX, -1f, 1f) : 0f;
                z = (Mathf.Abs(rawZ) > deadzone) ? Mathf.Clamp(rawZ, -1f, 1f) : 0f;
            }
        }

        SendCurrentInput(x, z);
        nextSendTime = Time.time + sendInterval;
    }

    private void SendCurrentInput(float x, float z)
    {
        RemoteControlMessage message = new RemoteControlMessage();
        message.x = x;
        message.z = z;
        message.yaw = yawInput;
        message.grab = grabRequested;
        message.release = releaseRequested;

        string json = JsonUtility.ToJson(message) + "\n";
        byte[] data = Encoding.UTF8.GetBytes(json);

        stream.Write(data, 0, data.Length);

        grabRequested = false;
        releaseRequested = false;
    }

    // Funciones para presionar y soltar botones de rotación
    public void RotateLeftHold() => yawInput = -1f;
    public void RotateRightHold() => yawInput = 1f;
    public void StopRotation() => yawInput = 0f;

    public void RequestGrab() => grabRequested = true;
    public void RequestRelease() => releaseRequested = true;

    public void Disconnect()
    {
        stream?.Close();
        client?.Close();
        stream = null;
        client = null;
        SetStatus("Desconectado");
    }

    private void OnApplicationQuit() => Disconnect();

    private void SetStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
            statusText.text = message;
    }
}