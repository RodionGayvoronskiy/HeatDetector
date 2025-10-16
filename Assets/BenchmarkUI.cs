using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BenchmarkUI : MonoBehaviour
{
	[Header("UI Elements")]
	public Button startButton;
	public Button stopButton;
	public TextMeshProUGUI temperatureText;
	public TextMeshProUGUI fpsText;
	public TextMeshProUGUI statusText;
    
	[Header("Settings")]
	public int stressThreads = 4;
    
	private ThermalBenchmark thermalBenchmark;
	private StressTester stressTester;

	private void Start()
	{
		thermalBenchmark = FindObjectOfType<ThermalBenchmark>();
		stressTester = gameObject.AddComponent<StressTester>();
        
		startButton.onClick.AddListener(StartBenchmark);
		stopButton.onClick.AddListener(StopBenchmark);
		stopButton.interactable = false;
		InvokeRepeating(nameof(UpdateUI), 0f, 0.5f);
	}
    
	void StartBenchmark()
	{
		startButton.interactable = false;
		stopButton.interactable = true;
		statusText.text = "BENCHMARK RUNNING...";
        
		stressTester.StartStressTest(stressThreads);
		thermalBenchmark?.StartMonitoring();
	}
    
	void StopBenchmark()
	{
		startButton.interactable = true;
		stopButton.interactable = false;
		statusText.text = "STOPPED";
        
		stressTester.StopStressTest();
		thermalBenchmark?.StopMonitoring();
	}
    
	void UpdateUI()
	{
		if (thermalBenchmark)
		{
			var tempData = thermalBenchmark.GetLastTemperature();
			temperatureText.text = $"CPU: {tempData.temp:F1}°C\nState: {tempData.thermalState}";
		}
        
		fpsText.text = $"FPS: {1f/Time.unscaledDeltaTime:F0}";
	}
}