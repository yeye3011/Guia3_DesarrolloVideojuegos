using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

[Serializable]
public class RemoteControlMessage
{
    public float x;
    public float z;
    public float yaw;
    public bool grab;
    public bool release;
}

public class RemoteObjectServer : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private int port = 7777;
    [SerializeField] private TMP_Text statusText;

    [Header("Controlled Object")]
    [SerializeField] private Transform controlledObject;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Grab System")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float grabRadius = 2.5f;

    private TcpListener listener;
    private TcpClient connectedClient;
    private Thread acceptThread;
    private Thread readThread;
    private volatile bool running;

    private readonly ConcurrentQueue<string> incomingLines = new ConcurrentQueue<string>();
    private RemoteControlMessage currentInput = new RemoteControlMessage();

    private GrabbableObject heldObject;
    private Rigidbody heldBody;

    private void Start()
    {
        if (controlledObject == null)
            controlledObject = transform;

        if (characterController == null)
            characterController = controlledObject.GetComponent<CharacterController>();

        if (holdPoint == null)
            holdPoint = controlledObject;

        WriteStatus($"IP: {GetBestIPv4()} PORT: {port}");
    }

    public void StartServer()
    {
        if (running)
            return;

        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            running = true;

            acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            acceptThread.Start();

            WriteStatus($"Servidor escuchando en {GetBestIPv4()}:{port}");
        }
        catch (Exception ex)
        {
            WriteStatus($"Error al iniciar servidor: {ex.Message}");
        }
    }

    private void AcceptLoop()
    {
        while (running)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                ReplaceClient(client);
            }
            catch (SocketException)
            {
                break;
            }
        }
    }

    private void ReplaceClient(TcpClient client)
    {
        connectedClient?.Close();
        connectedClient = client;

        readThread = new Thread(() => ReadLoop(client)) { IsBackground = true };
        readThread.Start();
    }

    private void ReadLoop(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        StringBuilder pendingText = new StringBuilder();

        while (running && client.Connected)
        {
            int count = 0;
            try
            {
                count = stream.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                break;
            }

            if (count == 0)
                break;

            string chunk = Encoding.UTF8.GetString(buffer, 0, count);
            pendingText.Append(chunk);

            string text = pendingText.ToString();
            string[] lines = text.Split('\n');

            for (int i = 0; i < lines.Length - 1; i++)
            {
                incomingLines.Enqueue(lines[i]);
            }

            pendingText.Clear();
            pendingText.Append(lines[lines.Length - 1]);
        }
    }

    private void Update()
    {
        while (incomingLines.TryDequeue(out string line))
        {
            try
            {
                RemoteControlMessage message = JsonUtility.FromJson<RemoteControlMessage>(line);
                if (message != null)
                {
                    currentInput.x = Mathf.Clamp(message.x, -1f, 1f);
                    currentInput.z = Mathf.Clamp(message.z, -1f, 1f);
                    currentInput.yaw = Mathf.Clamp(message.yaw, -1f, 1f);

                    if (message.grab)
                        TryGrab();

                    if (message.release)
                        Release();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error al deserializar JSON: " + ex.Message);
            }
        }

        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (controlledObject == null) return;

        Vector3 direction = controlledObject.forward * currentInput.z + controlledObject.right * currentInput.x;
        direction = Vector3.ClampMagnitude(direction, 1f);

        Vector3 moveVelocity = direction * moveSpeed * Time.deltaTime;

        if (characterController != null && characterController.enabled)
        {
            characterController.Move(moveVelocity);
        }
        else
        {
            controlledObject.Translate(moveVelocity, Space.World);
        }

        if (!Mathf.Approximately(currentInput.yaw, 0f))
        {
            float rotationAmount = currentInput.yaw * rotationSpeed * Time.deltaTime;
            controlledObject.Rotate(Vector3.up * rotationAmount);
        }
    }

    private void TryGrab()
    {
        if (heldObject != null)
            return;

        Collider[] hits = Physics.OverlapSphere(controlledObject.position, grabRadius);

        Collider nearest = hits
            .Where(hit => hit.GetComponent<GrabbableObject>() != null || hit.GetComponentInParent<GrabbableObject>() != null)
            .OrderBy(hit => Vector3.Distance(controlledObject.position, hit.transform.position))
            .FirstOrDefault();

        if (nearest == null)
            return;

        GrabbableObject grabbable = nearest.GetComponent<GrabbableObject>();
        if (grabbable == null)
            grabbable = nearest.GetComponentInParent<GrabbableObject>();

        if (grabbable == null)
            return;

        heldObject = grabbable;
        heldBody = heldObject.GetComponent<Rigidbody>();

        if (heldBody != null)
        {
            PigStabilizer stabilizer = heldObject.GetComponent<PigStabilizer>();
            if (stabilizer != null)
            {
                stabilizer.ReleaseStack();
            }

            heldBody.linearVelocity = Vector3.zero;
            heldBody.angularVelocity = Vector3.zero;
            heldBody.useGravity = false;
            heldBody.isKinematic = true;

            heldObject.transform.SetParent(holdPoint);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
        }
    }

    private void Release()
    {
        if (heldObject == null)
            return;

        heldObject.transform.SetParent(null);

        if (heldBody != null)
        {
            heldBody.isKinematic = false;
            heldBody.useGravity = true;
            heldBody.linearVelocity = Vector3.zero;
            heldBody.angularVelocity = Vector3.zero;
        }

        heldObject = null;
        heldBody = null;
    }

    private void OnApplicationQuit() => StopServer();

    private void StopServer()
    {
        running = false;
        connectedClient?.Close();
        listener?.Stop();
        acceptThread?.Join(100);
        readThread?.Join(100);
    }

    private void WriteStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
            statusText.text = message;
    }

 
    //Selecciona la mejor IP disponible dando prioridad a la tarjeta Wi-Fi real.
    
    private static string GetBestIPv4()
    {
        string[] ips = GetAllCandidateIPv4s();
        if (ips.Length == 0) return "0.0.0.0";

        // Palabras clave para identificar adaptadores Wi-Fi
        string[] wifiHints = { "wlan", "wifi", "wlo", "wl ", "wlp" };

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;

            string name = (ni.Name + " " + ni.Description).ToLowerInvariant();
            if (!wifiHints.Any(h => name.Contains(h))) continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ua in ipProps.UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    string ip = ua.Address.ToString();
                    if (ips.Contains(ip)) return ip;
                }
            }
        }

        // Si no detecta Wi-Fi explícito, devuelve la primera IP local válida
        return ips[0];
    }

    // Filtra interfaces inactivas, loopback, túneles y direcciones APIPA (169.254.x.x)
    private static string[] GetAllCandidateIPv4s()
    {
        var list = new List<string>();

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ua in ipProps.UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;

                IPAddress ip = ua.Address;
                if (IPAddress.IsLoopback(ip)) continue;

                // Descarta direcciones sin conexión real
                byte[] bytes = ip.GetAddressBytes();
                if (bytes[0] == 169 && bytes[1] == 254) continue;

                list.Add(ip.ToString());
            }
        }
        return list.Distinct().ToArray();
    }
    private void OnDrawGizmosSelected()
    {
        if (controlledObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(controlledObject.position, grabRadius);
        }
    }
}