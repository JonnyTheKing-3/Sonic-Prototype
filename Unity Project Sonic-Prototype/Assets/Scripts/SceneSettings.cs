using UnityEngine;

public class TimeControl : MonoBehaviour
{
    void Update()
    {
        // Set the time scale to the value of the timeScale variable.
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = .05f;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Time.timeScale = .1f;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Time.timeScale = 1;
        }
    }
}