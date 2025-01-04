using System.Net.WebSockets; // WebSocket ���� Ŭ����
using System.Text; // ���ڿ� ���ڵ�/���ڵ�
using System.Threading; // �񵿱� �۾����� ���Ǵ� CancellationToken
using System; // �⺻ �ý��� Ŭ���� (���� ó��, URI ��)
using Unity.FPS.Game; // FPS ���� ���� Unity ���ӽ����̽�
using UnityEngine; // Unity ���� �ھ� Ŭ����
using UnityEngine.EventSystems; // Unity�� �̺�Ʈ �ý��� (��ư Ŭ�� ��)

namespace Unity.FPS.Network // FPS ���� ��Ʈ��ũ ���� ���ӽ����̽�
{
    public class NetworkTestButton : MonoBehaviour
    {
        public string SceneName = ""; // �ε��� �� �̸�(���� �̻��)
        private ClientWebSocket _webSocket; // WebSocket Ŭ���̾�Ʈ �ν��Ͻ�
        public string ServerUri = "ws://172.10.7.27:5000"; // ������ WebSocket ���� URI

        /// <summary>
        /// ������ �޽��� ����
        /// </summary>
        private async System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            // �޽����� UTF8�� ����Ʈ �迭�� ��ȯ
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // ������ �޽��� ����
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("������ �޽����� ���½��ϴ�."); // Unity ����� â�� �α� ���
        }

        /// <summary>
        /// �����κ��� �޽��� ����
        /// </summary>
        private async System.Threading.Tasks.Task ReceiveMessageAsync()
        {
            // �޽��� ���ſ� ���� ����
            var buffer = new byte[1024];

            // �����κ��� �޽��� ����
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // �޽����� �ؽ�Ʈ ������ ��� ó��
            if (result.MessageType == WebSocketMessageType.Text)
            {
                // ������ �޽����� UTF8 ���ڿ��� ���ڵ�
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"�����κ��� �޽��� ����: {message}"); // Unity ����� â�� ���
            }
        }

        /// <summary>
        /// ���ø����̼� ���� �� WebSocket ���ҽ� ����
        /// </summary>
        private void OnApplicationQuit()
        {
            // WebSocket ������ ���� �ִ� ��� Dispose ȣ��
            _webSocket?.Dispose();
        }

        /// <summary>
        /// �� �����Ӹ��� ȣ��Ǿ� ��ư �̺�Ʈ ó��
        /// </summary>
        public void Update()
        {
            // ���� ���õ� UI�� �� ���� ������Ʈ�̰�, Ư�� �Է� ��ư�� ���ȴ��� Ȯ��
            if (EventSystem.current.currentSelectedGameObject == gameObject
                && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit�� �Է� Ű ���� �̸�
            {
                NetworkTesting(); // WebSocket ���� �� �޽��� ���� �׽�Ʈ ����
            }
        }

        /// <summary>
        /// WebSocket ���� �� �׽�Ʈ ����
        /// </summary>
        public void NetworkTesting()
        {
            ConnectWebSocketServer(); // WebSocket ������ ����
        }

        /// <summary>
        /// WebSocket �������� ������ �����ϰ� �޽��� �ۼ��� �׽�Ʈ
        /// </summary>
        private async void ConnectWebSocketServer()
        {
            _webSocket = new ClientWebSocket(); // WebSocket Ŭ���̾�Ʈ ����

            try
            {
                // ���� URI�� WebSocket ����
                await _webSocket.ConnectAsync(new Uri(ServerUri), CancellationToken.None);
                Debug.Log("WebSocket ������ ����Ǿ����ϴ�."); // ���� ���� �޽��� ���

                // ������ �޽��� ����
                await SendMessageAsync("�ȳ��ϼ���, ����!");

                // �����κ��� �޽��� ����
                await ReceiveMessageAsync();
            }
            catch (Exception ex)
            {
                // ���� �Ǵ� �޽��� �ۼ��� �� ���� �߻� �� �α� ���
                Debug.LogError($"WebSocket ���� ����: {ex.Message}");
            }
        }
    }
}
