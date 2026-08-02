using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>현재 씬의 모든 버튼 클릭에 맑고 선명한 공용 효과음을 연결합니다.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class SceneButtonAudio : MonoBehaviour
{
    // 모든 버튼을 클릭할 때 재생할 공용 효과음입니다.
    [SerializeField] private AudioClip clickClip;
    // 버튼 효과음을 출력하는 2D 오디오 소스입니다.
    private AudioSource clickSource;
    // 종료할 때 클릭 이벤트를 해제하기 위해 저장한 버튼 목록입니다.
    private readonly List<Button> registeredButtons = new List<Button>();

    /// <summary>에디터 구성 코드에서 공용 버튼 효과음을 연결합니다.</summary>
    /// <param name="clip">씬의 버튼에 사용할 클릭 효과음입니다.</param>
    public void Configure(AudioClip clip)
    {
        clickClip = clip;
    }

    /// <summary>버튼 효과음 전용 오디오 소스를 낮은 음량의 2D 재생 방식으로 설정합니다.</summary>
    private void Awake()
    {
        clickSource = GetComponent<AudioSource>();
        clickSource.playOnAwake = false;
        clickSource.loop = false;
        clickSource.spatialBlend = 0f;
        clickSource.volume = 0.52f;
    }

    /// <summary>비활성화된 종료 UI 버튼을 포함해 현재 씬의 모든 버튼에 클릭음을 등록합니다.</summary>
    private void Start()
    {
        Button[] sceneButtons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (Button sceneButton in sceneButtons)
        {
            if (sceneButton == null)
            {
                continue;
            }

            sceneButton.onClick.AddListener(PlayClick);
            registeredButtons.Add(sceneButton);
        }
    }

    /// <summary>씬이 닫힐 때 등록했던 모든 버튼 클릭 이벤트를 해제합니다.</summary>
    private void OnDestroy()
    {
        foreach (Button registeredButton in registeredButtons)
        {
            if (registeredButton != null)
            {
                registeredButton.onClick.RemoveListener(PlayClick);
            }
        }
        registeredButtons.Clear();
    }

    /// <summary>연결된 맑은 버튼 클릭 효과음을 한 번 재생합니다.</summary>
    private void PlayClick()
    {
        if (clickSource == null || clickClip == null)
        {
            return;
        }

        clickSource.PlayOneShot(clickClip);
    }
}
