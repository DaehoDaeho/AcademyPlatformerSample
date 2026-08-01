using UnityEditor;
using UnityEngine;

/// <summary>기존 질주형 적 프리팹에 빠른 추적 속도와 발밑 먼지 효과를 적용하는 편집기 도구입니다.</summary>
public static class DashEnemyUpgradeBuilder
{
    // 질주형 적 프리팹의 프로젝트 경로입니다.
    private const string ChasingEnemyPrefabPath =
        "Assets/AcademyPlatformer/Prefabs/ChasingEnemy.prefab";
    // 먼지 파티클에 사용할 공용 URP 머티리얼의 프로젝트 경로입니다.
    private const string ParticleMaterialPath =
        "Assets/AcademyPlatformer/Effects/VfxParticle.mat";

    /// <summary>기존 질주형 적 프리팹에 속도 설정과 먼지 효과 컴포넌트를 반영합니다.</summary>
    [MenuItem("Academy Platformer/Upgrade Dash Enemy")]
    public static void Build()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(ChasingEnemyPrefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("질주형 적 프리팹을 찾을 수 없습니다.");
            return;
        }

        try
        {
            ChasingEnemy chasingEnemy =
                prefabRoot.GetComponent<ChasingEnemy>();

            if (chasingEnemy == null)
            {
                Debug.LogError("질주형 적 프리팹에 ChasingEnemy가 없습니다.");
                return;
            }

            chasingEnemy.Configure(7f, 1.5f, 5.2f);

            EnemyDashDust dashDust =
                prefabRoot.GetComponent<EnemyDashDust>();

            if (dashDust == null)
            {
                dashDust = prefabRoot.AddComponent<EnemyDashDust>();
            }

            Material particleMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);
            dashDust.Configure(particleMaterial);

            PrefabUtility.SaveAsPrefabAsset(
                prefabRoot,
                ChasingEnemyPrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log("DASH_ENEMY_UPGRADE_COMPLETED");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
