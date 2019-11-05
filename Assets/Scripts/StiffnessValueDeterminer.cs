using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StiffnessValueDeterminer : MonoBehaviour
{
    WheelCollider wheel;
    public TMP_Text wheelRPM;

    void Start()
    {
        wheel = GetComponent<WheelCollider>();
        StartCoroutine(nameof(UpdateWheelRPM));
    }

    void FixedUpdate()
    {


        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            WheelFrictionCurve fFriction = wheel.forwardFriction;
            fFriction.stiffness = hit.collider.material.staticFriction /*- DetecSlip()*/;
            wheel.forwardFriction = fFriction;
            WheelFrictionCurve sFriction = wheel.sidewaysFriction;
            sFriction.stiffness = hit.collider.material.staticFriction /*- DetecSlip()*/;
            wheel.sidewaysFriction = sFriction;
        }
        //DetecSlip();
    }

    IEnumerator UpdateWheelRPM()
    {
        while (true)
        {
            wheelRPM.text = Mathf.Round(wheel.rpm).ToString();
            yield return new WaitForSeconds(1f);
        }
    }

    void DetecSlip()
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            if (hit.forwardSlip > 0.8f)
            {
                Debug.Log("Acceleration Slip");
            }
            if (hit.forwardSlip < -0.8f)
            {
                Debug.Log("Braking Slip");
            }
        }
    }
}
