using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class StressTester : MonoBehaviour
{
	private bool isRunning = false;
	private Thread[] cpuThreads;
	private List<Coroutine> gpuStressCoroutines;
	private List<GameObject> quads = new List<GameObject>();

	public void StartStressTest(int threadCount)
	{
		isRunning = true;

		// CPU Stress (многопоточный)
		cpuThreads = new Thread[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			cpuThreads[i] = new Thread(CpuStressWorker);
			cpuThreads[i].Start(i);
		}

		// GPU Stress (Unity корутины)
		for (int i = 0; i < 1; i++)
		{
			GameObject stressQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			stressQuad.name = i.ToString();
			quads.Add(stressQuad);
		}

		StartCoroutine(GpuStressWorker(quads));

		Debug.Log($"Started {threadCount} CPU threads + GPU stress");
	}

	public void StopStressTest()
	{
		isRunning = false;

		// Останавливаем CPU потоки
		if (cpuThreads != null)
		{
			for (int i = 0; i < cpuThreads.Length; i++)
			{
				if (cpuThreads[i] != null && cpuThreads[i].IsAlive)
				{
					cpuThreads[i].Interrupt();
					cpuThreads[i].Join(1000);
				}
			}
		}

		StopAllCoroutines();

		Debug.Log("Stress test stopped");
	}

	private void CpuStressWorker(object threadId)
	{
		int id = (int)threadId;
		long counter = 0;
		double[] buffer = new double[1000000]; // Больший буфер = больше нагрузки

		try
		{
			while (isRunning)
			{
				// Интенсивные вычисления (матричные операции)
				for (int i = 0; i < buffer.Length; i++)
				{
					buffer[i] = Math.Sin(i * 0.1) * Math.Exp(-counter * 1e-10) +
					            Math.Cos((i + id) * 0.05) * Math.Log(Math.Abs(counter) + 1);
					counter++;

					// Проверка прерывания
					if (Thread.CurrentThread.IsThreadPoolThread &&
					    counter % 1000000 == 0 && !isRunning)
						break;
				}

				// Prime95-style stress
				for (long n = 2; n < 1000000; n++)
				{
					bool isPrime = true;
					for (long i = 2; i * i <= n; i++)
					{
						if (n % i == 0)
						{
							isPrime = false;
							break;
						}
					}

					if (isPrime) counter++;
				}
			}
		}
		catch (ThreadInterruptedException)
		{
			Debug.Log($"CPU Thread {id} interrupted");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"CPU Thread {id} error: {e.Message}");
		}
	}

	private IEnumerator GpuStressWorker(List<GameObject> gameObjects)
	{
		Material stressMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

		while (isRunning)
		{
			foreach (GameObject stressQuad in gameObjects)
			{
				stressQuad.transform.localScale = Vector3.one * Random.Range(5, 10);

				MeshRenderer renderer = stressQuad.GetComponent<MeshRenderer>();
				renderer.material = stressMaterial;

				var a = Random.Range(0, 360);
				var b = Random.Range(0, 360);
				if (Random.Range(0f, 1f) > 0.5f)
				{
					stressQuad.transform.Rotate(Vector3.up * a * Time.unscaledDeltaTime);
				}
				else
				{
					stressQuad.transform.Rotate(Vector3.down * a * Time.unscaledDeltaTime);
				}
				
				if (Random.Range(0f, 1f) > 0.5f)
				{
					stressQuad.transform.Rotate(Vector3.right * b * Time.unscaledDeltaTime);
				}
				else
				{
					stressQuad.transform.Rotate(Vector3.left * b * Time.unscaledDeltaTime);
				}
			}
			yield return null;
		}
		
		yield return null;
	}
}