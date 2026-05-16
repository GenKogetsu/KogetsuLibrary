using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.DesignPatternCore
{
    [ExecuteAlways]
    public class CameraFollow3D : FollowAbility
    {
        [SerializeField] private bool _executeAlways = true;

        [Header("Orbit Camera Settings")]
        [SerializeField] private float _mouseSensitivity = 0.2f;
        [SerializeField] private float _smoothSpeed = 15f;

        [Header("Pitch Limits")]
        [SerializeField] private float _minPitch = -20f;
        [SerializeField] private float _maxPitch = 70f;

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSensitivity = 0.01f;
        [SerializeField] private float _minDistance = 0f;
        [SerializeField] private float _maxDistance = 15f;

        private float _yaw = 0f;
        private float _pitch = 15f;
        private float _currentDistance = 5f;


        private void Start()
        {
            _yaw = transform.eulerAngles.y;
            _pitch = transform.eulerAngles.x;

            _currentDistance = Mathf.Abs(Offset.z);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected override void LateUpdate()
        {
            if (!_executeAlways && !Application.isPlaying) return;

            if (FollowTarget == null) return;

            Vector2 mouseDelta = Vector2.zero;
            float scrollDelta = 0f;

            if (Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue();
                scrollDelta = Mouse.current.scroll.ReadValue().y;
            }

            _yaw += mouseDelta.x * _mouseSensitivity;
            _pitch -= mouseDelta.y * _mouseSensitivity;

            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            if (scrollDelta != 0f)
            {
                _currentDistance -= scrollDelta * _zoomSensitivity;
                _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);
                Offset.z = -_currentDistance;
            }

            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            Vector3 lookTarget = FollowTarget.position + new Vector3(0, Offset.y, 0);

            Vector3 desiredPosition = lookTarget + (targetRotation * new Vector3(0, 0, Offset.z));

            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime), Quaternion.Slerp(transform.rotation, targetRotation, _smoothSpeed * Time.deltaTime));
        }
    }
}