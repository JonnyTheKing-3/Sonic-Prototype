using UnityEngine;

public class FootStepSource : MonoBehaviour
{
    private SonicMovement player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
    }

    void OnTriggerEnter(Collider other) 
    {
        // if the player touched the ground, determined the material, and play a footstep sound based on the material
        if ((player.whatIsGround.value & (1 << other.gameObject.layer)) != 0)
        {
            
            float groundMaterial = 0;

            // LAYERS                                   MATERIALS
            // 3 = ground, 8 = loopdeloop, 9 = grass, 10 = dirt         1 = stone, 0 = grass, 2 = dirt
            switch (other.gameObject.layer)
            {
                case 3:
                case 8: groundMaterial = 1f; break;
                case 9: groundMaterial = 0f; break;
                case 10: groundMaterial = 2f; break;
            }

            FMODbanks.Instance.PlayFootStepSFX(gameObject, groundMaterial);
        }
    }
}
