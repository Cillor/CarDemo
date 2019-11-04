using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StiffnessValueDeterminer : MonoBehaviour
{
    WheelCollider wheel;

    void Start()
    {
        wheel = GetComponent<WheelCollider>();
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
