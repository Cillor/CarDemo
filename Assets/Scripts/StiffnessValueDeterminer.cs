using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StiffnessValueDeterminer : MonoBehaviour
{
    WheelCollider wheel;
    public TMP_Text wheelRPM;
    public TMP_Text wheelSpin;
    public Image tireLifeImage;

    float tireLife = 1;
    const float forwardTireDegradationConstant = 8;
    const float sidewaysTireDegradationConstant = 100;

    AudioSource audioSource;

    void Start()
    {
        wheel = GetComponent<WheelCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        wheelRPM.text = Mathf.Round(wheel.rpm).ToString();
        DetecSlip();

        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            WheelFrictionCurve fFriction = wheel.forwardFriction;
            fFriction.stiffness = hit.collider.material.staticFriction * tireLife;
            wheel.forwardFriction = fFriction;
            WheelFrictionCurve sFriction = wheel.sidewaysFriction;
            sFriction.stiffness = hit.collider.material.staticFriction * tireLife;
            wheel.sidewaysFriction = sFriction;
        }
    }

    void DetecSlip()
    {
        if (Mathf.Abs(wheel.rpm) < 0 && Drivetrain.carSpeedInMetersPerSecond < .05f)
        {
            //SMOKE EFFECT
        }

        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            float absoluteForwardSlip = Mathf.Abs(hit.forwardSlip);
            float absoluteSidewaysSlip = Mathf.Abs(hit.sidewaysSlip);
            float forwardDegradetion = (absoluteForwardSlip / forwardTireDegradationConstant);
            float sidewaysDegradetion = (absoluteSidewaysSlip / sidewaysTireDegradationConstant);
            tireLife -= ((sidewaysDegradetion + forwardDegradetion) * Time.deltaTime) / 100;

            Mathf.Clamp01(tireLife);

            tireLifeImage.fillAmount = tireLife;

            wheelSpin.text = Helper.Round(hit.forwardSlip, 2).ToString();
            if (Mathf.Abs(hit.sidewaysSlip) > 0.8f)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
                audioSource.pitch = Mathf.Clamp(Mathf.Abs(hit.sidewaysSlip), .8f, 1.2f);
            }
            else if (Mathf.Abs(hit.forwardSlip) > 0.8f)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
                audioSource.pitch = Mathf.Clamp(Mathf.Abs(hit.forwardSlip), .8f, 1.2f);
            }
            else
            {
                audioSource.Stop();
            }
        }
    }
}
