using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>현재 화면에 활성화된 버튼 UI가 있을 때만 마우스 커서를 표시합니다.</summary>
[DefaultExecutionOrder(1000)]
public sealed class CursorVisibilityController : MonoBehaviour
{
    // 모든 씬에서 공유할 커서 표시 관리자 인스턴스입니다.
    private static CursorVisibilityController instance;
    // 마지막 검사에서 결정된 커서 표시 상태입니다.
    private bool cursorShouldBeVisible;

    /// <summary>현재 버튼 UI 기준으로 결정된 커서 표시 상태를 제공합니다.</summary>
    public bool CursorShouldBeVisible => cursorShouldBeVisible;

    /// <summary>게임의 첫 씬이 로드되기 전에 전역 커서 관리자를 생성합니다.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBeforeFirstScene()
    {
        if (instance != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject(
            "Global Cursor Visibility");
        instance =
            controllerObject.AddComponent<CursorVisibilityController>();
    }

    /// <summary>중복 관리자를 제거하고 씬 전환 후에도 유지되도록 설정합니다.</summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyCursorVisibility(false);
    }

    /// <summary>매 프레임 UI 활성 상태 변경이 모두 끝난 뒤 커서 상태를 갱신합니다.</summary>
    private void LateUpdate()
    {
        RefreshCursorVisibility();
    }

    /// <summary>애플리케이션이 다시 활성화되면 현재 UI 상태에 맞춰 커서를 복원합니다.</summary>
    /// <param name="hasFocus">게임 창이 입력 포커스를 가지고 있는지 여부입니다.</param>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus == true)
        {
            RefreshCursorVisibility();
        }
    }

    /// <summary>관리자가 제거될 때 씬 로드 이벤트와 정적 참조를 정리합니다.</summary>
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>새 씬이 로드되면 이전 씬의 커서 상태를 지우고 새 UI를 다시 검사합니다.</summary>
    /// <param name="scene">새로 로드된 씬 정보입니다.</param>
    /// <param name="loadMode">씬이 단독 또는 추가 방식으로 로드됐는지 나타냅니다.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        ApplyCursorVisibility(false);
        RefreshCursorVisibility();
    }

    /// <summary>현재 활성화된 버튼 UI가 존재하는지 검사해 커서 표시 여부를 결정합니다.</summary>
    private void RefreshCursorVisibility()
    {
        Button[] activeButtons = Object.FindObjectsByType<Button>(
            FindObjectsSortMode.None);
        bool hasVisibleButton = false;
        foreach (Button activeButton in activeButtons)
        {
            bool buttonIsVisible =
                activeButton != null &&
                activeButton.isActiveAndEnabled == true &&
                activeButton.gameObject.activeInHierarchy == true;
            if (buttonIsVisible == true)
            {
                hasVisibleButton = true;
                break;
            }
        }

        if (hasVisibleButton != cursorShouldBeVisible ||
            Cursor.visible != hasVisibleButton)
        {
            ApplyCursorVisibility(hasVisibleButton);
        }
    }

    /// <summary>계산된 표시 상태를 시스템 커서와 내부 상태에 동시에 적용합니다.</summary>
    /// <param name="shouldBeVisible">커서를 화면에 표시해야 하는지 여부입니다.</param>
    private void ApplyCursorVisibility(bool shouldBeVisible)
    {
        cursorShouldBeVisible = shouldBeVisible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = shouldBeVisible;
    }
}
