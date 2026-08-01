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

    /// <summary>저장된 진행도를 읽어 버튼의 잠금 상태를 표시합니다.</summary>
    private void Start()
    {
        bool isUnlocked =
            StageProgressData.IsStageUnlocked(stageNumber);
        button.interactable = isUnlocked;
        if (isUnlocked == true)
        {
            labelText.text =
                "STAGE " + stageNumber + "\n" + conceptName;
        }
        else
        {
            labelText.text =
                "STAGE " + stageNumber + "\nLOCKED";
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
        string concept)
    {
        stageNumber = number;
        button = targetButton;
        labelText = label;
        conceptName = concept;
    }
}
