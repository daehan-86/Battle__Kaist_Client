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
    private List<GameObject> spawnedObjects = new List<GameObject>(); // ������ ��ü ���
    public class GamePlayer
    {
        public string Id { get; set; } // �÷��̾� ���� ID
        public string name { get; set; }
        public string naverId { get; set; }
        public object socket { get; set; } // �÷��̾� �̸�
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
        public string naverId { get; set; } // �÷��̾� ���� ID
        public Vector3 xyz { get; set; }
        public Vector3 dir { get; set; }
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
        if (response != null && response.p != null)
        {
            SpawnObjects(); // �����Ӹ��� ��ġ �� ȸ�� ������Ʈ
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
                    Debug.LogWarning("�̹� �۾� ���Դϴ�. �� ��û�� �����մϴ�.");
                    continue; // ��û ����
                }
                isProcessing = true;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var responseMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"���� ���� ����: {responseMessage}");

                    // JSON ������ȭ
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
                    Debug.Log("WebSocket ������ ����Ǿ����ϴ�.");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "����", CancellationToken.None);
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON ������ȭ ����: {ex.Message}");
                response = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"111�޽��� ���� ����: {ex.Message}");
            }
            finally
            {
                isProcessing = false; // �۾� �Ϸ�
            }
        }
    }
    void SpawnObjects()
    {
        int count = response.p.Count;

        // �ʿ��� ��ŭ ��ü ����
        while (spawnedObjects.Count < count)
        {
            GameObject newObject = Instantiate(objectToDuplicate, Vector3.zero, Quaternion.identity);
            spawnedObjects.Add(newObject);
        }

        // ���� ��ü ���� (���ʿ��� ��ü ����)
        while (spawnedObjects.Count > count)
        {
            Destroy(spawnedObjects[spawnedObjects.Count - 1]);
            spawnedObjects.RemoveAt(spawnedObjects.Count - 1);
        }

        // ��ġ �� ���� ������Ʈ
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
            // Transform ������Ʈ
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
            Vector3 shootPosition = new Vector3(obj.xyz.x, obj.xyz.y, obj.xyz.z); // �߻� ��ġ
            float pitchRad = obj.dir.x * Mathf.Deg2Rad;
            float yawRad = obj.dir.y * Mathf.Deg2Rad;

            // 2) ���� ���� ���
            Vector3 direction;
            direction.x = Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
            direction.y = -Mathf.Sin(pitchRad);
            direction.z = Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

            // 3) �ʿ��ϴٸ� ����ȭ
            direction.Normalize();
            SpawnBullet(shootPosition, direction);
        }
        // �ʿ��� ��ŭ ��ü ����
        response.b.Clear();
    }

    public void SpawnBullet(Vector3 position, Vector3 direction)
    {
        // ProjectileBase prefab�� ���� Instantiate
        ProjectileBase newProjectile = Instantiate(weaponController.ProjectilePrefab, position, Quaternion.LookRotation(direction));


        // ������ Shoot ���� �ʿ��ϸ� ȣ��
        newProjectile.Shoot(gameObject,position,direction); // or override Shoot if needed
    }
}
