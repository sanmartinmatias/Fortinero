using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class GhostController : MonoBehaviour
{
    private Animator _animator;
    private int _animIDSpeed, _animIDGrounded, _animIDJump, _animIDFreeFall, _animIDMotionSpeed;

    public float SyncWindow => WorldSyncManager.SYNC_INTERVAL; 

    private float _lerpTime;
    private bool _hasStarted = false;

    [Header("Local Physics")]
    public float Gravity = -15.0f;
    private float _verticalVelocity;
    private Vector3 _currentPhysicsPos;

    [Header("Pacing Settings")]
    public float SpeedDamping = 5f;
    public float StopThreshold = 0.05f;
    private float _currentAnimSpeed;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        
    }
    private List<Vector3> _pathPoints = new List<Vector3>();
    private PlayerData _oldData = new PlayerData();
    public void UpdateFromNetwork(PlayerData data)
    {
        if (data.recentActions == null || data.recentActions.Count == 0 || data.Equals(_oldData)) return;
        _oldData = data;

        TaskRunner.Instance.Cancel(data.playerId);
        float packetStartTime = data.recentActions[0].timestamp;

        for (int i = 0; i < data.recentActions.Count; i++)
        {
            PlayerAction action = data.recentActions[i];
            PlayerAction prevAction = data.recentActions[Mathf.Max(0, i - 1)];

            float relativeDelay = action.timestamp - packetStartTime;
            float prevDelay = prevAction.timestamp - packetStartTime;
            float safeDelay = Mathf.Max(0, relativeDelay);

            //Move and rotation, avery action counts.
            TaskRunner.Instance.Delay(prevDelay, () =>
            {   
                TaskRunner.Instance.Cancel($"Move:{data.playerId}");
                TaskRunner.Instance.Move(transform,action.Position,Quaternion.Euler(0,-action.RotationY,0), relativeDelay-prevDelay, $"Move:{data.playerId}");
                
            },data.playerId);

            TaskRunner.Instance.Delay(relativeDelay, () =>
            {
                UpdateMoveAnimation(action);
                switch (action.type)
                {   
                    case PlayerData.JUMP_EVENT: JumpEvent(); break;
                    case PlayerData.FALL_EVENT: FallEvent(); break;
                    case PlayerData.GROUNDED_EVENT: GroundedEvent(); break;
                }
            }, data.playerId);
        }
    }

    private Vector3 _lastActionPos;
    private float _lastActionTime;

   private float _currentAnimBlend; // Persistent variable to store the lerped value
    private const float SpeedChangeRate = 10.0f; // Matches the Move() logic rate

private void UpdateMoveAnimation(PlayerAction action)
{
    float timeDelta = action.timestamp - _lastActionTime;
    if (timeDelta <= 0) return;

    float distance = Vector3.Distance(_lastActionPos, action.Position);
    float physicalSpeed = distance / timeDelta;

    _currentAnimBlend = Mathf.Lerp(_currentAnimBlend, physicalSpeed, timeDelta * SpeedChangeRate);

    if (_currentAnimBlend < 0.01f) _currentAnimBlend = 0f;
    float inferredInputMagnitude = physicalSpeed > 0.1f ? 1f : 0f;
    _lastActionPos = action.Position;
    _lastActionTime = action.timestamp;
}

    private void GroundedEvent()
    {
        _animator.SetBool(_animIDGrounded,true);
        _animator.SetBool(_animIDJump, false);
        _animator.SetBool(_animIDFreeFall, false);
    }


    private void JumpEvent()
    {
        _animator.SetBool(_animIDGrounded,false);
        _animator.SetBool(_animIDJump,true);
    }

    private void FallEvent()
    {
        _animator.SetBool(_animIDGrounded,false);
        _animator.SetBool(_animIDFreeFall,true);
    }


    public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(transform.position), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(transform.position), FootstepAudioVolume);
        }
    }
}