using System.Net.WebSockets; // WebSocket 관련 클래스
using System.Text; // 문자열 인코딩/디코딩
using System.Threading; // 비동기 작업에서 사용되는 CancellationToken
using System; // 기본 시스템 클래스 (예외 처리, URI 등)
using Unity.FPS.Game; // FPS 게임 관련 Unity 네임스페이스
using UnityEngine; // Unity 엔진 코어 클래스
using UnityEngine.EventSystems; // Unity의 이벤트 시스템 (버튼 클릭 등)

namespace Unity.FPS.Network // FPS 게임 네트워크 관련 네임스페이스
{
    public class NetworkTestButton : MonoBehaviour
    {
        public string SceneName = ""; // 로드할 씬 이름(현재 미사용)
        private ClientWebSocket _webSocket; // WebSocket 클라이언트 인스턴스
        public string ServerUri = "ws://172.10.7.27:5000"; // 연결할 WebSocket 서버 URI

        /// <summary>
        /// 서버로 메시지 전송
        /// </summary>
        private async System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            // 메시지를 UTF8로 바이트 배열로 변환
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // 서버로 메시지 전송
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("서버에 메시지를 보냈습니다."); // Unity 디버그 창에 로그 출력
        }

        /// <summary>
        /// 서버로부터 메시지 수신
        /// </summary>
        private async System.Threading.Tasks.Task ReceiveMessageAsync()
        {
            // 메시지 수신용 버퍼 생성
            var buffer = new byte[1024];

            // 서버로부터 메시지 수신
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // 메시지가 텍스트 형식인 경우 처리
            if (result.MessageType == WebSocketMessageType.Text)
            {
                // 수신한 메시지를 UTF8 문자열로 디코딩
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"서버로부터 메시지 수신: {message}"); // Unity 디버그 창에 출력
            }
        }

        /// <summary>
        /// 애플리케이션 종료 시 WebSocket 리소스 정리
        /// </summary>
        private void OnApplicationQuit()
        {
            // WebSocket 연결이 열려 있는 경우 Dispose 호출
            _webSocket?.Dispose();
        }

        /// <summary>
        /// 매 프레임마다 호출되어 버튼 이벤트 처리
        /// </summary>
        public void Update()
        {
            // 현재 선택된 UI가 이 게임 오브젝트이고, 특정 입력 버튼이 눌렸는지 확인
            if (EventSystem.current.currentSelectedGameObject == gameObject
                && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit는 입력 키 매핑 이름
            {
                NetworkTesting(); // WebSocket 연결 및 메시지 전송 테스트 시작
            }
        }

        /// <summary>
        /// WebSocket 연결 및 테스트 실행
        /// </summary>
        public void NetworkTesting()
        {
            ConnectWebSocketServer(); // WebSocket 서버에 연결
        }

        /// <summary>
        /// WebSocket 서버와의 연결을 설정하고 메시지 송수신 테스트
        /// </summary>
        private async void ConnectWebSocketServer()
        {
            _webSocket = new ClientWebSocket(); // WebSocket 클라이언트 생성

            try
            {
                // 서버 URI로 WebSocket 연결
                await _webSocket.ConnectAsync(new Uri(ServerUri), CancellationToken.None);
                Debug.Log("WebSocket 서버에 연결되었습니다."); // 연결 성공 메시지 출력

                // 서버로 메시지 전송
                await SendMessageAsync("안녕하세요, 서버!");

                // 서버로부터 메시지 수신
                await ReceiveMessageAsync();
            }
            catch (Exception ex)
            {
                // 연결 또는 메시지 송수신 중 오류 발생 시 로그 출력
                Debug.LogError($"WebSocket 연결 오류: {ex.Message}");
            }
        }
    }
}
