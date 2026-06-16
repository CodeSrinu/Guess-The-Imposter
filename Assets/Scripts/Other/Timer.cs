using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private bool _isTimeRunning = false;
    private float _timeRemaining = 0f;
    private Action _onComplete;

    [SerializeField] private TextMeshProUGUI timerTxt;

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

    //private void Start()
    //{
    //    StartTimer(10f, _onComplete);
    //}

    public void StartTimer(float time, Action onComplete)
    {
        _isTimeRunning = true;
        _timeRemaining = time + 1f;
        _onComplete = onComplete;
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
