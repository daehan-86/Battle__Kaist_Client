using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.FPS.Gameplay;
using Newtonsoft.Json;

public class PlayerStateSender : MonoBehaviour
{
    private ClientWebSocket _webSocket;
    public PlayerCharacterController PlayerController; // PlayerCharacterController 연결
    public PlayerWeaponsManager WeaponManager; // PlayerCharacterController 연결
    public float sendInterval = 0.02f; // 전송 간격
    Quaternion cameraRotation = Camera.main.transform.rotation;
    private float _lastSendTime;

    void Start()
    {
        // WebSocket 초기화
        _webSocket = WebSocketService.WebSocket;
        try
        {
            Debug.Log("WebSocket 연결 성공!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WebSocket 연결 실패: {ex.Message}");
        }
    }

    void Update()
    {
        cameraRotation = Camera.main.transform.rotation;
        // 지정된 간격마다 위치 데이터 전송
        if (Time.time - _lastSendTime >= sendInterval)
        {
            SendPlayerState();
            _lastSendTime = Time.time;
        }
    }

    async void SendPlayerState()
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;

        // 위치 및 방향 정보 가져오기
        Vector3 position = PlayerController.CurrentPosition;
        Vector3 rotationEuler = PlayerController.CurrentRotation.eulerAngles; // Quaternion → Euler 변환
        float health = PlayerController.CurrentHealth;

        // 데이터 생성
        PlayerState playerState = new PlayerState
        {
            Type = "re",
            position = new Vector33 { x = position.x, y = position.y, z = position.z },
            rotation = new Vector33 { x = cameraRotation.eulerAngles.x, y = rotationEuler.y, z = rotationEuler.z },
            health = health, // 체력 정보 추가
            hasFired = Input.GetMouseButton(0),
            b_position = new Vector33 { x = WeaponManager.Weapon_p.x, y = WeaponManager.Weapon_p.y, z = WeaponManager.Weapon_p.z }
        };
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        // JSON 직렬화
        string jsonMessage = JsonConvert.SerializeObject(playerState);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        //Debug.Log("게임 참가 메시지를 서버로 전송했습니다.");
    }

    private void OnApplicationQuit()
    {
        if (_webSocket != null)
        {
            _webSocket.Dispose();
        }
    }
}

[System.Serializable]
public class PlayerState
{
    public string Type;
    public Vector33 position;
    public Vector33 rotation;
    public float health; // 체력 정보 추가
    public bool hasFired;
    public Vector33 b_position;
}

public class Vector33
{
    public float x;
    public float y;
    public float z;
}
