using UnityEngine;
using TMPro;

public class JudgeTextAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fadeInTime = 0.1f;
    public float displayTime = 0.25f;
    public float moveDistance = 0.5f;

    private TextMeshPro textMesh;
    private float timer = 0f;
    private Vector3 startPos;
    private Vector3 endPos;

    private Color rankColor;
    private string rankText;
    private int comboCount;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void SetText(string rank, Color color, int combo)
    {
        rankText = rank;
        rankColor = color;
        comboCount = combo;

        string hexColor = "#" + ColorUtility.ToHtmlStringRGB(color);

        string finalUserInfo = $"<color={hexColor}>{rank}</color>";

        if (combo > 0)
        {
            finalUserInfo += $"\n<size=70%><color=white>{combo}</color></size>";
        }

        textMesh.text = finalUserInfo;

        endPos = transform.position;
        startPos = endPos - new Vector3(0, moveDistance, 0);
        transform.position = startPos;
        SetAlpha(0);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer <= fadeInTime)
        {
            float rate = timer / fadeInTime;
            float easeRate = Mathf.Sin(rate * Mathf.PI * 0.5f);
            transform.position = Vector3.Lerp(startPos, endPos, easeRate);
            SetAlpha(rate);
        }
        else if (timer <= fadeInTime + displayTime)
        {
            transform.position = endPos;
            SetAlpha(1.0f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetAlpha(float alpha)
    {
        textMesh.alpha = alpha;
    }
}