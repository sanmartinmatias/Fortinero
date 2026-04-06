using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json; // <-- Now you can use this!

public class WorldSyncManager : MonoBehaviour {
    [Header("Settings")]
    public string apiUrl = "https://fortinero-vercel.vercel.app/api/session";
    public GameObject playerPrefab;
    public const float SYNC_INTERVAL = 5.0f;

    private string localPlayerId;
    private Dictionary<string, GameObject> ghostPlayers = new Dictionary<string, GameObject>();
    private Dictionary<string, PlayerData> _lastPlayers;
    void Start() {
        // Simple unique ID for this session
        localPlayerId = "Gaucho_" + Random.Range(1000, 9999);
        StartCoroutine(SyncLoop());
    }
    private const float FAST_RETRY_INTERVAL = 0.1f; // 100ms "Peek"

    IEnumerator SyncLoop() {
        while (true) {
            yield return StartCoroutine(FetchWorldState());
            
            yield return new WaitForSeconds(FAST_RETRY_INTERVAL);
        }
    }

    IEnumerator FetchWorldState() {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl)) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                //Debug.Log("Sync Success: " + request.downloadHandler.text);
                var response = JsonConvert.DeserializeObject<SessionResponse>(request.downloadHandler.text);
                
                if (response?.activePlayers != null) {
                    UpdateWorld(response.activePlayers);
                    yield break;
                }
            }
            
        }
    }

   void UpdateWorld(Dictionary<string, PlayerData> players) {
        foreach (var entry in players) {
            _lastPlayers = players;
            string id = entry.Key;
            if (id == localPlayerId) continue; // Fix for the "Mirror Ghost"

            if (!ghostPlayers.ContainsKey(id)) {
                GameObject g = Instantiate(playerPrefab, new Vector3(entry.Value.x, 0, entry.Value.y), Quaternion.identity);
                ghostPlayers.Add(id, g);
            } 
                
            GameObject ghost = ghostPlayers[id];
            var controller = ghost.GetComponent<GhostController>();
            controller.UpdateFromNetwork(entry.Value);
            
        }
    }
}

