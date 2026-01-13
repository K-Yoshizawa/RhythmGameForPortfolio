using UnityEngine;

public class UIBoundsVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public Color outlineColor = Color.yellow; // 線の色
    public bool showOnlySelected = false;     // 選択中のみ表示するか

    private void OnDrawGizmos()
    {
        // 選択中のみ表示モードの場合、選択されていない時は描画しない
        if (showOnlySelected) return;
        DrawRects();
    }

    private void OnDrawGizmosSelected()
    {
        // 選択中のみ表示モードの場合、ここで描画する
        if (showOnlySelected) DrawRects();
    }

    private void DrawRects()
    {
        // 自分以下のすべてのRectTransformを取得
        RectTransform[] rects = GetComponentsInChildren<RectTransform>();

        Gizmos.color = outlineColor;
        Vector3[] corners = new Vector3[4];

        foreach (var rect in rects)
        {
            // オブジェクトごとの四隅のワールド座標を取得
            rect.GetWorldCorners(corners);

            // 4本の線を引く (左下->左上->右上->右下->左下)
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
        }
    }
}