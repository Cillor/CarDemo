using UnityEngine;

public class Drivetrain : MonoBehaviour
{
    public Engine engine;
    public Gearbox gearbox;

    [Header("Engine")]
    public AnimationCurve engineTorque;

    [Space]
    [Header("Brakes")]
    [Range(0, 1)] public float frontBrakeBias;
    public float brakeTorque;

    [Space]
    [Header("Gearbox")]
    public float[] gearRatios;
    public float finalDriveRatio;


    [Space]
    [Header("Wheels")]
    public float maxSteerAngle;
    public GameObject frontWheelShape, rearWheelShape;

    private WheelCollider[] wheels;
    private Rigidbody rb;
    private float steeringWheelInput, acceleratorInput, brakeInput, clutchInput;
    private bool acceleratorPressed, clutchPressed;
    private float wheelRadius;
    public static float carSpeedInMetersPerSecond;
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
        engine.UserInput(acceleratorInput, acceleratorPressed, clutchPressed);
        GearChange();
    }

    private void GetInput()
    {
        steeringWheelInput = Input.GetAxis("SteeringWheel");
        acceleratorInput = Input.GetAxis("Accelerator");

        if (FuelConsumption.fuelInTank < 0)
            acceleratorInput = 0;

        brakeInput = Input.GetAxis("Brake");
        clutchPressed = Input.GetButton("Clutch");

        if (acceleratorInput > 0)
            acceleratorPressed = true;
        else
            acceleratorPressed = false;

        if (clutchPressed)
            clutchInput = 0;
        else
            clutchInput = 1;
    }

    private void GearChange()
    {
        if (Input.GetButtonDown("GearUp"))
        {
            if (!clutchPressed)
            {
                if (Random.value < .4f)
                    return;

                gearbox.damage++;
            }

            gearbox.ShiftUp();
        }

        if (Input.GetButtonDown("GearDown"))
        {
            if (!clutchPressed)
            {
                if (Random.value < .4f)
                    return;

                gearbox.damage++;
            }

            gearbox.ShiftDown();
        }
    }

    private void FixedUpdate()
    {
        Debug.Log(PilotShaftSpeed());
        engine.UpdateEngine(PilotShaftSpeed(), Time.deltaTime);

        SteerWheels();
        BrakeWheels();
        PowerWheels();
        CalculateCarSpeed();
        TractionControl();

        UpdateWheelPoses();
    }
    private float WheelShaftForce()
    {
        return engineTorque.Evaluate(engine.RPM) * gearbox.shaftDriveRatio / wheelRadius * clutchInput;
    }

    private float PilotShaftSpeed()
    {
        float wheelRPM = carSpeedInMetersPerSecond / wheelRadius;
        float pilotShaftRPM = wheelRPM * gearbox.shaftDriveRatio * 60 / 2 * Mathf.PI;

        return pilotShaftRPM;
    }

    void SteerWheels()
    {
        float steeringAngle = steeringWheelInput * maxSteerAngle;

        foreach (WheelCollider wheel in wheels)
        {
            bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
            if (isFrontWheels)
                wheel.steerAngle = steeringAngle;
        }
    }

    void PowerWheels()
    {
        foreach (WheelCollider wheel in wheels)
        {
            bool isBackWheels = wheel.transform.localPosition.z < carMiddlePosition;
            if (isBackWheels)
                wheel.motorTorque = WheelShaftForce() * acceleratorInput;
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