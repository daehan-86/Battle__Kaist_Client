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
        public string Type { get; set; } // 메시지 타입 (예: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // 메시지 데이터
    }
    public class Message2
    {
        public string t { get; set; } // 메시지 타입 (예: "REQUEST_NAVER_LOGIN")
        public object Data { get; set; } // 메시지 데이터
    }
    private ClientWebSocket _webSocket;
    private string _serverUri = "ws://172.10.7.27:5000/game0"; // WebSocket 서버 URI
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        _webSocket = WebSocketService.WebSocket;
        try
        {
            _webSocket = new ClientWebSocket();
            // 서버에 WebSocket 연결 요청
            await _webSocket.ConnectAsync(new Uri(_serverUri), CancellationToken.None);
            WebSocketService.WebSocket = _webSocket;
            Debug.Log("WebSocket 서버에 연결되었습니다.");

            // 네이버 로그인 요청 보내기
            await RequestJoinGame();

            // 서버 응답 수신
            await ListenForMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket 연결 오류: {ex.Message}");
        }
    }

    private async Task RequestJoinGame()
    {
        var message = new Message
        {
            Type = "JOIN_GAME",
            Data = WebSocketService.naverId
        };

        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("게임 참가 메시지를 서버로 전송했습니다.");
    }

    /// <summary>
    /// 서버 응답 수신
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
                    Debug.Log($"서버 응답 수신: {responseMessage}");

                    // JSON 역직렬화
                    var response = JsonConvert.DeserializeObject<Message2>(responseMessage);
                    if (response?.t == "RE")
                    {
                        SceneManager.LoadScene("MainScene");
                        break;
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
}
