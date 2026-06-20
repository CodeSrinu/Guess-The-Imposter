using JetBrains.Annotations;
using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private bool _isTimeRunning = false;
    private float _timeRemaining = 0f;
    private Action _onComplete;

    [SerializeField] private GameObject timerTxtPrefab;
    private TextMeshProUGUI timerTxt;

    public static Timer instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

    }

    private void Start()
    {
        GameObject obj = Instantiate(timerTxtPrefab, transform);
        timerTxt = obj.GetComponent<TextMeshProUGUI>();
    }

    public void StartTimer(float time, Action onComplete)
    {
        _isTimeRunning = true;
        _timeRemaining = time;
        _onComplete = onComplete;
        DisplayTime(_timeRemaining);
    }

    private void Update()
    {
        if(_isTimeRunning)
        {
            if(_timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
                DisplayTime(_timeRemaining);
            }
            else
            {
                _isTimeRunning = false;
                _timeRemaining = 0;
                DisplayTime(_timeRemaining);
                OnTimerEnd();
            }
            timerTxt.gameObject.SetActive(true);
        }

        if(!_isTimeRunning)
        {
            timerTxt.gameObject.SetActive(false);
        }
    }

    private void DisplayTime(float time)
    {
        if (time < 0) time = 0;

        int secs = Mathf.FloorToInt(time % 60);

        timerTxt.text = secs.ToString();
    }

    private void OnTimerEnd()
    {
        _onComplete?.Invoke();
    }

    public void StopTimer()
    {
        _isTimeRunning = false;
        _timeRemaining = 0f;
    }
}
