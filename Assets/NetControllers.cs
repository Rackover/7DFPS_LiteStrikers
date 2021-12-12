using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class NetControllers : Dictionary<string, Action<NativeWebSocket.WebSocket, string>> {

    public const string PROTOCOL_MOVESTATE = "MOV";
    public const string PROTOCOL_UPDATE_PLAYER = "UPP";
    public const string PROTOCOL_STATE = "STT";
    public const string PROTOCOL_ACKNOWLEDGE_STATE = "AKS";
    public const string PROTOCOL_DISCONNECT_PLAYER = "DIS";
    public const string PROTOCOL_SHOOTSTATE = "SHT";
    public const string PROTOCOL_SET_LOADOUT = "LDT";
    public const string PROTOCOL_MEOW = "MEW";
    public const string PROTOCOL_MISSILE_BIRTH = "MBI";
    public const string PROTOCOL_MISSILE_MOVESTATE = "MMV";
    public const string PROTOCOL_ELIMINATE_SELF = "ELS";
    public const string PROTOCOL_UPDATE_SCORE = "SCO";
    public const string PROTOCOL_REQUEST_SPAWN = "RSP";

    public NetControllers() {
        Add(PROTOCOL_MOVESTATE, MovePlayer);
        Add(PROTOCOL_UPDATE_PLAYER, ReceivePlayerUpdate);
        Add(PROTOCOL_UPDATE_SCORE, UpdateScores);
        Add(PROTOCOL_ACKNOWLEDGE_STATE, null);
        Add(PROTOCOL_STATE, InitializeState);
        Add(PROTOCOL_DISCONNECT_PLAYER, DisconnectPlayer);
        Add(PROTOCOL_MEOW, MakeMeow);
        Add(PROTOCOL_MISSILE_BIRTH, BirthMissile);
    }

    void ReceivePlayerUpdate(NativeWebSocket.WebSocket ws, string data)
    {
        var playerUpdate = JsonConvert.DeserializeObject<Client>(data);
        ReceivePlayerUpdate(playerUpdate);
    }

    void ReceivePlayerUpdate(Client client)
    {
        var player = Game.i.GetPlayerById(client.id);
        var move = new DeserializedPlayerMove(client.position, client.rotation);
        if (player == null)
        {
            Debug.Log("Adding new unknown player " + client.id);
            player = Game.i.AddPlayer(client.id, move.position, move.rotation, isLocal: false);
        }

        player.SetLoadout(client.loadout);
        // SetColor

        if (!player.IsSpawned && client.isSpawned)
        {
            Debug.Log("Spawning player " + player.id + " at " + move.position);
            player.transform.position = move.position;
            player.transform.LookAt(Vector3.zero);
            player.Spawn();
        }
        else if (player.IsSpawned && !client.isSpawned)
        {
            player.Eliminate();
        }
    }

    void BirthMissile(NativeWebSocket.WebSocket ws, string data)
    {
        var missileBirth = JsonConvert.DeserializeObject<Missile>(data);
        Game.i.SpawnMissile(missileBirth);
    }
    
    
    void MovePlayer(NativeWebSocket.WebSocket ws, string data) {
        var move = JsonConvert.DeserializeObject<PlayerMove>(data);
        var player = Game.i.GetPlayerById(move.id);
        var reDeser = new DeserializedPlayerMove(move);

        if (player == null) {
            Debug.Log($"Move state for unknown player {move.id}! Adding & spawning...");
            player = Game.i.AddPlayer(move.id, reDeser.position, reDeser.rotation, isLocal: false);
            player.Spawn();
        }

        if (player.IsLocal) {
            Debug.LogWarning("Received position for local player, should not happen!!");
            return;
        }

        player.UpdatePosition(reDeser);
    }

    void DisconnectPlayer(NativeWebSocket.WebSocket ws, string data) {
        var id = Convert.ToInt32(data);

        Debug.Log("Killing player " + id);

        Game.i.DisconnectPlayer(id);
    }

    void MakeMeow(NativeWebSocket.WebSocket ws, string data) {
        var id = Convert.ToInt32(data);

        Game.i.GetPlayerById(id)?.Meow();
    }

    void UpdateScores(NativeWebSocket.WebSocket ws, string data) {
        var state = JsonConvert.DeserializeObject<ScoreInfo>(data);

        bool localIsKiller = false;

        if (state.scores != null)
        {
            foreach (var k in state.scores.Keys)
            {
                if (Game.i.LocalPlayer && k == Game.i.LocalPlayer.id)
                {
                    var oldScore = Game.i.GetScore(k);

                    if (state.scores[k] > oldScore)
                    {
                        // I am a killer! Looking forward to player updates
                        localIsKiller = true;
                    }
                }

                Game.i.SetScore(k, state.scores[k]);
            }
        }

        if (state.playerUpdates != null)
        {
            for (int i = 0; i < state.playerUpdates.Length; i++)
            {
                var pu = state.playerUpdates[i];
                
                if (localIsKiller)
                {
                    if (pu.isSpawned == false && Game.i.GetPlayerById(pu.id)?.IsSpawned == true)
                    {
                        if (pu.id == Game.i.LocalPlayer.id)
                        {
                            // 😒
                        }
                        else
                        {
                            // I killed them!
                            Game.i.LocalPlayer.AcknowledgeKill(pu.id);
                        }
                    }
                }

                ReceivePlayerUpdate(pu);
            }
        }
    }

    void InitializeState(NativeWebSocket.WebSocket ws, string data) {
        var state = JsonConvert.DeserializeObject<GameState>(data);

        Game.i.DestroyAllPlayers();

        foreach (var client in state.clients) {
            var player = Game.i.AddPlayer(
                client.id,
                new Vector3(client.position.x, client.position.y, client.position.z),
                new Quaternion(client.rotation[0], client.rotation[1], client.rotation[2], client.rotation[3]),
                isLocal: client.isYou
            );

            if (client.isSpawned)
            {
                Debug.Log("Spawning player " + client.id);
                player.Spawn();
            }

        }

        foreach (var score in state.scores) {
            Game.i.SetScore(Convert.ToInt32(score.Key), Convert.ToInt32(score.Value));
        }

        Debug.Log("Sending ACK");
        ws.SendText(PROTOCOL_ACKNOWLEDGE_STATE);
    }

    [Serializable]
    public struct ScoreInfo {
        public Dictionary<int, int> scores;
        public Client[] playerUpdates;
    }

    [Serializable]
    public class GameState {
        public List<Client> clients;
        public Dictionary<string, string> scores = new Dictionary<string, string>();
        public string map;
    }


    [Serializable]
    public class PlayerMove {
        public int id;
        public Position position;
        public float[] rotation = new float[4];
        public bool isBoosting = false;
    }

    public class DeserializedPlayerMove {
        public Vector3 position;
        public Quaternion rotation;
        public bool isBoosting = false;

        public DeserializedPlayerMove() {

        }

        public DeserializedPlayerMove(Position position, float[] rotation)
        {
            this.position = new Vector3(position.x, position.y, position.z);
            this.rotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
        }

        public DeserializedPlayerMove(PlayerMove move) : this(move.position, move.rotation) {
            isBoosting = move.isBoosting;
        }
    }

    [Serializable]
    public class Client {
        public int id;
        public Position position;
        public float[] rotation = new float[4];
        public bool isYou = false;
        public bool isSpawned = false;
        public int color = 0;
        public Weapon.ELoadout loadout;
    }

    [Serializable]
    public struct Position {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public struct Missile
    {
        public int owner;
        public Weapon.ELoadout type;
        public int target;
        public Position position;
        public float[] initialRotation;
        public int id;
        public float lifetime;
    }
}
