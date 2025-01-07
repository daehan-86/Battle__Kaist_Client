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
    private List<GameObject> spawnedObjects = new List<GameObject>(); // ������ ��ü ���
    public class GamePlayer
    {
        public string Id { get; set; } // �÷��̾� ���� ID
        public string name { get; set; }
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
        public string Id { get; set; } // �÷��̾� ���� ID
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
                    if (response?.t == "RE")
                    {
                        Debug.Log("aaaa");
                        SpawnObjects();
                    }
                    Debug.Log("aaaa");
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
        for (int i = 0; i < count; i++)
        {
            var obj = response.p[i];
            Vector3 position = new Vector3(obj.xyz.x,obj.xyz.y,obj.xyz.z);
            Quaternion rotation = Quaternion.Euler(new Vector3(obj.dir.x, obj.dir.y, obj.dir.z));

            // Transform ������Ʈ
            spawnedObjects[i].transform.position = Vector3.Lerp(spawnedObjects[i].transform.position, position, Time.deltaTime * 10f);
            spawnedObjects[i].transform.rotation = Quaternion.Slerp(spawnedObjects[i].transform.rotation, rotation, Time.deltaTime * 10f);
        }
        //// �Էµ� ��ġ�� ���⿡ ���� ��ü ����
        //for (int i = 0; i < response.p.Count; i++)
        //{   
        //    var obj = response.p[i];
        //    // ��ġ ����
        //    Vector3 position = obj.xyz;

        //    // ȸ�� ���� (Euler Angles�� Quaternion���� ��ȯ)
        //    Quaternion rotation = Quaternion.Euler(obj.dir);

        //    // ��ü ���� �� ��ġ
        //    GameObject newObject = Instantiate(objectToDuplicate, position, rotation);

        //    // �̸� ���� (���� ����)
        //    newObject.name = $"{objectToDuplicate.name}_Copy_{i + 1}";

        //    //Debug.Log($"Spawned: {newObject.name} at {position} with rotation {spawnRotations[i]}");
        //}
    }
}
