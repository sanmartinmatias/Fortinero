using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json; // <-- Now you can use this!

public class WorldSyncManager : MonoBehaviour {
    [Header("Settings")]
    public string apiUrl = "https://fortinero-vercel.vercel.app/api/session";
    public GameObject playerPrefab;
    public const float SYNC_INTERVAL = 1.0f;

    private string localPlayerId;
    private Dictionary<string, GameObject> ghostPlayers = new Dictionary<string, GameObject>();
    private Dictionary<string, PlayerData> _lastPlayers;

    void Start() {
        // Simple unique ID for this session
        localPlayerId = "Gaucho_" + Random.Range(1000, 9999);
        StartCoroutine(SyncLoop());
    }

// Inside WorldSyncManager.cs
    void Update() {
        // foreach (var entry in ghostPlayers) {
        //     string id = entry.Key;
        //     GameObject ghost = entry.Value;
            
        //     // Get the latest data we received from the API for this ID
        //     PlayerData latestData = _lastPlayers[id]; 
        //     Vector3 targetPos = new Vector3(latestData.x, latestData.y, latestData.z);

        //     var controller = ghostPlayers[id].GetComponent<GhostController>();
        //     controller.UpdateFromNetwork(latestData);
        // }
}
    IEnumerator SyncLoop() {
        while (true) {
            yield return StartCoroutine(FetchWorldState());
            yield return new WaitForSeconds(SYNC_INTERVAL);
        }
    }

    IEnumerator FetchWorldState() {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl)) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                string rawJson = request.downloadHandler.text;
                
                // Newtonsoft handles the Dictionary perfectly
                var response = JsonConvert.DeserializeObject<SessionResponse>(rawJson);

                if (response?.activePlayers != null) {
                   UpdateWorld(response.activePlayers);
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
        } else {
            
            GameObject ghost = ghostPlayers[id];
           var controller = ghost.GetComponent<GhostController>();
            controller.UpdateFromNetwork(entry.Value);
        }
    }
}

IEnumerator MoveGhost(GameObject ghost, Vector3 target, float duration) {
    float elapsed = 0;
    Vector3 startPos = ghost.transform.position;
    Animator anim = ghost.GetComponent<Animator>();

    // 1. Calculate Speed based on distance
    float distance = Vector3.Distance(startPos, target);
    // If they moved significantly, set Speed to 6 (Running), else 0
    float targetSpeed = distance > 0.1f ? 6.0f : 0.0f;

    while (elapsed < duration) {
        // 2. Smoothly Lerp the position
        ghost.transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
        
        // 3. Update the Animator Speed
        // Use Lerp here too so the animation doesn't "snap"
        float currentSpeed = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f));

        // 4. Rotate to face the target
        if (distance > 0.1f) {
            Vector3 direction = (target - ghost.transform.position).normalized;
            if (direction != Vector3.zero) {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                ghost.transform.rotation = Quaternion.Slerp(ghost.transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }

        elapsed += Time.deltaTime;
        yield return null;
    }
    
    // Ensure they stop at the end
    anim.SetFloat("Speed", 0);
}
}

