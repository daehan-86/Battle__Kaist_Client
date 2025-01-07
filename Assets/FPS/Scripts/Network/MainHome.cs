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
        public string Type { get; set; } // 메시지 타입 (예: "REQUEST_NAVER_LOGIN")
        public object Data { get; set; } // 메시지 데이터
    }

    private ClientWebSocket _webSocket;
    private bool isqueue = false;
    private string _serverUri = "ws://172.10.7.27:5000/Home"; // WebSocket 서버 URI
    private async void Start()
    {
        //_webSocket = new ClientWebSocket();
        _webSocket = WebSocketService.WebSocket;
        try
        {
            _webSocket = new ClientWebSocket();
            // 서버에 WebSocket 연결 요청
            await _webSocket.ConnectAsync(new Uri(_serverUri), CancellationToken.None);
            Debug.Log("WebSocket 서버에 연결되었습니다.");

            // 네이버 로그인 요청 보내기
            await RequestInit();

            // 서버 응답 수신
            await RequestUserData();
            
            await ListenForMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket 연결 오류: {ex.Message}");
        }
    }
    public async Task RequestInit()
    {
        var message = new Message
        {
            Type = "REQUEST_INIT",
            Data = WebSocketService.naverId
        };

        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("유저데이터 초기화 요청함");
    }
    public async Task RequestUserData()
    {
        var message = new Message
        {
            Type = "REQUEST_USER_DATA",
            Data = null
        };

        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("유저데이터 요청함");
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
                    Debug.Log($"서버 응답 수신: {responseMessage}");

                    // JSON 역직렬화
                    var response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    if (response?.Type == "RESPONSE_USER_DATA")
                    {
                        //화면 갱신
                        break;
                    }
                    else if(response?.Type == "JOIN_GAME_ROOM")
                    {
                        WebSocketService.roomId = Convert.ToInt32(response.Data);
                        Debug.Log($"{WebSocketService.roomId}번 방참가~");
                        SceneManager.LoadScene("LoadingScene");
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("WebSocket 연결이 종료되었습니다.");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "닫힘", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"메시지 수신 오류: {ex.Message}");
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
        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("소켓 삭제 요청함");

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

        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("소켓 삭제 요청함");
        Application.Quit();
    }
}
