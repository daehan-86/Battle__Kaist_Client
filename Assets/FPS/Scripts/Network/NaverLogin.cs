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


// NaverLogin 클래스: Unity에서 네이버 로그인을 요청하는 기능을 구현한 스크립트
public class NaverLogin : MonoBehaviour
{
    public class Message
    {
        public string Type { get; set; } // 메시지 타입 (예: "REQUEST_NAVER_LOGIN")
        public string Data { get; set; } // 메시지 데이터
    }
    private ClientWebSocket _webSocket;
    private string _serverUri = "ws://172.10.7.27:5000/"; // WebSocket 서버 URI

    public void Update()
    {
        // 현재 선택된 UI가 이 게임 오브젝트이고, 특정 입력 버튼이 눌렸는지 확인
        if (EventSystem.current.currentSelectedGameObject == gameObject
            && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit는 입력 키 매핑 이름
        {
            NaverLoginStart(); // WebSocket 연결 및 메시지 전송 테스트 시작
        }
    }
    public void NaverLoginStart()
    {
        ConnectToServerAsync();
    }
    /// <summary>
    /// WebSocket 서버와 연결
    /// </summary>
    public async void ConnectToServerAsync()
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
            await RequestNaverLogin();

            // 서버 응답 수신
            await ListenForMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket 연결 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 서버에 네이버 로그인 요청 보내기
    /// </summary>
    private async Task RequestNaverLogin()
    {
        var message = new Message
        {
            Type = "REQUEST_NAVER_LOGIN",
            Data = null
        };

        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("네이버 로그인 요청 메시지를 서버로 전송했습니다.");
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
                    var response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    if (response?.Type == "NAVER_LOGIN_URL")
                    {
                        string loginUrl = response?.Data?.ToString();
                        Debug.Log($"네이버 로그인 URL: {loginUrl}");
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
