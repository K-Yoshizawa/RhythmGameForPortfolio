using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public KeyCode key;
    private List<NoteController> notes = new List<NoteController>();

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public GameObject judgeTextPrefab;

    // 判定定数
    const float TIME_PERFECT = 0.033f;
    const float TIME_GREAT = 0.066f;
    const float TIME_GOOD = 0.1f;

    Color COLOR_PERFECT = new Color(255f / 255f, 255f / 255f, 125f / 255f);
    Color COLOR_GREAT = new Color(255f / 255f, 125f / 255f, 0f / 255f);
    Color COLOR_GOOD = new Color(0f / 255f, 225f / 255f, 224f / 255f);
    Color COLOR_MISS = new Color(125f / 255f, 125f / 255f, 125f / 255f);

    private GameConductor conductor;
    private GameManager gameManager;

    private bool isHolding = false;
    private NoteController currentHoldNote;

    void Start()
    {
        conductor = FindObjectOfType<GameConductor>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (GameSession.IsAutoMode)
        {
            if (isHolding)
            {
                ProcessHold();
            }
            else
            {
                CheckAutoPlay();
            }
        }
        else
        {
            CheckMiss();

            if (isHolding)
            {
                ProcessHold();
            }
            else
            {
                if (Input.GetKeyDown(key))
                {
                    JudgeNote();
                }
            }
        }
    }

    void CheckAutoPlay()
    {
        if (notes.Count == 0) return;
        NoteController targetNote = notes[0];

        if (targetNote == null)
        {
            notes.RemoveAt(0);
            return;
        }

        if (conductor.CurrentTime >= targetNote.targetTime)
        {
            PerformPerfectHit(targetNote);
        }
    }

    void PerformPerfectHit(NoteController targetNote)
    {
        gameManager.AddJudge(3);
        SpawnEffects("PERFECT", COLOR_PERFECT);

        if (targetNote.isHold)
        {
            StartHold(targetNote);
        }
        else
        {
            Destroy(targetNote.gameObject);
        }

        notes.RemoveAt(0);
    }

    void ProcessHold()
    {
        if (currentHoldNote == null)
        {
            isHolding = false;
            return;
        }
        
        bool isPressed = GameSession.IsAutoMode || Input.GetKey(key);
        currentHoldNote.SetColorState(isPressed);

        float time = conductor.CurrentTime;

        if(currentHoldNote.HoldJudgeTimes.Count > 0)
        {
            float judgeTime = currentHoldNote.HoldJudgeTimes.Peek();
            if (time >= judgeTime)
            {
                if (isPressed)
                {
                    gameManager.AddJudge(3);
                    SpawnEffects("PERFECT", COLOR_PERFECT);
                }
                else
                {
                    gameManager.AddJudge(0);
                    SpawnEffects("MISS", COLOR_MISS);
                }
                currentHoldNote.HoldJudgeTimes.Dequeue();
            }
        }
        else { 
            FinishHold();
            return;
        }
    }

    void FinishHold()
    {
        if (currentHoldNote != null) Destroy(currentHoldNote.gameObject);
        isHolding = false;
        currentHoldNote = null;
    }

    void JudgeNote()
    {
        if (notes.Count == 0) return;
        NoteController targetNote = notes[0];

        if (targetNote == null)
        {
            notes.RemoveAt(0);
            return;
        }

        float diff = Mathf.Abs(targetNote.targetTime - conductor.CurrentTime);

        if (diff < TIME_GOOD)
        {
            string rank = "MISS";
            Color color = COLOR_MISS;
            int judge = 0;
            if (diff < TIME_PERFECT) { rank = "PERFECT"; color = COLOR_PERFECT; judge = 3; }
            else if (diff < TIME_GREAT) { rank = "GREAT"; color = COLOR_GREAT; judge = 2; }
            else { rank = "GOOD"; color = COLOR_GOOD; judge = 1; }

            gameManager.AddJudge(judge);
            SpawnEffects(rank, color);

            if (targetNote.isHold)
            {
                StartHold(targetNote);
            }
            else
            {
                Destroy(targetNote.gameObject);
            }

            notes.RemoveAt(0);
        }
    }

    void StartHold(NoteController note)
    {
        isHolding = true;
        currentHoldNote = note;

        bool isPressed = GameSession.IsAutoMode || Input.GetKey(key);
        currentHoldNote.SetColorState(isPressed);
    }

    void CheckMiss()
    {
        if (isHolding) return;

        if (notes.Count > 0)
        {
            NoteController targetNote = notes[0];

            if (targetNote == null) { notes.RemoveAt(0); return; }

            float diff = conductor.CurrentTime - targetNote.targetTime;

            if (diff > TIME_GOOD)
            {
                gameManager.AddJudge(0);
                SpawnEffects("MISS", COLOR_MISS);

                if (targetNote.isHold)
                {
                    StartHold(targetNote);
                }
                else
                {
                    Destroy(targetNote.gameObject);
                }

                notes.RemoveAt(0);
            }
        }
    }

    public void AddNote(NoteController note) { notes.Add(note); }
    void SpawnEffects(string rankText, Color color)
    {
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        if (judgeTextPrefab != null)
        {
            Vector3 textPos = transform.position + new Vector3(0, 2.0f, 0);
            int currentCombo = gameManager.GetCurrentCombo();

            GameObject obj = Instantiate(judgeTextPrefab, textPos, Camera.main.transform.rotation);
            obj.GetComponent<JudgeTextAnim>().SetText(rankText, color, currentCombo);
        }
    }
}