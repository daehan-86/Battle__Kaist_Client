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


// NaverLogin Ŭ����: Unity���� ���̹� �α����� ��û�ϴ� ����� ������ ��ũ��Ʈ
public class NaverLogin : MonoBehaviour
{
    public class Message
    {
        public string Type { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // �޽��� ������
    }
    private ClientWebSocket _webSocket;
    private string _serverUri = "ws://172.10.7.27:5000/"; // WebSocket ���� URI

    public void Update()
    {
        // ���� ���õ� UI�� �� ���� ������Ʈ�̰�, Ư�� �Է� ��ư�� ���ȴ��� Ȯ��
        if (EventSystem.current.currentSelectedGameObject == gameObject
            && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit�� �Է� Ű ���� �̸�
        {
            NaverLoginStart(); // WebSocket ���� �� �޽��� ���� �׽�Ʈ ����
        }
    }
    public void NaverLoginStart()
    {
        ConnectToServerAsync();
    }
    /// <summary>
    /// WebSocket ������ ����
    /// </summary>
    public async void ConnectToServerAsync()
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
            await RequestNaverLogin();

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
    private async Task RequestNaverLogin()
    {
        var message = new Message
        {
            Type = "REQUEST_NAVER_LOGIN",
            Data = null
        };

        // JSON ����ȭ
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("���̹� �α��� ��û �޽����� ������ �����߽��ϴ�.");
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
                    var response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    if (response?.Type == "NAVER_LOGIN_URL")
                    {
                        string loginUrl = response?.Data?.ToString();
                        Debug.Log($"���̹� �α��� URL: {loginUrl}");
                        Application.OpenURL(loginUrl);
                    }
                    else if(response?.Type == "NEW_USER")
                    {
                        WebSocketService.naverId = response.Data.ToString();
                        SceneManager.LoadScene("NewUserScene");
                    }
                    else if(response?.Type == "LOGIN_SUCCESS")
                    {
                        WebSocketService.naverId = response.Data.ToString();
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
