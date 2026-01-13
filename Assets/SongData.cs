using UnityEngine;
using System.Collections.Generic;

public enum Difficulty
{
    Basic = 0,
    Advanced = 1,
    Expert = 2,
    Master = 3
}

[System.Serializable]
public class SongInfo
{
    // header.txt から読む情報
    public string Title;
    public string Artist;
    public float Bpm;
    public float Offset;
    public float PreviewTime;
    public float Volume = 1.0f;

    public string DirectoryPath;
    public string AudioPath;
    public string ImagePath;


    public Dictionary<Difficulty, string> ChartPaths = new Dictionary<Difficulty, string>();
    public Dictionary<Difficulty, int> Levels = new Dictionary<Difficulty, int>();

    public int SongId;
}