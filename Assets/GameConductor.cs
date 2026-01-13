using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameConductor : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public ScoreLoader scoreLoader;

    [Header("Settings")]
    public float startDelay = 1.0f;

    public float CurrentTime => (float)(AudioSettings.dspTime - _dspStartTime - startDelay);

    private double _dspStartTime;
    private bool _isPlaying = false;

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private struct VisualMapEvent
    {
        public float time;
        public float cumulativeZ;
        public float scrollSpeed;
    }
    private List<VisualMapEvent> _visualMapEvents = new List<VisualMapEvent>();

    public void SetupVisualMapEvent(List<ScrollEvent> events)
    {
        _visualMapEvents.Clear();

        float currentZ = 0.0f;
        float previousTime = 0.0f;
        float previousRate = 1.0f;

        foreach (var e in events)
        {
            float duration = e.Time - previousTime;
            currentZ += previousRate * duration;

            _visualMapEvents.Add(new VisualMapEvent
            {
                time = e.Time,
                cumulativeZ = currentZ,
                scrollSpeed = e.Rate
            });

            previousTime = e.Time;
            previousRate = e.Rate;
        }

        // 最後のイベント以降をカバーするダミーイベントを追加
        _visualMapEvents.Add(new VisualMapEvent
        {
            time = 600f,
            cumulativeZ = -1f,
            scrollSpeed = 0f
        });

        foreach (var e in _visualMapEvents)
        {
            Debug.Log($"time = {e.time}, cumulativeZ = {e.cumulativeZ}, scrollSpeed = {e.scrollSpeed}");
        }
    }

    public float GetCumulativeZAtTime(float time)
    {
        for (int i = 0; i < _visualMapEvents.Count - 1; ++i)
        {
            VisualMapEvent currentEvent = _visualMapEvents[i];
            VisualMapEvent nextEvent = _visualMapEvents[i + 1];

            if(time >= currentEvent.time && time < nextEvent.time)
            {
                float deltaTime = time - currentEvent.time;
                return currentEvent.cumulativeZ + currentEvent.scrollSpeed * deltaTime;
            }
        }

        return 0.0f;
    }

    IEnumerator Start()
    {
        if(GameSession.SelectedSong == null)
        {
            Debug.LogError("No song selected!");
            yield break;
        }

        SongInfo song = GameSession.SelectedSong;
        Difficulty diff = GameSession.SelectedDifficulty;

        AudioClip clip = null;
        yield return AssetLoader.LoadAudio(song.AudioPath, (result) => clip = result);

        if (clip != null) audioSource.clip = clip;
        else Debug.LogError("Failed to load audio clip!");
        
        string chartPath = song.ChartPaths[diff];
        string chartText = File.ReadAllText(chartPath);
        scoreLoader.LoadChart(chartText, song);

        _dspStartTime = AudioSettings.dspTime;

        SetVolume(song.Volume);
        audioSource.PlayScheduled(_dspStartTime + startDelay);
        _isPlaying = true;
    }

    private void Update()
    {
        if (_isPlaying)
        {
            if (!audioSource.isPlaying)
            {
                _isPlaying = false;
                FindObjectOfType<GameManager>().EndGame();
            }
        }
    }
}
