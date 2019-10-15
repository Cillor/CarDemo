using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelConsumption : MonoBehaviour
{
    public static float fuelInTank;
    private float fuelMetering, acceleratorInput;
    private float litersConsumptionPerSecond;
    public int cylindersQuantity = 6, rpmPerInjection = 3;
    public float tankCapacityInLiters = 90, topSpeed = 75, desiredConsumption = 600;

    // Start is called before the first frame update
    void Start()
    {
        fuelInTank = tankCapacityInLiters;
    }

    // Update is called once per frame
    void Update()
    {
        acceleratorInput = Input.GetAxis("Accelerator");
        acceleratorInput = Mathf.Clamp(.1f, 1f, acceleratorInput);
        fuelMetering = (rpmPerInjection * 1000 * 60 * 825 * topSpeed) / (cylindersQuantity * CarController.engineRPM * desiredConsumption);
        fuelMetering *= acceleratorInput;

        if (CarController.engineRPM == 0)
            litersConsumptionPerSecond = 0;
        else
            litersConsumptionPerSecond = (cylindersQuantity * CarController.engineRPM * fuelMetering) / (rpmPerInjection * 1000 * 60 * 825) + 0.00001f;

        if (fuelInTank > 0)
            fuelInTank -= (litersConsumptionPerSecond * Time.deltaTime);

        //Debug.Log(fuelInTank);
    }
}
