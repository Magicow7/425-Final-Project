using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSX_Post_Process : MonoBehaviour
{
     [SerializeField]
    Material postEffectMaterial;

    private Camera _camera;

    
    void Start(){
        _camera = Camera.main;
        //set depth texture read to on
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest){
        RenderTexture RT = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        
      
        /*postEffectMaterial.SetTexture("_SceneDepthTex", sceneDepth.depthTexture);
        postEffectMaterial.SetTexture("_BoidDepthTex", boidDepth.depthTexture);
        postEffectMaterial.SetTexture("_BoidColorTex", boidDepth.colorTexture);*/

        Graphics.Blit(src, RT, postEffectMaterial, 0);
       
        Graphics.Blit(RT, dest);

        RenderTexture.ReleaseTemporary(RT);
    }
}
