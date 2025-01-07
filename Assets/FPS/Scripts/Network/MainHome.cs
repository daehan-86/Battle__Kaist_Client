using System;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;
public class MainHome : MonoBehaviour
{
    public class Message
    {
        public string Type { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public object Data { get; set; } // �޽��� ������
    }

    private ClientWebSocket _webSocket;
    private bool isqueue = false;
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
            await RequestUserData();
            
            await ListenForMessages();
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
            Type = "REQUEST_INIT",
            Data = WebSocketService.naverId
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���������� �ʱ�ȭ ��û��");
    }
    public async Task RequestUserData()
    {
        var message = new Message
        {
            Type = "REQUEST_USER_DATA",
            Data = null
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���������� ��û��");
    }
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
                    var response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    if (response?.Type == "RESPONSE_USER_DATA")
                    {
                        //ȭ�� ����
                        break;
                    }
                    else if(response?.Type == "JOIN_GAME_ROOM")
                    {
                        WebSocketService.roomId = Convert.ToInt32(response.Data);
                        Debug.Log($"{WebSocketService.roomId}�� ������~");
                        SceneManager.LoadScene("LoadingScene");
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
    public async void StartGame()
    {
        var message = new Message
        {
            Type = "ENTER_QUEUE",
            Data = null
        };
        isqueue = true;
        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���� ���� ��û��");

        await ListenForMessages();
    }

    public async void ExitGame()
    {
        isqueue = false;
        var message = new Message
        {
            Type = "EXIT",
            Data = null
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���� ���� ��û��");
        Application.Quit();
    }
}
