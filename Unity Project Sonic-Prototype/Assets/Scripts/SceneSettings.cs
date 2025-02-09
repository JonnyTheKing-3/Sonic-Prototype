using UnityEngine;

public class TimeControl : MonoBehaviour
{
    public float timeScale;
    public KeyCode IncreaseTimer;
    public KeyCode DecreaseTimer;
    void Update()
    {
        // Set the time scale to the value of the timeScale variable.
        if (Input.GetKeyDown(IncreaseTimer))
        {
            timeScale += .1f;
        }
        if (Input.GetKeyDown(DecreaseTimer))
        {
            timeScale -= .1f;
        }

        Time.timeScale = timeScale;
    }
}