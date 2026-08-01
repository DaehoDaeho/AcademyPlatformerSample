using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>현재 스테이지에 맞는 배경음악을 낮은 음량으로 반복 재생하고 종료 연출 시작 시 정지합니다.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class StageBackgroundMusic : MonoBehaviour
{
    // 현재 스테이지에서 반복 재생할 배경음악 클립입니다.
    [SerializeField] private AudioClip stageMusicClip;
    // 효과음보다 작게 유지할 배경음악 기본 음량입니다.
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.18f;
    // 배경음악을 반복 재생할 전용 오디오 소스입니다.
    private AudioSource musicSource;
    // 현재 씬 이름에서 확인한 스테이지 번호입니다.
    private int stageNumber;

    /// <summary>에디터 생성 코드에서 스테이지 음악과 재생 음량을 설정합니다.</summary>
    /// <param name="musicClip">현재 스테이지에서 재생할 음악 클립입니다.</param>
    /// <param name="volume">0에서 1 사이로 적용할 음악 음량입니다.</param>
    public void Configure(AudioClip musicClip, float volume)
    {
        stageMusicClip = musicClip;
        musicVolume = Mathf.Clamp01(volume);
    }

    /// <summary>스테이지 번호를 확인하고 음악 전용 오디오 소스를 설정합니다.</summary>
    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        stageNumber = ReadStageNumber();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    /// <summary>현재 스테이지 음악을 불러와 재생하고 게임 종료 연출 이벤트를 등록합니다.</summary>
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeathSequenceStarted += StopMusic;
            GameManager.Instance.ClearSequenceStarted += StopMusic;
            GameManager.Instance.GameFinished += StopMusicAfterFinish;
        }

        if (stageMusicClip == null)
        {
            Debug.LogError(
                "Stage " + stageNumber +
                " 씬의 StageBackgroundMusic에 배경음악이 연결되지 않았습니다.");
            return;
        }

        musicSource.clip = stageMusicClip;
        musicSource.Play();
    }

    /// <summary>오브젝트가 제거될 때 게임 종료 관련 이벤트 등록을 해제합니다.</summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeathSequenceStarted -= StopMusic;
            GameManager.Instance.ClearSequenceStarted -= StopMusic;
            GameManager.Instance.GameFinished -= StopMusicAfterFinish;
        }
    }

    /// <summary>현재 씬 이름 끝에 있는 숫자를 스테이지 번호로 변환합니다.</summary>
    /// <returns>유효한 스테이지 번호이며 변환할 수 없으면 1입니다.</returns>
    private int ReadStageNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string numberText = sceneName.Replace("Stage", string.Empty);
        int parsedStageNumber;
        if (int.TryParse(numberText, out parsedStageNumber) == false)
        {
            return 1;
        }

        return Mathf.Clamp(
            parsedStageNumber,
            1,
            StageProgressData.TotalStageCount);
    }

    /// <summary>게임오버 또는 클리어 연출이 시작되는 즉시 배경음악을 정지합니다.</summary>
    private void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>종료 연출 없이 게임이 끝나는 예외 상황에서도 배경음악을 정지합니다.</summary>
    /// <param name="won">스테이지 클리어 여부이며 음악 정지에는 사용하지 않습니다.</param>
    private void StopMusicAfterFinish(bool won)
    {
        StopMusic();
    }
}
