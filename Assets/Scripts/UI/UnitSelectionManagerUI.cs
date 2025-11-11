using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private RectTransform _selectionAreaRectTransform;
    [SerializeField] private Canvas _canvas;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;

        _selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_selectionAreaRectTransform.gameObject.activeSelf)
            UpdateVisual();
    }

    private void OnDisable()
    {
        UnitSelectionManager.Instance.OnSelectionAreaStart -= UnitSelectionManager_OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd -= UnitSelectionManager_OnSelectionAreaEnd;
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void UnitSelectionManager_OnSelectionAreaEnd() => _selectionAreaRectTransform.gameObject.SetActive(false);
    
    private void UnitSelectionManager_OnSelectionAreaStart()
    {
        _selectionAreaRectTransform.gameObject.SetActive(true);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();

        float canvasScale = _canvas.transform.localScale.x;
        _selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
        _selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
    }
    #endregion
}