using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class NetControllers : Dictionary<string, Action<NativeWebSocket.WebSocket, string>> {

    public const string PROTOCOL_MOVESTATE = "MOV";
    public const string PROTOCOL_UPDATE_SCORE = "SCO";
    public const string PROTOCOL_STATE = "STT";
    public const string PROTOCOL_ACKNOWLEDGE_STATE = "AKS";
    public const string PROTOCOL_KILL_PLAYER = "KIL";
    public const string PROTOCOL_SHOOTSTATE = "SHT";
    public const string PROTOCOL_SET_LOADOUT = "LDT";
    public const string PROTOCOL_MEOW = "MEW";

    public NetControllers() {
        Add(PROTOCOL_MOVESTATE, MovePlayer);
        Add(PROTOCOL_UPDATE_SCORE, UpdateScores);
        Add(PROTOCOL_ACKNOWLEDGE_STATE, null);
        Add(PROTOCOL_STATE, InitializeState);
        Add(PROTOCOL_KILL_PLAYER, KillPlayer);
        Add(PROTOCOL_MEOW, MakeMeow);
    }

    void MovePlayer(NativeWebSocket.WebSocket ws, string data) {
        var move = JsonConvert.DeserializeObject<PlayerMove>(data);
        var player = Game.i.GetPlayerById(move.id);
        var reDeser = new DeserializedPlayerMove(move);

        if (player == null) {
            Debug.Log("Spawning new unknown player " + move.id);
            player = Game.i.SpawnPlayer(move.id, reDeser.position, reDeser.rotation, isLocal: false);
        }

        if (player.IsLocal) {
            Debug.LogWarning("Received position for local player, should not happen!!");
            return;
        }

        player.UpdatePosition(reDeser);
    }

    void KillPlayer(NativeWebSocket.WebSocket ws, string data) {
        var id = Convert.ToInt32(data);

        Debug.Log("Killing player " + id);

        Game.i.KillPlayer(id);
    }

    void MakeMeow(NativeWebSocket.WebSocket ws, string data) {
        var id = Convert.ToInt32(data);

        Game.i.GetPlayerById(id)?.Meow();
    }

    void UpdateScores(NativeWebSocket.WebSocket ws, string data) {
        var state = JsonConvert.DeserializeObject<ScoreInfo>(data);
        Game.i.SetScore(state.clientId, state.newScore);
    }

    void InitializeState(NativeWebSocket.WebSocket ws, string data) {
        var state = JsonConvert.DeserializeObject<GameState>(data);

        Game.i.DestroyAllPlayers();

        foreach (var client in state.clients) {
            Game.i.SpawnPlayer(
                client.id,
                new Vector3(client.position.x, client.position.y, client.position.z),
                new Quaternion(client.rotation[0], client.rotation[1], client.rotation[2], client.rotation[3]),
                isLocal: client.isYou
            );
            
        }

        foreach (var score in state.scores) {
            Game.i.SetScore(Convert.ToInt32(score.Key), Convert.ToInt32(score.Value));
        }

        Debug.Log("Sending ACK");
        ws.SendText(PROTOCOL_ACKNOWLEDGE_STATE);
    }

    [Serializable]
    public class ScoreInfo {
        public int newScore = 1;
        public int clientId = -1;
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

        public DeserializedPlayerMove(PlayerMove move) {
            position = new Vector3(move.position.x, move.position.y, move.position.z);
            rotation = new Quaternion(move.rotation[0], move.rotation[1], move.rotation[2], move.rotation[3]);
            isBoosting = move.isBoosting;
        }
    }

    [Serializable]
    public class Client {
        public int id;
        public Position position;
        public float[] rotation = new float[4];
        public bool isYou = false;
        public int color = 0;
        public Weapon.ELoadout loadout;
    }

    [Serializable]
    public class Position {
        public float x = 0;
        public float y = 1;
        public float z = 0;
    }
}
