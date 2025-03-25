using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class FMODbanks : MonoBehaviour
{
    public static FMODbanks Instance { get; private set; }

    [SerializeField] public bool playMusicOnStart;
    [SerializeField] public bool playLevelMusicOnStart;
    [SerializeField] public bool playAmbienceOnStart;

    [Header("SFX")]

    [Header("Player")]
    public EventReference FootStepsSFX; // Works good
    public EventReference jumpSFX;  // Works good
    public EventReference homingAttackSFX; // Works 
    public EventReference homingLockOnSFX; // Not iplemented but here if anyone wants to use it :) Currently being used for when touching a ring because I don't have that sound right now
    public EventReference SpinDashSFX; // Works
    [Space]
    public EventReference SpinChargeSFX;  // Works good
    public EventReference SlideSFX; // Works good
    public EventReference PowerBoostSFX; // Works good


    [Header("Music/Ambience/Environment")]
    public EventReference Music; // Not implemented yet, but works with other event, so it's good
    public EventReference LevelMusic; // Using the Regular song right now, but it's here in case someone wants to make a cool track :)
    public EventReference Ambience; // Works Good

    [Header("Level Objects")]
    public EventReference BumperSFX;   // Works Good
    public EventReference GrindRailSFX; // Works good
    public EventReference DashPanelSFX; // Not implemented yet, but works with other event, so it's good


    private EventInstance footstepInstance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(this.gameObject);
        }

        if (playMusicOnStart) { PlayMusic();}
        if (playLevelMusicOnStart) { PlayLevelMusic();}
        if (playAmbienceOnStart) { PlayAmbience();}


        footstepInstance = RuntimeManager.CreateInstance(FootStepsSFX);

    }
    
    public void PlayFootStepSFX(GameObject OriginOfSound, float material)
    {
        footstepInstance.setParameterByName("Material", material);
        footstepInstance.start();
    }
    
    
    public void PlayJumpSFX(GameObject OriginOfSound)
    {
        RuntimeManager.PlayOneShotAttached(jumpSFX, OriginOfSound);
    }
    public void PlayHomingAttackSFX(GameObject OriginOfSound)
    {
        RuntimeManager.PlayOneShotAttached(homingAttackSFX, OriginOfSound);
    }
    public void PlayHomingLockOnSFX(GameObject OriginOfSound)
    {
        RuntimeManager.PlayOneShotAttached(homingLockOnSFX, OriginOfSound);
    }
    public void PlaySpinDashSFX(GameObject OriginOfSound)
    {
        RuntimeManager.PlayOneShotAttached(SpinDashSFX, OriginOfSound);
    }
    public void PlayBumperSFX(GameObject OriginOfSound)
    {
        RuntimeManager.PlayOneShotAttached(BumperSFX, OriginOfSound);
    }
    public void PlayDashPanelSFX(GameObject OriginOfSound)
    {
        RuntimeManager.PlayOneShotAttached(DashPanelSFX, OriginOfSound);
    }


    public static EventInstance SpinChargeInstance;
    public void StartSpinChargeSFX()
    {
        // Create the EventInstance for hover sound and start it
        SpinChargeInstance = RuntimeManager.CreateInstance(SpinChargeSFX);
        SpinChargeInstance.start();
    }
    public void StopSpinChargeSFX()
    {
        // If the hover sound is playing, stop it, and release the instance
        if (SpinChargeInstance.isValid())
        {
            SpinChargeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            SpinChargeInstance.release();
            SpinChargeInstance.clearHandle();
        }
    }

    public static EventInstance SlideInstance;
    public void StartSlideSFX()
    {
        // Create the EventInstance for hover sound and start it
        SlideInstance = RuntimeManager.CreateInstance(SlideSFX);
        SlideInstance.start();
    }
    public void StopSlideSFX()
    {
        // If the hover sound is playing, stop it, and release the instance
        if (SlideInstance.isValid())
        {
            SlideInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            SlideInstance.release();
            SlideInstance.clearHandle();
        }
    }

    public static EventInstance PowerBoostInstance;
    public void StartBoostSFX()
    {
        // Create the EventInstance for hover sound and start it
        PowerBoostInstance = RuntimeManager.CreateInstance(PowerBoostSFX);
        PowerBoostInstance.start();
    }
    public void StopBoostSFX()
    {
        // Used tbis way instead of regular fade out to prevent 10 second DSP warning
        if (PowerBoostInstance.isValid())
        {
            StartCoroutine(FadeOutAndStop(PowerBoostInstance, .3f));
        }
    }
    public IEnumerator FadeOutAndStop(EventInstance instance, float fadeDuration)
    {
        float startVolume;
        instance.getVolume(out startVolume);
        float timeElapsed = 0f;
        while(timeElapsed < fadeDuration)
        {
             float newVolume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeDuration);
             instance.setVolume(newVolume);
             timeElapsed += Time.deltaTime;
             yield return null;
        }
        // Ensure volume is zero
        instance.setVolume(0f);
        // Stop immediately and release resources
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
        instance.clearHandle();
    }


    public static EventInstance grindrailInstance;
    public void StartGrindRailSFX()
    {
        // Create the EventInstance for hover sound and start it
        grindrailInstance = RuntimeManager.CreateInstance(GrindRailSFX);
        grindrailInstance.start();
    }
    public void StopGrindRailSFX()
    {
        // If the hover sound is playing, stop it, and release the instance
        if (grindrailInstance.isValid())
        {
            grindrailInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            grindrailInstance.release();
            grindrailInstance.clearHandle();
        }
    }



    public static EventInstance musicInstance;
    public void PlayMusic()
    {
        // Create the EventInstance for hover sound and start it
        musicInstance = RuntimeManager.CreateInstance(Music);
        musicInstance.start();
    }

    public static EventInstance levelmusicInstance;
    public void PlayLevelMusic()
    {
        // Create the EventInstance for hover sound and start it
        levelmusicInstance = RuntimeManager.CreateInstance(LevelMusic);
        levelmusicInstance.start();
    }

    public static EventInstance ambienceInstance;
    public void PlayAmbience()
    {
        // Create the EventInstance for hover sound and start it
        ambienceInstance = RuntimeManager.CreateInstance(Ambience);
        ambienceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        ambienceInstance.start();
    }

    public void OnSceneSwitch()
    {
        StopBoostSFX();
        SpinChargeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        SpinChargeInstance.release();
    
        SlideInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        SlideInstance.release();

        PowerBoostInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        PowerBoostInstance.release();

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();

        levelmusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        levelmusicInstance.release();

        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();

        footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        footstepInstance.release();
    }
    
    public void OnDestroy()
    {
        // Make sure to release the instance when it's no longer needed
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();

        levelmusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        levelmusicInstance.release();

    }
}
