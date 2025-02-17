using UnityEngine;
using Unity.Cinemachine;

public class RailMovement : MonoBehaviour
{
    public SonicMovement player;
    public bool playerIsOnRail;
    public CinemachineSplineCart cart;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
        cart = GetComponent<CinemachineSplineCart>();
        playerIsOnRail = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerIsOnRail)
        {
            
        }
    }
}
