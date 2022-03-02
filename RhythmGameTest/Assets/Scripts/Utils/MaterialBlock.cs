

using UnityEngine;

// https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html
// http://thomasmountainborn.com/2016/05/25/materialpropertyblocks/
[RequireComponent(typeof(Renderer))]
public class MaterialBlock : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private Color _originColor;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    void Start()
    {
        _originColor = _renderer.material.GetColor("_Color");
    }
    public Color SetColor(Color c)
    {
        Color old = _renderer.material.GetColor("_Color");

        _propBlock.SetColor("_Color", c);
        _renderer.SetPropertyBlock(_propBlock);

        return old;
    }

    public void ResetColor()
    {
        SetColor(_originColor);
    }
}