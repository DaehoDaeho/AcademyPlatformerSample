using UnityEngine;

/// <summary>스테이지 선택 화면에서 타이틀로 돌아가는 기능을 제공합니다.</summary>
public sealed class StageSelectScreenController : MonoBehaviour
{
    /// <summary>페이드 효과와 함께 타이틀 화면으로 돌아갑니다.</summary>
    public void ReturnToTitle()
    {
        SceneFadeController.LoadSceneWithFade("Title");
    }
}
