using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBoostMeterMover : MonoBehaviour
{
    // Update the slider value based on boost meter
    
    
    private Slider slider;
    [SerializeField] private SonicMovement sonic;
    private void Start() { slider = GetComponent<Slider>(); }
    private void Update() { slider.value = sonic.BoostMeter; }
}
