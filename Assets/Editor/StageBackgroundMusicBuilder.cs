using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>세 스테이지의 분위기가 서로 다른 오리지널 루프 배경음악을 생성합니다.</summary>
public static class StageBackgroundMusicBuilder
{
    // 생성한 음악 파일을 저장할 Resources 폴더입니다.
    private const string MusicFolder = "Assets/Resources/Audio";
    // 음악 파형을 생성할 초당 샘플 수입니다.
    private const int SampleRate = 44100;
    // 각 배경음악이 반복되기 전까지 재생되는 시간입니다.
    private const float MusicDuration = 16f;

    /// <summary>밝음, 몽환, 긴장 콘셉트의 스테이지 음악 세 곡을 생성합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Build Stage Background Music")]
    public static void Build()
    {
        Directory.CreateDirectory(MusicFolder);
        CreateStageMusic(1, 120f, 60, 0.2f, 0.12f);
        CreateStageMusic(2, 96f, 57, 0.16f, 0.06f);
        CreateStageMusic(3, 138f, 52, 0.22f, 0.16f);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureMusicImporters();
        ConfigureStageScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("STAGE_BACKGROUND_MUSIC_BUILD_COMPLETED");
    }

    /// <summary>세 음악 파일이 씬 시작 전에 오디오 데이터를 미리 읽도록 임포트 설정을 적용합니다.</summary>
    private static void ConfigureMusicImporters()
    {
        int stageNumber = 1;
        while (stageNumber <= StageProgressData.TotalStageCount)
        {
            string musicPath =
                MusicFolder + "/StageMusic" + stageNumber + ".wav";
            AudioImporter musicImporter =
                AssetImporter.GetAtPath(musicPath) as AudioImporter;
            if (musicImporter != null)
            {
                AudioImporterSampleSettings sampleSettings =
                    musicImporter.defaultSampleSettings;
                sampleSettings.preloadAudioData = true;
                musicImporter.defaultSampleSettings = sampleSettings;
                musicImporter.loadInBackground = false;
                musicImporter.SaveAndReimport();
            }
            stageNumber++;
        }
    }

    /// <summary>모든 스테이지의 게임 관리자에 전용 음악 재생기와 음악 클립을 직접 연결합니다.</summary>
    private static void ConfigureStageScenes()
    {
        string previousScenePath = SceneManager.GetActiveScene().path;
        int stageNumber = 1;
        while (stageNumber <= StageProgressData.TotalStageCount)
        {
            string scenePath =
                "Assets/Scenes/Stage" + stageNumber + ".unity";
            Scene stageScene = EditorSceneManager.OpenScene(
                scenePath,
                OpenSceneMode.Single);
            GameManager gameManager =
                Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError(
                    "Stage " + stageNumber + "에서 GameManager를 찾을 수 없습니다.");
                stageNumber++;
                continue;
            }

            StageBackgroundMusic musicPlayer =
                gameManager.GetComponent<StageBackgroundMusic>();
            if (musicPlayer == null)
            {
                musicPlayer =
                    gameManager.gameObject.AddComponent<StageBackgroundMusic>();
            }

            AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                MusicFolder + "/StageMusic" + stageNumber + ".wav");
            musicPlayer.Configure(musicClip, 0.18f);
            EditorUtility.SetDirty(musicPlayer);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);
            stageNumber++;
        }

        if (string.IsNullOrEmpty(previousScenePath) == false)
        {
            EditorSceneManager.OpenScene(
                previousScenePath,
                OpenSceneMode.Single);
        }
    }

    /// <summary>지정한 템포와 음색으로 한 스테이지의 반복 음악 파형을 생성합니다.</summary>
    /// <param name="stageNumber">음악을 사용할 스테이지 번호입니다.</param>
    /// <param name="tempo">분당 박자 수입니다.</param>
    /// <param name="rootMidi">곡의 기준이 되는 MIDI 음 번호입니다.</param>
    /// <param name="melodyVolume">멜로디 음량입니다.</param>
    /// <param name="rhythmVolume">리듬 음량입니다.</param>
    private static void CreateStageMusic(
        int stageNumber,
        float tempo,
        int rootMidi,
        float melodyVolume,
        float rhythmVolume)
    {
        int totalSampleCount = Mathf.RoundToInt(MusicDuration * SampleRate);
        short[] samples = new short[totalSampleCount];
        float beatDuration = 60f / tempo;
        int[] melodyPattern = GetMelodyPattern(stageNumber);
        int[] chordPattern = GetChordPattern(stageNumber);
        int sampleIndex = 0;
        while (sampleIndex < totalSampleCount)
        {
            float songTime = (float)sampleIndex / SampleRate;
            int melodyStep = Mathf.FloorToInt(songTime / (beatDuration * 0.5f));
            float melodyStepTime =
                songTime % (beatDuration * 0.5f);
            float melodyProgress =
                melodyStepTime / (beatDuration * 0.5f);
            int melodyOffset =
                melodyPattern[melodyStep % melodyPattern.Length];
            float melodyFrequency = MidiToFrequency(rootMidi + melodyOffset);
            float melodyEnvelope = CalculateNoteEnvelope(melodyProgress);
            float melodyWave = Mathf.Sin(
                Mathf.PI * 2f * melodyFrequency * melodyStepTime);
            melodyWave += Mathf.Sin(
                Mathf.PI * 4f * melodyFrequency * melodyStepTime) * 0.22f;

            int chordStep = Mathf.FloorToInt(songTime / (beatDuration * 2f));
            int chordOffset = chordPattern[chordStep % chordPattern.Length];
            float bassFrequency = MidiToFrequency(rootMidi - 24 + chordOffset);
            float bassWave = Mathf.Sin(
                Mathf.PI * 2f * bassFrequency * songTime) * 0.7f;

            float beatTime = songTime % beatDuration;
            float beatProgress = beatTime / beatDuration;
            float kickFrequency = Mathf.Lerp(105f, 48f, beatProgress);
            float kickWave = Mathf.Sin(
                Mathf.PI * 2f * kickFrequency * beatTime) *
                Mathf.Exp(-11f * beatProgress);
            float hatWave = Mathf.Sin(
                Mathf.PI * 2f * 4300f * songTime) *
                Mathf.Exp(-35f *
                    ((songTime % (beatDuration * 0.5f)) /
                    (beatDuration * 0.5f)));

            float musicSample =
                melodyWave * melodyEnvelope * melodyVolume +
                bassWave * 0.11f +
                kickWave * rhythmVolume +
                hatWave * rhythmVolume * 0.28f;
            samples[sampleIndex] = (short)Mathf.Clamp(
                musicSample * short.MaxValue,
                short.MinValue,
                short.MaxValue);
            sampleIndex++;
        }

        string assetPath =
            MusicFolder + "/StageMusic" + stageNumber + ".wav";
        WriteWaveFile(assetPath, samples);
    }

    /// <summary>스테이지 콘셉트에 맞는 반복 멜로디 음정 배열을 반환합니다.</summary>
    /// <param name="stageNumber">패턴을 선택할 스테이지 번호입니다.</param>
    /// <returns>기준음에서 더할 반음 간격 배열입니다.</returns>
    private static int[] GetMelodyPattern(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new int[] { 0, 4, 7, 12, 7, 4, 2, 7, 5, 9, 12, 16, 12, 9, 7, 4 };
        }
        if (stageNumber == 2)
        {
            return new int[] { 0, 3, 7, 10, 7, 3, -2, 3, 5, 8, 12, 8, 5, 3, 0, -2 };
        }
        return new int[] { 0, 3, 7, 3, 10, 7, 3, 0, 2, 5, 8, 5, 12, 8, 5, 2 };
    }

    /// <summary>스테이지 콘셉트에 맞는 베이스 코드 진행을 반환합니다.</summary>
    /// <param name="stageNumber">진행을 선택할 스테이지 번호입니다.</param>
    /// <returns>기준음에서 더할 베이스 반음 간격 배열입니다.</returns>
    private static int[] GetChordPattern(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new int[] { 0, 5, 9, 7, 0, 5, 7, 0 };
        }
        if (stageNumber == 2)
        {
            return new int[] { 0, 8, 5, 10, 0, 5, 3, 10 };
        }
        return new int[] { 0, 3, 8, 7, 0, 8, 10, 7 };
    }

    /// <summary>MIDI 음 번호를 실제 파형 생성에 사용할 주파수로 변환합니다.</summary>
    /// <param name="midiNote">변환할 MIDI 음 번호입니다.</param>
    /// <returns>헤르츠 단위의 주파수입니다.</returns>
    private static float MidiToFrequency(int midiNote)
    {
        return 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
    }

    /// <summary>각 음의 처음과 끝에서 잡음이 생기지 않도록 음량 곡선을 계산합니다.</summary>
    /// <param name="progress">현재 음의 0에서 1 사이 진행 비율입니다.</param>
    /// <returns>현재 시점에 적용할 음량 비율입니다.</returns>
    private static float CalculateNoteEnvelope(float progress)
    {
        float attack = Mathf.Clamp01(progress / 0.08f);
        float release = Mathf.Clamp01((1f - progress) / 0.24f);
        return attack * release;
    }

    /// <summary>생성한 16비트 모노 샘플을 Unity가 읽을 수 있는 WAV 파일로 저장합니다.</summary>
    /// <param name="assetPath">프로젝트 기준 WAV 파일 경로입니다.</param>
    /// <param name="samples">파일에 기록할 오디오 샘플 배열입니다.</param>
    private static void WriteWaveFile(string assetPath, short[] samples)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
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
