using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Linq;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField] private GameObject blocker;
  [SerializeField] private UIManager uiManager;
  [SerializeField] private ParticipantUI participantUI;
  [SerializeField] private ChatUI chatUI;
  [SerializeField] private CrashHistoryManager crashHistoryManager;
  private SocketOptions socketOptions;
  private SocketManager MainSocketManager;
  private SocketManager ChatSocketManager;
  private Socket MainGameSocket;
  private Socket ChatSocket;
  [SerializeField] internal JSFunctCalls JSManager;
  [SerializeField] protected string TestSocketURI = "https://sl3l5zz3-5000.inc1.devtunnels.ms/";
  protected string SocketURI = null;
  [SerializeField] private string testToken;
  protected string gameNamespace = "playground-multiplayer"; //BackendChanges
  protected string chatNamespace = "chat";
  private bool hasEverConnected = false;
  private float lastPongTime = 0f;
  private float pingInterval = 2f;
  private bool waitingForPong = false;
  private int missedPongs = 0;
  private const int MaxMissedPongs = 5;
  private Coroutine PingRoutine;
  private string myAuth = null;
  internal bool isLoaded = false;
  internal bool PrevRoundAck = false;
  internal bool ReceivedRecordAck = false;
  [SerializeField] internal List<float> bets = new();
  [SerializeField] internal float tickInterval;
  [SerializeField] internal float takeoffEnd;
  [SerializeField] internal float crashDuration;
  [SerializeField] internal float roundDuration;
  [SerializeField] internal int chatCharCap;
  [SerializeField] internal int chatMessagesCap;
  [SerializeField] internal int maxHistoryCount = 17;
  [SerializeField] internal float MaxMult = 3;
  [SerializeField] internal float multFreq;
  [SerializeField] internal float balance = 0;
  [SerializeField] internal RoundStartData roundData;
  [SerializeField] internal AviatorState CurrentState = AviatorState.None;
  [SerializeField] internal KeyValuePair<bool, string> leftAck = new KeyValuePair<bool, string>(false, "");
  [SerializeField] internal KeyValuePair<bool, string> rightAck = new KeyValuePair<bool, string>(false, "");
  internal enum AviatorState
  {
    None,
    RoundStart,            // betting open
    TickerStart,           // plane flying
    Crashed                // plane crashed
  }

  private void Start()
  {
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);
    // Do something with the authToken
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    SocketURI = data.socketURL;
    myAuth = data.cookie;
  }

  private void Awake()
  {
    Application.runInBackground = true;
    isLoaded = false;
    blocker.SetActive(true);
  }

  private void OpenSocket()
  {
    SocketOptions options = new SocketOptions(); //Back2 Start
    options.AutoConnect = false;
    options.Reconnection = false;
    options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("authToken");
    StartCoroutine(WaitForAuthToken(options));
#else
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken
      };
    };
    options.Auth = authFunction;
    socketOptions = options;
    // Proceed with connecting to the server
    SetupGameSocketManager(options);
#endif
  }

  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      Debug.Log("My Socket is null");
      yield return null;
    }
    socketOptions = options;
    Debug.Log("My Auth is not null");
    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth
      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupGameSocketManager(options);
  }

  private void SetupGameSocketManager(SocketOptions options)
  {
#if UNITY_EDITOR
    // Create and setup SocketManager for Testing
    this.MainSocketManager = new SocketManager(new Uri(TestSocketURI), options);
#else
    // Create and setup SocketManager
    this.MainSocketManager = new SocketManager(new Uri(SocketURI), options);
#endif

    if (string.IsNullOrEmpty(gameNamespace) | string.IsNullOrWhiteSpace(gameNamespace))
    {
      MainGameSocket = this.MainSocketManager.Socket;
    }
    else
    {
      Debug.Log("Namespace used :" + gameNamespace);
      MainGameSocket = this.MainSocketManager.GetSocket("/" + gameNamespace);
    }
    // Set subscriptions
    MainGameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    MainGameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
    MainGameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
    MainGameSocket.On<string>("game:init", HandleGameInit);
    MainGameSocket.On<string>("game:crash", HandleGameCrash);
    MainGameSocket.On<string>("game:tick", HandleGameTick);
    MainGameSocket.On<string>("game:ticker_start", HandleTickerStart);
    MainGameSocket.On<string>("game:round_start", HandleRoundStart);
    // MainGameSocket.On<string>("game:crash_history", HandleCrashHistory);
    MainGameSocket.On<string>("leaderboard:addbet", HandleLeaderboardAddBet);
    MainGameSocket.On<string>("leaderboard:removebet", HandleLeaderboardRemoveBet);
    MainGameSocket.On<string>("leaderboard:usercashout", HandleLeaderboardUserCashout);

    MainSocketManager.Open();
  }

  void SetupChatSocketManager()
  {
#if UNITY_EDITOR
    // Create and setup SocketManager for Testing
    this.ChatSocketManager = new SocketManager(new Uri(TestSocketURI), socketOptions);
#else
    // Create and setup SocketManager
    this.ChatSocketManager = new SocketManager(new Uri(SocketURI), socketOptions);
#endif 

    if (string.IsNullOrEmpty(chatNamespace) | string.IsNullOrWhiteSpace(chatNamespace))
    {
      ChatSocket = this.ChatSocketManager.Socket;
    }
    else
    {
      Debug.Log("Namespace used :" + chatNamespace);
      ChatSocket = this.ChatSocketManager.GetSocket("/" + chatNamespace);
    }

    ChatSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, ChatOnConnected);
    ChatSocket.On(SocketIOEventTypes.Disconnect, ChatOnDisconnected);
    ChatSocket.On<Error>(SocketIOEventTypes.Error, OnError);
    ChatSocket.On<string>("chat:init", HandleChatInit);
    ChatSocket.On<string>("chat:result", HandleChatResult);

    ChatSocketManager.Open();
  }

  void ChatOnConnected(ConnectResponse resp)
  {
    Debug.Log("✅ Connected to chat server.");
  }

  void ChatOnDisconnected()
  {
    Debug.LogWarning("⚠️ Disconnected from chat server.");
  }

  void ChatOnError(Error err)
  {
    Debug.LogError("Chat Socket Error Message: " + err);
  }

  // Connected event handler implementation
  void OnConnected(ConnectResponse resp)
  {
    Debug.Log("✅ Connected to server.");

    if (hasEverConnected)
    {
      // uiManager.CheckAndClosePopups();
    }

    hasEverConnected = true;
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
    // SendPing();
  }
  private void OnError(Error err)
  {
    Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
  }
  private void OnDisconnected()
  {
    Debug.LogWarning("⚠️ Disconnected from server.");
    // uiManager.DisconnectionPopup();
    ResetPingRoutine();
  }
  private void OnPongReceived(string data)
  {
    // Debug.Log("✅ Received pong from server.");
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
    // Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
    // Debug.Log($"📦 Pong payload: {data}");
  }

  private void HandleGameInit(string data)
  {
    Debug.Log("INIT: " + data);

    JObject obj = JObject.Parse(data);
    JObject gameData = (JObject)obj["gameData"];

    if (gameData == null)
    {
      Debug.LogError("Game data missing from init message!");
      return;
    }

    // Safely read and convert numeric values
    tickInterval = (float?)gameData["tickInterval"] / 1000f ?? 0f;
    takeoffEnd = (float?)gameData["planeMotionVariable"] ?? 0f;
    crashDuration = (float?)gameData["crashInterval"] / 1000f ?? 0f;
    roundDuration = (float?)gameData["roundInterval"] / 1000f ?? 0f;
    maxHistoryCount = (int?)gameData["crashHistoryLimit"] ?? 17;
    MaxMult = (float?)gameData["maxMultiplier"] ?? 10;
    chatCharCap = (int?)gameData["chatMessageCharacterLimit"] ?? 0;
    chatMessagesCap = (int?)gameData["chatRoomMessagesLimit"] ?? 0;
    multFreq = (float?)gameData["multiplierFrequency"] ?? 0.02f;
    balance = (float)obj["player"]["balance"];

    // Handle bets array safely
    JArray betsArray = (JArray)gameData["bets"];
    if (betsArray != null)
    {
      bets = betsArray.Select(b => (float)b).ToList();
      uiManager.SetInit(bets, balance);
    }
    else
    {
      Debug.LogWarning("Bets array missing in game data.");
    }

    JArray crashHistory = (JArray)gameData["crashHistory"];
    if (crashHistory == null)
    {
      List<float> emptyList = new();
      crashHistoryManager.InitHistory(emptyList);
    }
    else
    {
      List<float> initCrashPoints = ((JArray)gameData["crashHistory"])
        .Select(x => (float)JObject.Parse(x.ToString())["crashPoint"])
        .ToList();
      crashHistoryManager.InitHistory(initCrashPoints); //Dummy, change append to last of the list if needed
    }

    Debug.Log($"TICK INTERVAL SET TO: {tickInterval}");
    SetupChatSocketManager();
  }

  private void HandleTickerStart(string data)
  {
    CurrentState = AviatorState.TickerStart;
    Debug.Log("TICKER_START: " + data);
    uiManager.OnTickerStart();
  }
  private void HandleRoundStart(string data)
  {
    CurrentState = AviatorState.RoundStart;
    Debug.Log("ROUND_START: " + data);
    roundData = JsonUtility.FromJson<RoundStartData>(data);
    uiManager.OnRoundStart(roundDuration, roundData);
    participantUI.PopulateFromRoundStart(roundData);
  }

  private void HandleGameTick(string data)
  {
    CurrentState = AviatorState.TickerStart;
    // Debug.Log("TICK: " + data);
    JObject obj = JObject.Parse(data);
    float mult = (float)obj["multiplier"];
    // Debug.Log("TICK: mult:" + mult);
    uiManager.OnMultiplierUpdate(mult, tickInterval);
  }

  private void HandleGameCrash(string data)
  {
    CurrentState = AviatorState.Crashed;
    Debug.Log("CRASH: " + data);
    JObject obj = JObject.Parse(data);
    float crashPoint = (float)obj["crashPoint"];
    uiManager.OnCrash(crashPoint, crashDuration);
    StartCoroutine(crashHistoryManager.AddCrash(crashPoint));
  }

  void HandleLeaderboardAddBet(string data)
  {
    Debug.Log("LEADERBOARD_ADDBET: " + data);
    participantUI.OnAddBet(JObject.Parse(data));
  }

  void HandleLeaderboardRemoveBet(string data)
  {
    Debug.Log("LEADERBOARD_REMOVEBET: " + data);
    participantUI.OnRemoveBet(JObject.Parse(data));
  }

  void HandleLeaderboardUserCashout(string data)
  {
    Debug.Log("LEADERBOARD_USERCASHOUT: " + data);
    participantUI.OnUserCashout(JObject.Parse(data));
  }

  void HandleChatInit(string data)
  {
    Debug.Log("CHAT INIT: " + data);
    JObject crashHistoryObj = JObject.Parse(data);
    JArray arr = (JArray)crashHistoryObj["chatHistory"];

    List<string> usernames = new();
    List<string> messages = new();
    foreach (var item in arr)
    {
      JObject obj = JObject.Parse(item.ToString());
      string username = obj["username"].ToString();
      string message = obj["message"].ToString();
      // Debug.Log(message);
      usernames.Add(username);
      messages.Add(message);
    }
    chatUI.InitChat(usernames, messages);
    blocker.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnEnter");
#endif
  }

  void HandleChatResult(string data)
  {
    Debug.Log("CHAT RESULT: " + data);
    JObject obj = JObject.Parse(data);
    chatUI.OnChatResult(obj["username"].ToString(), obj["message"].ToString());
  }

  private void SendPing()
  {
    ResetPingRoutine();
    PingRoutine = StartCoroutine(PingCheck());
  }

  void ResetPingRoutine()
  {
    if (PingRoutine != null)
    {
      StopCoroutine(PingRoutine);
    }
    PingRoutine = null;
  }

  private IEnumerator PingCheck()
  {
    while (true)
    {
      // Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

      if (missedPongs == 0)
      {
        // uiManager.CheckAndClosePopups();
      }

      // If waiting for pong, and timeout passed
      if (waitingForPong)
      {
        if (missedPongs == 2)
        {
          // uiManager.ReconnectionPopup();
        }
        missedPongs++;
        Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

        if (missedPongs >= MaxMissedPongs)
        {
          Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
          // uiManager.DisconnectionPopup();
          yield break;
        }
      }

      // Send next ping
      waitingForPong = true;
      lastPongTime = Time.time;
      // Debug.Log("📤 Sending ping...");
      SendDataWithNamespace("ping");
      yield return new WaitForSeconds(pingInterval);
    }
  }

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (MainGameSocket != null && MainGameSocket.IsOpen)
    {
      if (json != null)
      {
        MainGameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        MainGameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }

  internal void CloseGame()
  {
    Debug.Log("Unity: Closing Game");
    StartCoroutine(CloseSocket());
  }

  internal IEnumerator CloseSocket() //Back2 Start
  {
    // uiManager.RaycastBlocker.SetActive(true);
    ResetPingRoutine();

    Debug.Log("Closing Socket");

    MainSocketManager?.Close();
    ChatSocketManager?.Close();
    MainSocketManager = null;
    ChatSocketManager = null;

    Debug.Log("Waiting for socket to close");

    yield return new WaitForSeconds(0.5f);

    Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
  }

  internal void CashoutBet(CashoutData cashoutData)
  {
    Debug.Log("Cashing out bet: " + JsonUtility.ToJson(cashoutData));
    string jsonData = JsonUtility.ToJson(cashoutData);
    MainGameSocket.ExpectAcknowledgement<string>(OnBetAcknowledgementReceived).Emit("request", jsonData);
  }

  internal void CancelBet(CancelData cancelData)
  {
    Debug.Log("Cancelling bet: " + JsonUtility.ToJson(cancelData));
    string jsonData = JsonUtility.ToJson(cancelData);
    MainGameSocket.ExpectAcknowledgement<string>(OnBetAcknowledgementReceived).Emit("request", jsonData);
  }

  internal void PlaceBet(BetData betData)
  {
    Debug.Log("Placing bet: " + JsonUtility.ToJson(betData));
    string jsonData = JsonUtility.ToJson(betData);
    MainGameSocket.ExpectAcknowledgement<string>(OnBetAcknowledgementReceived).Emit("request", jsonData);
  }

  internal void RequestRecordsData(int Range, int By)
  {
    ReceivedRecordAck = false;
    string sortBy = "";
    string sortRange = "";

    switch (By)
    {
      case 0:
        sortBy = "x";
        break;
      case 1:
        sortBy = "wins";
        break;
      case 2:
        sortBy = "round";
        break;
    }

    switch (Range)
    {
      case 0:
        sortRange = "day";
        break;
      case 1:
        sortRange = "month";
        break;
      case 2:
        sortRange = "year";
        break;
    }

    RecordsData recordsData = new()
    {
      payload = new()
      {
        options = new()
        {
          sortBy = sortBy,
          sortRage = sortRange
        }
      }
    };
    string jsonData = JsonUtility.ToJson(recordsData);
    Debug.Log("Req records: " + jsonData);
    MainGameSocket.ExpectAcknowledgement<string>(RecordsAck).Emit("request", jsonData);
  }
  
  void RecordsAck(string data)
  {
    ReceivedRecordAck = true;
    Debug.Log("Records Ack: " + data);
  }

  internal void SendChatMessage(string message)
  {
    Debug.Log("Sending message: " + message);
    Message msg = new();
    msg.payload.message = message;
    string jsonData = JsonUtility.ToJson(msg);
    ChatSocket.Emit("request", jsonData);
  }

  void OnBetAcknowledgementReceived(string data)
  {
    Debug.Log("ack: " + data);
    if (leftAck.Key == false && leftAck.Value == "wait")
    {
      leftAck = new KeyValuePair<bool, string>(true, data);
    }
    if (rightAck.Key == false && rightAck.Value == "wait")
    {
      rightAck = new KeyValuePair<bool, string>(true, data);
    }
  }

  internal void SendPreviousRoundReq()
  {
    Debug.Log("Send Prev Round");
    PrevRoundAck = false;
    PrevRoundReqData reqData = new();
    MainGameSocket.ExpectAcknowledgement<string>(OnPrevRoundAck).Emit("request", JsonUtility.ToJson(reqData));
  }

  void OnPrevRoundAck(string data)
  {
    Debug.Log("PREV ROUND: " + data);
    PrevRoundAck = true;
  }
}

[Serializable]
public class RecordsData
{
  public string type = "GET_RECORDS";
  public RecordsDataPayload payload = new();
}

[Serializable]
public class RecordsDataPayload
{
  public Recordsoptions options = new();
}

[Serializable]
public class Recordsoptions
{
  public string sortRage;
  public string sortBy;
}

[Serializable]
public class PrevRoundReqData
{
  public string type = "PREVIOUS_ROUND";
}

[Serializable]
public class RoundStartData
{
  public string serverHash;
  public List<Participant> participants;
  public int totalBetAmount;
  public int totalWinAmount;
}

[Serializable]
public class Participant
{
  public string betId;
  public string userId;
  public string username;
  public int betAmount;
  public int multiplier;
  public int winAmount;
  public bool cashedOut;
  public string clientSeed;
}

[Serializable]
public class AuthTokenData
{
  public string socketURL;
  public string cookie;
}

[Serializable]
public class BetData
{
  public string type;
  public string roomId;
  public string serverHash;
  public BetAmountData payload;
}

[Serializable]
public class BetAmountData
{
  public int betIndex;
  public string clientSeed;
  public string betId;
}

[Serializable]
public class CashoutData
{
  public string type;
  public string roomId;
  public CashoutPayload payload;
}

[Serializable]
public class CashoutPayload
{
  public int betIndex;
  public string betId;
}

[Serializable]
public class CancelData
{
  public string type;
  public string roomId;
  public CancelPayload payload;
}

[Serializable]
public class CancelPayload
{
  public int betIndex;
  public string betId;
}

[Serializable]
public class AckData
{
  public bool success;
  public AckPayload payload;
  public Player player;
}

[Serializable]
public class Player
{
  public float balance;
}

[Serializable]
public class AckPayload
{
  public bool isUserInQueue;
  public string message;
  public string betId;
}

[Serializable]
public class Message
{
  public string type = "MESSAGE";
  public ChatMessageData payload = new();
}

[Serializable]
public class ChatMessageData
{
  public string message;
}
