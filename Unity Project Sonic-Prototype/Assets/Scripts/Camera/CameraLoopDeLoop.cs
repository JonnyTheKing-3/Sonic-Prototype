using Unity.Cinemachine;
using UnityEngine;

public class CameraLoopDeLoop : MonoBehaviour
{
    public LoopDeLoopCart ThisLoopsCart;
    public CinemachineSplineCart cameraLoopCart;

    public bool ActivateCameraLoopPath = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ThisLoopsCart = transform.root.GetComponentInChildren<LoopDeLoopCart>();
        cameraLoopCart = GetComponent<CinemachineSplineCart>();
    }

    // Update is called once per frame
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
