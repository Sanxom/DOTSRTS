using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorldPosition : MonoBehaviour
{
    public static MouseWorldPosition Instance { get; private set; }

    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private Camera _mainCam;
    #endregion

    #region Private Fields
    private InputSystem_Actions _gameInput;
    #endregion

    #region Public Properties
    public InputAction MousePositionAction { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        _gameInput = new();
        MousePositionAction = _gameInput.UI.Point;
    }
    private void OnEnable()
    {
        _gameInput.Enable();
    }

    private void OnDisable()
    {
        _gameInput.Disable();
    }
    #endregion

    #region Public Methods
    public Vector3 GetRaycastArrayPosition()
    {
        Ray mouseCameraRay = _mainCam.ScreenPointToRay(MousePositionAction.ReadValue<Vector2>());
        RaycastHit[] results = new RaycastHit[1];

        if (Physics.RaycastNonAlloc(mouseCameraRay, results) >= 1)
            return results[0].point;
        else
            return Vector3.zero;
    }

    public Vector3 GetPhysicsRaycastPosition()
    {
        Ray mouseCameraRay = _mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(mouseCameraRay, out RaycastHit hit))
            return hit.point;
        else
            return Vector3.zero;
    }

    public Vector3 GetPlaneRaycastPosition()
    {
        Ray mouseCameraRay = _mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new(Vector3.up, Vector3.zero);

        if (plane.Raycast(mouseCameraRay, out float distance))
            return mouseCameraRay.GetPoint(distance);
        else
            return Vector3.zero;
    }
    #endregion

    #region Private Methods
    #endregion
}