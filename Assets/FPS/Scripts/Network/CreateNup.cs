using System;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections.Generic;

public class CreateNup : MonoBehaviour
{
    public GameObject objectToDuplicate;
    private ClientWebSocket _webSocket = WebSocketService.WebSocket;
    public Message response;
    public class GamePlayer
    {
        public string? Id { get; set; } // �÷��̾� ���� ID
        public string? name { get; set; }
        public object? socket { get; set; } // �÷��̾� �̸�
        public Vector3 xyz { get; set; }
        public Vector3 dir { get; set; }
        public float hp { get; set; } = 100;
        public bool isDie { get; set; } = false;
    }
    public class Item
    {
        public int Id { get; set; } // �÷��̾� ���� ID
        public int k { get; set; } // �÷��̾� ���� ID
        public int cnt { get; set; }
        public Vector3 xyz { get; set; }
    }
    public class Bullet
    {
        public string? Id { get; set; } // �÷��̾� ���� ID
        public Vector3 xyz { get; set; }
        public float dir { get; set; } = 0;
    }
    public class Message
    {
        public string t { get; set; } // �޽��� Ÿ�� (��: "REQUEST_NAVER_LOGIN")
        public List<GamePlayer> p; // �޽��� ������
        public List<Item> i; // �޽��� ������
        public List<Bullet> b; // �޽��� ������
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await ListenForMessages();
    }

    // Update is called once per frame
    void Update()
    {

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
                    Debug.Log($"���� ���� ����: {responseMessage}");

                    // JSON ������ȭ
                    response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    if (response?.t == "RE")
                    {
                        SpawnObjects();
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
    void SpawnObjects()
    {
        foreach (Transform child in transform) // �θ� ������Ʈ �Ʒ� ��� �ڽ� ��ü ��ȸ
        {
            Destroy(child.gameObject);
        }
        // �Էµ� ��ġ�� ���⿡ ���� ��ü ����
        for (int i = 0; i < response.p.Count; i++)
        {   
            var obj = response.p[i];
            // ��ġ ����
            Vector3 position = obj.xyz;

            // ȸ�� ���� (Euler Angles�� Quaternion���� ��ȯ)
            Quaternion rotation = Quaternion.Euler(obj.dir);

            // ��ü ���� �� ��ġ
            GameObject newObject = Instantiate(objectToDuplicate, position, rotation);

            // �̸� ���� (���� ����)
            newObject.name = $"{objectToDuplicate.name}_Copy_{i + 1}";

            //Debug.Log($"Spawned: {newObject.name} at {position} with rotation {spawnRotations[i]}");
        }
    }
}
