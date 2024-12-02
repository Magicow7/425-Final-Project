using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimateWeapon : MonoBehaviour
{
    //This script is unfinished, but we could use it to animate the wand
    [FormerlySerializedAs("xAnimation"),SerializeField]
    private  AnimationCurve _xAnimation;
    [FormerlySerializedAs("animationRepeatTime"),SerializeField]
    private float _animationRepeatTime = 1;
    private float _time = 0;

    private Vector3 _startRot;
    // Start is called before the first frame update
    void Start()
    {
        _startRot = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if(_time >= _animationRepeatTime){
            _time -= _animationRepeatTime;
        }
        transform.rotation = Quaternion.Euler(_startRot + new Vector3(_xAnimation.Evaluate(_time),0,0));
    }
}
