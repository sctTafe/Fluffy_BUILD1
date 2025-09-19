using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Lightweight snapshot based movement sync to replace continuous NetworkTransform variable bandwidth.
/// Owner simulates with ThirdPersonController, sends snapshots only when movement changed enough or
/// at a capped rate. Remote clients buffer and interpolate in the past for smooth motion.
/// </summary>
[DisallowMultipleComponent]
public class MovementSnapshotSync : NetworkBehaviour
{
    // ---- Tuning (Owner Side Sending) ----
    [Header("Send Settings")] [Tooltip("Max snapshot send rate (Hz)")]
    [Range(2,60)] public int maxSendRate = 15; // ~15 Hz default
    [Tooltip("Always send if this time (seconds) has passed even if thresholds not exceeded")] public float maxSendInterval = 0.25f;
    [Tooltip("Min distance change before sending (meters)")] public float distanceThreshold = 0.05f;
    [Tooltip("Min rotation change before sending (degrees)")] public float angleThreshold = 2f;
    [Tooltip("If true and a NetworkTransform exists it will be disabled at runtime to avoid duplicate sync")] public bool disableExistingNetworkTransform = true;

    // ---- Tuning (Remote Interpolation) ----
    [Header("Interpolation Settings")] [Tooltip("Time (seconds) to stay behind real time for buffering")] public float interpolationBackTime = 0.1f; // 100 ms buffer
    [Tooltip("Hard cap for extrapolation when no future snapshot (seconds)")] public float maxExtrapolationTime = 0.2f;
    [Tooltip("Maximum number of buffered snapshots")] public int bufferSize = 32;
    [Tooltip("If large correction (> this distance) snap instead of lerp")] public float snapDistance = 3f;

    private CharacterController _controller;
    // Removed direct reference to the original third person controller script to avoid assembly dependency issues

    struct MovementSnapshot : INetworkSerializable
    {
        public float serverTime; // server time (seconds)
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref serverTime);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
        }
    }

    // Buffer for remote interpolation
    private readonly List<MovementSnapshot> _buffer = new List<MovementSnapshot>(64);

    // Owner tracking
    private MovementSnapshot _lastSentSnapshot;
    private bool _hasLastSnapshot;
    private float _sendTimer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _controller = GetComponent<CharacterController>();
        // No direct dependency on a specific third person controller script

        if (disableExistingNetworkTransform)
        {
            var nt = GetComponent<Unity.Netcode.Components.NetworkTransform>();
            if (nt != null) nt.enabled = false; // Prevent duplicate sync traffic
        }

        if (!IsOwner)
        {
            // Keep remote components enabled for animation; movement simulation already guarded by IsOwner checks in their own scripts
        }
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (IsOwner)
        {
            Owner_UpdateSendSnapshots();
        }
        else
        {
            Remote_UpdateInterpolation();
        }
    }

    // ---------------- Owner Side ----------------
    private void Owner_UpdateSendSnapshots()
    {
        _sendTimer += Time.deltaTime;
        float minInterval = 1f / Mathf.Max(1, maxSendRate);
        if (_sendTimer < minInterval) return; // rate limit

        // Gather current state
        var snapshot = new MovementSnapshot
        {
            serverTime = (float)NetworkManager.ServerTime.Time,
            position = transform.position,
            rotation = transform.rotation,
            velocity = _controller != null ? _controller.velocity : Vector3.zero
        };

        bool shouldSend = !_hasLastSnapshot;
        if (!shouldSend)
        {
            float dist = Vector3.Distance(snapshot.position, _lastSentSnapshot.position);
            float ang = Quaternion.Angle(snapshot.rotation, _lastSentSnapshot.rotation);
            if (dist > distanceThreshold || ang > angleThreshold || _sendTimer >= maxSendInterval)
                shouldSend = true;
        }

        if (shouldSend)
        {
            _sendTimer = 0f;
            _lastSentSnapshot = snapshot;
            _hasLastSnapshot = true;

            // Host can directly broadcast without extra RPC bounce
            if (IsServer)
            {
                BroadcastSnapshotClientRpc(snapshot);
            }
            else
            {
                SubmitSnapshotServerRpc(snapshot); // client -> server -> others
            }
        }
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SubmitSnapshotServerRpc(MovementSnapshot snapshot)
    {
        // Relay to other clients (including host if not same)
        BroadcastSnapshotClientRpc(snapshot);
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void BroadcastSnapshotClientRpc(MovementSnapshot snapshot)
    {
        if (IsOwner) return; // owner ignores its own snapshots
        AddSnapshot(snapshot);
    }

    private void AddSnapshot(MovementSnapshot snapshot)
    {
        // Maintain time order (snapshots should already be in order but guard anyway)
        if (_buffer.Count == 0 || snapshot.serverTime >= _buffer[_buffer.Count - 1].serverTime)
        {
            _buffer.Add(snapshot);
        }
        else
        {
            // Insert maintaining order (rare)
            int index = _buffer.FindIndex(s => s.serverTime > snapshot.serverTime);
            if (index < 0) _buffer.Add(snapshot);
            else _buffer.Insert(index, snapshot);
        }

        // Trim buffer size
        if (_buffer.Count > bufferSize)
        {
            int remove = _buffer.Count - bufferSize;
            _buffer.RemoveRange(0, remove);
        }
    }

    // ---------------- Remote Side ----------------
    private void Remote_UpdateInterpolation()
    {
        if (_buffer.Count == 0) return;

        double serverTime = NetworkManager.ServerTime.Time;
        double renderTime = serverTime - interpolationBackTime;

        // Remove snapshots that are too old (keep at least 2)
        while (_buffer.Count >= 2 && _buffer[1].serverTime <= renderTime - 1f) // keep 1 second of history
        {
            _buffer.RemoveAt(0);
        }

        // If renderTime is before the first snapshot just snap to first
        if (renderTime <= _buffer[0].serverTime)
        {
            ApplySnapshot(_buffer[0]);
            return;
        }

        // If renderTime is after the last snapshot we may need to extrapolate
        if (renderTime >= _buffer[_buffer.Count - 1].serverTime)
        {
            var last = _buffer[_buffer.Count - 1];
            float dt = (float)(renderTime - last.serverTime);
            if (dt <= maxExtrapolationTime)
            {
                Vector3 extrapolatedPos = last.position + last.velocity * dt;
                ApplyInterpolated(last.position, extrapolatedPos, last.rotation, last.rotation, 1f);
            }
            else
            {
                ApplySnapshot(last);
            }
            return;
        }

        // Find the two snapshots we are between
        MovementSnapshot prev = _buffer[0];
        MovementSnapshot next = _buffer[_buffer.Count - 1];
        for (int i = 0; i < _buffer.Count - 1; i++)
        {
            if (_buffer[i + 1].serverTime >= renderTime)
            {
                prev = _buffer[i];
                next = _buffer[i + 1];
                break;
            }
        }

        float length = next.serverTime - prev.serverTime;
        float t = length > 0.0001f ? (float)((renderTime - prev.serverTime) / length) : 0f;
        t = Mathf.Clamp01(t);
        ApplyInterpolated(prev.position, next.position, prev.rotation, next.rotation, t);
    }

    private void ApplySnapshot(MovementSnapshot snap)
    {
        if (Vector3.Distance(transform.position, snap.position) > snapDistance)
        {
            transform.position = snap.position; // hard snap large corrections
            transform.rotation = snap.rotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, snap.position, 0.5f);
            transform.rotation = Quaternion.Slerp(transform.rotation, snap.rotation, 0.5f);
        }
    }

    private void ApplyInterpolated(Vector3 aPos, Vector3 bPos, Quaternion aRot, Quaternion bRot, float t)
    {
        Vector3 targetPos = Vector3.Lerp(aPos, bPos, t);
        Quaternion targetRot = Quaternion.Slerp(aRot, bRot, t);

        if (Vector3.Distance(transform.position, targetPos) > snapDistance)
        {
            transform.position = targetPos; // snap if far
            transform.rotation = targetRot;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f); // mild smoothing
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.5f);
        }
    }
}
