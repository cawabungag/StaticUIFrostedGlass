using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;

public class StaticUIFrostedGlass : MonoBehaviour
{
	private const string SHADER_NAME = "UI/GaussianBlur";
	public RawImage _screenshotDisplay;
	public float _blurSize = 2f;

	private RenderTexture _currentlyDisplayedRT;
	private Stopwatch _stopwatch = new();

	private void OnEnable()
	{
		StartCoroutine(CaptureAndProcessScreenCoroutine());
	}

	private IEnumerator CaptureAndProcessScreenCoroutine()
	{
		_screenshotDisplay.enabled = false;
		_stopwatch.Reset();
		_stopwatch.Start();
		var overallStartTime = Time.realtimeSinceStartup;

		yield return new WaitForEndOfFrame();
		var afterWaitTime = Time.realtimeSinceStartup;
		UnityEngine.Debug.Log(
			$"[Benchmark] WaitForEndOfFrame: {(afterWaitTime - overallStartTime) * 1000:F2} ms");

		var width = Screen.width;
		var height = Screen.height;

		var sourceScreenshotTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		Material blurMaterial = null;
		var captureStartTime = Time.realtimeSinceStartup;

		try
		{
			sourceScreenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			sourceScreenshotTexture.Apply();
			var captureEndTime = Time.realtimeSinceStartup;
			UnityEngine.Debug.Log(
				$"[Benchmark] Screen Capture (ReadPixels + Apply): {(captureEndTime - captureStartTime) * 1000:F2} ms");

			var setupStartTime = Time.realtimeSinceStartup;
			var gaussianBlurShader = Shader.Find(SHADER_NAME);
			blurMaterial = new Material(gaussianBlurShader);
			var finalBlurredRT =
				RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default);
			UnityEngine.Debug.Log(
				"Temporary RenderTexture for final blur output (finalBlurredRT) obtained.");
			var setupEndTime = Time.realtimeSinceStartup;
			UnityEngine.Debug.Log(
				$"[Benchmark] Material & RT Setup: {(setupEndTime - setupStartTime) * 1000:F2} ms");


			var blitStartTime = Time.realtimeSinceStartup;
			blurMaterial.SetFloat("_BlurSize", _blurSize);
			blurMaterial.SetVector("_TexelSize", new Vector4(
				1.0f / width,
				1.0f / height,
				width,
				height
			));

			Graphics.Blit(sourceScreenshotTexture, finalBlurredRT, blurMaterial);
			var blitEndTime = Time.realtimeSinceStartup;
			UnityEngine.Debug.Log(
				$"[Benchmark] Graphics.Blit (Blur): {(blitEndTime - blitStartTime) * 1000:F2} ms");

			if (_screenshotDisplay != null)
			{
				if (_currentlyDisplayedRT != null && _currentlyDisplayedRT != finalBlurredRT)
				{
					RenderTexture.ReleaseTemporary(_currentlyDisplayedRT);
				}

				_screenshotDisplay.texture = finalBlurredRT;
				_currentlyDisplayedRT = finalBlurredRT;
			}
			else
			{
				RenderTexture.ReleaseTemporary(finalBlurredRT);
				_currentlyDisplayedRT = null;
			}
		}
		finally
		{
			if (sourceScreenshotTexture != null)
			{
				Destroy(sourceScreenshotTexture);
			}

			if (blurMaterial != null)
			{
				Destroy(blurMaterial);
			}
		}

		_screenshotDisplay.enabled = true;
		_stopwatch.Stop();
		var overallEndTime = Time.realtimeSinceStartup;
		UnityEngine.Debug.Log(
			$"[Benchmark] Total Coroutine Time (realtimeSinceStartup): {(overallEndTime - overallStartTime) * 1000:F2} ms");
		UnityEngine.Debug.Log(
			$"[Benchmark] Total Coroutine Time (Stopwatch): {_stopwatch.ElapsedMilliseconds} ms");
		UnityEngine.Debug.Log(
			$"Screen capture and blur processing complete. Blur Size: {_blurSize}. Dimensions: {width}x{height}");
	}

	private void OnDisable()
	{
		if (_currentlyDisplayedRT != null)
		{
			RenderTexture.ReleaseTemporary(_currentlyDisplayedRT);
			_currentlyDisplayedRT = null;
			if (_screenshotDisplay != null && _screenshotDisplay.texture == _currentlyDisplayedRT)
			{
				_screenshotDisplay.texture = null;
			}
		}
	}
}