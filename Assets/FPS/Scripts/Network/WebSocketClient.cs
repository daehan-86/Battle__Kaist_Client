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
            //Uri serverUri = new Uri("ws://<����_IP>:5000"); // ���� IP�� �Է��ϼ���
            try
            {
                await _webSocket.ConnectAsync(new Uri(ServerUri), CancellationToken.None);
                Debug.Log("WebSocket ������ ����Ǿ����ϴ�.");

                // ������ �޽��� ����
                await SendMessageAsync("�ȳ��ϼ���, ����!");

                // �����κ��� �޽��� ����
                await ReceiveMessageAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket ���� ����: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("������ �޽����� ���½��ϴ�.");
        }

        private async System.Threading.Tasks.Task ReceiveMessageAsync()
        {
            var buffer = new byte[1024];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"�����κ��� �޽��� ����: {message}");
            }
        }

        private void OnApplicationQuit()
        {
            _webSocket?.Dispose();
        }
    }

}