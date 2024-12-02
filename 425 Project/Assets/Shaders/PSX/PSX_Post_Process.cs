using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PsxPostProcess : MonoBehaviour
{
     [FormerlySerializedAs("postEffectMaterial"),SerializeField]
    Material _postEffectMaterial;

    private Camera _camera;

    
    void Start(){
        _camera = Camera.main;
        //set depth texture read to on
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest){
        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        
      
        /*postEffectMaterial.SetTexture("_SceneDepthTex", sceneDepth.depthTexture);
        postEffectMaterial.SetTexture("_BoidDepthTex", boidDepth.depthTexture);
        postEffectMaterial.SetTexture("_BoidColorTex", boidDepth.colorTexture);*/

        Graphics.Blit(src, rt, _postEffectMaterial, 0);
       
        Graphics.Blit(rt, dest);

        RenderTexture.ReleaseTemporary(rt);
    }
}
