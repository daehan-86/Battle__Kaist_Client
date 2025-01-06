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
        public string Type { get; set; } // 메시지 타입 (예: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // 메시지 데이터
    }

    private ClientWebSocket _webSocket;
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
            //await ListenForMessages();
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
            Type = "CREATE_USER",
            Data = WebSocketService.naverId
        };

        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("이름생성 요청함");
    }
    public async void StartGame()
    {

    }

    public async void ExitGame()
    {

    }
}
