using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game i { private set; get; }

    public List<Player> Players { get; set; } = new List<Player>();
    public Player LocalPlayer { get; private set; }

    public AudioSource AudioSource => LocalPlayer && LocalPlayer.IsSpawned ? LocalPlayer.source : source;

    public Vector2 MousePosition;

    public bool IsPressing { get; private set; }

    public bool IsFiring { get; private set; }

    public bool IsMobile { get; private set; }

    public float killZ;

    public bool IsConnected { get; private set; }

    [SerializeField] private MeshRenderer[] waterPlanes;
    [SerializeField] private bool forceMobile;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Camera observerCamera;
    [SerializeField] private float visibilityDistance = 400f;

    [SerializeField] private Gradient topGradient;
    [SerializeField] private Gradient botGradient;
    [SerializeField] private Material levelMat;

    WebSocket websocket;
    NetControllers controllers = new NetControllers();
    E_ConnectionState connectionState;
    Dictionary<int, int> scores = new Dictionary<int, int>();

    [SerializeField] private int timeOffset;

    const float secondsInDay = 86400f;
    float Time => (float)((System.DateTime.Now.TimeOfDay.TotalSeconds + 3600 * 6 + timeOffset) % secondsInDay) / secondsInDay;

    enum E_ConnectionState { CONNECTING, ERROR, OK };

    List<string> names = new List<string>();
    Dictionary<int, StandardMissile> liveRemoteMissiles = new Dictionary<int, StandardMissile>();

    private Material skyMat;
    private Material waterPlaneMat;

    [SerializeField] private AudioSource source;

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool checkIfMobile();

    private bool CheckIfMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             return checkIfMobile();
#endif
        return forceMobile;
    }

    void Awake()
    {
        i = this;

        Debug.Log(Mathf.Floor(Time * 100f));

        //waterPlane.transform.position  = Vector3.down* killZ;

        waterPlaneMat = Instantiate(waterPlanes[0].sharedMaterial);
        for (int i = 0; i < waterPlanes.Length; i++)
        {
            waterPlanes[i].sharedMaterial = waterPlaneMat;
        }

        names.AddRange(new string[]
        {
            "Maverick", "Goose", "Viper", "Iceman", "Hollywood", "Charlie", "Jester", "Stinger", "Wolfman", "Merlin", "Slider", "Chipper", "Sundown", "Sark", "Clu", "Yori", "Crom", "Ram", "Chip", "Thorne", "Rinzler", "Tesler", "Link", "Pavel", "Zero", "Hurricane", "Typhoon", "Tornado", "Mirage", "Castor", "Roc", "Louve", "Striker", "Lancaster", "Kanoziev", "Maddox", "Trooper", "Aiglon", "Manta", "Sugar", "Thunder", "Dancer", "Crow", "Raven", "Xunlai", "Moose"
        });

        StartCoroutine(ConnectSocket());

        IsMobile = CheckIfMobile();
        //mobileCamera.SetActive(IsMobile);
    }

    private void Start()
    {
        StartCoroutine(UpdatePlayersVisibility());

        skyMat = Instantiate(RenderSettings.skybox);

        RenderSettings.skybox = skyMat;
    }

    public void SpawnMissile(NetControllers.Missile missile) 
    {
        var owner = GetPlayerById(missile.owner);

        if (owner== null)
        {
            Debug.Log($"Missile for unknown owner {missile.owner}, discarding");
            return;
        }

        if (!owner.IsSpawned)
        {
            Debug.Log($"Attempted missile birth but owner is not spawned! {owner.id}");
            return;
        }

        if (missile.type == Weapon.ELoadout.HOMING)
        {
            // something to do here
        }
        else 
        {
            var missileScript = owner.weapon.SpawnMissile();
            missileScript.transform.position = new Vector3(missile.position.x, missile.position.y, missile.position.z);
            missileScript.transform.rotation = new Quaternion(missile.initialRotation[0], missile.initialRotation[1], missile.initialRotation[2], missile.initialRotation[3]);
        }
    }


    IEnumerator UpdatePlayersVisibility()
    {
        var wait = new WaitForEndOfFrame();
        var sqrMaxDist = visibilityDistance * visibilityDistance;

        while (true)
        {
            var camera = LocalPlayer && LocalPlayer.IsSpawned ? LocalPlayer.camera : observerCamera;

            for(var i = 0; i < Players.Count; i++)
            {
                if (Players[i] == null) continue;

                var player = Players[i];

                if (player.IsLocal) continue;
                if (!player.IsSpawned) continue;


                var vec = camera.transform.position- player.transform.position;
                var sqrDist = Vector3.SqrMagnitude(vec);
                bool isCloseEnough = !LocalPlayer.IsSpawned || sqrDist < sqrMaxDist;
                bool isInScreen = false;
                bool isNotBehindObject = false;

                if (isCloseEnough)
                {
                    var screenPoint = camera.WorldToScreenPoint(player.transform.position);

                    isInScreen = (screenPoint.x > 0 && screenPoint.y > 0 && screenPoint.x < Screen.width && screenPoint.y < Screen.height);

                    if (isInScreen && Vector3.Dot(vec, camera.transform.forward) < 0)
                    {
                        var distance = Mathf.Sqrt(sqrDist);
                        if (!LocalPlayer.IsSpawned || !Physics.Raycast(player.transform.position,  vec, out RaycastHit _, distance, LayerMask.GetMask("WorldStatic")))
                        {
                            player.screenPosition = screenPoint;
                            player.localDistanceMeters = distance;
                            isNotBehindObject = true;
                        }
                    }
                }
                
                player.isInScreen = isCloseEnough && isInScreen && isNotBehindObject;
                yield return wait;
            }
            yield return wait;
        }
    }

    IEnumerator ConnectSocket()
    {
        while (true)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                break;
            }
#endif

            if (connectionState == E_ConnectionState.OK)
            {
                yield return new WaitForSeconds(4f);
                continue;
            }
            else
            {
                if (IsConnected)
                {
                    DestroyAllPlayers();
                }

                IsConnected = false;
            }

            try
            {
                websocket?.CancelConnection();
                var _ = websocket?.Close();
            }
            catch { }
                
            InitWebSock();
            Debug.Log($"Waiting for connection");
            yield return new WaitForSeconds(4f);
        }
    }

    async void InitWebSock()
    {
        connectionState = E_ConnectionState.CONNECTING;

        var addr = "ws://rx.louve.systems:1235";
        //addr = "ws://localhost:1235";

        addr = "wss://pocketstrikers.louve.systems";

        //#if DEBUG
        //#endif

        websocket = new WebSocket(addr);
        websocket.OnOpen += () =>
        {
            Debug.Log($"Connected!!");
            IsConnected = true;
            connectionState = E_ConnectionState.OK;
        };

        websocket.OnError += (e) =>
        {
            connectionState = E_ConnectionState.ERROR;
            Debug.Log($"Error! {addr} {e}");
        };

        websocket.OnClose += (e) =>
        {
            connectionState = E_ConnectionState.ERROR;
            Debug.Log("Connection closed! " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                string controller = message.Substring(0, 3);
                message = message.Substring(3);

                if (controllers.ContainsKey(controller))
                {
                    Debug.Log($"Executing {controller}");
                    controllers[controller].Invoke(websocket, message);
                    Debug.Log($"Done!");
                }
                else
                {
                    Debug.LogWarning("Received junk controller " + controller + " with message " + message);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(System.Text.Encoding.UTF8.GetString(bytes));
            }


            // Debug.Log("OnMessage! " + message);
        };

        await websocket.Connect();
    }

    public Player AddPlayer(int id, Vector3 position, Quaternion rotation, bool isLocal = false)
    {
        var player = Object.Instantiate(playerPrefab).GetComponent<Player>();
        player.id = id;
        player.transform.position = position;
        player.transform.rotation = rotation;
        player.IsLocal = isLocal;

        if (player.IsLocal)
        {
            LocalPlayer = player;
        }

        Players.Add(player);

        return player;
    }

    public void DestroyAllPlayers()
    {
        Debug.Log("Destroying all players");
        foreach (var player in Players)
        {
            Destroy(player.gameObject);
        }

        Players.Clear();
    }

    public Player GetPlayerById(int id)
    {
        return Players.Find(o => o.id == id);
    }

    public void SendMyPosition(Vector3 position, Quaternion rotation, PlayerMovement infos)
    {
        websocket.SendText(NetControllers.PROTOCOL_MOVESTATE + Newtonsoft.Json.JsonConvert.SerializeObject(new NetControllers.PlayerMove()
        {
            id = LocalPlayer.id,
            isBoosting = infos.IsBoosting,
            position = new NetControllers.Position()
            {
                x = position.x,
                y = position.y,
                z = position.z
            },
            rotation = new float[] { rotation.x, rotation.y, rotation.z, rotation.w }
        }));
    }

    public void RequestSpawn()
    {
        websocket.SendText(NetControllers.PROTOCOL_REQUEST_SPAWN);
    }

    public void SendShootState(MissileRequest missileInfo)
    {
        websocket.SendText(NetControllers.PROTOCOL_SHOOTSTATE+Newtonsoft.Json.JsonConvert.SerializeObject(missileInfo));
    }

    public void SendLoadout(Weapon.ELoadout loadout)
    {
        websocket.SendText(NetControllers.PROTOCOL_SET_LOADOUT+(int)loadout);
    }


    public void SendMeow()
    {
        websocket.SendText(NetControllers.PROTOCOL_MEOW);
    }

    public void SetScore(int clientId, int score)
    {
        if (GetPlayerById(clientId) != null)
        {
            scores[clientId] = score;
        }
    }

    public int GetScore(int clientId)
    {
        return scores.ContainsKey(clientId) ? scores[clientId] : 0;
    }

    public string GetNameForId(int clientId)
    {
        ////Debug.Log("Getting name for client ID " + clientId + " among " + names.Count + " possibilities");
        return names[clientId % names.Count];
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif

        // 86400 seconds in a day
        var time = Time;

        waterPlaneMat.SetFloat("_Brightness", 2f * time + 0.5f);

        skyMat.SetColor("_Top", topGradient.Evaluate(time));
        skyMat.SetColor("_Middle", botGradient.Evaluate(time));

        levelMat.SetColor("_Color", botGradient.Evaluate(time));

        if (Input.touchCount > 0)
        {
            IsPressing = true;
            MousePosition = Vector2.zero;
            for (int i = 0; i < Input.touchCount; i++)
            {
                MousePosition += Input.touches[i].position;
            }

            MousePosition /= Input.touchCount;

            IsFiring = Input.touchCount > 4;
        }
        else
        {
            IsFiring = Input.GetMouseButton(1);
            IsPressing = Input.GetMouseButton(0);
            MousePosition = Input.mousePosition;
        }
    }

    public void EliminateMyself(Player killer)
    {
        Debug.Log($"Local player was eliminated by " + killer.name + "!");
        LocalPlayer.lastLocalKiller = killer.id;
        LocalPlayer.Eliminate();
        websocket.SendText(NetControllers.PROTOCOL_ELIMINATE_SELF+killer.id);
    }

    private void OnApplicationQuit()
    {
        var _ = websocket.Close();
    }

    public void DisconnectPlayer(int id)
    {
        try
        {
            Debug.Log("Finding player");
            var player = Players.Find(o => o.id == id);
            if (player)
            {
                Debug.Log("Found player!");
                Players.RemoveAll(o => o.id == id);
                Debug.Log("Removed player!");
                Destroy(player.gameObject);
                Debug.Log("Destroyed player!");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
