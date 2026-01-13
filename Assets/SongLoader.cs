using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class SongLoader : MonoBehaviour
{
    public static List<SongInfo> AllSongs = new List<SongInfo>();

    public void LoadSongs()
    {
        AllSongs.Clear();

        string songsRootPath = Path.Combine(Application.dataPath, "../Songs");
        if (!Directory.Exists(songsRootPath))
        {
            Directory.CreateDirectory(songsRootPath);
            return;
        }

        string[] songDirectories = Directory.GetDirectories(songsRootPath);
        foreach (var dir in songDirectories)
        {
            SongInfo song = ParseSongDirectory(dir);
            if (song != null)
            {
                song.SongId = AllSongs.Count;
                AllSongs.Add(song);
            }
        }
    }

    private SongInfo ParseSongDirectory(string directoryPath)
    {
        string headerPath = Path.Combine(directoryPath, "header.txt");
        if (!File.Exists(headerPath)) return null;

        string[] audioFiles = Directory.GetFiles(directoryPath, "music.*");
        if (audioFiles.Length == 0) return null;
        string autdioPath = audioFiles[0];

        SongInfo info = new SongInfo();
        info.DirectoryPath = directoryPath;
        info.AudioPath = autdioPath;

        if (!ParseHeader(headerPath, info))
        {
            Debug.LogError($"Header parse error: {directoryPath}");
            return null;
        }

        string[] imageFiles = Directory.GetFiles(directoryPath, "image.*");
        if (imageFiles.Length > 0) info.ImagePath = imageFiles[0];
        else info.ImagePath = null;

        CheckChartExist(directoryPath, "basic.txt", Difficulty.Basic, info);
        CheckChartExist(directoryPath, "advanced.txt", Difficulty.Advanced, info);
        CheckChartExist(directoryPath, "expert.txt", Difficulty.Expert, info);
        CheckChartExist(directoryPath, "master.txt", Difficulty.Master, info);

        if (info.ChartPaths.Count == 0) return null;

        return info;
    }

    private void CheckChartExist(string directoryPath, string fileName, Difficulty diff, SongInfo info)
    {
        string path = Path.Combine(directoryPath, fileName);
        if (File.Exists(path))
        {
            info.ChartPaths.Add(diff, path);
        }
    }

    private bool ParseHeader(string path, SongInfo info)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] split = line.Split(new char[] { '=' }, 2);
                if (split.Length < 2) continue;

                string key = split[0].Trim().ToUpper();
                string val = split[1].Trim();

                switch (key)
                {
                    case "TITLE":
                        info.Title = val;
                        break;
                    case "ARTIST":
                        info.Artist = val;
                        break;
                    case "BPM":
                        float.TryParse(val, out info.Bpm);
                        break;
                    case "OFFSET":
                        float.TryParse(val, out info.Offset);
                        break;
                    case "PREVIEW":
                        float.TryParse(val, out info.PreviewTime);
                        break;
                    case "VOLUME":
                        int.TryParse(val, out int volume);
                        info.Volume = Mathf.Clamp01(volume / 100f);
                        Debug.Log($"ParseHeader: volume={volume}, info.Volume={info.Volume}");
                        break;
                    case "BSCLEVEL":
                        if (int.TryParse(val, out int bscLv)) info.Levels[Difficulty.Basic] = bscLv;
                        break;
                    case "ADVLEVEL":
                        if (int.TryParse(val, out int advLv)) info.Levels[Difficulty.Advanced] = advLv;
                        break;
                    case "EXPLEVEL":
                        if (int.TryParse(val, out int expLv)) info.Levels[Difficulty.Expert] = expLv;
                        break;
                    case "MASLEVEL":
                        if (int.TryParse(val, out int masLv)) info.Levels[Difficulty.Master] = masLv;
                        break;
                }
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }
}