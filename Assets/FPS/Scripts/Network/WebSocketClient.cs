using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Unity.FPS.Network
{
    public class WebSocketClient : MonoBehaviour
    {
        private ClientWebSocket _webSocket;
        public string ServerUri = "ws://172.10.7.27:5000";

        async void Start()
        {
            _webSocket = new ClientWebSocket();
            //Uri serverUri = new Uri("ws://<서버_IP>:5000"); // 서버 IP를 입력하세요
            try
            {
                await _webSocket.ConnectAsync(new Uri(ServerUri), CancellationToken.None);
                Debug.Log("WebSocket 서버에 연결되었습니다.");

                // 서버에 메시지 전송
                await SendMessageAsync("안녕하세요, 서버!");

                // 서버로부터 메시지 수신
                await ReceiveMessageAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket 연결 오류: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("서버에 메시지를 보냈습니다.");
        }

        private async System.Threading.Tasks.Task ReceiveMessageAsync()
        {
            var buffer = new byte[1024];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"서버로부터 메시지 수신: {message}");
            }
        }

        private void OnApplicationQuit()
        {
            _webSocket?.Dispose();
        }
    }

}