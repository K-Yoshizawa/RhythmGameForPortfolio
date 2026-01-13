using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongSelectManager : MonoBehaviour
{
    [Header("References")]
    public SongLoader songLoader;
    public Transform songContainer;
    public GameObject songPanelPrefab;

    [Header("Settings")]
    public float panelSpacing = 1000f;
    public float moveSpeed = 10f;
    public TextMeshProUGUI autoModeText;

    [Header("Audio")]
    public AudioSource previewAudioSource;
    public float previewStartDelay = 0.5f;

    private Coroutine previewCoroutine;

    private List<SongPanelController> spawnedPanels = new List<SongPanelController>();
    private int currentSongIndex = 0;
    private int selectedDifficultyIndex = 0;
    private bool isDifficultySelectMode = false;

    void Start()
    {
        songLoader.LoadSongs();

        foreach(var song in SongLoader.AllSongs)
        {
            GameObject obj = Instantiate(songPanelPrefab, songContainer);
            SongPanelController ctrl = obj.GetComponent<SongPanelController>();
            ctrl.Setup(song);
            spawnedPanels.Add(ctrl);
        }

        UpdateSelectionVisuals();
        UpdateAutoModeUI();
        CallPreviewAudio();
    }

    void CallPreviewAudio()
    {
        if (previewCoroutine != null) StopCoroutine(previewCoroutine);
        previewCoroutine = StartCoroutine(PlayPreviewFlow());
    }

    void StopPreview()
    {
        if (previewCoroutine != null) StopCoroutine(previewCoroutine);
        if (previewAudioSource.isPlaying) previewAudioSource.Stop();
    }

    IEnumerator PlayPreviewFlow()
    {
        yield return new WaitForSeconds(previewStartDelay);
        
        SongInfo song = SongLoader.AllSongs[currentSongIndex];
        
        previewAudioSource.Stop();
        previewAudioSource.clip = null;

        AudioClip clip = null;
        yield return AssetLoader.LoadAudio(song.AudioPath, (result) => clip = result);

        if (clip != null)
        {
            previewAudioSource.clip = clip;
            previewAudioSource.time = song.PreviewTime;
            previewAudioSource.volume = song.Volume;
            previewAudioSource.Play();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameSession.IsAutoMode = !GameSession.IsAutoMode;
            UpdateAutoModeUI();
        }

        HandleInput();
        UpdatePanelPositions();
    }

    void HandleInput()
    {
        if (spawnedPanels.Count == 0) return;

        if (!isDifficultySelectMode)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                --currentSongIndex;
                if (currentSongIndex < 0) currentSongIndex = spawnedPanels.Count - 1;
                CallPreviewAudio();
            }
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++currentSongIndex;
                if (currentSongIndex >= spawnedPanels.Count) currentSongIndex = 0;
                CallPreviewAudio();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                isDifficultySelectMode = true;
                UpdateSelectionVisuals();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                --selectedDifficultyIndex;
                if (selectedDifficultyIndex < 0) selectedDifficultyIndex = 0;
                UpdateSelectionVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++selectedDifficultyIndex;
                if (selectedDifficultyIndex > 3) selectedDifficultyIndex = 3;
                UpdateSelectionVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                isDifficultySelectMode = false;
                UpdateSelectionVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
        }
    }

    void UpdateAutoModeUI()
    {
        if (autoModeText != null)
        {
            autoModeText.text = GameSession.IsAutoMode ? "AUTO: ON" : "AUTO: OFF";
        }
    }

    void UpdatePanelPositions()
    {
        for(int i = 0; i < spawnedPanels.Count; ++i)
        {
            float indexDiff = i - currentSongIndex;
            float targetX = indexDiff * panelSpacing;
            Vector3 targetPos = new Vector3(targetX, 0, 0);
            spawnedPanels[i].transform.localPosition = Vector3.Lerp(spawnedPanels[i].transform.localPosition, targetPos, Time.deltaTime * moveSpeed);
            
            bool isCenter = (i == currentSongIndex);
            spawnedPanels[i].SetFocus(isCenter);
        }
    }

    void UpdateSelectionVisuals()
    {
        var currentPanel = spawnedPanels[currentSongIndex];
        if (isDifficultySelectMode)
        {
            currentPanel.HighlightDifficulty(selectedDifficultyIndex);
        }
        else
        {
            currentPanel.HighlightDifficulty(-1);
        }
    }

    void StartGame()
    {
        SongInfo song = SongLoader.AllSongs[currentSongIndex];
        Difficulty diff = (Difficulty)selectedDifficultyIndex;

        if(!song.ChartPaths.ContainsKey(diff))
        {
            Debug.LogWarning("Selected difficulty chart not found!");
            return;
        }

        GameSession.SelectedSong = song;
        GameSession.SelectedDifficulty = diff;

        StopPreview();

        SceneManager.LoadScene("GameScene");
    }
}