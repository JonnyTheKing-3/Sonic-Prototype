using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationPositionUpdater : MonoBehaviour
{
    public Transform player;
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        transform.position = player.position;
    }
}
