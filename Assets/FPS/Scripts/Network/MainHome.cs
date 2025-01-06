using System;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Text;

public class MainHome : MonoBehaviour
{
    public class Message
    {
        public string Type { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // �޽��� ������
    }

    private ClientWebSocket _webSocket;
    private string _serverUri = "ws://172.10.7.27:5000/Home"; // WebSocket ���� URI
    private async void Start()
    {
        //_webSocket = new ClientWebSocket();
        _webSocket = WebSocketService.WebSocket;
        try
        {
            _webSocket = new ClientWebSocket();
            // ������ WebSocket ���� ��û
            await _webSocket.ConnectAsync(new Uri(_serverUri), CancellationToken.None);
            Debug.Log("WebSocket ������ ����Ǿ����ϴ�.");

            // ���̹� �α��� ��û ������
            await RequestInit();

            // ���� ���� ����
            //await ListenForMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket ���� ����: {ex.Message}");
        }
    }
    public async Task RequestInit()
    {
        var message = new Message
        {
            Type = "CREATE_USER",
            Data = WebSocketService.naverId
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("�̸����� ��û��");
    }
    public async void StartGame()
    {

    }

    public async void ExitGame()
    {

    }
}
