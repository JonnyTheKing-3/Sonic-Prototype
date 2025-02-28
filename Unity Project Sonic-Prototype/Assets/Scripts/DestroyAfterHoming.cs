using UnityEngine;

public class DestroyAfterHoming : MonoBehaviour
{
    public void DestroyTarget()
    {
        GameObject.Destroy(transform.root.gameObject);
    }
}
