using UnityEngine;

/// <summary>플레이어 사망 이벤트를 받아 화면 전체를 흑백으로 전환합니다.</summary>
public sealed class DeathScreenGrayscale : MonoBehaviour
{
    // 밝기를 흰색으로 바꿀 기준값입니다.
    [SerializeField, Range(0f, 1f)] private float whiteThreshold = 0.48f;
    // 사망 이벤트를 제공하는 게임 관리자입니다.
    private GameManager gameManager;

    /// <summary>현재 화면에 적용된 흑백 효과의 비율을 제공합니다.</summary>
    public float CurrentWeight
    {
        get
        {
            return Shader.GetGlobalFloat("BinaryEffectEnabled");
        }
    }

    /// <summary>게임 관리자를 찾아 사망 연출 시작 이벤트를 구독합니다.</summary>
    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.DeathSequenceStarted += Play;
        }

        Shader.SetGlobalFloat("BinaryEffectEnabled", 0f);
        Shader.SetGlobalFloat("BinaryThreshold", whiteThreshold);
    }

    /// <summary>오브젝트가 제거될 때 등록했던 사망 이벤트를 해제합니다.</summary>
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.DeathSequenceStarted -= Play;
        }
    }

    /// <summary>화면 전체의 흑백 전환 코루틴을 시작합니다.</summary>
    public void Play()
    {
        Shader.SetGlobalFloat("BinaryThreshold", whiteThreshold);
        Shader.SetGlobalFloat("BinaryEffectEnabled", 1f);
    }

    /// <summary>에디터 생성 코드에서 흰색과 검은색을 나눌 밝기 기준을 설정합니다.</summary>
    /// <param name="threshold">이 값보다 밝은 픽셀을 흰색으로 바꿀 기준입니다.</param>
    public void Configure(float threshold)
    {
        whiteThreshold = Mathf.Clamp01(threshold);
    }
}
