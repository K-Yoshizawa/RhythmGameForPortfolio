using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI scoreText;

    [Header("Left Panel Stats")]
    public TextMeshProUGUI maxComboValueText;
    public TextMeshProUGUI perfectValueText;
    public TextMeshProUGUI greatValueText;
    public TextMeshProUGUI goodValueText;
    public TextMeshProUGUI missValueText;

    [Header("Right Panel Info")]
    public TextMeshProUGUI songTitleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI difficultyText;
    public RawImage jacketImage;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip hitSound;

    private int currentCombo = 0;
    private int maxCombo = 0;
    private float currentScorePoints = 0;
    private float totalMaxPoints = 0;

    private int countPerfect = 0;
    private int countGreat = 0;
    private int countGood = 0;
    private int countMiss = 0;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        SetupSongInfo();
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void SetupSongInfo()
    {
        if (GameSession.SelectedSong == null)
        {
            Debug.LogWarning("No song selected for game scene!");
            return;
        }

        SongInfo song = GameSession.SelectedSong;
        Difficulty diff = GameSession.SelectedDifficulty;

        if (songTitleText != null) songTitleText.text = song.Title;
        if (artistText != null) artistText.text = song.Artist;

        if (difficultyText != null)
        {
            string levelVal = song.Levels.ContainsKey(diff) ? song.Levels[diff].ToString() : "?";
            difficultyText.text = $"{diff.ToString().ToUpper()} {levelVal}";
        }

        if (jacketImage != null)
        {
            StartCoroutine(AssetLoader.LoadTexture(song.ImagePath, (texture) =>
            {
                if (texture != null) jacketImage.texture = texture;
            }));
        }
    }

    public void SetMaxScoreBase(int totalNotesCount)
    {
        totalMaxPoints = totalNotesCount * 10f;
        UpdateUI();
    }

    // type: 0=Miss, 1=Good, 2=Great, 3=Perfect
    public void AddJudge(int type)
    {
        int scoreGain = 0;

        switch (type)
        {
            case 3: // Perfect
                currentCombo++;
                countPerfect++;
                scoreGain = 10;
                break;
            case 2: // Great
                currentCombo++;
                countGreat++;
                scoreGain = 9;
                break;
            case 1: // Good
                currentCombo++;
                countGood++;
                scoreGain = 6;
                break;
            case 0: // Miss
                currentCombo = 0;
                countMiss++;
                scoreGain = 0;
                break;
        }

        if (currentCombo > maxCombo) maxCombo = currentCombo;
        currentScorePoints += scoreGain;

        if (type != 0 && hitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitSound);
        }

        UpdateUI();
    }

    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    void UpdateUI()
    {
        int displayScore = 0;
        if (totalMaxPoints > 0)
        {
            displayScore = Mathf.FloorToInt(1000000f * currentScorePoints / totalMaxPoints);
        }

        if (scoreText != null)
        {
            string text = displayScore.ToString("0,000,000");
            text = text.Replace(",", "</mspace>,<mspace=0.65em>");
            scoreText.text = $"<mspace=0.65em>{text}</mspace>";
        }

        if (maxComboValueText != null) maxComboValueText.text = $"<mspace=0.65em>{maxCombo.ToString()}</mspace>";
        if (perfectValueText != null) perfectValueText.text = $"<mspace=0.65em>{countPerfect.ToString()}</mspace>";
        if (greatValueText != null) greatValueText.text = $"<mspace=0.65em>{countGreat.ToString()}</mspace>";
        if (goodValueText != null) goodValueText.text = $"<mspace=0.65em>{countGood.ToString()}</mspace>";
        if (missValueText != null) missValueText.text = $"<mspace=0.65em>{countMiss.ToString()}</mspace>";
    }

    public void EndGame()
    {
        GameResultData.Score = Mathf.FloorToInt(1000000f * currentScorePoints / totalMaxPoints);
        GameResultData.MaxCombo = maxCombo;
        GameResultData.PerfectCount = countPerfect;
        GameResultData.GreatCount = countGreat;
        GameResultData.GoodCount = countGood;
        GameResultData.MissCount = countMiss;

        SceneManager.LoadScene("ResultScene");
    }
}