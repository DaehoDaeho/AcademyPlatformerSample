using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>게임 종료용 팡파레와 실패음을 생성하고 모든 스테이지 HUD에 종료 연출을 연결합니다.</summary>
public static class GameEndPresentationBuilder
{
    // 생성한 클리어 팡파레를 저장할 프로젝트 경로입니다.
    private const string ClearClipPath =
        "Assets/AcademyPlatformer/Audio/GameClearFanfare.wav";
    // 생성한 게임 오버 효과음을 저장할 프로젝트 경로입니다.
    private const string GameOverClipPath =
        "Assets/AcademyPlatformer/Audio/GameOverSad.wav";
    // 생성할 오디오의 초당 샘플 수입니다.
    private const int SampleRate = 44100;

    /// <summary>두 효과음을 생성하고 세 스테이지의 종료 연출 컴포넌트에 연결합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Rebuild Game End Presentation")]
    public static void Build()
    {
        CreateToneSequence(
            ClearClipPath,
            new float[] { 523.25f, 659.25f, 783.99f, 1046.5f },
            new float[] { 0.18f, 0.18f, 0.24f, 0.82f },
            0.42f,
            true);
        CreateToneSequence(
            GameOverClipPath,
            new float[] { 392f, 329.63f, 261.63f, 196f },
            new float[] { 0.3f, 0.34f, 0.42f, 0.9f },
            0.34f,
            false);
        AssetDatabase.ImportAsset(
            ClearClipPath,
            ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(
            GameOverClipPath,
            ImportAssetOptions.ForceSynchronousImport);

        AudioClip clearClip =
            AssetDatabase.LoadAssetAtPath<AudioClip>(ClearClipPath);
        AudioClip gameOverClip =
            AssetDatabase.LoadAssetAtPath<AudioClip>(GameOverClipPath);
        int stageNumber = 1;
        while (stageNumber <= StageProgressData.TotalStageCount)
        {
            ConfigureStage(
                stageNumber,
                clearClip,
                gameOverClip);
            stageNumber++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("GAME_END_PRESENTATION_BUILD_COMPLETED");
    }

    /// <summary>지정한 스테이지 HUD에 게임 종료 연출 컴포넌트와 효과음을 연결합니다.</summary>
    /// <param name="stageNumber">구성할 스테이지 번호입니다.</param>
    /// <param name="clearClip">연결할 클리어 팡파레입니다.</param>
    /// <param name="gameOverClip">연결할 게임 오버 효과음입니다.</param>
    private static void ConfigureStage(
        int stageNumber,
        AudioClip clearClip,
        AudioClip gameOverClip)
    {
        string scenePath =
            "Assets/Scenes/Stage" + stageNumber + ".unity";
        Scene scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single);
        GameHUD hud =
            UnityEngine.Object.FindFirstObjectByType<GameHUD>();
        if (hud == null)
        {
            throw new InvalidOperationException(
                "Stage " + stageNumber + "에서 HUD를 찾을 수 없습니다.");
        }

        GameEndPresentation presentation =
            hud.GetComponent<GameEndPresentation>();
        if (presentation == null)
        {
            presentation =
                hud.gameObject.AddComponent<GameEndPresentation>();
        }

        presentation.Configure(clearClip, gameOverClip);
        EditorUtility.SetDirty(presentation);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>지정한 음계와 길이로 16비트 모노 WAV 효과음을 생성합니다.</summary>
    /// <param name="assetPath">WAV 파일을 저장할 프로젝트 경로입니다.</param>
    /// <param name="frequencies">순서대로 재생할 음의 주파수 배열입니다.</param>
    /// <param name="durations">각 음이 유지될 시간 배열입니다.</param>
    /// <param name="volume">전체 음량입니다.</param>
    /// <param name="brightTone">밝은 배음을 사용할지 여부입니다.</param>
    private static void CreateToneSequence(
        string assetPath,
        float[] frequencies,
        float[] durations,
        float volume,
        bool brightTone)
    {
        float totalDuration = 0f;
        foreach (float duration in durations)
        {
            totalDuration += duration;
        }

        int totalSampleCount =
            Mathf.CeilToInt(totalDuration * SampleRate);
        short[] samples = new short[totalSampleCount];
        int writeIndex = 0;
        int noteIndex = 0;
        while (noteIndex < frequencies.Length)
        {
            int noteSampleCount =
                Mathf.CeilToInt(durations[noteIndex] * SampleRate);
            int noteSampleIndex = 0;
            while (noteSampleIndex < noteSampleCount &&
                writeIndex < samples.Length)
            {
                float noteTime =
                    (float)noteSampleIndex / SampleRate;
                float noteProgress =
                    (float)noteSampleIndex / noteSampleCount;
                float envelope = CalculateEnvelope(noteProgress);
                float baseWave = Mathf.Sin(
                    Mathf.PI * 2f * frequencies[noteIndex] * noteTime);
                float harmonicStrength = brightTone == true ? 0.32f : 0.14f;
                float harmonicWave = Mathf.Sin(
                    Mathf.PI * 4f * frequencies[noteIndex] * noteTime) *
                    harmonicStrength;
                float sampleValue =
                    (baseWave + harmonicWave) * envelope * volume;
                samples[writeIndex] = (short)Mathf.Clamp(
                    sampleValue * short.MaxValue,
                    short.MinValue,
                    short.MaxValue);
                noteSampleIndex++;
                writeIndex++;
            }

            noteIndex++;
        }

        WriteWaveFile(assetPath, samples);
    }

    /// <summary>음의 시작과 끝이 튀지 않도록 빠른 시작과 부드러운 감쇠 곡선을 계산합니다.</summary>
    /// <param name="progress">현재 음의 0에서 1 사이 진행 비율입니다.</param>
    /// <returns>현재 샘플에 곱할 음량 비율입니다.</returns>
    private static float CalculateEnvelope(float progress)
    {
        float attack = Mathf.Clamp01(progress / 0.06f);
        float release = Mathf.Clamp01((1f - progress) / 0.24f);
        return attack * release;
    }

    /// <summary>16비트 모노 PCM 샘플을 표준 WAV 파일로 저장합니다.</summary>
    /// <param name="assetPath">프로젝트 기준 저장 경로입니다.</param>
    /// <param name="samples">저장할 오디오 샘플 배열입니다.</param>
    private static void WriteWaveFile(
        string assetPath,
        short[] samples)
    {
        string projectRoot =
            Directory.GetParent(Application.dataPath).FullName;
        string absolutePath = Path.Combine(projectRoot, assetPath);
        int dataSize = samples.Length * sizeof(short);
        using (FileStream stream = File.Create(absolutePath))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);
            foreach (short sample in samples)
            {
                writer.Write(sample);
            }
        }
    }
}
