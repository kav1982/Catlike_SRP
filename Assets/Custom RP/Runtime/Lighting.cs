﻿using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting 
{
    const string bufferName = "Lighting";


    CullingResults cullingResults;
    

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    const int maxDirLightCount = 4;

    Shadows shadows = new Shadows();

    public void Setup (ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        //Shadows shadows = new Shadows();
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        //SetupDirectionalLight();
        shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        shadows.Render();
        buffer.EndSample(bufferName);      
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();    
    }

    static int
       //dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
       //dirLIghtDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
       dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
       dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
       dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
       dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    static Vector4[]
        dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount],
        dirLightShadowData = new Vector4[maxDirLightCount];

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

        int dirLightCount = 0;

        for (int i=0; i<visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                    break;
            }
        }
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
        buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
    }


    void SetupDirectionalLight(int index, ref VisibleLight visibleLight) 
    {
        //Light light = RenderSettings.sun;
        //buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
        //buffer.SetGlobalVector(dirLIghtDirectionId, -light.transform.forward);
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2); //Get a column of the matrix.
        //shadows.ReserveDirectionalShadows(visibleLight.light, index);
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);
        

    }

    public void Cleanup()
    {
        shadows.Cleanup();       
    }
}
