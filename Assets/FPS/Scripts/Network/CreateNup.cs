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
    private List<GameObject> spawnedObjects = new List<GameObject>(); // 복제된 객체 목록
    public class GamePlayer
    {
        public string Id { get; set; } // 플레이어 고유 ID
        public string name { get; set; }
        public object socket { get; set; } // 플레이어 이름
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
        public string Id { get; set; } // 플레이어 고유 ID
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
        Debug.Log("0000");
        await Task.Delay(1000);

        Debug.Log("1111");
        await ListenForMessages();
        Debug.Log("5555");
    }

    // Update is called once per frame
    void Update()
    {

    }
    private bool isProcessing = false;
    private async Task ListenForMessages()
    {
        var buffer = new byte[1024 * 8];

        Debug.Log("2222");
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                Debug.Log("3333");
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                Debug.Log("4444");
                if (isProcessing)
                {
                    Debug.LogWarning("이미 작업 중입니다. 새 요청을 무시합니다.");
                    continue; // 요청 무시
                }
                isProcessing = true;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var responseMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"서버 응답 수신: {responseMessage}");

                    // JSON 역직렬화
                    response = JsonConvert.DeserializeObject<Message>(responseMessage);
                    //Debug.Log(response.ToString());
                    if (response?.t == "RE")
                    {
                        Debug.Log("aaaa");
                        SpawnObjects();
                    }
                    Debug.Log("aaaa");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("WebSocket 연결이 종료되었습니다.");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "닫힘", CancellationToken.None);
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON 역직렬화 오류: {ex.Message}");
                response = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"111메시지 수신 오류: {ex.Message}");
            }
            finally
            {
                isProcessing = false; // 작업 완료
            }
        }
    }
    void SpawnObjects()
    {
        int count = response.p.Count;

        // 필요한 만큼 객체 생성
        while (spawnedObjects.Count < count)
        {
            GameObject newObject = Instantiate(objectToDuplicate, Vector3.zero, Quaternion.identity);
            spawnedObjects.Add(newObject);
        }

        // 남은 객체 삭제 (불필요한 객체 제거)
        while (spawnedObjects.Count > count)
        {
            Destroy(spawnedObjects[spawnedObjects.Count - 1]);
            spawnedObjects.RemoveAt(spawnedObjects.Count - 1);
        }

        // 위치 및 방향 업데이트
        for (int i = 0; i < count; i++)
        {
            var obj = response.p[i];
            Vector3 position = new Vector3(obj.xyz.x,obj.xyz.y,obj.xyz.z);
            Quaternion rotation = Quaternion.Euler(new Vector3(obj.dir.x, obj.dir.y, obj.dir.z));

            // Transform 업데이트
            spawnedObjects[i].transform.position = Vector3.Lerp(spawnedObjects[i].transform.position, position, Time.deltaTime * 10f);
            spawnedObjects[i].transform.rotation = Quaternion.Slerp(spawnedObjects[i].transform.rotation, rotation, Time.deltaTime * 10f);
        }
        //// 입력된 위치와 방향에 따라 객체 생성
        //for (int i = 0; i < response.p.Count; i++)
        //{   
        //    var obj = response.p[i];
        //    // 위치 설정
        //    Vector3 position = obj.xyz;

        //    // 회전 설정 (Euler Angles를 Quaternion으로 변환)
        //    Quaternion rotation = Quaternion.Euler(obj.dir);

        //    // 객체 복사 및 배치
        //    GameObject newObject = Instantiate(objectToDuplicate, position, rotation);

        //    // 이름 지정 (선택 사항)
        //    newObject.name = $"{objectToDuplicate.name}_Copy_{i + 1}";

        //    //Debug.Log($"Spawned: {newObject.name} at {position} with rotation {spawnRotations[i]}");
        //}
    }
}
