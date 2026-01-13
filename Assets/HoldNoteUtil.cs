using UnityEngine;

public static class HoldNoteUtil
{
    // ホールドノーツの途中判定間隔を計算するメソッド
    public static float CalculateIntervalStep(float bpm)
    {
        float divisor = bpm;
        if (bpm < 60f) divisor *= 8f;
        else if(bpm < 120f) divisor *= 4f;
        else if (bpm < 240f) divisor *= 2f;
        else if (bpm >= 480f) divisor /= 2f;
        return 60f / divisor;
    }
}