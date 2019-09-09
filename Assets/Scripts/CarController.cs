using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TractionType { AWD, RWD, FWD }

public class CarController : MonoBehaviour
{
    public static bool isHandbrakePressed;

    private WheelCollider[] wheels;
    private float m_steeringWheelInput, m_acceleratorInput, m_steeringAngle, m_brakeInput, m_handBrakeInput;
    private int actualGear;
    public float engineRPM;
    private float wheelRPM;
    private float carSpeed;
    private Rigidbody rb;
    private float wTotalR, wRollingC;
    private float transmissionEfficiency = 0.7f;
    private float outputTorque, outputEngineForce;
    private float rollingResistence, drag, rrConstant, dragMultiplier;
    private bool slowingDown;
    private float engineRPMLimiter;

    private float slipForwardFriction = 0.3f, slipSidewayFriction = 0.42f;

    public float maxSteerAngle = 30;
    public GameObject wheelShape;
    private float revolutionsFactor = 60;
    public Text speedText;

    [Space]
    public float dragConstant;

    [Space]
    public AnimationCurve engineTorque;
    public float engineIdle, engineRPMTorqueCurveEnd;
    public float brakeTorque, handBrakeTorque, slowingDownTorque;
    public float finalDriveRatio;
    public float[] gearRatio;
    public Text engineRPMText, actualGearText;
    public Image engineRPMNeedle;
    private float minNeedleAng = 90f, maxNeedleAng = -90f;

    [Space]
    public TractionType tractionType;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();
        wTotalR = wheels[0].radius;
        wRollingC = wTotalR * 2f * Mathf.PI;
        actualGear = 1;
        rrConstant = 30 * dragConstant;

        for (int i = 0; i < wheels.Length; ++i)
        {
            var wheel = wheels[i];

            if (wheelShape != null)
            {
                var ws = GameObject.Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;

                if (wheel.transform.localPosition.x < 0f)
                    ws.transform.localScale = new Vector3(ws.transform.localScale.x * -1f, ws.transform.localScale.y, ws.transform.localScale.z);
            }
        }

        foreach (WheelCollider wheel in wheels)
        {
            wheel.brakeTorque = 0;
        }
    }

    public void Update()
    {
        GetInput();
    }

    public void GetInput()
    {
        GearChange();

        m_steeringWheelInput = Input.GetAxis("SteeringWheel");
        m_acceleratorInput = Input.GetAxis("Accelerator");
        m_brakeInput = Input.GetAxis("Brake");
    }

    public void Steer()
    {
        m_steeringAngle = m_steeringWheelInput * maxSteerAngle;

        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = m_steeringAngle;
        }
    }

    public void Accelerate()
    {
        if (LapTimer.countdownStart > 0)
            return;
        switch (tractionType)
        {
            case TractionType.AWD:
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = outputTorque;
                }
                break;
            case TractionType.RWD:
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel.transform.localPosition.z < 0)
                        wheel.motorTorque = outputTorque;
                }
                break;
            case TractionType.FWD:
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel.transform.localPosition.z > 0)
                        wheel.motorTorque = outputTorque;
                }
                break;
        }

        carSpeed = rb.velocity.magnitude;
        speedText.text = Helper.Round(carSpeed * 3.6f, 2) + "km/h";
    }

    public void Brake()
    {
        if (Input.GetButton("HandBrake"))
        {
            isHandbrakePressed = true;
            foreach (WheelCollider wheel in wheels)
            {
                if (wheel.transform.localPosition.z < 0)
                {
                    WheelFrictionCurve fFriction = wheel.forwardFriction;
                    fFriction.stiffness = slipForwardFriction;
                    wheel.forwardFriction = fFriction;
                    WheelFrictionCurve sFriction = wheel.sidewaysFriction;
                    sFriction.stiffness = slipSidewayFriction;
                    wheel.sidewaysFriction = sFriction;

                    wheel.brakeTorque = handBrakeTorque;
                    wheel.motorTorque = 0;
                }
            }
        }
        else
        {
            isHandbrakePressed = false;
            foreach (WheelCollider wheel in wheels)
            {
                if (!slowingDown)
                    wheel.brakeTorque = brakeTorque * m_brakeInput;
            }
        }
    }

    public void GearChange()
    {
        if (Input.GetButtonDown("GearUp"))
        {
            if (actualGear == 0 && carSpeed < 0.1f)
                actualGear++;
            else if (actualGear < gearRatio.Length - 1 && actualGear > 0)
                actualGear++;
        }
        if (Input.GetButtonDown("GearDown"))
        {
            if (actualGear == 1 && carSpeed < 0.1f)
                actualGear--;
            else if (actualGear > 1)
                actualGear--;
        }

        actualGearText.text = (actualGear - 1).ToString();
    }

    public void GearBox()
    {
        wheelRPM = carSpeed / wTotalR;
        engineRPM = wheelRPM * gearRatio[actualGear] * finalDriveRatio * revolutionsFactor / 2 * Mathf.PI;
        engineRPM *= Mathf.Lerp(.2f, 1f, Input.GetAxisRaw("Accelerator"));
        if (Mathf.Abs(engineRPM) < engineIdle && (actualGear == 2 || actualGear == 0))
        {
            engineRPM = engineIdle;
        }

        if (m_steeringWheelInput == 0)
        {
            if ((Mathf.Abs(engineRPM) > engineRPMTorqueCurveEnd || engineRPM == 0) || (m_acceleratorInput <= 0.1f && carSpeed > 1f))
            {
                outputEngineForce = 0;
                if (m_brakeInput == 0)
                {
                    slowingDown = true;
                    foreach (WheelCollider wheel in wheels)
                    {
                        float brakeMultiplierRPM = engineRPM;
                        if (brakeMultiplierRPM == 0)
                            brakeMultiplierRPM = 14000;
                        wheel.brakeTorque = slowingDownTorque * Mathf.Pow(2, Mathf.Abs(brakeMultiplierRPM) / engineRPMTorqueCurveEnd);
                    }
                }
                else
                {
                    slowingDown = false;
                }
            }
            else
            {
                slowingDown = false;
                outputEngineForce = engineTorque.Evaluate(Mathf.Abs(engineRPM)) * (gearRatio[actualGear] * finalDriveRatio) * transmissionEfficiency / wTotalR * m_acceleratorInput;
            }
        }
        else
        {
            slowingDown = false;
            outputEngineForce = engineTorque.Evaluate(Mathf.Abs(engineRPM)) * (gearRatio[actualGear] * finalDriveRatio) * transmissionEfficiency / wTotalR * m_acceleratorInput;
        }


        drag = dragConstant * carSpeed * carSpeed;
        rollingResistence = rrConstant * carSpeed;
        if (carSpeed < 0.1f)
        {
            drag = 0;
            rollingResistence = 0;
        }
        outputTorque = outputEngineForce - ((drag + rollingResistence) * Mathf.Sign(wheels[0].rpm));
        //Debug.Log("EngineRPM: " + engineRPM + "/ WheelRPM: " + wheels[0].rpm);

        float angRPM = Mathf.Lerp(minNeedleAng, maxNeedleAng, Mathf.Abs(engineRPM) / engineRPMTorqueCurveEnd);
        engineRPMNeedle.transform.eulerAngles = new Vector3(0, 0, angRPM);
        engineRPMText.text = Helper.Round(engineRPM, 0).ToString();

        foreach (WheelCollider wheel in wheels)
        {
            //Debug.Log("Wheel Torque: " + wheel.motorTorque + "/ Wheel Brake: " + wheel.brakeTorque);
        }
    }

    public void UpdateWheelPoses()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (wheelShape)
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
            }
        }
    }

    private void FixedUpdate()
    {
        Steer();
        GearBox();
        Accelerate();
        UpdateWheelPoses();
        Brake();
    }
}