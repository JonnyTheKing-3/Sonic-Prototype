using Unity.Cinemachine;
using UnityEngine;

public class CameraLoopDeLoop : MonoBehaviour
{
    public LoopDeLoopCart ThisLoopsCart;
    public CinemachineSplineCart cameraLoopCart;

    public bool ActivateCameraLoopPath = false;
    [Tooltip("The vertical offset the camera will end at when leaving the loop in relation to the end speed direction of the loop")] public float afterLoopCamOffset;


    void Start()
    {
        ThisLoopsCart = transform.root.GetComponentInChildren<LoopDeLoopCart>();
        cameraLoopCart = GetComponent<CinemachineSplineCart>();
    }

    void Update()
    {
        
        if (ThisLoopsCart.player.loopInfo == ThisLoopsCart && ThisLoopsCart.OnlyForwardDirection)
        {
            ThisLoopsCart.ActivateCameraLoopPath = true;
            cameraLoopCart.SplinePosition = ThisLoopsCart.cart.SplinePosition;
        }
        else
        {
            ThisLoopsCart.ActivateCameraLoopPath = false;
        }
    }
}
