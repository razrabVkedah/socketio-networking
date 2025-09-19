using System;
using System.Collections.Generic;
using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkComponents.Help;
using ClientSocketIO.NetworkData;
using ClientSocketIO.Types.NetworkUpdate;
using UnityEngine;
using TransformData = ClientSocketIO.Types.NetworkUpdate.TransformData;

namespace ClientSocketIO.NetworkComponents
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Transform))]
    public class NetworkTransform : BaseNetworkComponent
    {
        public TransformSyncType positionSyncType;
        public bool syncPositionX;
        public bool syncPositionY;
        public bool syncPositionZ;
        private float _lastSentPositionX;
        private float _lastSentPositionY;
        private float _lastSentPositionZ;

        public TransformSyncType rotationSyncType;
        public bool syncRotationX;
        public bool syncRotationY;
        public bool syncRotationZ;
        private float _lastSentRotationX;
        private float _lastSentRotationY;
        private float _lastSentRotationZ;

        public bool syncScaleX;
        public bool syncScaleY;
        public bool syncScaleZ;
        private float _lastSentScaleX;
        private float _lastSentScaleY;
        private float _lastSentScaleZ;

        public InterpolationType interpolationType;


        private const float StateLifeTime = 5f;
        private const float SyncTrashHold = 0.001f;
        private readonly TransformData _toSendData = new();


        #region EventFunctions

        private void Start()
        {
            var pos = GetSyncPosition();
            _lastSentPositionX = pos.x;
            _lastSentPositionY = pos.y;
            _lastSentPositionZ = pos.z;

            var rot = GetSyncRotation();
            _lastSentRotationX = rot.x;
            _lastSentRotationY = rot.y;
            _lastSentRotationZ = rot.z;

            var scale = transform.localScale;
            _lastSentScaleX = scale.x;
            _lastSentScaleY = scale.y;
            _lastSentScaleZ = scale.z;

            var interpolationTime = NetworkUpdateHandler.GetInterpolationTime();
            _states.Add(new State(pos, rot, scale, interpolationTime, interpolationTime));
        }

        protected override void Update2()
        {
            if ((Client.IsHost == true && authorityMode is AuthorityMode.ClientToHost or AuthorityMode.Both) ||
                (Client.IsHost == false && authorityMode is AuthorityMode.HostToClient or AuthorityMode.Both))
            {
                switch (interpolationType)
                {
                    case InterpolationType.None:
                        NoInterpolationUpdate();
                        break;
                    case InterpolationType.Interpolate:
                        InterpolationUpdate();
                        break;
                    case InterpolationType.LagrangeInterpolation:
                        LagrangeInterpolationUpdate();
                        break;
                    default:
                        Debug.LogError($"Wrong InterpolationType: {interpolationType}");
                        break;
                }
            }

            if ((Client.IsHost == true && authorityMode is AuthorityMode.HostToClient or AuthorityMode.Both) ||
                (Client.IsHost == false && authorityMode is AuthorityMode.ClientToHost or AuthorityMode.Both))
            {
                CheckDataToSend();
            }
        }

        #endregion

        #region ToServer

        private void CheckDataToSend()
        {
            var needToSend = false;
            var position = GetSyncPosition();
            if (syncPositionX && Math.Abs(position.x - _lastSentPositionX) > SyncTrashHold)
            {
                _lastSentPositionX = position.x;
                _toSendData.PositionX = position.x;
                needToSend = true;
            }

            if (syncPositionY && Math.Abs(position.y - _lastSentPositionY) > SyncTrashHold)
            {
                _lastSentPositionY = position.y;
                _toSendData.PositionY = position.y;
                needToSend = true;
            }

            if (syncPositionZ && Math.Abs(position.z - _lastSentPositionZ) > SyncTrashHold)
            {
                _lastSentPositionZ = position.z;
                _toSendData.PositionZ = position.z;
                needToSend = true;
            }

            var rotation = GetSyncRotation();
            if (syncRotationX && Math.Abs(rotation.x - _lastSentRotationX) > SyncTrashHold)
            {
                _lastSentRotationX = rotation.x;
                _toSendData.RotationX = rotation.x;
                needToSend = true;
            }

            if (syncRotationY && Math.Abs(rotation.y - _lastSentRotationY) > SyncTrashHold)
            {
                _lastSentRotationY = rotation.y;
                _toSendData.RotationY = rotation.y;
                needToSend = true;
            }

            if (syncRotationZ && Math.Abs(rotation.z - _lastSentRotationZ) > SyncTrashHold)
            {
                _lastSentRotationZ = rotation.z;
                _toSendData.RotationZ = rotation.z;
                needToSend = true;
            }

            var scale = transform.localScale;
            if (syncScaleX && Math.Abs(scale.x - _lastSentScaleX) > SyncTrashHold)
            {
                _lastSentScaleX = scale.x;
                _toSendData.ScaleX = scale.x;
                needToSend = true;
            }

            if (syncScaleY && Math.Abs(scale.y - _lastSentScaleY) > SyncTrashHold)
            {
                _lastSentScaleY = scale.y;
                _toSendData.ScaleY = scale.y;
                needToSend = true;
            }

            if (syncScaleZ && Math.Abs(scale.z - _lastSentScaleZ) > SyncTrashHold)
            {
                _lastSentScaleZ = scale.z;
                _toSendData.ScaleZ = scale.z;
                needToSend = true;
            }

            if (needToSend == false) return;

            if (_toSendData.BeginChangesServerTime < 0.0f) _toSendData.BeginChangesServerTime = Time.unscaledTime;
            _toSendData.networkMonoBehaviourId = GetNetworkMonoBehaviourId;
            _toSendData.serverTime = Time.unscaledTime;
            NetworkUpdateHandler.AddDataToSend(_toSendData);
        }

        protected override void OnSendDataToServer()
        {
            _toSendData.PositionX = null;
            _toSendData.PositionY = null;
            _toSendData.PositionZ = null;
            _toSendData.RotationX = null;
            _toSendData.RotationY = null;
            _toSendData.RotationZ = null;
            _toSendData.ScaleX = null;
            _toSendData.ScaleY = null;
            _toSendData.ScaleZ = null;
            _toSendData.BeginChangesServerTime = -1f;
        }

        #endregion

        #region FromServer

        private readonly List<State> _states = new();

        private void NoInterpolationUpdate()
        {
            if (_states.Count == 0) return;

            ApplySyncPosition(_states[^1].Position);
            ApplySyncRotation(_states[^1].Rotation);
            transform.localScale = _states[^1].Scale;
        }
        
        private void LagrangeInterpolationUpdate()
        {
            switch (_states.Count)
            {
                case 0:
                    return;
                case 1:
                    ApplySyncPosition(_states[0].Position);
                    ApplySyncRotation(_states[0].Rotation);
                    transform.localScale = _states[0].Scale;
                    return;
            }

            var interpolationTime = NetworkUpdateHandler.GetInterpolationTime();

            for (var i = 0; i < _states.Count - 1; i++)
            {
                if (_states[i].Time > interpolationTime || _states[i + 1].Time <= interpolationTime) continue;
                if (_states[i].Time > _states[i + 1].BeginChangesTime)
                {
                    Debug.LogWarning(
                        $"BeginChanges server time is greater! {_states[i].Time} {_states[i + 1].BeginChangesTime}");
                }

                var t = (interpolationTime - _states[i + 1].BeginChangesTime) /
                        (_states[i + 1].Time - _states[i + 1].BeginChangesTime);
                ApplySyncPosition(Vector3.Lerp(_states[i].Position, _states[i + 1].Position, t));
                ApplySyncRotation(Quaternion.Lerp(Quaternion.Euler(_states[i].Rotation),
                    Quaternion.Euler(_states[i + 1].Rotation), t));
                transform.localScale = Vector3.Lerp(_states[i].Scale, _states[i + 1].Scale, t);
                return;
            }

            ApplySyncPosition(_states[^1].Position);
            ApplySyncRotation(_states[^1].Rotation);
            transform.localScale = _states[^1].Scale;
        }

        private void InterpolationUpdate()
        {
            switch (_states.Count)
            {
                case 0:
                    return;
                case 1:
                    ApplySyncPosition(_states[0].Position);
                    ApplySyncRotation(_states[0].Rotation);
                    transform.localScale = _states[0].Scale;
                    return;
            }

            var interpolationTime = NetworkUpdateHandler.GetInterpolationTime();

            for (var i = 0; i < _states.Count - 1; i++)
            {
                if (_states[i].Time > interpolationTime || _states[i + 1].Time <= interpolationTime) continue;
                if (_states[i].Time > _states[i + 1].BeginChangesTime)
                {
                    Debug.LogWarning(
                        $"BeginChanges server time is greater! {_states[i].Time} {_states[i + 1].BeginChangesTime}");
                }

                var t = (interpolationTime - _states[i + 1].BeginChangesTime) /
                        (_states[i + 1].Time - _states[i + 1].BeginChangesTime);
                ApplySyncPosition(Vector3.Lerp(_states[i].Position, _states[i + 1].Position, t));
                ApplySyncRotation(Quaternion.Lerp(Quaternion.Euler(_states[i].Rotation),
                    Quaternion.Euler(_states[i + 1].Rotation), t));
                transform.localScale = Vector3.Lerp(_states[i].Scale, _states[i + 1].Scale, t);
                return;
            }

            ApplySyncPosition(_states[^1].Position);
            ApplySyncRotation(_states[^1].Rotation);
            transform.localScale = _states[^1].Scale;
        }

        protected override void OnGetDataFromServer(BaseNetworkData data)
        {
            var transformData = (TransformData)data;

            var state = _states.Count == 0
                ? new State(GetSyncPosition(), GetSyncRotation(), transform.localScale,
                    transformData.BeginChangesServerTime, data.serverTime)
                : new State(_states[^1].Position, _states[^1].Rotation, _states[^1].Scale,
                    transformData.BeginChangesServerTime, data.serverTime);

            if (syncPositionX && transformData.PositionX.HasValue) state.Position.x = transformData.PositionX.Value;
            if (syncPositionY && transformData.PositionY.HasValue) state.Position.y = transformData.PositionY.Value;
            if (syncPositionZ && transformData.PositionZ.HasValue) state.Position.z = transformData.PositionZ.Value;
            if (syncRotationX && transformData.RotationX.HasValue) state.Rotation.x = transformData.RotationX.Value;
            if (syncRotationY && transformData.RotationY.HasValue) state.Rotation.y = transformData.RotationY.Value;
            if (syncRotationZ && transformData.RotationZ.HasValue) state.Rotation.z = transformData.RotationZ.Value;
            if (syncScaleX && transformData.ScaleX.HasValue) state.Scale.x = transformData.ScaleX.Value;
            if (syncScaleY && transformData.ScaleY.HasValue) state.Scale.y = transformData.ScaleY.Value;
            if (syncScaleZ && transformData.ScaleZ.HasValue) state.Scale.z = transformData.ScaleZ.Value;

            ClearOldStates();
            _states.Add(state);
        }

        private void ClearOldStates()
        {
            while (_states.Count > 2)
            {
                if (NetworkUpdateHandler.GetInterpolationTime() - _states[0].Time < StateLifeTime) break;

                _states.RemoveAt(0);
            }
        }

        #endregion

        #region Local&World

        private Vector3 GetSyncPosition()
        {
            var transformRef = transform;
            return positionSyncType switch
            {
                TransformSyncType.World => transformRef.position,
                TransformSyncType.Local => transformRef.localPosition,
                _ => throw new ArgumentOutOfRangeException(
                    $"Error: {GetNetworkMonoBehaviourId} Position Sync Type is {positionSyncType}")
            };
        }

        private void ApplySyncPosition(Vector3 position)
        {
            switch (positionSyncType)
            {
                case TransformSyncType.World:
                    transform.position = position;
                    break;
                case TransformSyncType.Local:
                    transform.localPosition = position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Error: {GetNetworkMonoBehaviourId} Position Sync Type is {positionSyncType}");
            }
        }

        private Vector3 GetSyncRotation()
        {
            var transformRef = transform;
            return rotationSyncType switch
            {
                TransformSyncType.World => transformRef.eulerAngles,
                TransformSyncType.Local => transformRef.localEulerAngles,
                _ => throw new ArgumentOutOfRangeException(
                    $"Error: {GetNetworkMonoBehaviourId} Rotation Sync Type is {rotationSyncType}")
            };
        }

        private void ApplySyncRotation(Vector3 rotation)
        {
            switch (rotationSyncType)
            {
                case TransformSyncType.World:
                    transform.eulerAngles = rotation;
                    break;
                case TransformSyncType.Local:
                    transform.localEulerAngles = rotation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Error: {GetNetworkMonoBehaviourId} Rotation Sync Type is {rotationSyncType}");
            }
        }

        private void ApplySyncRotation(Quaternion rotation)
        {
            switch (rotationSyncType)
            {
                case TransformSyncType.World:
                    transform.rotation = rotation;
                    break;
                case TransformSyncType.Local:
                    transform.localRotation = rotation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Error: {GetNetworkMonoBehaviourId} Rotation Sync Type is {rotationSyncType}");
            }
        }

        #endregion
    }
}