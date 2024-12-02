using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MouseLook : MonoBehaviour
{
     public static MouseLook instance;

    [FormerlySerializedAs("clampInDegrees"),Header("Settings")]
    public Vector2 _clampInDegrees = new Vector2(360, 180);
    [FormerlySerializedAs("lockCursor")] public bool _lockCursor = true;
    [Space]
    private Vector2 _sensitivity = new Vector2(2, 2);
    [FormerlySerializedAs("smoothing"),Space]
    public Vector2 _smoothing = new Vector2(3, 3);

    [FormerlySerializedAs("characterBody"),Header("First Person")]
    public GameObject _characterBody;

    private Vector2 _targetDirection;
    private Vector2 _targetCharacterDirection;

    private Vector2 _mouseAbsolute;
    private Vector2 _smoothMouse;

    private Vector2 _mouseDelta;

    [FormerlySerializedAs("scoped"),HideInInspector]
    public bool _scoped;

    void Start()
    {
        instance = this;

        // Set target direction to the camera's initial orientation.
        _targetDirection = transform.localRotation.eulerAngles;

        // Set target direction for the character body to its inital state.
        if (_characterBody)
            _targetCharacterDirection = _characterBody.transform.localRotation.eulerAngles;
        
        if (_lockCursor)
            LockCursor();

    }

    public void LockCursor()
    {
        // make the cursor hidden and locked
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        // make the cursor hidden and locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Allow the script to clamp based on a desired target value.
        var targetOrientation = Quaternion.Euler(_targetDirection);
        var targetCharacterOrientation = Quaternion.Euler(_targetCharacterDirection);

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        _mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        _mouseDelta = Vector2.Scale(_mouseDelta, new Vector2(_sensitivity.x * _smoothing.x, _sensitivity.y * _smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, _mouseDelta.x, 1f / _smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, _mouseDelta.y, 1f / _smoothing.y);

        // Find the absolute mouse movement value from point zero.
        _mouseAbsolute += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (_clampInDegrees.x < 360)
            _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -_clampInDegrees.x * 0.5f, _clampInDegrees.x * 0.5f);

        // Then clamp and apply the global y value.
        if (_clampInDegrees.y < 360)
            _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -_clampInDegrees.y * 0.5f, _clampInDegrees.y * 0.5f);

        transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;

        // If there's a character body that acts as a parent to the camera
        if (_characterBody)
        {
            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
            _characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
            transform.position = _characterBody.transform.position;

            yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }
        else
        {
            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }
    }
}
