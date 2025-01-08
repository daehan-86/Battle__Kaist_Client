using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BackToMainPage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Update()
    {
        // 현재 선택된 UI가 이 게임 오브젝트이고, 특정 입력 버튼이 눌렸는지 확인
        if (EventSystem.current.currentSelectedGameObject == gameObject
            && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit는 입력 키 매핑 이름
        {
            BackMain(); // WebSocket 연결 및 메시지 전송 테스트 시작
        }
    }
    public void BackMain()
    {
        SceneManager.LoadScene("MainHomeScene");
    }
}
