using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateWeapon : MonoBehaviour
{
    //This script is unfinished, but we could use it to animate the wand
    [SerializeField]
    private  AnimationCurve xAnimation;
    [SerializeField]
    private float animationRepeatTime = 1;
    private float time = 0;

    private Vector3 startRot;
    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time >= animationRepeatTime){
            time -= animationRepeatTime;
        }
        transform.rotation = Quaternion.Euler(startRot + new Vector3(xAnimation.Evaluate(time),0,0));
    }
}
