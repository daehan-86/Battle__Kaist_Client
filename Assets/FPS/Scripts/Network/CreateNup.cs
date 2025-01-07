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
        public string? Id { get; set; } // 플레이어 고유 ID
        public string? name { get; set; }
        public object? socket { get; set; } // 플레이어 이름
        public Vector3 xyz { get; set; }
        public Vector3 dir { get; set; }
        public float hp { get; set; } = 100;
        public bool isDie { get; set; } = false;
    }
    public class Item
    {
        public int Id { get; set; } // 플레이어 고유 ID
        public int k { get; set; } // 플레이어 고유 ID
        public int cnt { get; set; }
        public Vector3 xyz { get; set; }
    }
    public class Bullet
    {
        public string? Id { get; set; } // 플레이어 고유 ID
        public Vector3 xyz { get; set; }
        public float dir { get; set; } = 0;
    }
    public class Message
    {
        public string t { get; set; } // 메시지 타입 (예: "REQUEST_NAVER_LOGIN")
        public List<GamePlayer> p; // 메시지 데이터
        public List<Item> i; // 메시지 데이터
        public List<Bullet> b; // 메시지 데이터
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
                    Debug.Log($"서버 응답 수신: {responseMessage}");

                    // JSON 역직렬화
                    response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    if (response?.t == "RE")
                    {
                        SpawnObjects();
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
    void SpawnObjects()
    {
        foreach (Transform child in transform) // 부모 오브젝트 아래 모든 자식 객체 순회
        {
            Destroy(child.gameObject);
        }
        // 입력된 위치와 방향에 따라 객체 생성
        for (int i = 0; i < response.p.Count; i++)
        {   
            var obj = response.p[i];
            // 위치 설정
            Vector3 position = obj.xyz;

            // 회전 설정 (Euler Angles를 Quaternion으로 변환)
            Quaternion rotation = Quaternion.Euler(obj.dir);

            // 객체 복사 및 배치
            GameObject newObject = Instantiate(objectToDuplicate, position, rotation);

            // 이름 지정 (선택 사항)
            newObject.name = $"{objectToDuplicate.name}_Copy_{i + 1}";

            //Debug.Log($"Spawned: {newObject.name} at {position} with rotation {spawnRotations[i]}");
        }
    }
}
