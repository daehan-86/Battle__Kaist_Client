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
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

public class CreateNup : MonoBehaviour
{
    public GameObject objectToDuplicate;
    private ClientWebSocket _webSocket = WebSocketService.WebSocket;
    [SerializeField] private WeaponController weaponController;
    //[SerializeField] private Vector3 firePosition;
    //[SerializeField] private Vector3 fireDirection;
    public Message response;
    private List<GameObject> spawnedObjects = new List<GameObject>(); // 복제된 객체 목록
    public class GamePlayer
    {
        public string Id { get; set; } // 플레이어 고유 ID
        public string name { get; set; }
        public string naverId { get; set; }
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
        public string naverId { get; set; } // 플레이어 고유 ID
        public Vector3 xyz { get; set; }
        public Vector3 dir { get; set; }
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
        if (response != null && response.p != null)
        {
            SpawnObjects(); // 프레임마다 위치 및 회전 업데이트
            ShootEvent();
        }
    }
    private bool isProcessing = false;
    private async Task ListenForMessages()
    {
        var buffer = new byte[1024 * 8];

        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
                    if (response?.t == "WIN")
                    {
                        //SpawnObjects();
                        //await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                        SceneManager.LoadScene("WinScene");
                    }
                    else if(response?.t == "YOU_DIE")
                    {
                        //await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                        SceneManager.LoadScene("LoseScene");
                    }
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
        int k = 0;
        for (int i = 0; i < count; i++)
        {
            var obj = response.p[i];
            if (obj.naverId == WebSocketService.naverId)
            {
                continue;
            }
            Vector3 position = new Vector3(obj.xyz.x,obj.xyz.y+1,obj.xyz.z);
            Quaternion rotation = Quaternion.Euler(new Vector3(0, obj.dir.y+90, obj.dir.z));
            // Transform 업데이트
            spawnedObjects[k].transform.position = Vector3.Lerp(spawnedObjects[k].transform.position, position, Time.deltaTime * 10f);
            spawnedObjects[k].transform.rotation = Quaternion.Slerp(spawnedObjects[k].transform.rotation, rotation, Time.deltaTime * 10f);
            k++;
        }
    }
    void ShootEvent()
    {
        int count = response.b.Count;

        for(int i = 0; i < count; i++)
        {
            var obj = response.b[i];
            if (obj.naverId == WebSocketService.naverId)
            {
                continue;
            }
            Vector3 shootPosition = new Vector3(obj.xyz.x, obj.xyz.y, obj.xyz.z); // 발사 위치
            float pitchRad = obj.dir.x * Mathf.Deg2Rad;
            float yawRad = obj.dir.y * Mathf.Deg2Rad;

            // 2) 방향 벡터 계산
            Vector3 direction;
            direction.x = Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
            direction.y = -Mathf.Sin(pitchRad);
            direction.z = Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

            // 3) 필요하다면 정규화
            direction.Normalize();
            SpawnBullet(shootPosition, direction);
        }
        // 필요한 만큼 객체 생성
        response.b.Clear();
    }

    public void SpawnBullet(Vector3 position, Vector3 direction)
    {
        // ProjectileBase prefab을 직접 Instantiate
        ProjectileBase newProjectile = Instantiate(weaponController.ProjectilePrefab, position, Quaternion.LookRotation(direction));


        // 별도의 Shoot 로직 필요하면 호출
        newProjectile.Shoot(gameObject,position,direction); // or override Shoot if needed
    }
}
