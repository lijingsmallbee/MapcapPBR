using UnityEngine;
using System.Collections;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class MC_CreateMatCap : MonoBehaviour
{
	public Camera screenshotCamera;
	public Material modifyMaterial;
	public Material previewMaterial;
	[Tooltip("View the saved PNG in the file browser on save")]
	public bool revealOnSave = true;

	private void Awake()
	{
		var renderTarget = new RenderTexture(screenshotCamera.targetTexture.width,screenshotCamera.targetTexture.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
		screenshotCamera.targetTexture = renderTarget;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(MC_CreateMatCap))]
public class MC_CreateMatCap_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(10);

		if (GUILayout.Button("Save MatCap PNG", GUILayout.Height(40)))
		{
			bool reveal = ((MC_CreateMatCap)target).revealOnSave;
			CaptureScreenshot("MatCap", reveal);
		}

		if (GUILayout.Button("Generate 10*10"))
		{
			string path = EditorUtility.SaveFolderPanel("Save MatCap Texture", "MatCap", "png");
			CaptureScreenshot(10,10,path);
		}
	}

	void CaptureScreenshot(string filename, bool reveal)
	{
		var screenshotCamera = ((MC_CreateMatCap)target).screenshotCamera;
		Graphics.SetRenderTarget(screenshotCamera.targetTexture);
		var texture = new Texture2D(screenshotCamera.targetTexture.width, screenshotCamera.targetTexture.height, TextureFormat.ARGB32, false);
		texture.ReadPixels(new Rect(0, 0, screenshotCamera.targetTexture.width, screenshotCamera.targetTexture.height), 0, 0, false);
		texture.Apply(false, false);
		Graphics.SetRenderTarget(null);

		string path = EditorUtility.SaveFilePanelInProject("Save MatCap Texture", "MatCap", "png", "Save your MatCap image");
		if (!string.IsNullOrEmpty(path))
		{
			// Save the PNG file
			File.WriteAllBytes(path, texture.EncodeToPNG());
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
			if (reveal)
			{
				EditorUtility.RevealInFinder(path);
			}

			// Disable compression by default
			var textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.wrapMode = TextureWrapMode.Clamp;

			// Apply to preview material if needed
			var previewMaterial = ((MC_CreateMatCap)target).previewMaterial;
			if (previewMaterial != null)
			{
				var generatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				if (generatedTexture != null)
				{
					previewMaterial.SetTexture("_MatCap", generatedTexture);
				}
			}
		}

		DestroyImmediate(texture);
	}

	void CaptureScreenshot(int w, int h,string folder)
	{
		
		var screenshotCamera = ((MC_CreateMatCap)target).screenshotCamera;
		var oldTAar = screenshotCamera.targetTexture;
		//创建32位 render texture，需要用srgb的图，用32位线性纹理，会有问题
		var renderTarget = new RenderTexture(screenshotCamera.targetTexture.width,screenshotCamera.targetTexture.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
		screenshotCamera.targetTexture = renderTarget;
		var deltaI = 1.0f / w;
		var deltaJ = 1.0f / h;
		for (int i = 0; i <= w; ++i)
		{
			for (int j = 0; j <= h; ++j)
			{
				var mat = ((MC_CreateMatCap) target).modifyMaterial;
				if (mat)
				{
					mat.SetFloat("_Metallic",deltaI*i);
					mat.SetFloat("_Smoothness",deltaJ*j);
				}
				screenshotCamera.Render();
				Graphics.SetRenderTarget(screenshotCamera.targetTexture);
				var texture = new Texture2D(screenshotCamera.targetTexture.width, screenshotCamera.targetTexture.height, TextureFormat.RGBA32, false);
				texture.ReadPixels(new Rect(0, 0, screenshotCamera.targetTexture.width, screenshotCamera.targetTexture.height), 0, 0, false);
				texture.Apply(false, false);
				Graphics.SetRenderTarget(null);
				var fileName = string.Format("m_{0}_s_{1}.png", i, j);
				var path = Path.Combine(folder, fileName);
				File.WriteAllBytes(path, texture.EncodeToPNG());
				DestroyImmediate(texture);

				var localPath = path.Substring(path.IndexOf("Assets"));
				var importer = AssetImporter.GetAtPath(localPath);
				if (importer != null)
				{
					TextureImporter textureImporter = importer as TextureImporter;
					if (textureImporter != null)
					{
						textureImporter.sRGBTexture = true;
						textureImporter.SaveAndReimport();
					}
				}
			}
		}

		screenshotCamera.targetTexture = oldTAar;

	}
}
#endif