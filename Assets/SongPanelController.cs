using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image jacketImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI bpmText;
    public GameObject scoreInfoGroup;
    public GameObject difficultyGroup;

    [Header("Difficulty Highlights")]
    public GameObject[] selectionOutlines;

    [Header("Difficulty Buttons")]
    public TextMeshProUGUI basicLabel;
    public TextMeshProUGUI advancedLabel;
    public TextMeshProUGUI expertLabel;
    public TextMeshProUGUI masterLabel;

    [System.Serializable]
    public class ScoreRowUI
    {
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI rankText;
    }

    [Header("Score Info Rows")]
    public ScoreRowUI basicScoreRow;
    public ScoreRowUI advancedScoreRow;
    public ScoreRowUI expertScoreRow;
    public ScoreRowUI masterScoreRow;

    public void Setup(SongInfo song)
    {
        titleText.text = song.Title;
        artistText.text = song.Artist;
        bpmText.text = $"BPM: {song.Bpm}";

        StartCoroutine(AssetLoader.LoadSprite(song.ImagePath, (sprite) =>
        {
            if (sprite != null) jacketImage.sprite = sprite;
        }));

        UpdateDifficultyLabel(basicLabel, song, Difficulty.Basic, "BASIC");
        UpdateDifficultyLabel(advancedLabel, song, Difficulty.Advanced, "ADVANCED");
        UpdateDifficultyLabel(expertLabel, song, Difficulty.Expert, "EXPERT");
        UpdateDifficultyLabel(masterLabel, song, Difficulty.Master, "MASTER");

        UpdateScorfeList(song);
    }

    void UpdateDifficultyLabel(TextMeshProUGUI label, SongInfo song, Difficulty diff, string diffName)
    {
        if (label == null) return;

        string levelText = "-";

        if(song.ChartPaths.ContainsKey(diff))
        {
            if (song.Levels.ContainsKey(diff)) levelText = song.Levels[diff].ToString();
            else levelText = "?";
        }

        label.text = $"{diffName}\n<size=150%>{levelText}</size>";
    }

    void UpdateScorfeList(SongInfo song)
    {
        UpdateSingleScoreRow(basicScoreRow, song, Difficulty.Basic);
        UpdateSingleScoreRow(advancedScoreRow, song, Difficulty.Advanced);
        UpdateSingleScoreRow(expertScoreRow, song, Difficulty.Expert);
        UpdateSingleScoreRow(masterScoreRow, song, Difficulty.Master);
    }

    void UpdateSingleScoreRow(ScoreRowUI row, SongInfo song, Difficulty diff)
    {
        bool hasChart = song.ChartPaths.ContainsKey(diff);

        if (!hasChart)
        {
            if (row.scoreText != null) row.scoreText.text = "-";
            if (row.rankText != null) row.rankText.text = "-";
            return;
        }

        string key = $"HighScore_{song.Title}_{diff.ToString()}";
        int highScore = PlayerPrefs.GetInt(key, 0);
        row.scoreText.text = $"<mspace=0.65em>{highScore.ToString("0,000,000").Replace(",", "</mspace>,<mspace=0.65em>")}</mspace>";

        row.rankText.text = CalculateRank(highScore);
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

    public void SetFocus(bool isCenter)
    {
        //artistText.gameObject.SetActive(isCenter);
        //bpmText.gameObject.SetActive(isCenter);
        //scoreInfoGroup.SetActive(isCenter);
        //difficultyGroup.SetActive(isCenter);

        if (isCenter)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.one * 0.8f;
        }
    }

    public void HighlightDifficulty(int difficultyIndex)
    {
        for (int i = 0; i < selectionOutlines.Length; ++i)
        {
            if (selectionOutlines[i] != null)
            {
                bool isSelected = (i == difficultyIndex);
                selectionOutlines[i].SetActive(isSelected);
            }
        }
    }
}