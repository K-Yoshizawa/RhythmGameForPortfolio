using UnityEngine;

public class ScoreLoader : MonoBehaviour
{
    public GameConductor conductor;
    public GameObject notePrefab;
    public GameObject holdPrefab;
    public GameObject barLinePrefab;

    public LaneManager[] laneManagers;

    [Header("Option")]
    [Range(10.0f, 100.0f)]
    public float noteSpeed = 20.0f;

    [Header("Visuals")]
    public Color[] laneColors;

    private float[] lanePositions = { -3.75f, -2.25f, -0.75f, 0.75f, 2.25f, 3.75f };

    public void LoadChart(string chartText, SongInfo songInfo)
    {
        ChartData chartData = ChartLoader.LoadChart(chartText, songInfo.Bpm, songInfo.Offset);
        conductor.SetupVisualMapEvent(chartData.ScrollEvents);

        int totalJudgments = chartData.TotalJudgments;
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.SetMaxScoreBase(totalJudgments);
        }

        //Debug.Log($"Load Complete: Title={chartData.Title}, NotesCount={chartData.Notes.Count}");

        foreach (var noteData in chartData.Notes)
        {
            SpawnNoteObject(noteData);
        }

        foreach (float time in chartData.BarLineTimes)
        {
            SpawnBarLine(time);
        }
    }

    void SpawnNoteObject(NoteData data)
    {
        if (data.Type == 3) return;

        GameObject prefab = (data.Type == 2) ? holdPrefab : notePrefab;
        if (prefab == null) prefab = notePrefab;

        GameObject obj = Instantiate(prefab);

        obj.transform.position = new Vector3(lanePositions[data.LaneIndex], 0.1f, 0f);

        NoteController controller = obj.GetComponent<NoteController>();
        if (controller != null)
        {
            Color noteColor = Color.white;
            if (laneColors != null && data.LaneIndex < laneColors.Length)
            {
                noteColor = laneColors[data.LaneIndex];
            }

            controller.Initialize(conductor, data, noteColor, noteSpeed);

            if (data.LaneIndex >= 0 && data.LaneIndex < laneManagers.Length)
            {
                laneManagers[data.LaneIndex].AddNote(controller);
            }
        }
    }

    void SpawnBarLine(float time)
    {
        GameObject obj = Instantiate(barLinePrefab);
        obj.transform.position = new Vector3(0, 0.1f, 0f);

        NoteController controller = obj.GetComponent<NoteController>();
        if (controller != null)
        {
            NoteData dummyData = new NoteData
            {
                Time = time,
                LaneIndex = 0,
                Type = 0
            };
            controller.Initialize(conductor, dummyData, Color.gray, noteSpeed);
            controller.autoDestroy = true;
        }
    }
}