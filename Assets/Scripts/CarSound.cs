using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    Drivetrain drivetrain;
    AudioSource engineSource;
    // Start is called before the first frame update
    void Start()
    {
        drivetrain = GetComponent<Drivetrain>();
        engineSource = GetComponent<AudioSource>();
        engineSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        engineSource.pitch = drivetrain.engine.RPM / (drivetrain.engine.RPMLimit / 2);
    }
}
