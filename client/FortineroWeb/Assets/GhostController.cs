using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    [Header("Pacing Settings")]
    [SerializeField] private float _speedChangeRate = 10.0f;
    [SerializeField] private float _rotationSpeed = 15.0f;
    [SerializeField] private float _interpolationDelay = 0.1f;

    private Animator _animator;
    private int _animIDSpeed, _animIDGrounded, _animIDJump, _animIDFreeFall, _animIDMotionSpeed;

    private Queue<PlayerAction> _actionBuffer = new Queue<PlayerAction>();
    private float _playbackClock; 
    private bool _isPlaybackStarted = false;
    private PlayerAction _targetAction;
     private PlayerAction _previousAction;
    private float _lastBufferedTimestamp;
    private float _currentAnimBlend;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    public void UpdateFromNetwork(PlayerData data)
    {
        if (data.recentActions == null || data.recentActions.Count == 0) return;

        foreach (var action in data.recentActions)
        {

            if (!_isPlaybackStarted)
                {
                    // Start the clock slightly behind the first timestamp to create the buffer
                    _playbackClock = action.Timestamp;
                    _isPlaybackStarted = true;
                }

            if (_actionBuffer.Count == 0 || action.Timestamp > _lastBufferedTimestamp)
            {
                _actionBuffer.Enqueue(action);
                _lastBufferedTimestamp = action.Timestamp;
            }
        }
    }

    private void Update()
    {
        if (!_isPlaybackStarted) return;
        if (_actionBuffer.Count == 0) return;
        _playbackClock += Time.deltaTime;
        ProcessBuffer();
        ApplyMovement();
    }

   

private void ProcessBuffer()
{
    if (_actionBuffer.Count == 0) return;

    // If we don't have a target, or we've passed the current target's timestamp
    if (_targetAction == null || _playbackClock >= _targetAction.Timestamp)
    {
        if (_actionBuffer.Count > 0)
        {
            // The old target becomes the new starting point
            _previousAction = _targetAction; 
            _targetAction = _actionBuffer.Dequeue();

            // If this is the first move, bridge the gap from current position
            if (_previousAction == null)
            {
                _previousAction = _targetAction;
            }
            
            ExecuteEvents(_previousAction);
        }
    }
}

private void ApplyMovement()
{
    if (_previousAction == null || _targetAction == null) return;

    // 1. Calculate the "Alpha" (0 to 1) of the current move segment
    float segmentDuration = _targetAction.Timestamp - _previousAction.Timestamp;
    float timePassedInSegment = _playbackClock - _previousAction.Timestamp;
    
    // t is the percentage of completion for this specific movement jump
    float t = segmentDuration > 0 ? timePassedInSegment / segmentDuration : 1f;
    t = Mathf.Clamp01(t);

    // 2. Linear Interpolation (Lerp) for position and rotation
    // This ensures a constant velocity between the two points
    Vector3 newPos = Vector3.Lerp(_previousAction.Position, _targetAction.Position, t);
    float newRot = Mathf.LerpAngle(_previousAction.RotationY, _targetAction.RotationY, t);

    // 3. Calculate actual velocity for the Animator
    // Speed = Distance of this segment / Time of this segment
    float segmentSpeed = Vector3.Distance(_previousAction.Position, _targetAction.Position) / Mathf.Max(0.001f, segmentDuration);
    
    // Apply
    transform.position = newPos;
    transform.rotation = Quaternion.Euler(0, newRot, 0);

    UpdateMoveAnimation(segmentSpeed, t);
}

private void UpdateMoveAnimation(float speed, float t)
{
    // If we are at the end of a segment and no more data is in buffer, fade to 0
    float targetAnimSpeed = (_actionBuffer.Count == 0 && t >= 1.1f) ? 0f : speed;

    _currentAnimBlend = Mathf.Lerp(_currentAnimBlend, targetAnimSpeed, Time.deltaTime * _speedChangeRate);
    if (_currentAnimBlend < 0.01f) _currentAnimBlend = 0f;

    _animator.SetFloat(_animIDSpeed, _currentAnimBlend);
    _animator.SetFloat(_animIDMotionSpeed, targetAnimSpeed > 0.1f ? 1f : 0f);
}

    private void ExecuteEvents(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerData.JUMP_EVENT:
                _animator.SetBool(_animIDGrounded, false);
                _animator.SetBool(_animIDJump, true);
                break;
            case PlayerData.FALL_EVENT:
                _animator.SetBool(_animIDGrounded, false);
                _animator.SetBool(_animIDFreeFall, true);
                break;
            case PlayerData.GROUNDED_EVENT:
                _animator.SetBool(_animIDGrounded, true);
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
                break;
        }
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

    private void OnDrawGizmos()
{
    // Draw the "Future" path in the buffer (Yellow)
    if (_actionBuffer != null && _actionBuffer.Count > 0)
    {
        Gizmos.color = Color.yellow;
        Vector3 lastPoint = (_targetAction != null) ? _targetAction.Position : transform.position;
        
        foreach (var action in _actionBuffer)
        {
            Gizmos.DrawLine(lastPoint, action.Position);
            Gizmos.DrawSphere(action.Position, 0.1f);
            lastPoint = action.Position;
        }
    }

    // Draw the "Current" active segment (Green)
    if (_previousAction != null && _targetAction != null)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_previousAction.Position, _targetAction.Position);
        Gizmos.DrawCube(_targetAction.Position, Vector3.one * 0.2f);
    }
}
   
}