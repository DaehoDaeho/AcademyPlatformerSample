using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>수업용 플랫포머의 애셋, 프리팹과 메인 씬을 자동으로 생성합니다.</summary>
public static class SampleProjectBuilder
{
    // 자동 생성 애셋의 루트 경로를 저장하는 상수입니다.
    private const string Root = "Assets/AcademyPlatformer";
    // 자동 생성 아트 애셋 경로를 저장하는 상수입니다.
    private const string Art = Root + "/Art";
    // 자동 생성 애니메이션 경로를 저장하는 상수입니다.
    private const string Animations = Root + "/Animations";
    // 오디오 애셋 경로를 저장하는 상수입니다.
    private const string Audio = Root + "/Audio";
    // 타일 애셋 경로를 저장하는 상수입니다.
    private const string TileAssets = Root + "/Tiles";
    // 물리 머티리얼 경로를 저장하는 상수입니다.
    private const string Physics = Root + "/Physics";
    // 자동 생성 이펙트 에셋 경로를 저장하는 상수입니다.
    private const string Effects = Root + "/Effects";
    // 프리팹 경로를 저장하는 상수입니다.
    private const string Prefabs = Root + "/Prefabs";
    // 씬 경로를 저장하는 상수입니다.
    private const string Scenes = "Assets/Scenes";

    [InitializeOnLoadMethod]
    /// <summary>스크립트 컴파일 후 프리팹의 누락된 시각 요소를 자동 복구합니다.</summary>
    private static void RepairMissingVisualsAfterCompile()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "/Player.prefab"); // prefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            SpriteRenderer renderer = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>() : null; // renderer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Animator animator = prefab != null ? prefab.GetComponentInChildren<Animator>() : null; // animator 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            PlayerDeathSequence deathSequence =
                prefab != null ? prefab.GetComponent<PlayerDeathSequence>() : null; // 새 사망 연출이 프리팹에 적용됐는지 확인합니다.
            PlayerClearSequence clearSequence =
                prefab != null ? prefab.GetComponent<PlayerClearSequence>() : null; // 새 클리어 연출이 프리팹에 적용됐는지 확인합니다.
            PlayerVfxFeedback vfxFeedback =
                prefab != null ? prefab.GetComponent<PlayerVfxFeedback>() : null; // 상황별 플레이어 이펙트가 프리팹에 적용됐는지 확인합니다.
            if (renderer != null &&
                (renderer.sprite == null ||
                animator == null ||
                animator.runtimeAnimatorController == null ||
                deathSequence == null ||
                clearSequence == null ||
                vfxFeedback == null))
            {
                Debug.Log("Missing sprite references detected. Rebuilding Academy Platformer visuals...");
                Build();
            }
        };
    }

    [MenuItem("Tools/Academy Platformer/Rebuild Sample")]
    /// <summary>수업용 프로젝트의 모든 생성 애셋과 메인 씬을 다시 만듭니다.</summary>
    public static void Build()
    {
        EnsureFolder("Assets", "AcademyPlatformer");
        EnsureFolder(Root, "Art");
        EnsureFolder(Root, "Animations");
        EnsureFolder(Root, "Audio");
        EnsureFolder(Root, "Tiles");
        EnsureFolder(Root, "Physics");
        EnsureFolder(Root, "Effects");
        EnsureFolder(Root, "Prefabs");
        EnsureFolder("Assets", "Scenes");
        EnsureGroundLayer();

        Sprite square = MakeSprite("Square", new Color(1f, 1f, 1f, 1f)); // square 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Sprite circle = MakeCircleSprite(); // circle 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        CharacterAnimationAssets playerAnimations = CreatePlayerAnimations(); // playerAnimations 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        CharacterAnimationAssets patrolEnemyAnimations = CreateEnemyAnimations(
            "Pink Man", "PatrolEnemy", 10f, 14f); // 순찰형 적의 애니메이션 묶음입니다.
        CharacterAnimationAssets chasingEnemyAnimations = CreateEnemyAnimations(
            "Virtual Guy", "ChasingEnemy", 10f, 14f); // 추적형 적의 애니메이션 묶음입니다.
        CharacterAnimationAssets rangedEnemyAnimations = CreateEnemyAnimations(
            "Ninja Frog", "RangedEnemy", 8f, 10f); // 원거리 적의 대기 애니메이션 묶음입니다.
        TerrainTiles terrainTiles = CreateTerrainTiles(); // terrainTiles 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        PhysicsMaterial2D frictionlessMaterial = CreateFrictionlessMaterial(); // frictionlessMaterial 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AudioClip jumpClip = LoadRequiredAsset<AudioClip>(Audio + "/GameJump.wav"); // jumpClip 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AudioClip collectClip = LoadRequiredAsset<AudioClip>(Audio + "/GameCollectStar.wav"); // collectClip 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AudioClip damagedClip = LoadRequiredAsset<AudioClip>(Audio + "/GamePlayerDamaged.wav"); // damagedClip 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AudioClip stompClip = LoadRequiredAsset<AudioClip>(Audio + "/GameEnemyStomp.wav"); // stompClip 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Material particleMaterial = CreateParticleMaterial(); // URP에서 원샷 파티클을 표시할 공용 머티리얼입니다.
        GameObject collectibleEffectPrefab = CreateParticleEffectPrefab(
            "StarCollectEffect", particleMaterial,
            new Color(1f, 0.92f, 0.2f), Color.white,
            16, 0.5f, 2.2f, 4.8f, 0.13f, 0.24f, 0.15f); // Star 획득 시 퍼지는 금빛 이펙트입니다.
        GameObject stompEffectPrefab = CreateParticleEffectPrefab(
            "EnemyStompEffect", particleMaterial,
            new Color(1f, 0.72f, 0.18f), new Color(1f, 0.95f, 0.75f),
            12, 0.38f, 2.5f, 5f, 0.12f, 0.22f, 0.2f); // 적 밟기 시 퍼지는 충격 이펙트입니다.
        GameObject damagedEffectPrefab = CreateParticleEffectPrefab(
            "PlayerDamagedEffect", particleMaterial,
            new Color(1f, 0.12f, 0.08f), new Color(1f, 0.55f, 0.12f),
            14, 0.42f, 3f, 5.5f, 0.14f, 0.26f, 0.18f); // 플레이어 피격 시 퍼지는 붉은 이펙트입니다.
        GameObject playerPrefab = CreatePlayerPrefab( // playerPrefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            playerAnimations, frictionlessMaterial, jumpClip, collectClip, damagedClip, stompClip,
            collectibleEffectPrefab, stompEffectPrefab, damagedEffectPrefab);
        GameObject patrolEnemyPrefab = CreatePatrolEnemyPrefab(patrolEnemyAnimations); // 순찰형 적 프리팹입니다.
        GameObject chasingEnemyPrefab = CreateChasingEnemyPrefab(
            chasingEnemyAnimations); // 추적형 적 프리팹입니다.
        GameObject projectilePrefab = CreateEnemyProjectilePrefab(circle); // 느리게 이동하는 적 투사체 프리팹입니다.
        GameObject rangedEnemyPrefab = CreateRangedEnemyPrefab(
            rangedEnemyAnimations, projectilePrefab); // 투사체를 발사하는 원거리 적 프리팹입니다.
        GameObject starPrefab = CreateStarPrefab(circle); // starPrefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        GameObject goalPrefab = CreateGoalPrefab(square); // goalPrefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        BuildScene(playerPrefab, patrolEnemyPrefab, chasingEnemyPrefab, rangedEnemyPrefab,
            starPrefab, goalPrefab, terrainTiles);
        MultiStageBuilder.BuildStageScenes();
        TitleScreenBuilder.BuildTitleScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Academy Platformer sample rebuilt successfully.");
    }

    /// <summary>누락된 파티클 이펙트 세 종류를 다시 만들고 플레이어 프리팹에 연결합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Rebuild Player Effects")]
    public static void RebuildPlayerEffects()
    {
        Material particleMaterial = CreateParticleMaterial();
        GameObject collectibleEffectPrefab = CreateParticleEffectPrefab(
            "StarCollectEffect",
            particleMaterial,
            new Color(1f, 0.92f, 0.2f),
            Color.white,
            16,
            0.5f,
            2.2f,
            4.8f,
            0.13f,
            0.24f,
            0.15f);
        GameObject stompEffectPrefab = CreateParticleEffectPrefab(
            "EnemyStompEffect",
            particleMaterial,
            new Color(1f, 0.72f, 0.18f),
            new Color(1f, 0.95f, 0.75f),
            12,
            0.38f,
            2.5f,
            5f,
            0.12f,
            0.22f,
            0.2f);
        GameObject damagedEffectPrefab = CreateParticleEffectPrefab(
            "PlayerDamagedEffect",
            particleMaterial,
            new Color(1f, 0.12f, 0.08f),
            new Color(1f, 0.55f, 0.12f),
            14,
            0.42f,
            3f,
            5.5f,
            0.14f,
            0.26f,
            0.18f);

        string playerPrefabPath = Prefabs + "/Player.prefab";
        GameObject playerRoot =
            PrefabUtility.LoadPrefabContents(playerPrefabPath);
        PlayerVfxFeedback vfxFeedback =
            playerRoot.GetComponent<PlayerVfxFeedback>();
        if (vfxFeedback != null)
        {
            vfxFeedback.Configure(
                collectibleEffectPrefab,
                stompEffectPrefab,
                damagedEffectPrefab);
            PrefabUtility.SaveAsPrefabAsset(
                playerRoot,
                playerPrefabPath);
        }

        PrefabUtility.UnloadPrefabContents(playerRoot);
        AssetDatabase.SaveAssets();
        Debug.Log("PLAYER_EFFECTS_REBUILT");
    }

    /// <summary>지정한 상위 폴더 안에 필요한 하위 폴더가 존재하도록 보장합니다.</summary>
    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child; // path 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (AssetDatabase.IsValidFolder(path) == false)
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    /// <summary>지정한 경로에서 필수 애셋을 불러오고 누락 시 예외를 발생시킵니다.</summary>
    private static T LoadRequiredAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"Required asset is missing: {path}");
        }
        return asset;
    }

    /// <summary>플레이어 모서리 걸림을 방지하는 무마찰 물리 머티리얼을 생성합니다.</summary>
    private static PhysicsMaterial2D CreateFrictionlessMaterial()
    {
        const string path = Physics + "/PlayerFrictionless.physicsMaterial2D"; // path 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path); // material 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (material == null)
        {
            material = new PhysicsMaterial2D("Player Frictionless");
            AssetDatabase.CreateAsset(material, path);
        }
        material.friction = 0f;
        material.bounciness = 0f;
        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>프로젝트의 여섯 번째 레이어를 지면 레이어로 설정합니다.</summary>
    private static void EnsureGroundLayer()
    {
        SerializedObject tags = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]); // tags 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        SerializedProperty layers = tags.FindProperty("layers"); // layers 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        SerializedProperty slot = layers.GetArrayElementAtIndex(6); // slot 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (string.IsNullOrEmpty(slot.stringValue) == true)
        {
            slot.stringValue = "Ground";
        }
        tags.ApplyModifiedProperties();
    }

    /// <summary>단색 사각형 텍스처와 스프라이트를 생성합니다.</summary>
    private static Sprite MakeSprite(string name, Color color)
    {
        string path = Art + "/" + name + ".png"; // path 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Texture2D texture = new(16, 16, TextureFormat.RGBA32, false); // texture 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Color[] pixels = new Color[256];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path); // importer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();
        return LoadImportedSprite(path);
    }

    /// <summary>수집 아이템에 사용할 원형 스프라이트를 생성합니다.</summary>
    private static Sprite MakeCircleSprite()
    {
        string path = Art + "/Circle.png"; // path 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Texture2D texture = new(32, 32, TextureFormat.RGBA32, false); // texture 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Color[] pixels = new Color[1024];
        Vector2 center = new(15.5f, 15.5f); // center 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distanceFromCenter =
                    Vector2.Distance(new Vector2(x, y), center); // 현재 픽셀과 원 중심 사이의 거리입니다.
                bool insideCircle =
                    distanceFromCenter < 14f; // 현재 픽셀이 원 내부에 포함되는지 여부입니다.
                if (insideCircle == true)
                {
                    pixels[y * 32 + x] = Color.white;
                }
                else
                {
                    pixels[y * 32 + x] = Color.clear;
                }
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path); // importer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();
        return LoadImportedSprite(path);
    }

    /// <summary>지정한 경로의 텍스처를 스프라이트로 불러옵니다.</summary>
    private static Sprite LoadImportedSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(); // sprite 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (sprite == null)
        {
            throw new InvalidDataException($"Sprite import failed: {path}");
        }
        return sprite;
    }

    /// <summary>캐릭터 애니메이터 컨트롤러와 기본 스프라이트를 함께 보관합니다.</summary>
    private readonly struct CharacterAnimationAssets
    {
        // 캐릭터가 사용할 런타임 애니메이터 컨트롤러입니다.
        public readonly RuntimeAnimatorController Controller;
        // 애니메이션 재생 전 표시할 기본 스프라이트입니다.
        public readonly Sprite DefaultSprite;

        /// <summary>캐릭터 애니메이션 애셋 묶음을 생성합니다.</summary>
        public CharacterAnimationAssets(RuntimeAnimatorController controller, Sprite defaultSprite)
        {
            Controller = controller;
            DefaultSprite = defaultSprite;
        }
    }

    /// <summary>플레이어의 애니메이션 클립과 컨트롤러를 생성합니다.</summary>
    private static CharacterAnimationAssets CreatePlayerAnimations()
    {
        const string character = "Assets/Sprites/Characters/Mask Dude"; // character 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Sprite[] idleFrames = LoadCharacterFrames(character + "/Idle (32x32).png"); // idleFrames 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimationClip idle = CreateSpriteClip("Player_Idle", idleFrames, 12f, true); // idle 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimationClip run = CreateSpriteClip("Player_Run", LoadCharacterFrames(character + "/Run (32x32).png"), 16f, true); // run 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimationClip jump = CreateSpriteClip("Player_Jump", LoadCharacterFrames(character + "/Jump (32x32).png"), 12f, false); // jump 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.

        string controllerPath = Animations + "/Player.controller"; // controllerPath 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idleState = machine.AddState("Idle"); // idleState 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimatorState runState = machine.AddState("Run"); // runState 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimatorState jumpState = machine.AddState("Jump"); // jumpState 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        idleState.motion = idle;
        runState.motion = run;
        jumpState.motion = jump;
        machine.defaultState = idleState;

        AddTransition(idleState, runState, AnimatorConditionMode.Greater, 0.1f, "Speed");
        AddTransition(runState, idleState, AnimatorConditionMode.Less, 0.1f, "Speed");
        AnimatorStateTransition toJump = machine.AddAnyStateTransition(jumpState); // toJump 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        ConfigureTransition(toJump);
        toJump.canTransitionToSelf = false;
        toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
        AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState); // jumpToIdle 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        ConfigureTransition(jumpToIdle);
        jumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        AnimatorStateTransition jumpToRun = jumpState.AddTransition(runState); // jumpToRun 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        ConfigureTransition(jumpToRun);
        jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        return new CharacterAnimationAssets(controller, idleFrames[0]);
    }

    /// <summary>지정한 캐릭터를 사용하는 적의 애니메이션 클립과 컨트롤러를 생성합니다.</summary>
    /// <param name="characterFolder">캐릭터 스프라이트가 들어 있는 폴더 이름입니다.</param>
    /// <param name="assetPrefix">생성되는 애셋 이름 앞에 붙일 구분 문자열입니다.</param>
    /// <param name="idleFrameRate">대기 애니메이션의 초당 프레임 수입니다.</param>
    /// <param name="runFrameRate">이동 애니메이션의 초당 프레임 수입니다.</param>
    private static CharacterAnimationAssets CreateEnemyAnimations(
        string characterFolder, string assetPrefix, float idleFrameRate, float runFrameRate)
    {
        string character = "Assets/Sprites/Characters/" + characterFolder; // 선택한 캐릭터 폴더의 전체 애셋 경로입니다.
        Sprite[] idleFrames = LoadCharacterFrames(character + "/Idle (32x32).png"); // idleFrames 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimationClip idle = CreateSpriteClip(
            assetPrefix + "_Idle", idleFrames, idleFrameRate, true); // 적의 대기 애니메이션 클립입니다.
        AnimationClip run = CreateSpriteClip(
            assetPrefix + "_Run", LoadCharacterFrames(character + "/Run (32x32).png"),
            runFrameRate, true); // 적의 이동 애니메이션 클립입니다.

        string controllerPath = Animations + "/" + assetPrefix + ".controller"; // 적 종류별 애니메이터 컨트롤러 경로입니다.
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idleState = machine.AddState("Idle"); // idleState 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimatorState runState = machine.AddState("Run"); // runState 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        idleState.motion = idle;
        runState.motion = run;
        machine.defaultState = idleState;
        AddTransition(idleState, runState, AnimatorConditionMode.Greater, 0.1f, "Speed");
        AddTransition(runState, idleState, AnimatorConditionMode.Less, 0.1f, "Speed");
        return new CharacterAnimationAssets(controller, idleFrames[0]);
    }

    /// <summary>스프라이트 시트의 프레임을 번호 순서대로 불러옵니다.</summary>
    private static Sprite[] LoadCharacterFrames(string path)
    {
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path) // frames 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            .OfType<Sprite>()
            .OrderBy(sprite => FrameIndex(sprite.name))
            .ToArray();
        if (frames.Length == 0)
        {
            throw new InvalidDataException($"No sliced sprites found: {path}");
        }
        return frames;
    }

    /// <summary>스프라이트 이름에서 프레임 번호를 추출합니다.</summary>
    private static int FrameIndex(string spriteName)
    {
        int separator = spriteName.LastIndexOf('_'); // 스프라이트 이름에서 번호 앞의 구분자 위치입니다.
        if (separator < 0)
        {
            return 0;
        }
        string indexText = spriteName[(separator + 1)..]; // 구분자 뒤에서 추출한 프레임 번호 문자열입니다.
        bool parsed = int.TryParse(indexText, out int index); // 프레임 번호 문자열을 정수로 변환했는지 여부입니다.
        if (parsed == false)
        {
            return 0;
        }
        return index;
    }

    /// <summary>스프라이트 배열을 사용하는 애니메이션 클립을 생성합니다.</summary>
    private static AnimationClip CreateSpriteClip(string name, Sprite[] frames, float frameRate, bool loop)
    {
        string path = Animations + "/" + name + ".anim"; // path 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AssetDatabase.DeleteAsset(path);
        AnimationClip clip = new() { name = name, frameRate = frameRate }; // clip 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length]; // keys 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        for (int i = 0; i < frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = frames[i] };
        }
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite"); // binding 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        SerializedObject serializedClip = new(clip); // serializedClip 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = loop;
        serializedClip.ApplyModifiedProperties();
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    /// <summary>두 애니메이터 상태 사이에 조건 전환을 추가합니다.</summary>
    private static void AddTransition(AnimatorState from, AnimatorState to,
        AnimatorConditionMode mode, float threshold, string parameter)
    {
        AnimatorStateTransition transition = from.AddTransition(to); // transition 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        ConfigureTransition(transition);
        transition.AddCondition(mode, threshold, parameter);
    }

    /// <summary>애니메이터 전환이 즉시 반응하도록 공통 값을 설정합니다.</summary>
    private static void ConfigureTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0.05f;
    }

    /// <summary>지형의 윗면, 채움과 공중 발판 타일을 함께 보관합니다.</summary>
    private readonly struct TerrainTiles
    {
        // 지면 윗면의 왼쪽 끝 타일입니다.
        public readonly TileBase TopLeft;
        // 지면 윗면의 가운데 타일입니다.
        public readonly TileBase TopMiddle;
        // 지면 윗면의 오른쪽 끝 타일입니다.
        public readonly TileBase TopRight;
        // 지면 채움의 왼쪽 끝 타일입니다.
        public readonly TileBase FillLeft;
        // 지면 채움의 가운데 타일입니다.
        public readonly TileBase FillMiddle;
        // 지면 채움의 오른쪽 끝 타일입니다.
        public readonly TileBase FillRight;
        // 공중 발판의 왼쪽 끝 타일입니다.
        public readonly TileBase FloatingLeft;
        // 공중 발판의 가운데 타일입니다.
        public readonly TileBase FloatingMiddle;
        // 공중 발판의 오른쪽 끝 타일입니다.
        public readonly TileBase FloatingRight;

        /// <summary>지형 생성에 사용할 모든 타일 참조를 저장합니다.</summary>
        public TerrainTiles(TileBase topLeft, TileBase topMiddle, TileBase topRight,
            TileBase fillLeft, TileBase fillMiddle, TileBase fillRight, // fillLeft 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            TileBase floatingLeft, TileBase floatingMiddle, TileBase floatingRight) // floatingLeft 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        {
            TopLeft = topLeft;
            TopMiddle = topMiddle;
            TopRight = topRight;
            FillLeft = fillLeft;
            FillMiddle = fillMiddle;
            FillRight = fillRight;
            FloatingLeft = floatingLeft;
            FloatingMiddle = floatingMiddle;
            FloatingRight = floatingRight;
        }
    }

    /// <summary>지형에 사용할 타일 애셋 묶음을 생성합니다.</summary>
    private static TerrainTiles CreateTerrainTiles()
    {
        return new TerrainTiles(
            CreateTileAsset("Ground_Top_Left", 1),
            CreateTileAsset("Ground_Top_Middle", 2),
            CreateTileAsset("Ground_Top_Right", 3),
            CreateTileAsset("Ground_Fill_Left", 4),
            CreateTileAsset("Ground_Fill_Middle", 5),
            CreateTileAsset("Ground_Fill_Right", 6),
            CreateTileAsset("Platform_Left", 14),
            CreateTileAsset("Platform_Middle", 15),
            CreateTileAsset("Platform_Right", 16));
    }

    /// <summary>타일 스프라이트와 충돌 설정을 포함한 타일 애셋을 생성합니다.</summary>
    private static Tile CreateTileAsset(string assetName, int sourceNumber)
    {
        string sourcePath = $"Assets/Sprites/Tiles/{sourceNumber}.png"; // sourcePath 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sourcePath); // sprite 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (sprite == null)
        {
            throw new InvalidDataException($"Tile sprite import failed: {sourcePath}");
        }
        string assetPath = $"{TileAssets}/{assetName}.asset"; // assetPath 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath); // tile 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        bool isNew = tile == null; // isNew 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (isNew == true)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
        }
        tile.name = assetName;
        tile.sprite = sprite;
        // Full grid collision gives predictable platforming even when artwork has transparent edges.
        tile.colliderType = Tile.ColliderType.Grid;
        if (isNew == true)
        {
            AssetDatabase.CreateAsset(tile, assetPath);
        }
        else
        {
            EditorUtility.SetDirty(tile);
        }
        return tile;
    }

    /// <summary>Tilemap과 통합 충돌체를 생성하고 전체 지형을 배치합니다.</summary>
    private static void CreateTerrainTilemap(TerrainTiles tiles)
    {
        GameObject gridObject = new("Grid"); // gridObject 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        GameObject groundObject = new("Ground Tilemap"); // groundObject 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        groundObject.layer = 6;
        groundObject.transform.SetParent(gridObject.transform);
        Tilemap tilemap = groundObject.AddComponent<Tilemap>(); // tilemap 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        TilemapRenderer renderer = groundObject.AddComponent<TilemapRenderer>(); // renderer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        renderer.sortingOrder = 0;

        TilemapCollider2D tilemapCollider = groundObject.AddComponent<TilemapCollider2D>(); // tilemapCollider 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        Rigidbody2D body = groundObject.AddComponent<Rigidbody2D>(); // body 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        body.bodyType = RigidbodyType2D.Static;
        body.simulated = true;
        CompositeCollider2D composite = groundObject.AddComponent<CompositeCollider2D>(); // composite 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;

        // Six ground islands create readable chapters while short gaps keep every jump fair.
        PaintGroundSection(tilemap, tiles, -2, 7, -4);
        PaintGroundSection(tilemap, tiles, 10, 18, -4);
        PaintGroundSection(tilemap, tiles, 22, 29, -4);
        PaintGroundSection(tilemap, tiles, 33, 42, -4);
        PaintGroundSection(tilemap, tiles, 46, 53, -4);
        PaintGroundSection(tilemap, tiles, 57, 67, -4);

        // Bridges keep at least two cells of headroom above ground for the 1.3-unit player collider.
        PaintFloatingPlatform(tilemap, tiles, 6, 9, -1);
        PaintFloatingPlatform(tilemap, tiles, 12, 14, -1);
        PaintFloatingPlatform(tilemap, tiles, 16, 18, 0);
        PaintFloatingPlatform(tilemap, tiles, 20, 23, -1);
        PaintFloatingPlatform(tilemap, tiles, 25, 28, -1);
        PaintFloatingPlatform(tilemap, tiles, 30, 33, 1);
        PaintFloatingPlatform(tilemap, tiles, 34, 37, 3);
        PaintFloatingPlatform(tilemap, tiles, 36, 39, -1);
        PaintFloatingPlatform(tilemap, tiles, 41, 44, 0);
        PaintFloatingPlatform(tilemap, tiles, 48, 50, -1);
        PaintFloatingPlatform(tilemap, tiles, 52, 54, 0);
        PaintFloatingPlatform(tilemap, tiles, 55, 58, -1);
        PaintFloatingPlatform(tilemap, tiles, 59, 61, 0);
        PaintFloatingPlatform(tilemap, tiles, 62, 66, 2);
        PaintFloatingPlatform(tilemap, tiles, 66, 70, 5);
        PaintFloatingPlatform(tilemap, tiles, 61, 65, 8);
        PaintFloatingPlatform(tilemap, tiles, 67, 72, 11);
        PaintFloatingPlatform(tilemap, tiles, 61, 65, 14);
        PaintFloatingPlatform(tilemap, tiles, 66, 71, 17);
        PaintFloatingPlatform(tilemap, tiles, 60, 65, 20);
        PaintFloatingPlatform(tilemap, tiles, 66, 72, 23);
        PaintFloatingPlatform(tilemap, tiles, 61, 65, 26);
        PaintFloatingPlatform(tilemap, tiles, 66, 71, 29);
        PaintFloatingPlatform(tilemap, tiles, 72, 79, 32);
        tilemap.CompressBounds();
        if (tilemap.GetUsedTilesCount() == 0)
        {
            throw new InvalidDataException("Terrain Tilemap contains no painted cells.");
        }
        tilemapCollider.ProcessTilemapChanges();
        composite.GenerateGeometry();
        if (composite.pathCount == 0)
        {
            throw new InvalidDataException("Terrain CompositeCollider2D contains no generated paths.");
        }
        EditorUtility.SetDirty(tilemap);
    }

    /// <summary>윗면과 채움 타일로 지면 구간을 그립니다.</summary>
    private static void PaintGroundSection(Tilemap tilemap, TerrainTiles tiles, int startX, int endX, int topY)
    {
        PaintStrip(tilemap, startX, endX, topY, tiles.TopLeft, tiles.TopMiddle, tiles.TopRight);
        PaintStrip(tilemap, startX, endX, topY - 1, tiles.FillLeft, tiles.FillMiddle, tiles.FillRight);
    }

    /// <summary>한 줄로 구성된 공중 발판을 그립니다.</summary>
    private static void PaintFloatingPlatform(Tilemap tilemap, TerrainTiles tiles, int startX, int endX, int y)
    {
        PaintStrip(tilemap, startX, endX, y, tiles.FloatingLeft, tiles.FloatingMiddle, tiles.FloatingRight);
    }

    /// <summary>왼쪽, 가운데, 오른쪽 타일을 사용해 가로 타일 줄을 그립니다.</summary>
    private static void PaintStrip(Tilemap tilemap, int startX, int endX, int y,
        TileBase left, TileBase middle, TileBase right) // left 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
    {
        for (int x = startX; x <= endX; x++)
        {
            TileBase tile = middle; // 현재 위치에 배치할 타일입니다.
            if (x == startX)
            {
                tile = left;
            }
            else if (x == endX)
            {
                tile = right;
            }
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }

    /// <summary>URP에서 세 종류의 파티클 이펙트가 함께 사용할 머티리얼을 생성합니다.</summary>
    /// <returns>생성하거나 갱신한 파티클 머티리얼을 반환합니다.</returns>
    private static Material CreateParticleMaterial()
    {
        // 파티클 머티리얼을 저장할 프로젝트 경로입니다.
        string materialPath = Effects + "/VfxParticle.mat";

        // URP에서 빛의 영향을 받지 않고 파티클을 표시할 셰이더입니다.
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (particleShader == null)
        {
            throw new InvalidDataException("URP particle shader could not be found.");
        }

        // 기존에 생성되어 있는 공용 파티클 머티리얼입니다.
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (material == null)
        {
            material = new Material(particleShader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else
        {
            material.shader = particleShader;
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    /// <summary>지정한 색상과 움직임을 사용하는 일회성 원형 파티클 프리팹을 생성합니다.</summary>
    /// <param name="name">이펙트 에셋과 오브젝트에 사용할 이름입니다.</param>
    /// <param name="material">파티클을 표시할 URP 머티리얼입니다.</param>
    /// <param name="firstColor">무작위 색상 범위의 첫 번째 색상입니다.</param>
    /// <param name="secondColor">무작위 색상 범위의 두 번째 색상입니다.</param>
    /// <param name="particleCount">한 번에 방출할 파티클 개수입니다.</param>
    /// <param name="lifeTime">각 파티클이 유지되는 시간입니다.</param>
    /// <param name="minimumSpeed">파티클의 최소 이동 속도입니다.</param>
    /// <param name="maximumSpeed">파티클의 최대 이동 속도입니다.</param>
    /// <param name="minimumSize">파티클의 최소 크기입니다.</param>
    /// <param name="maximumSize">파티클의 최대 크기입니다.</param>
    /// <param name="radius">파티클이 처음 생성되는 원의 반지름입니다.</param>
    /// <returns>저장된 일회성 파티클 프리팹을 반환합니다.</returns>
    private static GameObject CreateParticleEffectPrefab(
        string name,
        Material material,
        Color firstColor,
        Color secondColor,
        int particleCount,
        float lifeTime,
        float minimumSpeed,
        float maximumSpeed,
        float minimumSize,
        float maximumSize,
        float radius)
    {
        GameObject root = new(name); // 원샷 파티클 이펙트의 루트 오브젝트입니다.
        ParticleSystem particleSystem =
            root.AddComponent<ParticleSystem>(); // 파티클의 생성과 이동을 담당하는 컴포넌트입니다.
        ParticleSystem.MainModule main = particleSystem.main; // 파티클의 공통 재생 설정입니다.
        main.loop = false;
        main.playOnAwake = true;
        main.duration = 0.15f;
        main.startLifetime = lifeTime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(minimumSpeed, maximumSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minimumSize, maximumSize);
        main.startColor = new ParticleSystem.MinMaxGradient(firstColor, secondColor);
        main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
        main.gravityModifier = 0.35f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = particleCount;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission =
            particleSystem.emission; // 한 번에 방출할 파티클 수를 설정하는 모듈입니다.
        emission.rateOverTime = 0f;
        ParticleSystem.Burst burst =
            new(0f, (short)particleCount); // 재생 시작 시 한 번만 발생하는 파티클 묶음입니다.
        emission.SetBursts(new[] { burst });

        ParticleSystem.ShapeModule shape =
            particleSystem.shape; // 파티클이 원형으로 퍼져 나가게 하는 생성 모양입니다.
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;
        shape.radiusThickness = 1f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
            particleSystem.colorOverLifetime; // 수명이 끝날수록 파티클을 투명하게 만드는 모듈입니다.
        colorOverLifetime.enabled = true;
        Gradient fadeGradient = new(); // 파티클의 수명에 따른 색상과 투명도 곡선입니다.
        fadeGradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = fadeGradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime =
            particleSystem.sizeOverLifetime; // 파티클이 사라질 때 크기도 줄이는 모듈입니다.
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new( // 파티클 크기가 빠르게 나타난 뒤 0으로 줄어드는 곡선입니다.
            new Keyframe(0f, 0.25f),
            new Keyframe(0.15f, 1f),
            new Keyframe(1f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer particleRenderer =
            root.GetComponent<ParticleSystemRenderer>(); // 파티클의 화면 표시를 담당하는 렌더러입니다.
        particleRenderer.material = material;
        particleRenderer.sortingOrder = 8;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            root, Effects + "/" + name + ".prefab"); // 저장된 일회성 이펙트 프리팹입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>플레이어의 물리, 입력, 애니메이션, 전투와 사운드 컴포넌트를 포함한 프리팹을 생성합니다.</summary>
    private static GameObject CreatePlayerPrefab(CharacterAnimationAssets animations,
        PhysicsMaterial2D frictionlessMaterial, // frictionlessMaterial 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        AudioClip jumpClip, AudioClip collectClip, AudioClip damagedClip, AudioClip stompClip,
        GameObject collectibleEffectPrefab, GameObject stompEffectPrefab,
        GameObject damagedEffectPrefab) // 상황별 플레이어 이펙트 프리팹입니다.
    {
        GameObject root = new("Player"); // root 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        root.tag = "Player";
        Rigidbody2D body = root.AddComponent<Rigidbody2D>(); // body 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        body.freezeRotation = true;
        body.gravityScale = 3.2f;
        CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>(); // collider 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        collider.size = new Vector2(0.8f, 1.3f);
        collider.sharedMaterial = frictionlessMaterial;
        root.AddComponent<PlayerInputReader>();
        root.AddComponent<PlayerMotor2D>();
        Health health = root.AddComponent<Health>(); // health 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.

        GameObject visual = Child(root, "Visual", new Vector2(0f, -0.02f), animations.DefaultSprite, // visual 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Color.white, new Vector2(1.35f, 1.35f));
        visual.GetComponent<SpriteRenderer>().sortingOrder = 5;
        Animator animator = visual.AddComponent<Animator>(); // animator 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        animator.runtimeAnimatorController = animations.Controller;

        GameObject sensorObject = new("GroundSensor"); // sensorObject 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        sensorObject.transform.SetParent(root.transform);
        sensorObject.transform.localPosition = new Vector3(0f, -0.72f, 0f);
        GroundSensor sensor = sensorObject.AddComponent<GroundSensor>(); // sensor 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        sensor.Configure(1 << 6);
        PlayerJump jump = root.AddComponent<PlayerJump>(); // jump 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        jump.Configure(sensor);
        root.AddComponent<PlayerStompAttack>();
        PlayerAnimationController animationController = root.AddComponent<PlayerAnimationController>();
        animationController.Configure(animator, sensor);
        PlayerDamageFeedback damageFeedback = root.AddComponent<PlayerDamageFeedback>(); // damageFeedback 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        damageFeedback.Configure(body, visual.GetComponent<SpriteRenderer>());
        PlayerDeathSequence deathSequence =
            root.AddComponent<PlayerDeathSequence>(); // 입력 차단과 캐릭터 사망 연출을 담당하는 컴포넌트입니다.
        deathSequence.Configure(
            body,
            visual.GetComponent<SpriteRenderer>(),
            root.GetComponent<PlayerInputReader>(),
            root.GetComponent<PlayerMotor2D>(),
            jump,
            damageFeedback);
        deathSequence.ConfigureTiming(2.2f, 1.15f);
        PlayerClearSequence clearSequence =
            root.AddComponent<PlayerClearSequence>(); // 승리 점프와 금빛 점멸을 담당하는 클리어 연출 컴포넌트입니다.
        clearSequence.Configure(
            body,
            visual.GetComponent<SpriteRenderer>(),
            root.GetComponent<PlayerInputReader>(),
            root.GetComponent<PlayerMotor2D>(),
            jump,
            health);
        AudioSource audioSource = root.AddComponent<AudioSource>(); // audioSource 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        PlayerAudioFeedback audioFeedback = root.AddComponent<PlayerAudioFeedback>(); // audioFeedback 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        audioFeedback.Configure(jumpClip, collectClip, damagedClip, stompClip);
        PlayerVfxFeedback vfxFeedback =
            root.AddComponent<PlayerVfxFeedback>(); // 획득, 밟기와 피격 파티클을 생성하는 시각 효과 컴포넌트입니다.
        vfxFeedback.Configure(
            collectibleEffectPrefab,
            stompEffectPrefab,
            damagedEffectPrefab);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "/Player.prefab"); // prefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>순찰, 접촉 피해와 애니메이션을 포함한 적 프리팹을 생성합니다.</summary>
    private static GameObject CreatePatrolEnemyPrefab(CharacterAnimationAssets animations)
    {
        GameObject root = new("Patrol Enemy"); // root 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Rigidbody2D body = root.AddComponent<Rigidbody2D>(); // body 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        body.freezeRotation = true;
        body.gravityScale = 3f;
        BoxCollider2D collider = root.AddComponent<BoxCollider2D>(); // collider 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        collider.size = new Vector2(1.1f, 0.9f);
        root.AddComponent<EnemyNavigationSensor>();
        root.AddComponent<PatrolEnemy>();
        root.AddComponent<StompableEnemy>();
        root.AddComponent<DamageDealer>();
        GameObject visual = Child(root, "Visual", new Vector2(0f, 0.08f), animations.DefaultSprite, // visual 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Color.white, new Vector2(1.25f, 1.25f));
        visual.GetComponent<SpriteRenderer>().sortingOrder = 4;
        Animator animator = visual.AddComponent<Animator>(); // animator 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        animator.runtimeAnimatorController = animations.Controller;
        root.AddComponent<EnemyAnimationController>().Configure(animator);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "/PatrolEnemy.prefab"); // prefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>평상시 순찰하고 시야 안의 플레이어를 추적하는 적 프리팹을 생성합니다.</summary>
    /// <param name="animations">추적형 적이 사용할 애니메이션 묶음입니다.</param>
    private static GameObject CreateChasingEnemyPrefab(CharacterAnimationAssets animations)
    {
        GameObject root = CreateEnemyBody("Chasing Enemy", animations); // 추적형 적의 루트 오브젝트입니다.
        root.AddComponent<EnemyNavigationSensor>();
        ChasingEnemy chasingEnemy = root.AddComponent<ChasingEnemy>(); // 순찰과 시야 기반 추적을 담당하는 행동 컴포넌트입니다.
        chasingEnemy.Configure(7f, 1.5f, 5.2f);
        EnemyDashDust dashDust =
            root.AddComponent<EnemyDashDust>(); // 질주 중 발밑 먼지를 담당하는 컴포넌트입니다.
        dashDust.Configure(CreateParticleMaterial());
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            root, Prefabs + "/ChasingEnemy.prefab"); // 저장된 추적형 적 프리팹입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>플레이어가 점프로 피할 수 있는 느린 적 투사체 프리팹을 생성합니다.</summary>
    /// <param name="sprite">투사체의 모습을 표현할 스프라이트입니다.</param>
    /// <returns>저장된 적 투사체 프리팹을 반환합니다.</returns>
    private static GameObject CreateEnemyProjectilePrefab(Sprite sprite)
    {
        GameObject root = new("Enemy Projectile"); // 적 투사체의 루트 오브젝트입니다.
        Rigidbody2D body = root.AddComponent<Rigidbody2D>(); // 일정한 속도 이동에 사용할 물리 본체입니다.
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>(); // 피해 판정에 사용할 원형 트리거입니다.
        collider.isTrigger = true;
        collider.radius = 0.5f;
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>(); // 투사체의 모습을 화면에 표시합니다.
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 0.45f, 0.15f, 1f);
        renderer.sortingOrder = 5;
        root.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
        root.AddComponent<EnemyProjectileMovement>();
        root.AddComponent<EnemyProjectileDamage>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            root, Prefabs + "/EnemyProjectile.prefab"); // 저장된 적 투사체 프리팹입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>제자리에서 주변을 살피다가 플레이어에게 투사체를 발사하는 적 프리팹을 생성합니다.</summary>
    /// <param name="animations">원거리 적이 사용할 캐릭터 애니메이션 묶음입니다.</param>
    /// <param name="projectilePrefab">원거리 적이 발사할 투사체 프리팹입니다.</param>
    /// <returns>저장된 원거리 적 프리팹을 반환합니다.</returns>
    private static GameObject CreateRangedEnemyPrefab(
        CharacterAnimationAssets animations, GameObject projectilePrefab)
    {
        GameObject root = CreateEnemyBody("Ranged Enemy", animations); // 원거리 적의 루트 오브젝트입니다.
        Rigidbody2D body = root.GetComponent<Rigidbody2D>(); // 원거리 적을 제자리에 고정할 물리 본체입니다.
        body.constraints = RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation;
        SpriteRenderer renderer =
            root.GetComponentInChildren<SpriteRenderer>(); // 바라보는 방향을 표시할 캐릭터 렌더러입니다.
        RangedEnemyLookout lookout =
            root.AddComponent<RangedEnemyLookout>(); // 두리번거리기와 플레이어 감지를 담당합니다.
        lookout.Configure(7f, 2.5f, 1.4f, renderer);
        GameObject firePointObject = new("Fire Point"); // 투사체가 시작되는 위치를 나타냅니다.
        firePointObject.transform.SetParent(root.transform);
        firePointObject.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        EnemyProjectileLauncher launcher =
            root.AddComponent<EnemyProjectileLauncher>(); // 감지 결과에 따라 발사를 담당합니다.
        launcher.Configure(projectilePrefab, firePointObject.transform, lookout, 2.4f, 0.6f);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            root, Prefabs + "/RangedEnemy.prefab"); // 저장된 원거리 적 프리팹입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>특수 적이 공통으로 사용하는 물리, 접촉 피해와 애니메이션 구성을 생성합니다.</summary>
    /// <param name="name">생성할 적 루트 오브젝트의 이름입니다.</param>
    /// <param name="animations">적에게 연결할 애니메이션 묶음입니다.</param>
    private static GameObject CreateEnemyBody(string name, CharacterAnimationAssets animations)
    {
        GameObject root = new(name); // 특수 적 프리팹의 루트 오브젝트입니다.
        Rigidbody2D body = root.AddComponent<Rigidbody2D>(); // 중력과 수평 이동에 사용하는 물리 본체입니다.
        body.freezeRotation = true;
        body.gravityScale = 3f;
        BoxCollider2D collider = root.AddComponent<BoxCollider2D>(); // 플레이어와 지형 충돌에 사용하는 콜라이더입니다.
        collider.size = new Vector2(1.1f, 0.9f);
        root.AddComponent<StompableEnemy>();
        root.AddComponent<DamageDealer>();
        GameObject visual = Child(root, "Visual", new Vector2(0f, 0.08f),
            animations.DefaultSprite, Color.white, new Vector2(1.25f, 1.25f)); // 적 캐릭터의 시각 요소입니다.
        visual.GetComponent<SpriteRenderer>().sortingOrder = 4;
        Animator animator = visual.AddComponent<Animator>(); // 적의 상태 애니메이션을 재생하는 애니메이터입니다.
        animator.runtimeAnimatorController = animations.Controller;
        root.AddComponent<EnemyAnimationController>().Configure(animator);
        return root;
    }

    /// <summary>점수 수집과 회전 효과를 포함한 Star 프리팹을 생성합니다.</summary>
    private static GameObject CreateStarPrefab(Sprite sprite)
    {
        GameObject root = new("Star"); // root 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>(); // renderer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 0.85f, 0.1f);
        renderer.sortingOrder = 3;
        root.transform.localScale = Vector3.one * 0.55f;
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>(); // collider 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        collider.isTrigger = true;
        root.AddComponent<Collectible>();
        root.AddComponent<Rotator>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "/Star.prefab"); // prefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>승리 트리거와 깃발 모양을 포함한 목표 프리팹을 생성합니다.</summary>
    private static GameObject CreateGoalPrefab(Sprite sprite)
    {
        GameObject root = new("Goal Flag"); // root 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>(); // trigger 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.2f, 3.5f);
        root.AddComponent<Goal>();
        Child(root, "Pole", new Vector2(0f, 0f), sprite, new Color(0.85f, 0.9f, 0.95f), new Vector2(0.15f, 3.5f));
        Child(root, "Flag", new Vector2(0.62f, 1.2f), sprite, new Color(0.25f, 1f, 0.45f), new Vector2(1.2f, 0.75f));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "/Goal.prefab"); // prefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>모든 프리팹, 지형, 배경, UI를 배치해 메인 씬을 생성합니다.</summary>
    private static void BuildScene(GameObject playerPrefab, GameObject patrolEnemyPrefab,
        GameObject chasingEnemyPrefab, GameObject rangedEnemyPrefab, GameObject starPrefab,
        GameObject goalPrefab, TerrainTiles terrainTiles) // goalPrefab 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // scene 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        scene.name = "Main";

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.gameObject.AddComponent<AudioListener>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.backgroundColor = new Color(0.08f, 0.12f, 0.22f);
        camera.transform.position = new Vector3(4f, 1.5f, -10f);
        UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;

        CreateParallaxBackground(camera.transform);
        CreateTerrainTilemap(terrainTiles);

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab); // player 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        player.transform.position = new Vector3(-0.5f, -2.1f, 0f);
        Health health = player.GetComponent<Health>(); // health 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.

        SpawnEnemy(patrolEnemyPrefab, "Patrol Enemy A", 14f, -2.2f, 12.5f, 17f, 2f);
        SpawnEnemy(patrolEnemyPrefab, "Patrol Enemy C", 21f, 0.8f, 20.6f, 22.4f, 1.4f);
        SpawnSpecialEnemy(chasingEnemyPrefab, "Chasing Enemy A", 26f, -2.2f);
        SpawnEnemy(patrolEnemyPrefab, "Patrol Enemy B", 37.5f, 0.8f, 36.5f, 38.5f, 2.2f);
        SpawnEnemy(patrolEnemyPrefab, "Patrol Enemy D", 34.7f, 4.8f, 34.5f, 36.8f, 1.3f);
        SpawnEnemy(patrolEnemyPrefab, "Patrol Enemy E", 56.5f, 0.8f, 55.5f, 57.5f, 1.5f);
        SpawnSpecialEnemy(chasingEnemyPrefab, "Chasing Enemy B", 61f, -2.2f);
        SpawnSpecialEnemy(rangedEnemyPrefab, "Ranged Enemy A", 52f, -2.2f);
        SpawnEnemy(patrolEnemyPrefab, "Vertical Patrol A", 68f, 5.8f, 66.5f, 69.5f, 1.35f);
        SpawnEnemy(patrolEnemyPrefab, "Vertical Patrol B", 63f, 14.8f, 61.5f, 64.5f, 1.25f);
        SpawnSpecialEnemy(rangedEnemyPrefab, "Vertical Ranged Enemy", 70f, 11.8f);
        SpawnSpecialEnemy(chasingEnemyPrefab, "Vertical Chasing Enemy", 63f, 20.8f);
        SpawnEnemy(patrolEnemyPrefab, "Summit Patrol", 75.5f, 32.8f, 73f, 78f, 1.4f);

        Vector2[] stars = // stars 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        {
            new(3f, -2f), new(8f, 1f), new(13f, 1f), new(17f, 2f),
            new(22f, 1f), new(27f, 1f), new(32f, 3f), new(35.5f, 5f),
            new(42.5f, 2f), new(49f, 1f), new(53f, 2f), new(64f, 4f),
            new(68f, 7f), new(63f, 10f), new(70f, 13f), new(63f, 16f),
            new(68.5f, 19f), new(62.5f, 22f), new(69f, 25f), new(63f, 28f),
            new(68.5f, 31f), new(77.5f, 34f)
        };
        foreach (Vector2 position in stars)
        {
            GameObject star = (GameObject)PrefabUtility.InstantiatePrefab(starPrefab); // star 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            star.transform.position = position;
        }
        GameObject goal = (GameObject)PrefabUtility.InstantiatePrefab(goalPrefab); // goal 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        goal.transform.position = new Vector3(78f, 34.75f);

        GameObject killZone = new("Fall Kill Zone"); // killZone 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        killZone.transform.position = new Vector3(32f, -7f);
        BoxCollider2D killCollider = killZone.AddComponent<BoxCollider2D>(); // killCollider 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        killCollider.isTrigger = true;
        killCollider.size = new Vector2(150f, 2f);
        killZone.AddComponent<FallDeathZone>();

        GameManager manager = new GameObject("Game Manager").AddComponent<GameManager>();
        manager.Configure(
            health,
            stars.Length,
            player.GetComponent<PlayerDeathSequence>(),
            player.GetComponent<PlayerClearSequence>());
        CameraFollow follow = camera.gameObject.AddComponent<CameraFollow>();
        follow.Configure(player.transform, 0f, 77f);
        CreateHud(health);
        CreateDeathGrayscaleEffect();

        EditorSceneManager.SaveScene(scene, Scenes + "/Main.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Scenes + "/Title.unity", true),
            new EditorBuildSettingsScene(Scenes + "/Main.unity", true)
        };
        PlayerSettings.productName = "Academy Platformer Sample";
        PlayerSettings.companyName = "Game Academy";
        Selection.activeObject = player;
    }

    /// <summary>플레이어 사망 시 화면 전체를 흑백으로 만드는 전역 후처리 볼륨을 생성합니다.</summary>
    private static void CreateDeathGrayscaleEffect()
    {
        GameObject effectObject = new("Death Screen Grayscale");
        DeathScreenGrayscale grayscale = effectObject.AddComponent<DeathScreenGrayscale>();
        grayscale.Configure(0.48f);
    }

    /// <summary>카메라 뒤에 세 개의 패럴랙스 배경 레이어를 생성합니다.</summary>
    private static void CreateParallaxBackground(Transform cameraTransform)
    {
        GameObject root = new("Parallax Background"); // root 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        CreateParallaxLayer(root.transform, cameraTransform, "Far Sky",
            "Assets/ThirdParty/MagicalRoad/Layers/back.png",
            new Vector3(32f, 0f, 0f), 0.05f, -30);
        CreateParallaxLayer(root.transform, cameraTransform, "Distant Forest",
            "Assets/ThirdParty/MagicalRoad/Layers/middle.png",
            new Vector3(32f, -0.8f, 0f), 0.2f, -20);
        CreateParallaxLayer(root.transform, cameraTransform, "Near Trees",
            "Assets/ThirdParty/MagicalRoad/Layers/tree.png",
            new Vector3(32f, -1.6f, 0f), 0.4f, -10);
    }

    /// <summary>배경 이미지 하나를 반복 배치하는 패럴랙스 레이어를 생성합니다.</summary>
    private static void CreateParallaxLayer(Transform parent, Transform cameraTransform, string name,
        string spritePath, Vector3 position, float factor, int sortingOrder) // spritePath 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
    {
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter; // importer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (importer == null)
        {
            throw new InvalidDataException($"Background texture import failed: {spritePath}");
        }
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Repeat;
        TextureImporterSettings textureSettings = new();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(textureSettings);
        importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath); // sprite 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (sprite == null)
        {
            throw new InvalidDataException($"Background sprite load failed: {spritePath}");
        }

        GameObject layer = new(name); // layer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        layer.transform.SetParent(parent);
        layer.transform.position = position;
        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>(); // renderer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.tileMode = SpriteTileMode.Continuous;
        renderer.size = new Vector2(
            100f, sprite.bounds.size.y); // 가로 방향만 반복하고 세로 방향은 원본 이미지 높이를 유지합니다.
        float verticalFactor = 1f;
        if (name == "Distant Forest")
        {
            verticalFactor = 0.72f;
        }
        if (name == "Near Trees")
        {
            verticalFactor = 0.42f;
        }
        layer.AddComponent<ParallaxLayer>().Configure(
            cameraTransform,
            factor,
            verticalFactor);
    }

    /// <summary>지정한 위치와 순찰 범위로 적 프리팹 인스턴스를 배치합니다.</summary>
    /// <param name="prefab">배치할 순찰형 적 프리팹입니다.</param>
    /// <param name="name">씬에서 사용할 적 오브젝트의 이름입니다.</param>
    /// <param name="startX">적이 시작할 수평 좌표입니다.</param>
    /// <param name="startY">적이 시작할 세로 좌표입니다.</param>
    /// <param name="leftX">순찰 범위의 왼쪽 경계입니다.</param>
    /// <param name="rightX">순찰 범위의 오른쪽 경계입니다.</param>
    /// <param name="speed">순찰 이동 속도입니다.</param>
    private static void SpawnEnemy(GameObject prefab, string name, float startX, float startY,
        float leftX, float rightX, float speed) // leftX 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
    {
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab); // enemy 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        enemy.name = name;
        enemy.transform.position = new Vector3(startX, startY);
        enemy.GetComponent<PatrolEnemy>().Configure(leftX, rightX, speed);
    }

    /// <summary>고유 행동 설정을 프리팹에 포함한 특수 적을 지정한 높이에 배치합니다.</summary>
    /// <param name="prefab">배치할 특수 적 프리팹입니다.</param>
    /// <param name="name">씬에서 사용할 적 오브젝트의 이름입니다.</param>
    /// <param name="startX">적이 시작할 수평 좌표입니다.</param>
    /// <param name="startY">적이 시작할 세로 좌표입니다.</param>
    private static void SpawnSpecialEnemy(GameObject prefab, string name, float startX, float startY)
    {
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab); // 씬에 배치된 특수 적 인스턴스입니다.
        enemy.name = name;
        enemy.transform.position = new Vector3(startX, startY);
    }

    /// <summary>부모 아래에 스프라이트 렌더러를 가진 자식 오브젝트를 생성합니다.</summary>
    private static GameObject Child(GameObject parent, string name, Vector2 localPosition, Sprite sprite, Color color, Vector2 scale)
    {
        GameObject child = new(name); // child 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        child.transform.SetParent(parent.transform);
        child.transform.localPosition = localPosition;
        child.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); // renderer 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        renderer.sprite = sprite;
        renderer.color = color;
        return child;
    }

    /// <summary>상태 표시와 게임 종료 패널을 포함한 HUD를 생성합니다.</summary>
    private static void CreateHud(Health health)
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject =
                new GameObject("Event System"); // UI 버튼 입력을 처리할 이벤트 시스템 오브젝트입니다.
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObject = new("HUD"); // canvasObject 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        Canvas canvas = canvasObject.AddComponent<Canvas>(); // canvas 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        canvas.sortingOrder = 100;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // font 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.

        Text status = CreateText(canvasObject.transform, "Status", font, 28, TextAnchor.UpperLeft); // status 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        RectTransform statusRect = status.rectTransform; // statusRect 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        statusRect.anchorMin = statusRect.anchorMax = new Vector2(0f, 1f);
        statusRect.pivot = new Vector2(0f, 1f);
        statusRect.anchoredPosition = new Vector2(24f, -20f);
        statusRect.sizeDelta = new Vector2(600f, 60f);

        Text message = CreateText(canvasObject.transform, "Message", font, 26, TextAnchor.MiddleCenter); // message 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        RectTransform messageRect = message.rectTransform; // messageRect 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        messageRect.anchorMin = new Vector2(0f, 0f);
        messageRect.anchorMax = new Vector2(1f, 0.18f);
        messageRect.offsetMin = messageRect.offsetMax = Vector2.zero;

        GameObject fadeObject = new(
            "Death Fade",
            typeof(RectTransform),
            typeof(Image)); // 사망 연출 중 화면을 천천히 어둡게 만드는 오브젝트입니다.
        fadeObject.transform.SetParent(canvasObject.transform, false);
        RectTransform fadeRect =
            fadeObject.GetComponent<RectTransform>(); // 화면 전체를 덮는 페이드 이미지 영역입니다.
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = fadeRect.offsetMax = Vector2.zero;
        Image fadeImage =
            fadeObject.GetComponent<Image>(); // 사망 화면 페이드에 사용할 이미지입니다.
        fadeImage.color = new Color(0.02f, 0.01f, 0.04f, 0f);
        fadeImage.raycastTarget = false;

        GameObject endScreen = new("Game Over Screen", typeof(RectTransform), typeof(Image)); // endScreen 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        endScreen.transform.SetParent(canvasObject.transform, false);
        RectTransform screenRect = endScreen.GetComponent<RectTransform>(); // screenRect 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.offsetMin = screenRect.offsetMax = Vector2.zero;
        endScreen.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.05f, 0.86f);

        GameObject card = new("Center Panel", typeof(RectTransform), typeof(Image)); // card 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        card.transform.SetParent(endScreen.transform, false);
        RectTransform cardRect = card.GetComponent<RectTransform>(); // cardRect 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(860f, 390f);
        card.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.18f, 0.96f);

        Text endTitle = CreateText(card.transform, "Title", font, 64, TextAnchor.MiddleCenter); // endTitle 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        RectTransform titleRect = endTitle.rectTransform; // titleRect 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        titleRect.anchorMin = new Vector2(0f, 0.42f);
        titleRect.anchorMax = new Vector2(1f, 0.9f);
        titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;

        Text endGuide = CreateText(card.transform, "Guide", font, 30, TextAnchor.MiddleCenter); // endGuide 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        RectTransform guideRect = endGuide.rectTransform; // guideRect 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        guideRect.anchorMin = new Vector2(0f, 0.1f);
        guideRect.anchorMax = new Vector2(1f, 0.45f);
        guideRect.offsetMin = guideRect.offsetMax = Vector2.zero;

        Button restartButton = CreateHudButton(
            card.transform,
            "Restart Button",
            "RESTART",
            new Vector2(-270f, -125f),
            font);
        Button nextStageButton = CreateHudButton(
            card.transform,
            "Next Stage Button",
            "NEXT STAGE",
            new Vector2(0f, -125f),
            font);
        Button stageSelectButton = CreateHudButton(
            card.transform,
            "Stage Select Button",
            "STAGE SELECT",
            new Vector2(270f, -125f),
            font);

        endScreen.SetActive(false);
        GameHUD hud = canvasObject.AddComponent<GameHUD>();
        hud.Configure(
            health,
            status,
            message,
            endScreen,
            endTitle,
            endGuide,
            fadeImage,
            restartButton,
            nextStageButton,
            stageSelectButton);
        UnityEventTools.AddPersistentListener(
            restartButton.onClick,
            hud.RestartStage);
        UnityEventTools.AddPersistentListener(
            nextStageButton.onClick,
            hud.GoToNextStage);
        UnityEventTools.AddPersistentListener(
            stageSelectButton.onClick,
            hud.GoToStageSelect);
    }

    /// <summary>게임 종료 카드에서 사용할 이동 버튼 하나를 생성합니다.</summary>
    /// <param name="parent">버튼이 소속될 카드 Transform입니다.</param>
    /// <param name="name">버튼 오브젝트 이름입니다.</param>
    /// <param name="label">버튼에 표시할 문구입니다.</param>
    /// <param name="position">카드 중앙을 기준으로 한 버튼 위치입니다.</param>
    /// <param name="font">버튼 문구에 사용할 글꼴입니다.</param>
    private static Button CreateHudButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Font font)
    {
        GameObject buttonObject = new(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(230f, 64f);
        buttonObject.GetComponent<Image>().color =
            new Color(0.08f, 0.42f, 0.58f, 1f);

        Text text = CreateText(
            buttonObject.transform,
            "Label",
            font,
            22,
            TextAnchor.MiddleCenter);
        text.text = label;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return buttonObject.GetComponent<Button>();
    }

    /// <summary>지정한 부모 아래에 기본 UI 텍스트를 생성합니다.</summary>
    private static Text CreateText(Transform parent, string name, Font font, int size, TextAnchor alignment)
    {
        GameObject go = new(name, typeof(RectTransform)); // go 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>(); // text 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
