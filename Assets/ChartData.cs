using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public int LaneIndex;   // 流れてくるレーン番号(0-5)
    public int Type;        // 1:Tap, 2:HoldStart, 3:HoldEnd
    public float Time;      // 判定時間(秒) / ホールド始点の時間(秒)
    public float Bpm;

    // Hold Note用
    public float Duration;  // ホールド継続時間(秒)
    public List<float> HoldJudgeTimes = new List<float>(); // 途中判定時間リスト(秒)
}

[System.Serializable]
public class ScrollEvent
{
    public float Time;          // イベント発生時間(秒)
    public float Rate;          // スクロール倍率
}

public class ChartData
{
    public string Title = "No Title";
    public string Artist = "No Artist";
    public string Designer = "No Designer";
    public string MusicAudioPath = "";
    public string Genre = "";
    public int Level = 0;

    public float Bpm = 120;
    public float Offset = 0;
    public int BeatNumerator = 4;
    public int BeatDenominator = 4;

    public int TotalJudgments = 0;

    public List<NoteData> Notes = new List<NoteData>();
    public List<float> BarLineTimes = new List<float>();
    public List<ScrollEvent> ScrollEvents = new List<ScrollEvent>();
}