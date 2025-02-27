using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GearboxType { Automatic, Sequential, Manual }
public class Drivetrain : MonoBehaviour
{
    public Engine engine;
    public Gearbox gearbox;

    [Header("Engine")]
    public AnimationCurve engineTorque;
    public int engineBraking = 2;

    public void SetEngineBraking(Slider engineBrakingSlider)
    {
        engineBraking = Mathf.RoundToInt(engineBrakingSlider.value);
    }


    [Space]
    [Header("Brakes")]
    [Range(0, 1)] public float frontBrakeBias;
    public void SetFrontBrakeBias(Slider brakeBiasSlider)
    {
        frontBrakeBias = brakeBiasSlider.value;
        frontBrakeBias = Mathf.Clamp01(frontBrakeBias);
    }
    public float brakeTorque;

    [Space]
    [Header("Gearbox")]
    public float[] gearRatios;
    public float finalDriveRatio;

    [Space]
    [Header("Wheels")]
    public float maxSteerAngle;
    public GameObject frontWheelShape, rearWheelShape;

    [Space]
    [Header("Control Options")]
    public GearboxType gearboxType = GearboxType.Sequential;

    public void SetGearboxType(TMP_Dropdown gearboxTypeDropdown)
    {
        switch (gearboxTypeDropdown.value)
        {
            case 0:
                gearboxType = GearboxType.Manual;
                break;
            case 1:
                gearboxType = GearboxType.Sequential;
                break;
            case 2:
                gearboxType = GearboxType.Automatic;
                hasClutch = false;
                Toggle clutchToggle = GameObject.Find("hasClutch").GetComponent<Toggle>();
                clutchToggle.isOn = false;
                break;
        }
    }
    public bool hasClutch = true;
    public void SetClutch(Toggle clutchToggle)
    {
        hasClutch = clutchToggle.isOn;

        if (gearboxType == GearboxType.Automatic)
        {
            hasClutch = false;
            clutchToggle.isOn = false;
        }
    }

    private WheelCollider[] wheels;
    private Rigidbody rb;
    private float steeringWheelInput, acceleratorInput, brakeInput, clutchInput;
    private bool acceleratorPressed, clutchPressed;
    private float wheelRadius;
    public static float carSpeedInMetersPerSecond;
    private const int carTopSpeed = 86;

    private const float carMiddlePosition = 0;

    private void Start()
    {
        gearbox = new Gearbox(gearRatios, finalDriveRatio);
        engine = new Engine(gearbox);

        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();

        wheelRadius = wheels[3].radius;

        InitializeWheelShapes();
    }

    void InitializeWheelShapes()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (frontWheelShape && rearWheelShape)
            {
                bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
                if (isFrontWheels)
                {
                    GameObject inSceneWheelShape = GameObject.Instantiate(frontWheelShape);
                    inSceneWheelShape.transform.parent = wheel.transform;

                    bool isRightWheel = wheel.transform.localPosition.x > carMiddlePosition;
                    if (isRightWheel)
                        inSceneWheelShape.transform.localScale = new Vector3(inSceneWheelShape.transform.localScale.x * -1f, inSceneWheelShape.transform.localScale.y, inSceneWheelShape.transform.localScale.z);
                }
                else
                {
                    GameObject inSceneWheelShape = GameObject.Instantiate(rearWheelShape);
                    inSceneWheelShape.transform.parent = wheel.transform;
                    bool isRightWheel = wheel.transform.localPosition.x > carMiddlePosition;
                    if (isRightWheel)
                        inSceneWheelShape.transform.localScale = new Vector3(inSceneWheelShape.transform.localScale.x * -1f, inSceneWheelShape.transform.localScale.y, inSceneWheelShape.transform.localScale.z);
                }
            }
        }
    }

    void UpdateWheelPoses()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (frontWheelShape && rearWheelShape)
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

    private void Update()
    {
        GetInput();
        engine.UserInput(acceleratorInput, acceleratorPressed, EngineEngagedWithWheels());
        GearChange();
    }

    private void GetInput()
    {
        steeringWheelInput = Input.GetAxis("SteeringWheel");

        brakeInput = Input.GetAxis("Brake");

        acceleratorInput = Input.GetAxis("Accelerator");
        if (FuelConsumption.fuelInTank < 0)
            acceleratorInput = 0;

        if (acceleratorInput > 0)
            acceleratorPressed = true;
        else
            acceleratorPressed = false;

        if (hasClutch)
        {
            clutchPressed = Input.GetButton("Clutch");
            if (clutchPressed)
                clutchInput = 0;
            else
                clutchInput = 1;
        }
        else
            clutchInput = 1;
    }

    bool canUseAutomaticGear = true;
    void GearChange()
    {
        switch (gearboxType)
        {
            case GearboxType.Automatic:
                if (canUseAutomaticGear)
                {
                    StartCoroutine(nameof(AutomaticGearChange));
                    canUseAutomaticGear = false;
                }
                break;
            case GearboxType.Sequential:
                SequentialGearChange();
                StopCoroutine(nameof(AutomaticGearChange));
                canUseAutomaticGear = true;
                break;
            case GearboxType.Manual:
                StopCoroutine(nameof(AutomaticGearChange));
                canUseAutomaticGear = true;
                ManualGearChange();
                break;
        }
    }

    IEnumerator AutomaticGearChange()
    {
        while (true)
        {
            if (carSpeedInMetersPerSecond < 0.1f)
            {
                if (brakeInput > .9f && gearbox.actualGear == gearbox.firstGear)
                {
                    gearbox.actualGear = gearbox.reverseGear;
                }
                else if (brakeInput > .9f && gearbox.actualGear == gearbox.reverseGear)
                {
                    gearbox.actualGear = gearbox.neutralGear;
                }
            }

            if (engine.RPM > 13000)
                if (gearbox.actualGear > gearbox.reverseGear)
                    gearbox.ShiftUp();
            if (engine.RPM < 9000)
                if (gearbox.actualGear > gearbox.firstGear)
                    gearbox.ShiftDown();

            engine.damage = 0;
            yield return new WaitForSeconds(.5f);
        }
    }

    private void ManualGearChange()
    {
        if (Input.GetButtonDown("ReverseGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.reverseGear;
        }
        if (Input.GetButtonDown("NeutralGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.neutralGear;
        }
        if (Input.GetButtonDown("FirstGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.firstGear;
        }
        if (Input.GetButtonDown("SecondGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.secondGear;
        }
        if (Input.GetButtonDown("ThirdGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.thirdGear;
        }
        if (Input.GetButtonDown("FourthGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.fourthGear;
        }
        if (Input.GetButtonDown("FifthGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.fifthGear;
        }
        if (Input.GetButtonDown("SixthGear"))
        {
            TestClutchPress();
            gearbox.actualGear = gearbox.sixthGear;
        }
    }

    private void SequentialGearChange()
    {
        if (Input.GetButtonDown("GearUp"))
        {
            TestClutchPress();

            gearbox.ShiftUp();
        }

        if (Input.GetButtonDown("GearDown"))
        {
            TestClutchPress();

            gearbox.ShiftDown();
        }
    }

    void TestClutchPress()
    {
        if (hasClutch)
        {
            if (!clutchPressed)
            {
                if (Random.value < .6f)
                    return;

                gearbox.damage++;
            }
        }
    }
    float time = 0;
    private void FixedUpdate()
    {
        engine.UpdateEngine(carSpeedInMetersPerSecond, PilotShaftSpeed(gearbox.actualGear), Time.deltaTime);

        SteerWheels();
        BrakeWheels();
        PowerWheels();
        CalculateCarSpeed();
        TractionControl();

        UpdateWheelPoses();
    }

    private float WheelShaftForce(int desiredGear)
    {
        float shaftDriveRatio = gearbox.gearRatios[desiredGear] * gearbox.finalDriveRatio;
        return engineTorque.Evaluate(engine.GetCarRPM(PilotShaftSpeed(desiredGear))) * shaftDriveRatio / wheelRadius * clutchInput;
    }

    bool EngineEngagedWithWheels()
    {
        if (clutchPressed)
            return false;
        if (gearbox.actualGear == gearbox.neutralGear)
            return false;

        return true;

    }

    private float PilotShaftSpeed(int desiredGear)
    {
        float shaftDriveRatio = gearbox.gearRatios[desiredGear] * gearbox.finalDriveRatio;
        float wheelRPM = carSpeedInMetersPerSecond / wheelRadius;
        float pilotShaftRPM = wheelRPM * shaftDriveRatio * 60 / 2 * Mathf.PI;

        return pilotShaftRPM;
    }

    void SteerWheels()
    {
        float steeringAngle = steeringWheelInput * maxSteerAngle;

        foreach (WheelCollider wheel in wheels)
        {
            bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
            if (isFrontWheels)
                wheel.steerAngle = steeringAngle * Mathf.Clamp((1 - carSpeedInMetersPerSecond / carTopSpeed), .1f, 1);
        }
    }

    void PowerWheels()
    {
        if (LapTimer.countdownStart > 0)
            return;

        foreach (WheelCollider wheel in wheels)
        {
            float carSpeed = carSpeedInMetersPerSecond;
            if (carSpeed < carTopSpeed / engineBraking)
                carSpeed = 0;

            bool isBackWheels = wheel.transform.localPosition.z < carMiddlePosition;
            if (isBackWheels)
                wheel.motorTorque = WheelShaftForce(gearbox.actualGear) * Mathf.Clamp(acceleratorInput, carSpeed / (carTopSpeed * engineBraking), 1);
        }
    }

    void BrakeWheels()
    {
        foreach (WheelCollider wheel in wheels)
        {
            float frontAxleBrakeTorque = brakeTorque * frontBrakeBias;
            float individualFrontBrakeTorque = frontAxleBrakeTorque / 2;
            float rearBrakeBias = 1 - frontBrakeBias;
            float rearAxleBrakeTorque = brakeTorque * rearBrakeBias;
            float individualRearBrakeTorque = rearAxleBrakeTorque / 2;

            bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
            if (isFrontWheels)
            {
                wheel.brakeTorque = individualFrontBrakeTorque * brakeInput;
            }
            else
            {
                wheel.brakeTorque = individualRearBrakeTorque * brakeInput;
            }
        }
    }

    void CalculateCarSpeed()
    {
        float sidewaysSpeed = rb.velocity.x;
        float forwardSpeed = rb.velocity.z;
        carSpeedInMetersPerSecond = Mathf.Sqrt(sidewaysSpeed * sidewaysSpeed + forwardSpeed * forwardSpeed);
    }

    void TractionControl()
    {
        foreach (WheelCollider wheel in wheels)
        {
            float carSpeedInMetersPerMinutes = carSpeedInMetersPerSecond * 60;
            float wheelCircunference = 2 * Mathf.PI * wheel.radius;
            float theoreticalWheelRPM = carSpeedInMetersPerMinutes / wheelCircunference;
            const float wheelRPMValueModifier = 500;
            float acceptableWheelRPM = theoreticalWheelRPM + wheelRPMValueModifier;

            bool isWheelRPMHigherThanAcceptableRPM = Mathf.Abs(wheel.rpm) > acceptableWheelRPM;
            if (isWheelRPMHigherThanAcceptableRPM)
                wheel.motorTorque = 0;
        }
    }
}