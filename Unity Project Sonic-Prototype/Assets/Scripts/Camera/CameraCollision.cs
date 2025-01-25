using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public float minDistance = 1.0f;
    public float maxDistance = 1.0f;
    public float smooth = 10f;

    private Vector3 dollyDir;
    public Vector3 dollyDirAdjusted;
    public float distance;
    
    void Start()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    void Update()
    {
        Vector3 desiredCamPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if (Physics.Linecast(transform.parent.position, desiredCamPos, out hit))
        {
            distance = Mathf.Clamp((hit.distance * .87f), minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
    }
}
