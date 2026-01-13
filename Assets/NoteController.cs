using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float targetTime;
    public float noteSpeed = 10.0f;
    public bool autoDestroy = false;

    [Header("Hold Settings")]
    public Transform bodyTransform;
    public bool isHold = false;
    public float duration = 0;

    private GameConductor _conductor;
    private Renderer _bodyRenderer;
    private Color _originalColor;
    private Color _darkColor;

    public Queue<float> HoldJudgeTimes = new Queue<float>();

    public void Initialize(GameConductor conductor, NoteData data, Color laneColor, float baseSpeed)
    {
        _conductor = conductor;
        targetTime = data.Time;
        noteSpeed = baseSpeed;
        isHold = data.Type == 2;
        duration = data.Duration;

        // ノーツに関しては色の設定
        if (bodyTransform != null)
        {
            _bodyRenderer = bodyTransform.GetComponent<Renderer>();
            if (_bodyRenderer != null)
            {
                _originalColor = laneColor;
                _bodyRenderer.material.color = _originalColor;
                _darkColor = _originalColor * 0.5f;
            }

            // ホールドノーツについては追加の初期化設定
            if (isHold) UpdateHoldVisual(duration);
            HoldJudgeTimes = new Queue<float>(data.HoldJudgeTimes);
        }
    }

    void Update()
    {
        if (_conductor == null) return;

        UpdateVisuals();

        if (autoDestroy && transform.position.z < -5.0f)
        {
            Destroy(gameObject);
        }
    }

    void UpdateVisuals()
    {
        float currentTime = _conductor.CurrentTime;
        float targetZ = _conductor.GetCumulativeZAtTime(targetTime);
        float currentZ = _conductor.GetCumulativeZAtTime(currentTime);
        float zPosition = (targetZ - currentZ) * noteSpeed;

        if (isHold)
        {
            float holdEndZ = _conductor.GetCumulativeZAtTime(targetTime + duration);
            float length;
            if (currentTime >= targetTime)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                length = (holdEndZ - currentZ) * noteSpeed;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, zPosition);
                length = (holdEndZ - targetZ) * noteSpeed;
            }
            if (length < 0) length = 0;
            UpdateHoldVisual(length);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zPosition);
        }
    }

    void UpdateHoldVisual(float length)
    {
        if (bodyTransform == null) return;
        Vector3 scale = bodyTransform.localScale;
        scale.z = length;
        bodyTransform.localScale = scale;
        bodyTransform.localPosition = new Vector3(0, 0, length / 2);
    }

    public void SetColorState(bool isActive)
    {
        if (_bodyRenderer == null) return;
        _bodyRenderer.material.color = isActive ? _originalColor : _darkColor;
    }
}