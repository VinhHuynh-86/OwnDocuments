using UnityEngine;
using System.Collections;
 
public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
    int qty = 0;
    float currentAvgFPS = 0;

#if ENABLE_CHEAT
	void Update()
	{
		if(!Define.IsShowDebugInfo)
		{
			return;
		}
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}
 
	void OnGUI()
	{
		if(!Define.IsShowDebugInfo)
		{
			return;
		}
		int w = Screen.width, h = Screen.height;
 
		GUIStyle style = new GUIStyle();
 
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h / 25;
		style.normal.textColor = new Color (1.0f, 0.0f, 0.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;

        ++qty;
        currentAvgFPS += (fps - currentAvgFPS)/qty;

		string text = string.Format("{0:0.0} ms ({1:0.} fps) - Average FPS: {2:0.} - Render: {3} x {4}", msec, fps, currentAvgFPS, Screen.width, Screen.height);
		GUI.Label(rect, text, style);

		rect = new Rect(0, h - (h * 2 / 100), w, h * 2 / 100);
		style.alignment = TextAnchor.LowerLeft;
		style.fontSize = h / 50;
		style.normal.textColor = new Color (1.0f, 1.0f, 0.0f, 1.0f);
		string profileText = string.Format("QualityLevel: {0}\nphysics_Mode: {1}\nlighting_maxQualityLimit: {2}\nparticles_maxQualityLimit: {3}\nload_allStadium: {4}\ndof: {5}\ntextures_hightQuality:{6}",
											gameoptions.GameOptions.effects.qualityLevel.ToString()
											,gameoptions.GameOptions.effects.physicsMode
											,gameoptions.GameOptions.effects.lightingLimit
											,gameoptions.GameOptions.effects.particlesLimit
											,gameoptions.GameOptions.effects.loadAllStadium
											,gameoptions.GameOptions.effects.dof
											,gameoptions.GameOptions.effects.texturesHightQuality
											);

		GUI.Label(rect, profileText, style);

		rect = new Rect(0, h - (h * 2 / 100), w, h * 2 / 100);
		style.alignment = TextAnchor.LowerRight;
		style.fontSize = h / 50;
		style.normal.textColor = new Color (0.0f, 1.0f, 0.0f, 1.0f);

		string hardwardTxt = GraphicProfiler.GraphicsProfiler._hardwareInfoSB.ToString();
		GUI.Label(rect, hardwardTxt, style);
	}
#endif
}