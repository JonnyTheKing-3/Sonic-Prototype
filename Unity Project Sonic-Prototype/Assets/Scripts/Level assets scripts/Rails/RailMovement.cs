using UnityEngine;
using Unity.Cinemachine;

public class RailMovement : MonoBehaviour
{
    public bool playerIsOnRail;
    public float startSpeed;    // Speed in which player entered the rail
    
    [Header("REFERENCES")]
    public SonicMovement player;
    public CinemachineSplineCart cart;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
        cart = GetComponent<CinemachineSplineCart>();
        playerIsOnRail = false;
    }

   void FixedUpdate()
    {
        if (playerIsOnRail)
        {
            
        }
    }
}
