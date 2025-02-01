using UnityEngine;

public class TimeControl : MonoBehaviour
{
    public float timeScale;
    void Update()
    {
        // Set the time scale to the value of the timeScale variable.
        if (Input.GetKeyDown(KeyCode.P))
        {
            timeScale += .1f;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            timeScale -= .1f;
        }

        Time.timeScale = timeScale;
    }
}