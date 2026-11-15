//DR Sprite Depth by SeleniumSoul for DR:Distrust
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
    private Matrix4x4[] depthMatrices;

    void Start()
    {
        depthMatrices = new Matrix4x4[Depth];
        UpdateMat(GetComponent<Renderer>().material);
    }

    void LateUpdate()
    {
        if (DepthMesh == null || DepthMaterial == null || !ShowDepth)
            return;

        for (int matriceInt = 0; matriceInt < Depth; matriceInt++)
        {
            Vector3 _pos = transform.position - transform.forward * ((matriceInt + 1) * DepthDistance);
            Quaternion _rotation = transform.rotation;
            Vector3 _scale = transform.lossyScale;

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
