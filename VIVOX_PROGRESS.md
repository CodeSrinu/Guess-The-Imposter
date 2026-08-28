# Vivox Chat Integration Progress - Guess The Imposter

## Project State: 2026-08-28
**Current Focus:** Implementing online voice and text chat using Unity Vivox SDK (v16+).

### ✅ Completed
- **Data Structures**: 
  - `ChatMessage.cs`: Data class for chat messages (SenderName, Text, Timestamp).
- **Core Logic (`ChatManager.cs`)**:
  - Singleton pattern implemented with `DontDestroyOnLoad`.
  - `Initialize()`: Handles `VivoxService.Instance.InitializeAsync()` and subscribes to `ChannelMessageReceived`.
  - `JoinChannel(string channelName)`: Joins a channel using the Relay join code.
  - `LeaveChannel()`: Cleans up channel connection.
  - `SendMessage(string text)`: Sends text to the active channel.
  - `HandleChannelMessageReceived(VivoxMessage message)`: Event handler for incoming messages that invokes the `OnMessageReceived` action for the UI.
- **System Integration**:
  - `MainMenuUI.cs`: Calls `ChatManager.Instance.Initialize()` during the sign-in flow.
  - `LobbyManager.cs`: Calls `ChatManager.Instance.JoinChannel(roomCode)` upon successful lobby creation or join.

### 🛠️ Technical Specifications (Vivox v16+)
- **Messaging**: Using `VivoxService.Instance.ChannelMessageReceived` (not the legacy `TextMessageReceived`).
- **Local Mute**: Planned use of `VivoxService.Instance.MuteInputDevice()` and `UnmuteInputDevice()`.
- **Local Deafen**: Planned use of `VivoxService.Instance.SetOutputDeviceVolume(0)` or iterative participant muting.

### ⏳ Pending Tasks
- [ ] **UI Implementation**:
  - `ChatPanelUI`: Scroll view, Input Field, and Send button.
  - `ChatMessageItem`: Prefab for message bubbles (Left/Right alignment based on `message.FromSelf`).
  - `ChatToggleBtn`: Toggle panel visibility and manage unread badge.
  - **Audio Toggles**: Mute/Deafen buttons with icon swapping.
- [ ] **AI Integration**:
  - Integrating LLM-powered AI players into the chat (Online only).
  - AI behavior: Personalities, suspicion levels, and trust.

### 📌 Notes for Next Session
- The UI should be kept "thin," delegating all Vivox logic to `ChatManager`.
- Ensure `ChatManager` unsubscribes from `ChannelMessageReceived` in `OnDestroy` to prevent memory leaks.
- AI players will be treated as standard Vivox users, sending messages via the `ChatManager` logic.
