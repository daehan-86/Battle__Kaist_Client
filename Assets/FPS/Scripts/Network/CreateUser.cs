using System.Net.WebSockets;
using System.Threading;
using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.UI;

public class CreateUser : MonoBehaviour
{
    public class Message
    {
        public string Type { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // �޽��� ������
    }
    
    private ClientWebSocket _webSocket;
    private string _serverUri = "ws://172.10.7.27:5000/Creat"; // WebSocket ���� URI
    public GameObject CreateView;
    public InputField InputName; // InputField ����
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Update()
    {
        // ���� ���õ� UI�� �� ���� ������Ʈ�̰�, Ư�� �Է� ��ư�� ���ȴ��� Ȯ��
        if (EventSystem.current.currentSelectedGameObject == gameObject
            && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit�� �Է� Ű ���� �̸�
        {
            CreateUserStart(); // WebSocket ���� �� �޽��� ���� �׽�Ʈ ����
        }
    }
    public void CreateUserStart()
    {
        if (InputName.text != "")
        {
            CreateUserToServerAsync();
        }
        else
        {
            Debug.Log("�� ���ڿ��� ���� ���� ���� ����");
        }
    }

    public async void CreateUserToServerAsync()
    {
        //_webSocket = new ClientWebSocket();
        _webSocket = WebSocketService.WebSocket;
        try
        {
            // ������ WebSocket ���� ��û
            await _webSocket.ConnectAsync(new Uri(_serverUri), CancellationToken.None);
            Debug.Log("WebSocket ������ ����Ǿ����ϴ�.");

            // ���̹� �α��� ��û ������
            await RequestCreateUser();

            // ���� ���� ����
            await ListenForMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket ���� ����: {ex.Message}");
        }
    }
    /// <summary>
    /// ������ ���̹� �α��� ��û ������
    /// </summary>
    private async Task RequestCreateUser()
    {
        Debug.Log(InputName.text);
        var message = new Message
        {
            Type = "CREATE_USER",
            Data = $"{WebSocketService.naverId},{InputName.text}"
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("�̸����� ��û��");
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
                    var response = JsonConvert.DeserializeObject<Message>(responseMessage);if (response?.Type == "LOGIN_SUCCESS")
                    {
                        SceneManager.LoadScene("MainHomeScene");
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
