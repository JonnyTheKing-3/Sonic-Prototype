using UnityEngine;

public class StickToObject : MonoBehaviour
{
    public Transform StickTo;
    void Update()
    {
        transform.position = StickTo.position;
    }
}
