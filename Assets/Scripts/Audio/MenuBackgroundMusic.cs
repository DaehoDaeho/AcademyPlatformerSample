using UnityEngine;

/// <summary>스테이지 선택 화면에서 은은한 전용 배경음악을 낮은 음량으로 반복 재생합니다.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class MenuBackgroundMusic : MonoBehaviour
{
    // 스테이지 선택 화면에서 반복 재생할 전용 음악 클립입니다.
    [SerializeField] private AudioClip musicClip;
    // 메뉴 조작을 방해하지 않도록 낮게 유지할 음악 음량입니다.
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.11f;
    // 음악을 출력할 2D 오디오 소스입니다.
    private AudioSource musicSource;

    /// <summary>에디터 생성 코드에서 메뉴 음악과 음량을 설정합니다.</summary>
    /// <param name="clip">스테이지 선택 화면에서 재생할 음악입니다.</param>
    /// <param name="volume">0에서 1 사이의 재생 음량입니다.</param>
    public void Configure(AudioClip clip, float volume)
    {
        musicClip = clip;
        musicVolume = Mathf.Clamp01(volume);
    }

    /// <summary>메뉴 음악 전용 오디오 소스를 2D 반복 재생 방식으로 설정합니다.</summary>
    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    /// <summary>씬에 미리 연결된 스테이지 선택 배경음악을 재생합니다.</summary>
    private void Start()
    {
        if (musicClip == null)
        {
            Debug.LogError(
                "StageSelect 씬의 MenuBackgroundMusic에 음악이 연결되지 않았습니다.");
            return;
        }

        musicSource.clip = musicClip;
        musicSource.Play();
    }
}
