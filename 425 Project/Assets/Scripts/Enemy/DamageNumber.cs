using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float _speed = 1;
    [SerializeField] private float _alphaSpeed = 2;
    [SerializeField] private float _textAlphaSpeed = 4;
    [SerializeField] private float _yDistance = 0.5f;
    [SerializeField] private float _maxVisibleTime = 1f;
    
    private LookAtCamera _lookAtCamera;
    private CanvasGroup _group;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _textMesh;
    private CanvasGroup _textMeshGroup;

    private Vector3 _initialPosition;

    private bool _showing = false;
    private bool _isMaxHeight = false;
    private float _hideTime = 0;
    private float _lastTime = 0;

    private String _switchTo = "";
    private float _damageTotal = 0;
    
    private void Start()
    {
        _lookAtCamera = gameObject.AddComponent<LookAtCamera>();
        _group = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _textMeshGroup = _textMesh.GetComponent<CanvasGroup>();

        _initialPosition = _rectTransform.position;
    }

    private void Update()
    {
        _lookAtCamera.enabled = _showing;

        if (_showing && _switchTo != "")
        {
            if (_textMeshGroup.alpha > 0)
            {
                _textMeshGroup.alpha -= _textAlphaSpeed * Time.deltaTime;
            }
            else
            {
                _textMesh.text = _switchTo;
                _switchTo = "";
            }
        }
        else if (_showing && _switchTo == "" && _textMeshGroup.alpha < 1)
        {
            _textMeshGroup.alpha += _textAlphaSpeed * Time.deltaTime;
        }
        
        if (_showing && !_isMaxHeight)
        {
            if (_rectTransform.position.y < _initialPosition.y + _yDistance)
            {
                var vector3 = _rectTransform.position;
                vector3.y += _speed * Time.deltaTime;
                _rectTransform.position = vector3;

                _group.alpha += _alphaSpeed * Time.deltaTime;
            }
            else {
                _isMaxHeight = true;
            }
        }
        else if (_showing && Time.time >= _hideTime)
        {
            if (_group.alpha > 0)
            {
                _group.alpha -= _alphaSpeed * Time.deltaTime;
            }
            else
            {
                Reset();
            }
        }
        else if (_showing && _isMaxHeight && _group.alpha < 1)
        {
            _group.alpha += _alphaSpeed * Time.deltaTime;
        }
    }

    private void Reset()
    {
        _showing = false;
        _isMaxHeight = false;
        _damageTotal = 0;
        _rectTransform.position = _initialPosition;
        _group.alpha = 0;
    }

    public void ShowDamageNumber(float damage)
    {
        string result = "";

        _damageTotal += damage;
        
        if (_damageTotal >= 10000)
        {
            _damageTotal /= 1000;
            result = _damageTotal.ToString("0") + "k"; 
        }
        else if (_damageTotal >= 1000)
        {
            _damageTotal /= 1000;
            result = _damageTotal.ToString("0.0") + "k"; 
        }
        else if (_damageTotal >= 100)
        {
            result = _damageTotal.ToString("0"); 
        }
        else if (_damageTotal >= 10)
        {
            result = _damageTotal.ToString("0.0"); 
        }
        else
        {
            result = _damageTotal.ToString("0.00");
        }

        _hideTime = Time.time + _maxVisibleTime + _yDistance / _speed;
        if (!_showing || _lastTime - Time.time < 1 / _textAlphaSpeed)
        {
            _textMesh.text = result;
        }
        else
        {
            _switchTo = result;
        }
        _lastTime = Time.time;
        _showing = true;
    }
}
