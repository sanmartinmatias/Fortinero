using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData {
    public float x;
    public float y;
    public long lastSeen;
    public GameState gameState;
    public string playerId;
    public List<PlayerAction> recentActions;
    public float z;
}

[Serializable]
public class GameState {
    public int health;
}

[Serializable]
public class SessionResponse {
    public string status;
    public Dictionary<string, PlayerData> activePlayers;
}

[Serializable]
public class PlayerAction {
    public string type; // "run", "idle", "attack"
    public float timestamp;
    public float x;
    public float y;
    public float z;
}

// Inside PlayerData
