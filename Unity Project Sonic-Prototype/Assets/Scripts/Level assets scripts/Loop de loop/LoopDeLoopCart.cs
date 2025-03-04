using Unity.Cinemachine;
using UnityEngine.Splines;
using UnityEngine;
using System.Data.Common;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class LoopDeLoopCart : MonoBehaviour
{

    [SerializeField, Min(0)] public int cartIterations; // higher = accuracy in landing on rail, lower = performance
    [SerializeField, Range(0.000000001f,1)] public float roughIterations;

    [Tooltip("Turn this on if you want the Loop-de-loop to be traversable in only one direction. Keep in mind that if this is off, the direction for the loop de loop will be based on the camera angle")]
    public bool OnlyForwardDirection = false;
    public float AngleLimitForTangentSwitching = 40f;

    private Vector3 lastVel = Vector3.one;

    private CinemachineSplineCart cart;
    private SplineContainer railPath;
    private SonicMovement player;
    private Camera cam;

    void Start()
    {
        cart = GetComponent<CinemachineSplineCart>();
        railPath = transform.parent.GetComponentInChildren<SplineContainer>();
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
        cam = Camera.main;
    }

    private int lastScalar;
    void Update()
    {
        // Only do any of this if the player is on THIS loop. 
        if (player.loopInfo != this) { return; }

        // Move cart to where player is
        float newCartPos = GetClosestPointOnTrack(player.transform.position);
        cart.SplinePosition = newCartPos;
        
        // Get the tangent, which is going to be the forward for the movement
        Vector3 positionVector = cart.Spline.EvaluateTangent(cart.SplinePosition);
        positionVector.Normalize();

        float angle = Vector3.Angle(player.transform.up, Vector3.up);
        // Debug.Log(angle);

        // only change the tangent before we start looping. The tangent will make a full 360, so if we don't have this check, 
        // when the player is reaching the top of the loop de loop, the tangent will change because it's in opposite direction of where it began
        if (!OnlyForwardDirection)
        {
            if (angle < AngleLimitForTangentSwitching)
            {
                // Check in which direction they are going in and make tangent match that based on camera position
                Vector3 cameraAngle = transform.position - cam.transform.position;
                float dot = Vector3.Dot(cameraAngle, positionVector);
                positionVector *= dot > 0f ? 1 : -1;
                lastScalar = dot > 0f ? 1 : -1;

                Debug.DrawRay(transform.position + transform.up, positionVector *3f, Color.green);
            }
            else 
            {
                positionVector *= lastScalar;
                Debug.DrawRay(transform.position + transform.up, positionVector *3f, Color.red);
            }
        }
        else 
        {
            Debug.DrawRay(transform.position + transform.up, positionVector *3f, Color.red);
        }

        // Keep the orientation of the player at the tangent of the loop position (the first child of the player holder is the orientation)
        player.transform.parent.GetChild(0).transform.forward = positionVector;

        lastVel = player.rb.linearVelocity != Vector3.zero ? player.rb.linearVelocity.normalized : lastVel;
    }

    private float GetClosestPointOnTrack(Vector3 position)
    {
        cart.PositionUnits = PathIndexUnit.Normalized; 

        float roughStep = roughIterations; // Rough initial search step size
        float closestPoint = 0f;
        float closestDistance = Mathf.Infinity;

        // **1. Rough Search** (Quickly find an approximate closest point)
        for (float i = 0f; i <= 1f; i += roughStep)
        {
            Vector3 pointOnSpline = railPath.EvaluatePosition(i);
            float distance = Vector3.Distance(position, pointOnSpline);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = i;
            }
        }

        // **2. Binary Search for Precision**
        float left = Mathf.Max(closestPoint - roughStep, 0f);
        float right = Mathf.Min(closestPoint + roughStep, 1f);
    
        int iterations = 20; // Binary search refinement iterations

        for (int i = 0; i < iterations; i++)
        {
            float mid1 = left + (right - left) / 3f;
            float mid2 = right - (right - left) / 3f;

            Vector3 pos1 = railPath.EvaluatePosition(mid1);
            Vector3 pos2 = railPath.EvaluatePosition(mid2);

            float dist1 = Vector3.Distance(position, pos1);
            float dist2 = Vector3.Distance(position, pos2);

            if (dist1 < dist2)
            {
                right = mid2;
            }
            else
            {
                left = mid1;
            }

            if (dist1 < closestDistance)
            {
                closestDistance = dist1;
                closestPoint = mid1;
            }
            if (dist2 < closestDistance)
            {
                closestDistance = dist2;
                closestPoint = mid2;
            }
        }

        return closestPoint;
    }

}
