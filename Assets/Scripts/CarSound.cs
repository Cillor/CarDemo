using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    public CarController carControllerScript;
    public AudioSource engineSource;
    // Start is called before the first frame update
    void Start()
    {
        engineSource = GetComponent<AudioSource> ();
        engineSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        engineSource.pitch = carControllerScript.engineRPM/(carControllerScript.engineRPMTorqueCurveEnd/2);
        //engineSource.pitch += 0.001f;
    }
}
