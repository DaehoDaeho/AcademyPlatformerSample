using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>타이틀 화면의 클릭과 키보드 입력을 받아 스테이지 선택 화면으로 이동합니다.</summary>
public sealed class TitleScreenController : MonoBehaviour, IPointerClickHandler
{
    // 시작 입력을 받았을 때 불러올 씬의 이름입니다.
    [SerializeField] private string gameSceneName = "StageSelect";
    // 여러 입력이 동시에 들어와 씬 전환이 중복 요청되는 것을 막는 값입니다.
    private bool hasRequestedStart;

    /// <summary>화면 전체가 클릭되면 스테이지 선택 화면으로 이동합니다.</summary>
    /// <param name="eventData">클릭 위치를 포함하는 포인터 이벤트 정보입니다.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        StartGame();
    }

    /// <summary>Enter 또는 Space 키를 누르면 스테이지 선택 화면으로 이동합니다.</summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) == true ||
            Input.GetKeyDown(KeyCode.Space) == true)
        {
            StartGame();
        }
    }

    /// <summary>씬 전환이 중복되지 않도록 확인한 뒤 페이드 전환을 요청합니다.</summary>
    public void StartGame()
    {
        if (hasRequestedStart == true)
        {
            return;
        }

        hasRequestedStart = true;
        SceneFadeController.LoadSceneWithFade(gameSceneName);
    }

    /// <summary>에디터 생성 코드에서 이동할 씬의 이름을 설정합니다.</summary>
    /// <param name="sceneName">불러올 씬의 이름입니다.</param>
    public void Configure(string sceneName)
    {
        gameSceneName = sceneName;
    }
}
