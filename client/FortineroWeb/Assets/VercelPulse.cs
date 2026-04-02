
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class VercelPulse : MonoBehaviour {
    private string apiUpdateUrl = "https://fortinero-vercel.vercel.app/api/session";
    private GameState playerGameState;

    void Start() {
        // Pulse every 2 seconds to stay under Vercel/Upstash free limits
        playerGameState = new GameState();
        InvokeRepeating("UpdatePosition", 0, 2.0f);
    }

    void UpdatePosition() {
        StartCoroutine(SendPulse());
    }

    IEnumerator SendPulse() {
        PlayerData myData = new PlayerData {
            playerId = "Player_Gaucho_1", // You can generate this on login
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            gameState = playerGameState,
            lastSeen = (long)Time.time
        };

        string json = JsonUtility.ToJson(myData);
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(apiUpdateUrl, json)) {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) {
                // Parse the nearbyPlayers JSON here to spawn/move other players
                Debug.Log("Sync Success: " + www.downloadHandler.text);
            }
        }
    }
}
