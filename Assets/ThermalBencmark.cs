using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class ThermalBenchmark : MonoBehaviour
{
    public TemperatureData lastTemperature;
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass thermalReader;
#endif
    
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
	private static extern int GetThermalState();
    
	[DllImport("__Internal")]  
	private static extern float GetEstimatedTemperature();
#endif

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        thermalReader = new AndroidJavaClass("com.carx.benchmark.ThermalReader");
#endif
    }
    
    public void StartMonitoring()
    {
        InvokeRepeating(nameof(UpdateTemperature), 0f, 1f); // 1 Hz
    }
    
    public void StopMonitoring()
    {
        CancelInvoke(nameof(UpdateTemperature));
    }
    
    public TemperatureData GetLastTemperature()
    {
        return lastTemperature ?? new TemperatureData { temp = -1, thermalState = ThermalState.Nominal};
    }
    
    void UpdateTemperature()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (thermalReader == null) return;
        
        try 
        {
            AndroidJavaObject tempMap = thermalReader.CallStatic<AndroidJavaObject>("getTemperatureMap");
            
            if (tempMap != null)
            {
                lastTemperature = new TemperatureData
                {
                    temp = GetMapFloat(tempMap, "cpu"),
                };
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Temperature update error: {e.Message}");
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        int state = GetThermalState();
        
        lastTemperature = new TemperatureData
        {
            temp = GetEstimatedTemperature(),
            thermalState = (ThermalState) state,
        };
#endif
    }
    
    private float GetMapFloat(AndroidJavaObject map, string key)
    {
        try 
        {
            AndroidJavaObject value = map.Call<AndroidJavaObject>("get", key);
            return value?.Call<float>("floatValue") ?? -1f;
        }
        catch 
        {
            return -1f;
        }
    }
}

[Serializable]
public class TemperatureData
{
    public float temp;
    public ThermalState thermalState;
}

public enum ThermalState
{
    Nominal = 0,
    Fair = 1,
    Serious = 2,
    Critical = 3
}