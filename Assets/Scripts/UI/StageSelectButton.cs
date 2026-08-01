using UnityEngine;
using UnityEngine.UI;

/// <summary>스테이지 하나의 해금 상태를 표시하고 입장 요청을 처리합니다.</summary>
public sealed class StageSelectButton : MonoBehaviour
{
    // 이 버튼이 나타내는 스테이지 번호입니다.
    [SerializeField, Min(1)] private int stageNumber = 1;
    // 클릭 가능 여부를 제어할 버튼입니다.
    [SerializeField] private Button button;
    // 스테이지 이름과 잠금 상태를 표시할 문구입니다.
    [SerializeField] private Text labelText;
    // 스테이지별 콘셉트를 설명하는 문구입니다.
    [SerializeField] private string conceptName;
    // 오버월드 노드의 현재 상태 색상을 표시할 이미지입니다.
    [SerializeField] private Image nodeImage;
    // 스테이지의 클리어, 도전 가능 또는 잠금 상태를 표시할 문구입니다.
    [SerializeField] private Text statusText;
    // 잠긴 스테이지 위에 표시할 자물쇠 문구 오브젝트입니다.
    [SerializeField] private GameObject lockObject;
    // 스테이지가 해금됐을 때 노드에 사용할 고유 색상입니다.
    [SerializeField] private Color unlockedColor = Color.white;

    /// <summary>저장된 진행도를 읽어 버튼의 잠금 상태를 표시합니다.</summary>
    private void Start()
    {
        bool isUnlocked =
            StageProgressData.IsStageUnlocked(stageNumber);
        button.interactable = isUnlocked;
        bool isCleared =
            StageProgressData.IsStageCleared(stageNumber);
        if (isUnlocked == true)
        {
            labelText.text =
                stageNumber.ToString();
            nodeImage.color = isCleared == true
                ? new Color(0.18f, 0.78f, 0.42f, 1f)
                : unlockedColor;
            statusText.text = isCleared == true
                ? "CLEARED"
                : "AVAILABLE";
            statusText.color = isCleared == true
                ? new Color(0.55f, 1f, 0.68f, 1f)
                : new Color(1f, 0.9f, 0.45f, 1f);
            lockObject.SetActive(false);
        }
        else
        {
            labelText.text =
                stageNumber.ToString();
            nodeImage.color = new Color(0.18f, 0.2f, 0.24f, 0.92f);
            statusText.text = "LOCKED";
            statusText.color = new Color(0.62f, 0.66f, 0.72f, 1f);
            lockObject.SetActive(true);
        }
    }

    /// <summary>해금된 스테이지를 페이드 효과와 함께 불러옵니다.</summary>
    public void EnterStage()
    {
        if (StageProgressData.IsStageUnlocked(stageNumber) == false)
        {
            return;
        }

        SceneFadeController.LoadSceneWithFade(
            StageProgressData.GetStageSceneName(stageNumber));
    }

    /// <summary>에디터 생성 코드에서 버튼에 필요한 정보를 설정합니다.</summary>
    /// <param name="number">이 버튼이 나타낼 스테이지 번호입니다.</param>
    /// <param name="targetButton">클릭 가능 여부를 제어할 버튼입니다.</param>
    /// <param name="label">상태를 표시할 문구입니다.</param>
    /// <param name="concept">스테이지 콘셉트 이름입니다.</param>
    public void Configure(
        int number,
        Button targetButton,
        Text label,
        string concept,
        Image icon,
        Text status,
        GameObject lockIcon,
        Color stageColor)
    {
        stageNumber = number;
        button = targetButton;
        labelText = label;
        conceptName = concept;
        nodeImage = icon;
        statusText = status;
        lockObject = lockIcon;
        unlockedColor = stageColor;
    }
}
