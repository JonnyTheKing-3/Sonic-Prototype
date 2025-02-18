using UnityEngine;
using UnityEngine.VFX;

public class BlueTrail : MonoBehaviour
{
    public SonicMovement player;
    public Vector3 offset;
    public VisualEffect vfx;
    
    void Start()
    {
        player = GetComponentInParent<SonicMovement>();
        vfx = GetComponent<VisualEffect>();
    }

    void Update()
    {
        transform.position = player.transform.position + offset;
        vfx.SetVector3("TrailDirection", -player.LastSpeedDirection);
    }
}
