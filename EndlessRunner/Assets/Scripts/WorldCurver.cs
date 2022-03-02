using UnityEngine;

[ExecuteInEditMode]
public class WorldCurver : MonoBehaviour
{
	[Range(-0.1f, 0.1f)]
	public float curveStrengthX = 0.01f;
	[Range(-0.1f, 0.1f)]
	public float curveStrengthY = 0.01f;

    int m_CurveStrengthX_ID;
	int m_CurveStrengthY_ID;

    private void OnEnable()
    {
        m_CurveStrengthX_ID = Shader.PropertyToID("_CurveStrengthX");
		m_CurveStrengthY_ID = Shader.PropertyToID("_CurveStrengthY");
    }

	void Update()
	{
		Shader.SetGlobalFloat(m_CurveStrengthX_ID, curveStrengthX);
		Shader.SetGlobalFloat(m_CurveStrengthY_ID, curveStrengthY);
	}
}
