using System;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadingGame : MonoBehaviour
{
    public class Message
    {
        public string Type { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // �޽��� ������
    }
    public class Message2
    {
        public string t { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public object Data { get; set; } // �޽��� ������
    }
    private ClientWebSocket _webSocket;
    private string _serverUri = "ws://172.10.7.27:5000/game0"; // WebSocket ���� URI
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        _webSocket = WebSocketService.WebSocket;
        try
        {
            _webSocket = new ClientWebSocket();
            // ������ WebSocket ���� ��û
            await _webSocket.ConnectAsync(new Uri(_serverUri), CancellationToken.None);
            WebSocketService.WebSocket = _webSocket;
            Debug.Log("WebSocket ������ ����Ǿ����ϴ�.");

            // ���̹� �α��� ��û ������
            await RequestJoinGame();

            // ���� ���� ����
            await ListenForMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket ���� ����: {ex.Message}");
        }
    }

    private async Task RequestJoinGame()
    {
        var message = new Message
        {
            Type = "JOIN_GAME",
            Data = WebSocketService.naverId
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���� ���� �޽����� ������ �����߽��ϴ�.");
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    private async Task ListenForMessages()
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var responseMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"���� ���� ����: {responseMessage}");

                    // JSON ������ȭ
                    var response = JsonConvert.DeserializeObject<Message2>(responseMessage);
                    if (response?.t == "RE")
                    {
                        SceneManager.LoadScene("MainScene");
                        break;
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("WebSocket ������ ����Ǿ����ϴ�.");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "����", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"�޽��� ���� ����: {ex.Message}");
            }
        }
    }
}
