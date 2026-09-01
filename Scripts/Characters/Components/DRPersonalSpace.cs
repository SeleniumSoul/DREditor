//DR Personal Space script for DRDistrust by SeleniumSoul
//To be used explicitly inconjunction with DRSpriteDepth

using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DRPersonalSpace : MonoBehaviour
{
    [SerializeField] private float SocialDistance = 2.5f;
    
    private DRSpriteDepth childShadow;
    private SphereCollider sphereCollider;
    
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        childShadow = GetComponent<DRSpriteDepth>();

        sphereCollider.isTrigger = true;
        sphereCollider.radius = SocialDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        childShadow.HideShadow(true);
    }

    private void OnTriggerExit(Collider other)
    {
        childShadow.HideShadow(false);
    }
}
