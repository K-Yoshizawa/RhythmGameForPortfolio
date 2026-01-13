using System.Collections.Generic;
using UnityEngine;
using System;

public static class ChartLoader
{
    // ホールドノーツの状態管理用クラス
    private class HoldState
    {
        public NoteData Note;       // ホールドノーツのデータ
        public float LastJudgeTime; // 最後に判定が行われた時間
    }

    public static ChartData LoadChart(string textData, float baseBpm, float baseOffset)
    {
        ChartData data = new ChartData();
        data.Bpm = baseBpm;
        data.Offset = baseOffset;
        data.ScrollEvents.Add(new ScrollEvent { Time = 0.0f, Rate = 1.0f });

        string[] lines = textData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        bool isBody = false;                // ヘッダー部と譜面データ部の切り替えフラグ
        bool isNoBarLine = false;           // 小節線を描画しないフラグ
        float currentBpm = baseBpm;         // 現在のBPM
        int currentBeatNum = 4;             // 現在の拍子分子
        int currentBeatDen = 4;             // 現在の拍子分母
        float currentScrollScale = 1.0f;    // 現在のスクロール倍率
        float currentTime = baseOffset;     // 現在の時間位置

        Dictionary<int, HoldState> activeHolds = new Dictionary<int, HoldState>();

        int lineIndex = 0;
        while (lineIndex < lines.Length)
        {
            string line = lines[lineIndex].Trim();

            if (line.StartsWith("#START"))
            {
                isBody = true;
                currentBpm = baseBpm;
                currentBeatNum = data.BeatNumerator;
                currentBeatDen = data.BeatDenominator;
                currentTime = baseOffset;
                ++lineIndex;
                continue;
            }
            if (line.StartsWith("#END")) break;

            if (!isBody)
            {
                ParseHeader(line, data);
                ++lineIndex;
                continue;
            }

            // ここ以降は譜面データ部分の解析

            if (line.StartsWith("---"))
            {
                ++lineIndex;    // skip "---"

                while (lineIndex < lines.Length)
                {
                    string cmdLine = lines[lineIndex].Trim();
                    Debug.Log(cmdLine);
                    if (IsLaneData(cmdLine)) break;
                    Debug.Log("This is Command");

                    float scrollTemp = currentScrollScale;

                    ParseBodyCommand(cmdLine, ref currentBpm, ref currentBeatNum, ref currentBeatDen, ref currentScrollScale, ref isNoBarLine);

                    if (Math.Abs(currentScrollScale - scrollTemp) > 0.001f)
                    {
                        data.ScrollEvents.Add(new ScrollEvent { Time = currentTime, Rate = currentScrollScale });
                    }

                    ++lineIndex;
                }
            }

            if (!isNoBarLine) data.BarLineTimes.Add(currentTime);
            isNoBarLine = false;

            float secondsPerBeat = 60f / currentBpm;
            float measureDuration = secondsPerBeat * (4f / currentBeatDen) * currentBeatNum;

            // ここ以降は各レーンの小節データの解析

            for (int lane = 0; lane < 6; ++lane)
            {
                if (lineIndex >= lines.Length) break;
                string laneLine = lines[lineIndex].Trim();

                int notesCount = laneLine.Length;
                float timePerNote = measureDuration / notesCount;
                float noteTime = currentTime;
                for (int i = 0; i < notesCount; ++i, noteTime += timePerNote)
                {
                    char c = laneLine[i];

                    if (c == '1')
                    {
                        data.Notes.Add(new NoteData { LaneIndex = lane, Type = 1, Time = noteTime, Bpm = currentBpm });
                    }
                    else if (c == '2')
                    {
                        NoteData holdStart = new NoteData { LaneIndex = lane, Type = 2, Time = noteTime, Bpm = currentBpm };
                        data.Notes.Add(holdStart);

                        activeHolds[lane] = new HoldState { Note = holdStart, LastJudgeTime = noteTime };
                    }
                    else if (c == '3')
                    {
                        NoteData holdEnd = new NoteData { LaneIndex = lane, Type = 3, Time = noteTime };
                        data.Notes.Add(holdEnd);

                        if (activeHolds.ContainsKey(lane))
                        {
                            HoldState state = activeHolds[lane];
                            state.Note.Duration = noteTime - state.Note.Time;
                            ProcessHoldJudgements(state, noteTime - 0.01f, currentBpm);
                            if (Mathf.Abs(state.LastJudgeTime - noteTime) > 0.001f)
                            {
                                state.Note.HoldJudgeTimes.Add(noteTime);
                            }
                            else
                            {
                                int lastIndex = state.Note.HoldJudgeTimes.Count - 1;
                                if (lastIndex >= 0)
                                {
                                    state.Note.HoldJudgeTimes[lastIndex] = noteTime;
                                }
                            }
                            activeHolds.Remove(lane);
                        }
                    }
                }
                ++lineIndex;
            }
            currentTime += measureDuration;

            // 保持中のホールドノーツの状態更新
            foreach (var kvp in activeHolds)
            {
                ProcessHoldJudgements(kvp.Value, currentTime, currentBpm);
            }
        }
        data.TotalJudgments = CalculateTotalJudgments(data);
        return data;
    }

    private static void ProcessHoldJudgements(HoldState state, float endTime, float bpm)
    {
        float interval = HoldNoteUtil.CalculateIntervalStep(bpm);
        float nextJudgeTime = state.LastJudgeTime + interval;

        while (nextJudgeTime <= endTime + 0.001f)
        {
            state.Note.HoldJudgeTimes.Add(nextJudgeTime);
            state.LastJudgeTime = nextJudgeTime;
            nextJudgeTime += interval;
        }
    }

    private static bool IsLaneData(string line)
    {
        if (string.IsNullOrEmpty(line)) return false;
        return char.IsDigit(line[0]);
    }

    private static void ParseHeader(string line, ChartData data)
    {
        string[] split = line.Split(new char[] { '=' }, 2);
        if (split.Length < 2) return;

        string key = split[0].Trim();
        string val = split[1].Trim();

        switch (key)
        {
            case "#TITLE":
                data.Title = val;
                break;
            case "#ARTIST":
                data.Artist = val;
                break;
            case "#MUSIC":
                data.MusicAudioPath = val;
                break;
            case "#BEAT":
                ParseBeat(val, out data.BeatNumerator, out data.BeatDenominator);
                break;
        }
    }

    private static void ParseBodyCommand(string line, ref float bpm, ref int beatNum, ref int beatDen, ref float scroll, ref bool isNoBarLine)
    {
        string[] split = line.Split(new char[] { '=' }, 2);
        if (split.Length < 2) return;

        string cmd = split[0].Trim();
        string val = split[1].Trim();

        Debug.Log($"cmd = {cmd}, val = {val}");

        switch (cmd)
        {
            case "#BPM":
                float.TryParse(val, out bpm);
                break;
            case "#BEAT":
                ParseBeat(val, out beatNum, out beatDen);
                break;
            case "#SCROLL":
                float.TryParse(val, out scroll);
                break;
            case "#NOBARLINE":
                isNoBarLine = true;
                break;
        }
    }

    private static void ParseBeat(string val, out int num, out int den)
    {
        num = 4;
        den = 4;
        string[] parts = val.Split('/');
        if (parts.Length >= 2)
        {
            int.TryParse(parts[0], out num);
            int.TryParse(parts[1], out den);
        }
    }

    public static int CalculateTotalJudgments(ChartData chartData)
    {
        int count = 0;
        foreach (var note in chartData.Notes)
        {
            if (note.Type == 1) // Tap Note
            {
                count += 1;
            }
            else if (note.Type == 2) // Hold Start
            {
                count += 1 + note.HoldJudgeTimes.Count;
            }
        }
        return count;
    }
}