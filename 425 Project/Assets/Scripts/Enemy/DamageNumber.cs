using System;
using System.Collections;
using System.Collections.Generic;
using Stat;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float _maxVisibleTime = 1f;
    
    [SerializeField] private List<DamageThreshold> _thresholds = new List<DamageThreshold>();
    
    private LookAtCamera _lookAtCamera;
    private CanvasGroup _group;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _textMesh;

    private Vector3 _initialPosition;
    private ResourceStat _maxHealth;

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

        _initialPosition = _rectTransform.position;
    }
    
    public void Initialize(ResourceStat maxHealth)
    {
        _maxHealth = maxHealth;
    }

    private void Update()
    {
        _lookAtCamera.enabled = _showing;

        if (Time.time > _hideTime)
        {
            _group.alpha = 0;
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
        if (damage <= 0.01)
        {
            return;
        }
        
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

        float damagePercent = _damageTotal / _maxHealth.MaxValue;

        DamageThreshold prevThres = _thresholds[0];
        DamageThreshold threshold = _thresholds[0];

        foreach (var t in _thresholds)
        {
            if (damagePercent < t.threshold)
            {
                threshold = t;
                break;
            }
            prevThres = t;
        }

        float size = Mathf.Lerp(threshold.size, prevThres.size, (threshold.threshold - damagePercent) / (threshold.threshold - prevThres.threshold));
        _group.transform.localScale = new Vector3(size, size, size);
        _textMesh.color = Color.Lerp(threshold.color, prevThres.color, (threshold.threshold - damagePercent) / (threshold.threshold - prevThres.threshold));

        _hideTime = Time.time + _maxVisibleTime;
        _textMesh.text = result;
        _group.alpha = 1;
            
        _lastTime = Time.time;
        _showing = true;
    }
    
    [Serializable]
    private class DamageThreshold
    {
        public float threshold;
        public Color color;
        public float size;
    }
}
