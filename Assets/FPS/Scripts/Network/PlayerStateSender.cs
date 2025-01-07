using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.FPS.Gameplay;
using Newtonsoft.Json;

public class PlayerStateSender : MonoBehaviour
{
    private ClientWebSocket _webSocket;
    public PlayerCharacterController PlayerController; // PlayerCharacterController ����
    public float sendInterval = 0.1f; // ���� ����

    private float _lastSendTime;

    async void Start()
    {
        // WebSocket �ʱ�ȭ
        _webSocket = WebSocketService.WebSocket;
        try
        {
            Debug.Log("WebSocket ���� ����!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WebSocket ���� ����: {ex.Message}");
        }
    }

    void Update()
    {
        // ������ ���ݸ��� ��ġ ������ ����
        if (Time.time - _lastSendTime >= sendInterval)
        {
            SendPlayerState();
            _lastSendTime = Time.time;
        }
    }

    async void SendPlayerState()
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;

        // ��ġ �� ���� ���� ��������
        Vector3 position = PlayerController.CurrentPosition;
        Vector3 rotationEuler = PlayerController.CurrentRotation.eulerAngles; // Quaternion �� Euler ��ȯ
        float health = PlayerController.CurrentHealth;

        // ������ ����
        PlayerState playerState = new PlayerState
        {
            Type = "re",
            position = position,
            rotation = rotationEuler,
            health = health // ü�� ���� �߰�
        };
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(playerState,settings);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���� ���� �޽����� ������ �����߽��ϴ�.");
    }

    private void OnApplicationQuit()
    {
        if (_webSocket != null)
        {
            _webSocket.Dispose();
        }
    }
}

[System.Serializable]
public class PlayerState
{
    public string Type;
    public Vector3 position;
    public Vector3 rotation;
    public float health; // ü�� ���� �߰�
}
