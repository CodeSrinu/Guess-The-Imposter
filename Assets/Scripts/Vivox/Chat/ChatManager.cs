using System;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;
    public event Action<ChatMessage> OnMessageReceived;
    private string _currentChannelName;
    private bool _isInitialized = false;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        } 

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task InitializeVivox()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            _isInitialized = true;

            VivoxService.Instance.ChannelMessageReceived += HandleChannelMsgReceived;
            Debug.Log("Vivox Initialized succesfully");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void HandleChannelMsgReceived(VivoxMessage message)
    {
        ChatMessage msg = new ChatMessage(message.SenderDisplayName, message.MessageText, message.ReceivedTime.ToString("HH:mm"));

        OnMessageReceived?.Invoke(msg);
    }

    public async Task JoinChannel(string chanelName)
    {
        if (!_isInitialized) return;

        try
        {
            _currentChannelName = chanelName;
            await VivoxService.Instance.JoinGroupChannelAsync(chanelName, ChatCapability.TextAndAudio);      
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async Task LeaveChannel()
    {
        if (string.IsNullOrEmpty(_currentChannelName)) return;

        try
        {
            await VivoxService.Instance.LeaveChannelAsync(_currentChannelName);
            _currentChannelName = null;                                   
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async Task SendTextMessage(string text)
    {
        try
        {
            await VivoxService.Instance.SendChannelTextMessageAsync(_currentChannelName, text);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnDestroy()
    {
        if(VivoxService.Instance != null) 
            VivoxService.Instance.ChannelMessageReceived -= HandleChannelMsgReceived;
    }
}
