using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>모든 스테이지의 타일 발판과 합성 콜라이더 위치를 다시 맞추고 검증합니다.</summary>
public static class PlatformColliderAlignmentFixer
{
    // 검사하고 수정할 전체 스테이지 개수입니다.
    private const int StageCount = 3;

    /// <summary>세 스테이지의 콜라이더 형상을 다시 생성하고 타일 중심의 충돌 여부를 검사합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Fix Platform Collider Alignment")]
    public static void FixAndValidateAllStages()
    {
        bool allStagesPassed = true;
        int stageNumber = 1;
        while (stageNumber <= StageCount)
        {
            bool stagePassed = FixAndValidateStage(stageNumber);
            allStagesPassed = stagePassed == true && allStagesPassed == true;
            stageNumber++;
        }

        AssetDatabase.SaveAssets();
        if (allStagesPassed == true)
        {
            Debug.Log("PLATFORM_COLLIDER_ALIGNMENT_VALIDATION_PASSED");
            return;
        }

        Debug.LogError("PLATFORM_COLLIDER_ALIGNMENT_VALIDATION_FAILED");
    }

    /// <summary>지정한 스테이지를 열어 타일맵 콜라이더를 재생성하고 정렬 상태를 검사합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    private static bool FixAndValidateStage(int stageNumber)
    {
        string scenePath =
            "Assets/Scenes/Stage" + stageNumber + ".unity";
        Scene scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single);
        Tilemap tilemap = Object.FindFirstObjectByType<Tilemap>();
        TilemapCollider2D tilemapCollider =
            Object.FindFirstObjectByType<TilemapCollider2D>();
        if (tilemap == null || tilemapCollider == null)
        {
            Debug.LogError(
                "Stage " + stageNumber +
                "에서 타일맵 또는 타일맵 콜라이더를 찾지 못했습니다.");
            return false;
        }

        MarkAllTilesChanged(tilemap, tilemapCollider);
        tilemap.RefreshAllTiles();
        tilemapCollider.ProcessTilemapChanges();
        CompositeCollider2D compositeCollider =
            tilemapCollider.GetComponent<CompositeCollider2D>();
        if (compositeCollider != null)
        {
            compositeCollider.GenerateGeometry();
        }

        Physics2D.SyncTransforms();
        EditorUtility.SetDirty(tilemap);
        EditorUtility.SetDirty(tilemapCollider);
        if (compositeCollider != null)
        {
            EditorUtility.SetDirty(compositeCollider);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Collider2D effectiveCollider = compositeCollider != null
            ? compositeCollider
            : tilemapCollider;
        int mismatchCount =
            CountColliderMismatches(tilemap, effectiveCollider);
        if (mismatchCount == 0)
        {
            Debug.Log(
                "Stage " + stageNumber +
                " 플랫폼과 콜라이더 정렬 검증 통과");
            return true;
        }

        Debug.LogError(
            "Stage " + stageNumber + "에서 " + mismatchCount +
            "개의 타일과 콜라이더가 어긋나 있습니다.");
        return false;
    }

    /// <summary>저장 과정에서 사라진 타일 변경 기록을 복원해 콜라이더가 모든 셀을 다시 계산하게 합니다.</summary>
    /// <param name="tilemap">변경 상태를 다시 등록할 발판 타일맵입니다.</param>
    /// <param name="tilemapCollider">타일 제거 상태를 먼저 처리할 타일맵 콜라이더입니다.</param>
    private static void MarkAllTilesChanged(
        Tilemap tilemap,
        TilemapCollider2D tilemapCollider)
    {
        List<Vector3Int> occupiedPositions =
            new List<Vector3Int>();
        List<TileBase> occupiedTiles =
            new List<TileBase>();
        BoundsInt cellBounds = tilemap.cellBounds;
        foreach (Vector3Int cellPosition in cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(cellPosition);
            if (tile == null)
            {
                continue;
            }

            occupiedPositions.Add(cellPosition);
            occupiedTiles.Add(tile);
            tilemap.SetTile(cellPosition, null);
        }

        tilemap.RefreshAllTiles();
        tilemapCollider.ProcessTilemapChanges();

        int tileIndex = 0;
        while (tileIndex < occupiedPositions.Count)
        {
            tilemap.SetTile(
                occupiedPositions[tileIndex],
                occupiedTiles[tileIndex]);
            tileIndex++;
        }
    }

    /// <summary>충돌 형상이 있는 각 타일 중심에 실제 콜라이더가 존재하는지 검사합니다.</summary>
    /// <param name="tilemap">검사할 발판 타일맵입니다.</param>
    /// <param name="effectiveCollider">실제 충돌을 제공하는 타일맵 또는 합성 콜라이더입니다.</param>
    private static int CountColliderMismatches(
        Tilemap tilemap,
        Collider2D effectiveCollider)
    {
        int mismatchCount = 0;
        BoundsInt cellBounds = tilemap.cellBounds;
        foreach (Vector3Int cellPosition in cellBounds.allPositionsWithin)
        {
            Tile.ColliderType colliderType =
                tilemap.GetColliderType(cellPosition);
            if (colliderType == Tile.ColliderType.None)
            {
                continue;
            }

            Vector3 worldCenter =
                tilemap.GetCellCenterWorld(cellPosition);
            if (effectiveCollider.OverlapPoint(worldCenter) == false)
            {
                mismatchCount++;
                Debug.LogError(
                    "콜라이더가 없는 발판 타일 위치: " +
                    cellPosition);
            }
        }

        return mismatchCount;
    }
}
