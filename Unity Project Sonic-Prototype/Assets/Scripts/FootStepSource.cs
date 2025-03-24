using UnityEngine;

public class FootStepSource : MonoBehaviour
{
    private SonicMovement player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
    }

    void OnTriggerEnter(Collider other) {if (other.gameObject.layer == 3) { FMODbanks.Instance.PlayFootStepSFX(gameObject);}}
}
