﻿using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game i { private set; get; }

    public List<Player> Players { get; set; } = new List<Player>();
    public Player LocalPlayer { get; private set; }

    public Vector2 MousePosition;

    public bool IsPressing { get; private set; }

    public bool IsFiring { get; private set; }

    public bool IsMobile { get; private set; }

    [SerializeField] private bool forceMobile;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bowlPrefab;
    [SerializeField] private GameObject mobileCamera;
    [SerializeField] private float visibilityDistance = 400f;

    WebSocket websocket;
    NetControllers controllers = new NetControllers();
    E_ConnectionState connectionState;
    Dictionary<int, int> scores = new Dictionary<int, int>();

    enum E_ConnectionState { CONNECTING, ERROR, OK };

    List<string> names = new List<string>();

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

        names.Add("Maverick");
        names.Add("Goose");
        names.Add("Viper");
        names.Add("Iceman");
        names.Add("Hollywood");
        names.Add("Charlie");
        names.Add("Jester");
        names.Add("Stinger");
        names.Add("Wolfman");
        names.Add("Merlin");
        names.Add("Slider");
        names.Add("Chipper");
        names.Add("Sundown");
        names.Add("Sark");
        names.Add("Clu");
        names.Add("Yori");
        names.Add("Crom");
        names.Add("Ram");
        names.Add("Chip");
        names.Add("Thorne");
        names.Add("Rinzler");
        names.Add("Tesler");
        names.Add("Link");
        names.Add("Pavel");
        names.Add("Zero");
        names.Add("Hurricane");
        names.Add("Typhoon");
        names.Add("Tornado");
        names.Add("Mirage");
        names.Add("Castor");
        names.Add("Roc");
        names.Add("Louve");
        names.Add("Striker");
        names.Add("Lancaster");
        names.Add("Kanoziev");
        names.Add("Maddox");


        StartCoroutine(ConnectSocket());

        IsMobile = CheckIfMobile();
        //mobileCamera.SetActive(IsMobile);
    }

    private void Start()
    {
        StartCoroutine(UpdatePlayersVisibility());
    }

    IEnumerator UpdatePlayersVisibility()
    {
        var wait = new WaitForEndOfFrame();
        var sqrMaxDist = visibilityDistance * visibilityDistance;
        while (true)
        {

            for(var i = 0; i < Players.Count; i++)
            {
                if (Players[i] == null) continue;

                var player = Players[i];

                if (player.IsLocal) continue;

                var vec = LocalPlayer.transform.position- player.transform.position;
                var sqrDist = Vector3.SqrMagnitude(vec);
                bool isCloseEnough = sqrDist < sqrMaxDist;
                bool isInScreen = false;
                bool isNotBehindObject = false;

                if (isCloseEnough)
                {
                    var screenPoint = Camera.main.WorldToScreenPoint(player.transform.position);

                    isInScreen = screenPoint.x > 0 && screenPoint.y > 0 && screenPoint.x < Screen.width && screenPoint.y < Screen.height;

                    if (isInScreen && Vector3.Dot(vec, Camera.main.transform.forward) < 0)
                    {
                        var distance = Mathf.Sqrt(sqrDist);
                        if (!Physics.Raycast(player.transform.position,  vec, out RaycastHit info, distance, LayerMask.GetMask("WorldStatic")))
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

            InitWebSock();
            Debug.Log($"Waiting for connection");
            yield return new WaitForSeconds(4f);
        }
    }

    async void InitWebSock()
    {
        connectionState = E_ConnectionState.CONNECTING;

        var addr = "ws://rx.louve.systems:1235";

        //#if DEBUG
        //        addr = "wss://microstrikers.louve.systems";
        //#endif

        websocket = new WebSocket(addr);
        websocket.OnOpen += () =>
        {
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

                ////Debug.Log(controller + " => " + message);

                if (controllers.ContainsKey(controller))
                {
                    controllers[controller].Invoke(websocket, message);
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

    public void SpawnPlayer(int id)
    {
        // todo !
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


    public void SendMeow()
    {
        websocket.SendText(NetControllers.PROTOCOL_MEOW);
    }

    public void SetScore(int clientId, int score)
    {
        if (GetPlayerById(clientId) != null)
        {
            if (!scores.ContainsKey(clientId) || scores[clientId] < score)
            {
            }

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

    private void OnApplicationQuit()
    {
        var _ = websocket.Close();
    }

    public void DisconnectPlayer(int id)
    {
        var player = Players.Find(o => o.id == id);
        if (player)
        {
            Destroy(player.gameObject);
        }
    }
}
