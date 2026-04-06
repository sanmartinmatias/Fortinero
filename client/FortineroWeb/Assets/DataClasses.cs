using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData {

    public const string JUMP_EVENT = "Jump";
    public const string FALL_EVENT = "Fall";
    public const string POSITION_EVENT  = "Position";
    public const string ROTATION_EVENT = "Rotation";
    public const string  GROUNDED_EVENT = "Grounded";
    public Vector3 Position => new (x, y, z);
    public DateTime LastSeen => new DateTime(lastSeen);



    public float x;
    public float y;
    public long lastSeen;
    public GameState gameState;
    public string playerId;
    public List<PlayerAction> recentActions;
    public float z;

    
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return playerId == ((PlayerData)obj).playerId && Position == ((PlayerData)obj).Position && lastSeen == ((PlayerData)obj).lastSeen;
    }
    
    public override int GetHashCode()
    {
        return playerId.GetHashCode() + Position.GetHashCode() + lastSeen.GetHashCode();
    }
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
    public string type;
    public ushort timestamp;
    public ushort ry; 
    public ushort s;  
    public ushort x;
    public ushort y;
    public ushort z;

    public PlayerAction() {}

    public PlayerAction(Vector3 position, float rotation, float timestamp, string type)
    {
        this.type = type;
        this.timestamp = Mathf.FloatToHalf(timestamp);
        this.ry = Mathf.FloatToHalf(rotation);
        this.x = Mathf.FloatToHalf(position.x);
        this.y = Mathf.FloatToHalf(position.y);
        this.z = Mathf.FloatToHalf(position.z);
    }

    public float RotationY => Mathf.HalfToFloat(ry);
    public Vector3 Position => new (Mathf.HalfToFloat(x), Mathf.HalfToFloat( y),Mathf.HalfToFloat(z));
    public float Timestamp => Mathf.HalfToFloat(timestamp);
}

// Inside PlayerData
