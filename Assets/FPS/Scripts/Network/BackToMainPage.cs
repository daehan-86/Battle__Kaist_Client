using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BackToMainPage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Update()
    {
        // ���� ���õ� UI�� �� ���� ������Ʈ�̰�, Ư�� �Է� ��ư�� ���ȴ��� Ȯ��
        if (EventSystem.current.currentSelectedGameObject == gameObject
            && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)) // GameConstants.k_ButtonNameSubmit�� �Է� Ű ���� �̸�
        {
            BackMain(); // WebSocket ���� �� �޽��� ���� �׽�Ʈ ����
        }
    }
    public void BackMain()
    {
        SceneManager.LoadScene("MainHomeScene");
    }
}
