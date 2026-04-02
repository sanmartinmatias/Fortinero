using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    private Animator _animator;
    private int _animIDSpeed, _animIDGrounded, _animIDJump, _animIDFreeFall, _animIDMotionSpeed;

    [Header("Sync Settings")]
    public float TeleportThreshold = 10f; // Distance before we snap instantly
    public float SyncWindow => WorldSyncManager.SYNC_INTERVAL;       // Matches your API poll rate (2s)
    
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _lerpTime;
    private float _animationBlend;
    private Queue<PlayerAction> _actionQueue = new Queue<PlayerAction>();

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        
        _targetPos = transform.position;
    }

    public void UpdateFromNetwork(PlayerData data)
    {
        _startPos = transform.position;
        _targetPos = new Vector3(data.x, data.y, data.z);
        
        // 1. Teleport Check: If they moved too far, don't walk, just snap.
        if (Vector3.Distance(_startPos, _targetPos) > TeleportThreshold)
        {
            transform.position = _targetPos;
            _startPos = _targetPos;
        }

        // 2. Reset the Lerp clock
        // This ensures the ghost reaches _targetPos in exactly SyncWindow seconds
        _lerpTime = 0f;

        // 3. Queue new actions
        if (data.recentActions != null)
        {
            foreach (var action in data.recentActions) _actionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        _lerpTime += Time.deltaTime;
        float t = _lerpTime / SyncWindow; // Normalized time (0 to 1)

        // 1. Precise Position Sync
        // t = 1.0 means we are exactly at the target at the 2-second mark
        if (t <= 1.0f)
        {
            transform.position = Vector3.Lerp(_startPos, _targetPos, t);
        }
        else
        {
            // If we go past 1.0, we are waiting for the next pulse. 
            // Stay at target to avoid drifting.
            transform.position = _targetPos;
        }

        // 2. Rotation & Animations
        Vector3 direction = (_targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(direction.x,0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
        }


        float distanceRemaining = Vector3.Distance(transform.position, _targetPos);
        if (distanceRemaining > 0.1f)
        {
           
            
            _animationBlend = Mathf.Lerp(_animationBlend, 5.335f, Time.deltaTime * 10f);
        }
        else
        {
            _animationBlend = Mathf.Lerp(_animationBlend, 0f, Time.deltaTime * 10f);
        }

        // 3. Action Replay (Triggering based on position/progress)
        if (_actionQueue.Count > 0)
        {
            // We check if we've passed the timestamp or reached the spatial trigger
            var action = _actionQueue.Peek();
            if (t >= 0.5f || _actionQueue.Count > 3) // Example logic: trigger halfway through pulse
            {
                HandleAction(_actionQueue.Dequeue());
            }
        }

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, t < 1.0f ? 1f : 0f);
        _animator.SetBool(_animIDGrounded, true);
    }

    private void HandleAction(PlayerAction action)
    {
        if (action.type.ToLower() == "jump") _animator.SetBool(_animIDJump, true);
    }

    public void OnFootstep(AnimationEvent animationEvent) { }
    public void OnLand(AnimationEvent animationEvent) { }
}