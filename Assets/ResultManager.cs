using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ResultManager : MonoBehaviour
{
    [Header("Header Info")]
    public TextMeshProUGUI songTitleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI difficultyText;

    [Header("Visuals")]
    public RawImage jacketImage;
    public TextMeshProUGUI rankText;

    [Header("Score Info")]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Judgment Details")]
    public TextMeshProUGUI maxComboText;
    public TextMeshProUGUI perfectText;
    public TextMeshProUGUI greatText;
    public TextMeshProUGUI goodText;
    public TextMeshProUGUI missText;

    private const string SCENE_SONG_SELECT = "SongSelectScene";
    private const string SCENE_GAME = "GameScene";

    void Start()
    {
        if (GameSession.SelectedSong == null)
        {
            Debug.LogError("No song selected for result screen!");
            return;
        }

        SongInfo song = GameSession.SelectedSong;
        Difficulty diff = GameSession.SelectedDifficulty;

        songTitleText.text = song.Title;
        artistText.text = song.Artist;

        string levelVal = song.Levels.ContainsKey(diff) ? song.Levels[diff].ToString() : "?";
        difficultyText.text = $"{diff.ToString().ToUpper()} {levelVal}";

        StartCoroutine(AssetLoader.LoadTexture(song.ImagePath, (texture) =>
        {
            if (texture != null) jacketImage.texture = texture;
        }));

        int currentScore = GameResultData.Score;
        string text = currentScore.ToString("0,000,000");
        text = text.Replace(",", "</mspace>,<mspace=0.65em>");
        currentScoreText.text = $"<mspace=0.65em>{text}</mspace>";

        maxComboText.text = $"<mspace=0.65em>{GameResultData.MaxCombo.ToString()}</mspace>";
        perfectText.text = $"<mspace=0.65em>{GameResultData.PerfectCount.ToString()}</mspace>";
        greatText.text = $"<mspace=0.65em>{GameResultData.GreatCount.ToString()}</mspace>";
        goodText.text = $"<mspace=0.65em>{GameResultData.GoodCount.ToString()}</mspace>";
        missText.text = $"<mspace=0.65em>{GameResultData.MissCount.ToString()}</mspace>";

        ProcessHighScore(song.Title, diff, currentScore);

        rankText.text = CalculateRank(currentScore);
    }

    void ProcessHighScore(string songTitle, Difficulty diff, int currentScore)
    {
        string key = $"HighScore_{songTitle}_{diff.ToString()}";

        int oldHighScore = PlayerPrefs.GetInt(key, 0);
        string text = oldHighScore.ToString("0,000,000");
        text = text.Replace(",", "</mspace>,<mspace=0.65em>");
        text = $"<mspace=0.65em>{text}</mspace>";
        string scoreDiffText = "";

        int diffVal = currentScore - oldHighScore;
        scoreDiffText = diffVal.ToString("N0");
        scoreDiffText = scoreDiffText.Replace(",", "</mspace>,<mspace=0.65em>");
        if (GameSession.IsAutoMode)
        {
            scoreDiffText = $"-";
            highScoreText.text = $"<mspace=0.65em>{oldHighScore.ToString("0,000,000").Replace(",", "</mspace>,<mspace=0.65em>")}</mspace>";
        }
        else if (diffVal > 0)
        {
            scoreDiffText = $"<color=#00FFE0>(<mspace=0.65em>+{scoreDiffText}</mspace>)</color>";

            PlayerPrefs.SetInt(key, currentScore);
            PlayerPrefs.Save();

            highScoreText.text = $"<mspace=0.65em>{currentScore.ToString("0,000,000").Replace(",", "</mspace>,<mspace=0.65em>")}</mspace>";
        }
        else if (diffVal < 0)
        {
            scoreDiffText = $"<color=#FF4545>(<mspace=0.65em>{scoreDiffText}</mspace>)</color>";
            highScoreText.text = $"<mspace=0.65em>{oldHighScore.ToString("0,000,000").Replace(",", "</mspace>,<mspace=0.65em>")}</mspace>";
        }
        else
        {
            scoreDiffText = "(<mspace=0.65em>+0</mspace>)";
            highScoreText.text = $"<mspace=0.65em>{oldHighScore.ToString("0,000,000").Replace(",", "</mspace>,<mspace=0.65em>")}</mspace>";
        }

        highScoreText.text += $"\n<size=80%>{scoreDiffText}</size>";
    }

    string CalculateRank(int score)
    {
        if (score >= 997000) return "<color=#FAF9E4>SSS+</color>";
        else if (score >= 990000) return "<color=#FAF9E4>SSS</color>";
        else if (score >= 980000) return "<color=#FAF9E4>SS</color>";
        else if (score >= 960000) return "<color=#FAF9E4>S</color>";
        else if (score >= 925000) return "<color=#FFD700>AAA</color>";
        else if (score >= 880000) return "<color=#FFD700>AA</color>";
        else if (score >= 830000) return "<color=#FFD700>A</color>";
        else if (score >= 770000) return "<color=#EFEFEF>B</color>";
        else if (score >= 500000) return "<color=#A26748>C</color>";
        else return "<color=#7D7D7D>D</color>";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SCENE_SONG_SELECT);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SCENE_GAME);
        }
    }
}