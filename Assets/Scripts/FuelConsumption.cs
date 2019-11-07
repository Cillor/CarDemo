using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelConsumption : MonoBehaviour
{
    public static float fuelInTank;
    private float fuelMetering, acceleratorInput;
    private float litersConsumptionPerSecond;
    public int cylindersQuantity = 6, rpmPerInjection = 3;
    public float tankCapacityInLiters = 90, topSpeedInMetersPerSecond = 84, desiredConsumptionInMetersPerLiter = 700;

    [Space]
    public float fuelDensityInKgPerLiter = 0.73f;
    Drivetrain drivetrain;

    Rigidbody rb;
    float carMass;

    // Start is called before the first frame update
    void Start()
    {
        drivetrain = GetComponent<Drivetrain>();
        fuelInTank = tankCapacityInLiters;
        rb = GetComponent<Rigidbody>();
        carMass = rb.mass;
    }

    // Update is called once per frame
    void Update()
    {
        acceleratorInput = Input.GetAxis("Accelerator");
        acceleratorInput = Mathf.Clamp(.1f, 1f, acceleratorInput);
        fuelMetering = (rpmPerInjection * 1000 * 60 * 825 * topSpeedInMetersPerSecond) / (cylindersQuantity * drivetrain.engine.RPM * desiredConsumptionInMetersPerLiter);
        fuelMetering *= acceleratorInput;

        if (drivetrain.engine.RPM == 0)
            litersConsumptionPerSecond = 0;
        else
            litersConsumptionPerSecond = (cylindersQuantity * drivetrain.engine.RPM * fuelMetering) / (rpmPerInjection * 1000 * 60 * 825) + 0.00001f;

        if (fuelInTank > 0)
            fuelInTank -= (litersConsumptionPerSecond * Time.deltaTime);

        rb.mass = carMass + (fuelInTank * fuelDensityInKgPerLiter);
        //Debug.Log(fuelInTank);
    }
}
