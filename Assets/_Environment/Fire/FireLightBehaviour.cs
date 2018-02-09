using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This script is used to simulate the flickering of a fire/flame.
/// FireLightBehaviour script that makes a light flicker between two intensities.
/// - Set the two different intensities and ranges the light should vary between.
/// - Set the time interval min and max. A random value is chosen between these that makes the flicker time vary.
/// </summary>
public class FireLightBehaviour : MonoBehaviour
{
    // Light intensity.
    public float IntensityMin;
    public float IntensityMax;

    // Light range.
    public float RangeMin;
    public float RangeMax;

    // Minimum flicker time.
    public float FlickerTimeMin;

    // Maximum flicker time.
    public float FlickerTimeMax;

    // Current time to flicker. (set randomly between flicker time min and max)
    private float _currentFlickerTime;

    // Flicker time passed.
    private float _timeFlickered = 0;

    // Holds if light is flickering on or off.
    private bool _isFlickeringOn = true;

    // Use this for initialization
    void Start()
    {
        CalculateFlickerTime();
    }

    // Update is called once per frame
    void Update()
    {
        // Update flicker time.
        if (_isFlickeringOn)
        {
            _timeFlickered += Time.deltaTime;
            if (_timeFlickered >= _currentFlickerTime)
            {
                CalculateFlickerTime();
                _timeFlickered = _currentFlickerTime;
                _isFlickeringOn = false;
            }
        }
        else
        {
            _timeFlickered -= Time.deltaTime;
            if (_timeFlickered <= 0)
            {
                CalculateFlickerTime();
                _timeFlickered = 0;
                _isFlickeringOn = true;
            }
        }

        float lightLerp = _timeFlickered / _currentFlickerTime;

        GetComponent<Light>().intensity = Mathf.Lerp(IntensityMin, IntensityMax, lightLerp);
        GetComponent<Light>().range = Mathf.Lerp(RangeMin, RangeMax, lightLerp);
    }

    // Sets a new random flicker time between the min and max flicker time.
    private void CalculateFlickerTime()
    {
        _currentFlickerTime = Random.Range(FlickerTimeMin, FlickerTimeMax);

        //Debug.Log("--- Flicker time: " + _currentFlickerTime + " ---");
    }
}
