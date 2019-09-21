using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    CarController carControllerScript;
    AudioSource engineSource;
    // Start is called before the first frame update
    void Start()
    {
        carControllerScript = GetComponent<CarController>();
        engineSource = GetComponent<AudioSource>();
        engineSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        engineSource.pitch = CarController.engineRPM / (carControllerScript.engineRPMLimit / 2);
    }
}
