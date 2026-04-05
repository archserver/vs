using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages host/join/solo flow and owns the NGO NetworkManager lifecycle
public class NetworkGameManager : MonoBehaviour
{
    public static NetworkGameManager NGMInstance;

    // when playing without networking is true which skips all NGO calls
    public static bool IsSolo { get; private set; } = true;

    [SerializeField] private string gameSceneName = "Game1";   // scene to load at start of game
    [SerializeField] private ushort port = 7654;               // Network port
    [SerializeField] private GameObject playerPrefab;          // Player prefab spawn manually after Game1 loads

    private Transform[] spawnPoints;                           // find spawn locations in Game1 at runtime
    private int _nextSpawnIndex = 0;                           // tracks which spawn point to use next

    private void Awake()
    {
        if (NGMInstance != null && NGMInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        NGMInstance = this;
        DontDestroyOnLoad(gameObject);     // persist into Game1 scene
    }

    private void Start()
    {
        // Need to know when clients connect or disconnect
        NetworkManager.Singleton.OnClientConnectedCallback    += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback   += OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure           += OnTransportFailure;
        SceneManager.sceneLoaded                              += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback    -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback   -= OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure           -= OnTransportFailure;
        SceneManager.sceneLoaded                              -= OnSceneLoaded;
    }

    // when Game1 loads, spawn players for all connected clients
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameSceneName) return;
        _nextSpawnIndex = 0;
        GameObject spawnRoot = GameObject.Find("SpawnPoints");
        if (spawnRoot != null)
        {
            spawnPoints = new Transform[spawnRoot.transform.childCount];
            for (int i = 0; i < spawnRoot.transform.childCount; i++)
                spawnPoints[i] = spawnRoot.transform.GetChild(i);
        }
        // spawn a player object for every connected client turned off auto-spawn 
        if (!IsSolo && NetworkManager.Singleton.IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        Vector3 pos = spawnPoints != null && spawnPoints.Length > 0
            ? spawnPoints[_nextSpawnIndex % spawnPoints.Length].position
            : Vector3.zero;
        _nextSpawnIndex++;
        var obj = Instantiate(playerPrefab, pos, Quaternion.identity);
        obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    // --- public API called by MultiplayerMenuUI ---

    // PC starts as host — runs both server and local client
    public void HostGame()
    {
        IsSolo = false;
        _nextSpawnIndex = 0;
        SetPort(port);
        NetworkManager.Singleton.StartHost();
        // host drives scene loading — NGO will sync clients automatically
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    // Phone joins by entering the host's local WiFi IP — scene is loaded by the host via NGO
    public void JoinGame(string hostIp)
    {
        IsSolo = false;
        SetAddress(hostIp, port);
        NetworkManager.Singleton.StartClient();
        // do NOT load scene here — NGO syncs the client to the host's scene on connect
    }

    // dedicated server — runs game logic only, no local player
    public void StartServer()
    {
        IsSolo = false;
        SetPort(port);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    // Single player — no networking, original game flow
    public void PlaySolo()
    {
        IsSolo = true;
        SceneManager.LoadScene(gameSceneName);
    }

    // shut down networking cleanly (called on quit or returning to main menu)
    public void Shutdown()
    {
        if (!IsSolo && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        IsSolo = true;
    }

    // --- NGO callbacks ---

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] Client connected: {clientId}");
        // if Game1 is already loaded (client joined after host), spawn their player now
        if (!IsSolo && NetworkManager.Singleton.IsServer && SceneManager.GetActiveScene().name == gameSceneName)
            SpawnPlayerForClient(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] Client disconnected: {clientId}");

        // if the host dropped and we are a client, return to main menu
        if (!IsSolo && !NetworkManager.Singleton.IsHost && clientId == NetworkManager.ServerClientId)
        {
            Shutdown();
            SceneManager.LoadScene("MainMenu");
        }
    }

    // called when the transport cannot connect (e.g. no server at that IP)
    private void OnTransportFailure()
    {
        Debug.Log("[NetworkGameManager] Transport failure — could not connect");
        Shutdown();
        // return to main menu so the player can try again
        SceneManager.LoadScene("MainMenu");
    }

    // --- helpers ---

    private void SetPort(ushort p)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null) transport.SetConnectionData("0.0.0.0", p);
    }

    private void SetAddress(string ip, ushort p)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null) transport.SetConnectionData(ip, p);
    }
}
