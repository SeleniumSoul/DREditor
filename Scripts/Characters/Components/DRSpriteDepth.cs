//DR Sprite Depth by SeleniumSoul for DR:Distrust
//Note: This SHOULD be a shader script. Until I figure out how to use shaders, this will be stuck as just a monoscript.

using UnityEngine;

public class DRSpriteDepth : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private int Depth;
    [SerializeField] private float DepthDistance = 0.001f;
    [Header("Assets")]
    [SerializeField] private Mesh DepthMesh;
    [SerializeField] private Material DepthMaterial;

    private bool ShowDepth = true;
    private GameObject[] ShadowObjects;
    private Matrix4x4[] depthMatrices;

    void Start() => UpdateMat(GetComponent<Renderer>().material);

    void Update()
    {
        if (DepthMesh == null || DepthMaterial == null || !ShowDepth) return;

        depthMatrices = new Matrix4x4[Depth];

        for (int matriceInt = 0; matriceInt < Depth; matriceInt++)
        {
            Vector3 _pos = transform.position - transform.forward * ((matriceInt + 1) *  DepthDistance);
            Quaternion _rotation = transform.rotation;
            Vector3 _scale = transform.localScale;

            depthMatrices[matriceInt] = Matrix4x4.TRS(_pos, _rotation, _scale);
        }

        Graphics.DrawMeshInstanced(DepthMesh, 0, DepthMaterial, depthMatrices, depthMatrices.Length, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, gameObject.layer);
    }

    public void UpdateMat(Material face)
    {
        DepthMaterial.mainTexture = face.mainTexture;
    }

    public void HideShadow(bool toggle)
    {
        ShowDepth = toggle;
    }
}
