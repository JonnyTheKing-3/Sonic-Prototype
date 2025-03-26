using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{
    void Update()
    {
        // Loads sample scene (Asset scene)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FMODbanks.Instance.OnSceneSwitch();
            SceneManager.LoadScene(0);
        }

        // Loads level
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FMODbanks.Instance.OnSceneSwitch();
            SceneManager.LoadScene(1);
        }

        // Loads forest
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FMODbanks.Instance.OnSceneSwitch();
            SceneManager.LoadScene(2);
        }
    }
}
