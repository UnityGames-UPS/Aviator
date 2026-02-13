using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Linq;
using DG.Tweening;
using System.Globalization;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField] private GameObject blocker;
  [SerializeField] private UIManager uiManager;
  [SerializeField] private ParticipantManager participantUI;
  [SerializeField] private ChatManager chatUI;
  [SerializeField] private CrashHistoryManager crashHistoryManager;
  [SerializeField] private CrashHistoryPopupManager crashHistoryPopupManager;
  [SerializeField] private PlayerCountManager playerCountManager;
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
  private const int MaxMissedPongs = 15;
  private Coroutine PingRoutine;
  private string myAuth = null;
  internal bool isLoaded = false;
  internal bool PrevRoundAck = false;
  internal bool BetHistAck = false;
  internal bool ReceivedRecordAck = false;
  [SerializeField] internal List<float> bets = new();
  [SerializeField] internal float tickInterval;
  [SerializeField] internal float takeOffDuration;
  [SerializeField] internal float crashDuration;
  [SerializeField] internal float roundDuration;
  [SerializeField] internal int chatCharCap;
  [SerializeField] internal int chatMessagesCap;
  [SerializeField] internal int maxHistoryCount = 17;
  [SerializeField] internal float MaxMult = 3;
  [SerializeField] internal float multFreq;
  [SerializeField] internal float balance = 0;
  [SerializeField] internal string userId = "";
  [SerializeField] internal int playerCount = 0;
  [SerializeField] internal BetHistoryData BetHistoryData = new();
  [SerializeField] internal LastRoundResult lastRoundResult = new();
  [SerializeField] internal RoundStartData roundData = new();
  [SerializeField] internal List<CrashHistoryRoundData> crashHistoryRounds = new();
  [SerializeField] internal AnalyticsRoot analyticsData = new();
  [SerializeField] internal AviatorState CurrentState = AviatorState.None;
  [SerializeField] internal KeyValuePair<bool, string> leftAck = new(false, "");
  [SerializeField] internal KeyValuePair<bool, string> rightAck = new(false, "");
  private readonly Queue<bool> pendingAckSideOrder = new();
  private string pendingLeftExpectedBetId = "";
  private string pendingRightExpectedBetId = "";
  private int pendingLeftExpectedBetIndex = -1;
  private int pendingRightExpectedBetIndex = -1;
  internal enum AviatorState
  {
    None,
    RoundStart,            // betting open
    TickerStart,           // plane flying
    Crashed                // plane crashed
  }

  private Coroutine disconnectTimerCoroutine;
  [SerializeField] private float disconnectDelay = 300f;

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
    Debug.Log("New build for vimal 3");
    Application.runInBackground = true;
    DOTween.Init();
    DOTween.defaultTimeScaleIndependent = true;
    DOTween.SetTweensCapacity(500, 50);
    blocker.SetActive(true);
    isLoaded = false;
    Debug.Log("prod build");
  }

  private void OnApplicationFocus(bool hasFocus)
  {
    if (!hasFocus)
    {
      // App lost focus, start disconnect timer
      disconnectTimerCoroutine = StartCoroutine(DisconnectTimer());
    }
    else
    {
      // App regained focus, cancel disconnect timer
      if (disconnectTimerCoroutine != null)
      {
        StopCoroutine(disconnectTimerCoroutine);
        disconnectTimerCoroutine = null;
        Debug.Log("Disconnect timer cancelled. App regained focus.");
      }
    }
  }

  private IEnumerator DisconnectTimer()
  {
    Debug.Log($"App lost focus. Disconnect timer started for {disconnectDelay} seconds.");
    yield return new WaitForSeconds(disconnectDelay);

    Debug.Log("Disconnect timer finished. Disconnecting due to prolonged focus loss.");
    MainGameSocket.Disconnect();
    ChatSocket.Disconnect();
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
    float startTime = Time.realtimeSinceStartup;
    const float timeoutSeconds = 10f;
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      if (Time.realtimeSinceStartup - startTime >= timeoutSeconds)
      {
        Debug.LogError("WaitForAuthToken timed out after 10 seconds waiting for auth token.");
        yield break;
      }
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      if (Time.realtimeSinceStartup - startTime >= timeoutSeconds)
      {
        Debug.LogError("WaitForAuthToken timed out after 10 seconds waiting for socket URI.");
        yield break;
      }
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
    MainGameSocket.On<string>("leaderboard:addbet", HandleLeaderboardAddBet);
    MainGameSocket.On<string>("leaderboard:removebet", HandleLeaderboardRemoveBet);
    MainGameSocket.On<string>("leaderboard:usercashout", HandleLeaderboardUserCashout);

    MainGameSocket.On<string>("room:joined", HandlePlayerJoined);
    MainGameSocket.On<string>("room:left", HandlePlayerLeft);

    MainGameSocket.On<string>("pong", OnPongReceived);

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
      uiManager.CheckAndClosePopups();
    }

    hasEverConnected = true;
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
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
    ClearPendingBetAcks();
    uiManager.DisconnectionPopup();
    uiManager.ResetGame();
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
    takeOffDuration = (float?)gameData["planeMotionVariable"] ?? 0f;
    crashDuration = (float?)gameData["crashInterval"] / 1000f ?? 0f;
    roundDuration = (float?)gameData["roundInterval"] / 1000f ?? 0f;
    maxHistoryCount = (int?)gameData["crashHistoryLimit"] ?? 17;
    MaxMult = (float?)gameData["maxMultiplier"] ?? 10;
    chatCharCap = (int?)gameData["chatMessageCharacterLimit"] ?? 0;
    chatMessagesCap = (int?)gameData["chatRoomMessagesLimit"] ?? 0;
    multFreq = (float?)gameData["minMultiplierFrequency"] ?? 0.02f;
    balance = (float?)obj["player"]["balance"] ?? 0.00f;
    userId = (string)gameData["userId"] ?? "";
    playerCount = (int?)gameData["playerCount"] ?? 1;
    playerCountManager.UpdatePlayerCount(playerCount);

    // Handle bets array safely
    JArray betsArray = (JArray)gameData["bets"];
    if (betsArray != null)
    {
      bets = betsArray.Select(b => (float)b).ToList();
      uiManager.SetInit(bets, balance, userId);
    }
    else
    {
      Debug.LogWarning("Bets array missing in game data.");
    }

    JArray crashHistory = (JArray)gameData["crashHistory"];
    if (crashHistory == null)
    {
      crashHistoryRounds = NormalizeCrashHistoryList(new List<CrashHistoryRoundData>());
      crashHistoryManager.InitHistory(crashHistoryRounds);
      if (crashHistoryPopupManager != null)
        crashHistoryPopupManager.InitHistory(crashHistoryRounds);
    }
    else
    {
      crashHistoryRounds = NormalizeCrashHistoryList(ParseCrashHistoryRounds(crashHistory));
      crashHistoryManager.InitHistory(crashHistoryRounds);
      if (crashHistoryPopupManager != null)
        crashHistoryPopupManager.InitHistory(crashHistoryRounds);
    }

    List<Participant> participants = new();
    float totalBet = 0;
    float totalWin = 0;
    RoundStartData roundData = null;
    JObject leaderboardInfoObj = JObject.Parse(data);
    JToken leaderboardToken = obj.SelectToken("gameData.leaderboardInfo");
    if (leaderboardToken == null || leaderboardToken.Type == JTokenType.Null)
    {
      Debug.LogWarning("ℹ️ No leaderboard data available yet (likely first round or no bets).");
    }
    else
    {
      JObject leaderboard = (JObject)obj["gameData"]?["leaderboardInfo"];
      JArray participantsArray = (JArray)leaderboard["participants"];
      participants = participantsArray.ToObject<List<Participant>>();

      totalBet = leaderboard.Value<float>("totalBetAmount");
      totalWin = leaderboard.Value<float>("totalWinAmount");

      roundData = new()
      {
        participants = participants,
        totalBetAmount = totalBet,
        totalWinAmount = totalWin
      };
    }

    Debug.Log($"TICK INTERVAL SET TO: {tickInterval}");
    SetupChatSocketManager();
    if (roundData != null)
      participantUI.PopulateFromRoundStart(roundData);
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
    if (CurrentState == AviatorState.Crashed) return;

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
    string hash = obj.Value<string>("hash") ?? obj.Value<string>("combinedHash");
    string roundId = obj.Value<string>("roundId") ?? obj.Value<string>("round_id");
    string createdAt = obj.Value<string>("createdAt") ?? obj.Value<string>("created_at");
    string serverSeed = obj.Value<string>("serverSeed");
    JToken clientSeedsToken = obj["clientSeeds"] ?? obj["client_seeds"];
    JToken userIdsToken = obj["userIds"] ?? obj["user_ids"];
    JToken usedClientSeedRecordsToken = obj["usedClientSeedRecords"];

    if (string.IsNullOrWhiteSpace(createdAt))
      createdAt = DateTime.UtcNow.ToString("o");

    CrashHistoryRoundData crashData = new()
    {
      roundId = roundId ?? "",
      serverSeed = serverSeed ?? "",
      createdAt = createdAt ?? "",
      hash = hash ?? "",
      combinedHash = obj.Value<string>("combinedHash") ?? "",
      crashPoint = crashPoint,
      userIds = new List<string>(),
      clientSeeds = new List<string>()
    };
    PopulateUserSeedData(clientSeedsToken, userIdsToken, crashData.userIds, crashData.clientSeeds);
    PopulateUsedClientSeedRecords(usedClientSeedRecordsToken, crashData.userIds, crashData.clientSeeds);

    if (!string.IsNullOrEmpty(serverSeed))
      uiManager.UpdateServerSeed(serverSeed);
    uiManager.OnCrash(crashPoint, crashDuration);
    if (crashHistoryRounds == null)
      crashHistoryRounds = new List<CrashHistoryRoundData>();
    crashHistoryRounds.Add(crashData);
    if (crashHistoryRounds.Count > maxHistoryCount)
      crashHistoryRounds.RemoveAt(0);
    StartCoroutine(crashHistoryManager.AddCrash(crashData));
    if (crashHistoryPopupManager != null)
      crashHistoryPopupManager.AddCrash(crashData);
  }

  private List<CrashHistoryRoundData> ParseCrashHistoryRounds(JArray crashHistory)
  {
    List<CrashHistoryRoundData> rounds = new();

    foreach (var token in crashHistory)
    {
      JObject roundObj = null;
      if (token == null || token.Type == JTokenType.Null)
        continue;

      if (token.Type == JTokenType.String)
      {
        string raw = token.Value<string>();
        if (string.IsNullOrWhiteSpace(raw))
          continue;

        try
        {
          roundObj = JObject.Parse(raw);
        }
        catch (Exception ex)
        {
          Debug.LogWarning($"Unable to parse crash history round: {ex.Message}");
          continue;
        }
      }
      else if (token.Type == JTokenType.Object)
      {
        roundObj = (JObject)token;
      }

      if (roundObj == null)
        continue;

      JToken clientSeedsToken = roundObj["clientSeeds"] ?? roundObj["client_seeds"];
      JToken userIdsToken = roundObj["userIds"] ?? roundObj["user_ids"];
      JToken usedClientSeedRecordsToken = roundObj["usedClientSeedRecords"];
      CrashHistoryRoundData round = new()
      {
        roundId = roundObj.Value<string>("roundId") ?? roundObj.Value<string>("round_id") ?? "",
        serverSeed = roundObj.Value<string>("serverSeed") ?? roundObj.Value<string>("server_seed") ?? "",
        createdAt = roundObj.Value<string>("finishedAt") ?? roundObj.Value<string>("createdAt") ?? roundObj.Value<string>("created_at") ?? "",
        hash = roundObj.Value<string>("hash") ?? roundObj.Value<string>("serverHash") ?? roundObj.Value<string>("server_hash") ?? roundObj.Value<string>("combinedHash") ?? "",
        combinedHash = roundObj.Value<string>("combinedHash") ?? "",
        crashPoint = ParseFloatOrDefault(roundObj["crashPoint"], 0f),
        userIds = new List<string>(),
        clientSeeds = new List<string>()
      };

      PopulateUserSeedData(clientSeedsToken, userIdsToken, round.userIds, round.clientSeeds);
      PopulateUsedClientSeedRecords(usedClientSeedRecordsToken, round.userIds, round.clientSeeds);
      rounds.Add(round);
    }

    return rounds;
  }

  private List<CrashHistoryRoundData> NormalizeCrashHistoryList(List<CrashHistoryRoundData> rounds)
  {
    var list = rounds ?? new List<CrashHistoryRoundData>();
    if (maxHistoryCount <= 0)
      return list;

    if (list.Count < maxHistoryCount)
    {
      if (crashHistoryManager != null)
        list.AddRange(crashHistoryManager.GenerateRandomCrashes(maxHistoryCount - list.Count));
    }
    else if (list.Count > maxHistoryCount)
      list = list.GetRange(list.Count - maxHistoryCount, maxHistoryCount);

    return list;
  }

  private void PopulateUserSeedData(JToken clientSeedsToken, JToken userIdsToken, List<string> userIds, List<string> clientSeeds)
  {
    if (userIdsToken is JArray userIdsArray)
    {
      foreach (var userToken in userIdsArray)
      {
        string id = userToken?.ToString() ?? "";
        if (!string.IsNullOrWhiteSpace(id))
          userIds.Add(id);
      }
    }

    if (clientSeedsToken == null || clientSeedsToken.Type == JTokenType.Null)
      return;

    if (clientSeedsToken is JObject seedsObject)
    {
      foreach (var prop in seedsObject.Properties())
      {
        if (!string.IsNullOrWhiteSpace(prop.Name))
          userIds.Add(prop.Name);

        string seed = prop.Value?.ToString() ?? "";
        if (!string.IsNullOrWhiteSpace(seed))
          clientSeeds.Add(seed);
      }
      return;
    }

    if (clientSeedsToken is JArray seedsArray)
    {
      foreach (var seedToken in seedsArray)
      {
        string seed = seedToken?.ToString() ?? "";
        if (!string.IsNullOrWhiteSpace(seed))
          clientSeeds.Add(seed);
      }
      return;
    }

    string singleSeed = clientSeedsToken.ToString();
    if (!string.IsNullOrWhiteSpace(singleSeed))
      clientSeeds.Add(singleSeed);
  }

  private void PopulateUsedClientSeedRecords(JToken usedClientSeedRecordsToken, List<string> userIds, List<string> clientSeeds)
  {
    if (usedClientSeedRecordsToken == null || usedClientSeedRecordsToken.Type == JTokenType.Null)
      return;

    if (usedClientSeedRecordsToken is JObject)
      return;

    if (usedClientSeedRecordsToken is JArray records)
    {
      foreach (var record in records)
      {
        if (record == null || record.Type != JTokenType.Object)
          continue;

        string userId = record.Value<string>("userId") ?? "";
        string seed = record.Value<string>("seed") ?? "";
        if (!string.IsNullOrWhiteSpace(userId))
          userIds.Add(userId);
        if (!string.IsNullOrWhiteSpace(seed))
          clientSeeds.Add(seed);
      }
    }
  }

  private float ParseFloatOrDefault(JToken token, float fallback)
  {
    if (token == null || token.Type == JTokenType.Null)
      return fallback;

    if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
      return token.Value<float>();

    if (float.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
      return parsed;

    return fallback;
  }

  void HandleLeaderboardAddBet(string data)
  {
    Debug.Log("LEADERBOARD_ADDBET: " + data);
    try
    {
      var obj = JObject.Parse(data);
      var pToken = obj["participant"];
      if (pToken == null)
      {
        Debug.LogError("leaderboard:addbet payload missing 'participant'");
        return;
      }

      Participant p = pToken.ToObject<Participant>();
      if (p == null || string.IsNullOrEmpty(p.betId))
      {
        Debug.LogError("Failed to deserialize participant or missing betId");
        return;
      }

      participantUI.OnAddBet(p);
    }
    catch (Exception ex)
    {
      Debug.LogError($"Error parsing leaderboard:addbet: {ex.Message}\n{data}");
    }
  }

  void HandleLeaderboardRemoveBet(string data)
  {
    Debug.Log("LEADERBOARD_REMOVEBET: " + data);
    try
    {
      var obj = JObject.Parse(data);
      string betId = obj.Value<string>("betId");
      if (string.IsNullOrEmpty(betId))
      {
        Debug.LogWarning("leaderboard:removebet missing betId");
        return;
      }

      participantUI.OnRemoveBet(betId);
    }
    catch (Exception ex)
    {
      Debug.LogError($"Error parsing leaderboard:removebet: {ex.Message}\n{data}");
    }
  }

  void HandleLeaderboardUserCashout(string data)
  {
    Debug.Log("LEADERBOARD_USERCASHOUT: " + data);
    try
    {
      var obj = JObject.Parse(data);
      string betId = obj.Value<string>("betId");
      if (string.IsNullOrEmpty(betId))
      {
        Debug.LogWarning("leaderboard:usercashout missing betId");
        return;
      }

      float win = obj.Value<float?>("winAmount") ?? 0f;
      float mult = obj.Value<float?>("multiplier") ?? 0f;

      participantUI.OnUserCashout(betId, win, mult);
    }
    catch (Exception ex)
    {
      Debug.LogError($"Error parsing leaderboard:usercashout: {ex.Message}\n{data}");
    }
  }

  void HandlePlayerJoined(string data)
  {
    Debug.Log("PLAYER_JOINED: " + data);
    JObject playerObj = JObject.Parse(data);
    playerCountManager.UpdatePlayerCount((int)playerObj["playerCount"]);
  }

  void HandlePlayerLeft(string data)
  {
    Debug.Log("PLAYER_LEFT: " + data);
    JObject playerObj = JObject.Parse(data);
    playerCountManager.UpdatePlayerCount((int)playerObj["playerCount"]);
  }

  void HandleChatInit(string data)
  {
    Debug.Log("CHAT INIT: " + data);
    SendPing();
    JObject crashHistoryObj = JObject.Parse(data);
    JArray arr = (JArray)crashHistoryObj["chatHistory"];

    List<string> usernames = new();
    List<string> messages = new();
    foreach (var item in arr)
    {
      JObject obj = JObject.Parse(item.ToString());
      string username = obj["userId"].ToString();
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
    chatUI.OnChatResult(obj["userId"].ToString(), obj["message"].ToString());
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
        uiManager.CheckAndClosePopups();
      }

      // If waiting for pong, and timeout passed
      if (waitingForPong)
      {
        if (missedPongs == 2)
        {
          uiManager.ReconnectionPopup();
        }
        missedPongs++;
        Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

        if (missedPongs >= MaxMissedPongs)
        {
          Debug.LogError("❌ Unable to connect to server — "+ MaxMissedPongs + " consecutive pongs missed.");
          uiManager.DisconnectionPopup();
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
    blocker.SetActive(true);
    ResetPingRoutine();
    ClearPendingBetAcks();

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

  internal void CashoutBet(CashoutData cashoutData, bool isLeft)
  {
    Debug.Log("Cashing out bet: " + JsonUtility.ToJson(cashoutData));
    RegisterPendingBetAck(isLeft, cashoutData.payload.betId, cashoutData.payload.betIndex);
    string jsonData = JsonUtility.ToJson(cashoutData);
    MainGameSocket.ExpectAcknowledgement<string>(BetAcks).Emit("request", jsonData);
  }

  internal void CancelBet(CancelData cancelData, bool isLeft)
  {
    Debug.Log("Cancelling bet: " + JsonUtility.ToJson(cancelData));
    RegisterPendingBetAck(isLeft, cancelData.payload.betId, cancelData.payload.betIndex);
    string jsonData = JsonUtility.ToJson(cancelData);
    MainGameSocket.ExpectAcknowledgement<string>(BetAcks).Emit("request", jsonData);
  }

  internal void PlaceBet(BetData betData, bool isLeft)
  {
    Debug.Log("Placing bet: " + JsonUtility.ToJson(betData));
    RegisterPendingBetAck(isLeft, betData.payload.betId, betData.payload.betIndex);
    string jsonData = JsonUtility.ToJson(betData);
    MainGameSocket.ExpectAcknowledgement<string>(BetAcks).Emit("request", jsonData);
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
    Debug.Log("Records Ack: " + data);
    analyticsData = JsonUtility.FromJson<AnalyticsRoot>(data);
    ReceivedRecordAck = true;
  }

  internal void SendChatMessage(string message)
  {
    Debug.Log("Sending message: " + message);
    Message msg = new();
    msg.payload.message = message;
    string jsonData = JsonUtility.ToJson(msg);
    ChatSocket.Emit("request", jsonData);
  }

  void BetAcks(string data)
  {
    Debug.Log("ack: " + data);
    bool leftWaiting = leftAck.Key == false && leftAck.Value == "wait";
    bool rightWaiting = rightAck.Key == false && rightAck.Value == "wait";

    if (!leftWaiting && !rightWaiting)
    {
      Debug.LogWarning($"Ack received with no pending side: {data}");
      return;
    }

    bool? routedSide = ResolveAckSide(data, leftWaiting, rightWaiting);
    if (!routedSide.HasValue)
    {
      routedSide = leftWaiting ? true : false;
      Debug.LogWarning("Unable to resolve ack side from payload; falling back to first waiting side.");
    }

    if (routedSide.Value)
    {
      leftAck = new KeyValuePair<bool, string>(true, data);
      pendingLeftExpectedBetId = "";
      pendingLeftExpectedBetIndex = -1;
      RemoveSideFromPendingQueue(true);
    }
    else
    {
      rightAck = new KeyValuePair<bool, string>(true, data);
      pendingRightExpectedBetId = "";
      pendingRightExpectedBetIndex = -1;
      RemoveSideFromPendingQueue(false);
    }
  }

  private void RegisterPendingBetAck(bool isLeft, string expectedBetId, int expectedBetIndex)
  {
    if (isLeft)
    {
      leftAck = new KeyValuePair<bool, string>(false, "wait");
      pendingLeftExpectedBetId = expectedBetId ?? "";
      pendingLeftExpectedBetIndex = expectedBetIndex;
    }
    else
    {
      rightAck = new KeyValuePair<bool, string>(false, "wait");
      pendingRightExpectedBetId = expectedBetId ?? "";
      pendingRightExpectedBetIndex = expectedBetIndex;
    }

    pendingAckSideOrder.Enqueue(isLeft);
  }

  private bool? ResolveAckSide(string data, bool leftWaiting, bool rightWaiting)
  {
    if (leftWaiting && !rightWaiting) return true;
    if (!leftWaiting && rightWaiting) return false;

    try
    {
      JObject obj = JObject.Parse(data);
      string ackBetId = obj.SelectToken("payload.betId")?.Value<string>() ?? obj["betId"]?.Value<string>();
      int? ackBetIndex = obj.SelectToken("payload.betIndex")?.Value<int?>() ?? obj["betIndex"]?.Value<int?>();

      bool leftById = !string.IsNullOrEmpty(ackBetId) && pendingLeftExpectedBetId == ackBetId;
      bool rightById = !string.IsNullOrEmpty(ackBetId) && pendingRightExpectedBetId == ackBetId;

      if (leftById ^ rightById)
      {
        return leftById;
      }

      bool leftByIndex = ackBetIndex.HasValue && pendingLeftExpectedBetIndex == ackBetIndex.Value;
      bool rightByIndex = ackBetIndex.HasValue && pendingRightExpectedBetIndex == ackBetIndex.Value;

      if (leftByIndex ^ rightByIndex)
      {
        return leftByIndex;
      }
    }
    catch (Exception ex)
    {
      Debug.LogWarning($"Unable to parse ack payload for side resolution: {ex.Message}");
    }

    while (pendingAckSideOrder.Count > 0)
    {
      bool candidate = pendingAckSideOrder.Dequeue();
      if (candidate && leftWaiting) return true;
      if (!candidate && rightWaiting) return false;
    }

    return null;
  }

  private void RemoveSideFromPendingQueue(bool side)
  {
    if (pendingAckSideOrder.Count == 0) return;

    Queue<bool> rebuilt = new();
    bool removed = false;
    while (pendingAckSideOrder.Count > 0)
    {
      bool value = pendingAckSideOrder.Dequeue();
      if (!removed && value == side)
      {
        removed = true;
        continue;
      }
      rebuilt.Enqueue(value);
    }

    while (rebuilt.Count > 0)
    {
      pendingAckSideOrder.Enqueue(rebuilt.Dequeue());
    }
  }

  private void ClearPendingBetAcks()
  {
    pendingAckSideOrder.Clear();
    pendingLeftExpectedBetId = "";
    pendingRightExpectedBetId = "";
    pendingLeftExpectedBetIndex = -1;
    pendingRightExpectedBetIndex = -1;
    leftAck = new KeyValuePair<bool, string>(false, "");
    rightAck = new KeyValuePair<bool, string>(false, "");
  }

  internal void SendPreviousRoundReq()
  {
    PrevRoundAck = false;
    PrevRoundReqData reqData = new();
    MainGameSocket.ExpectAcknowledgement<string>(OnPrevRoundAck).Emit("request", JsonUtility.ToJson(reqData));
  }

  void OnPrevRoundAck(string data)
  {
    Debug.Log("PREV ROUND: " + data);
    PrevRoundRoot prevRound = JsonUtility.FromJson<PrevRoundRoot>(data);
    if (prevRound.payload.lastRoundResult != null)
    {
      lastRoundResult = prevRound.payload.lastRoundResult;
    }
    else
    {
      lastRoundResult = new()
      {
        crashPoint = 0.00f
      };
    }
    PrevRoundAck = true;
  }

  internal void OnRequestBetHistory()
  {
    Debug.Log("Requesting Bet History");
    BetHistAck = false;
    BetHistoryReqData data = new();
    string json = JsonUtility.ToJson(data);
    MainGameSocket.ExpectAcknowledgement<string>(OnBetHistoryAck).Emit("request", json);
  }

  void OnBetHistoryAck(string data)
  {
    Debug.Log("BET_HIST: " + data);
    BetHistoryData = JsonUtility.FromJson<BetHistoryData>(data);
    BetHistAck = true;
  }
}

[Serializable]
public class BetHistoryData
{
  public BetHistoryDataPayload payload;
}

[Serializable]
public class BetHistoryDataPayload
{
  public List<BetHistory> betHistory;
}

[Serializable]
public class BetHistory
{
  public string user_id;
  public float bet_amount = -1;
  public float win_amount = -1;
  public string created_at;
  public float multiplier = -1;
}

[Serializable]
public class BetHistoryReqData
{
  public string type = "BET_HISTORY";
}

[Serializable]
public class LastRoundResult
{
  public float crashPoint;
  public List<Participant> participants;
}

[Serializable]
public class PrevRoundPayload
{
  public LastRoundResult lastRoundResult;
}

[Serializable]
public class PrevRoundRoot
{
  public bool success;
  public string id;
  public PrevRoundPayload payload;
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
  public float totalBetAmount;
  public float totalWinAmount;
}

[Serializable]
public class Participant
{
  public string betId;
  public string userId;
  public string username;
  public float betAmount;
  public float multiplier;
  public float winAmount;
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
  public float winAmount;
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

[Serializable]
public class AnalyticsRecord
{
  public string round_id;
  public string roundId;
  public string created_at;
  public string createdAt;
  public string user_id;
  public string userId;
  public float bet_amount;
  public float betAmount;
  public float win_amount;
  public float winAmount;
  public float multiplier;
  public RoundDetails round_details;
}

[Serializable]
public class AnalyticsPayload
{
  public List<AnalyticsRecord> analyticsRecords;
}

[Serializable]
public class AnalyticsRoot
{
  public bool success;
  public string id;
  public AnalyticsPayload payload;
}

[Serializable]
public class RoundDetails
{
  public float crashPoint;
  public string server_seed;
  public string serverSeed;
  public List<UsedClientSeedRecord> usedClientSeedRecords;
  public List<string> client_seeds;
  public List<string> clientSeeds;
  public string hash;
  public string server_hash;
  public string created_at;
  public string createdAt;
  public List<string> user_ids;
  public List<string> userIds;
}

[Serializable]
public class UsedClientSeedRecord
{
  public string userId;
  public string seed;
}

[Serializable]
public class CrashHistoryRoundData
{
  public string roundId;
  public string serverSeed;
  public string createdAt;
  public string hash;
  public string combinedHash;
  public float crashPoint;
  public List<string> userIds;
  public List<string> clientSeeds;
}
