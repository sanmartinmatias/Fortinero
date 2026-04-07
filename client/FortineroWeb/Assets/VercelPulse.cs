
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;

public class VercelPulse : MonoBehaviour {
    private string apiUpdateUrl = "https://fortinero-vercel.vercel.app/api/session";
    private GameState playerGameState;
    [SerializeField] private ThirdPersonController _playerController;
    public const float STEPS_PER_SYNC = 5;
    
    private List<PlayerAction> _pendingActions = new List<PlayerAction>();

    void OnEnable() {
        // Subscribe once. The lambda captures the current state when the event fires.
        EventBus.Subscribe(PlayerData.JUMP_EVENT, () => RecordAction(PlayerData.JUMP_EVENT));
        EventBus.Subscribe(PlayerData.FALL_EVENT, () => RecordAction(PlayerData.FALL_EVENT));
        EventBus.Subscribe(PlayerData.GROUNDED_EVENT, () => RecordAction(PlayerData.GROUNDED_EVENT));
        EventBus.Subscribe(PlayerData.ROTATION_EVENT, () => RecordAction(PlayerData.ROTATION_EVENT));
        EventBus.Subscribe(PlayerData.SHOOT_EVENT, () => RecordAction(PlayerData.SHOOT_EVENT));
    }

    void OnDisable()
    {
        
    }
    PlayerAction _lastRecordedAction = null;

    void RecordAction(string type) {
        
        if (_lastRecordedAction != null && type == _lastRecordedAction.type &&  Time.time - _lastRecordedAction.Timestamp < 1f/STEPS_PER_SYNC)
            return;
        PlayerAction item = new(transform.position, _playerController.transform.eulerAngles.y, Time.time, type);
        _lastRecordedAction = item;
        _pendingActions.Add(item);
    }

    void Start() {
        playerGameState = new GameState();
        StartCoroutine(UpdatePosition());
    }



IEnumerator UpdatePosition() {
    while (true) {
            PlayerData myData = new PlayerData {
                playerId = "Player_1",
                x = transform.position.x,
                y = transform.position.y,
                z = transform.position.z,
                recentActions = new List<PlayerAction>(_pendingActions) 
            };

            _pendingActions.Clear();
            
            yield return StartCoroutine(SendPulse(myData));
            yield return new WaitForSeconds(WorldSyncManager.SYNC_INTERVAL);
        }
    }

    IEnumerator SendPulse(PlayerData myData) {
       
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
