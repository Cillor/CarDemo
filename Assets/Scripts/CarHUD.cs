using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Drivetrain))]
public class CarHUD : MonoBehaviour
{
    public TMP_Text actualGearText, speedText, fuelInTankText, rpmText;
    Drivetrain drivetrain;
    private float minNeedleAng = 90f, maxNeedleAng = -90f;
    public Image engineRPMNeedle;

    void Start()
    {
        drivetrain = GetComponent<Drivetrain>();
    }

    void Update()
    {
        ChangeTexts();
        ChangeRPMNeedle();
    }

    public void ChangeTexts()
    {
        fuelInTankText.text = Helper.Round(FuelConsumption.fuelInTank, 1) + " litros";
        actualGearText.text = (drivetrain.gearbox.actualGear - 1).ToString();
        speedText.text = Helper.Round(Drivetrain.carSpeedInMetersPerSecond * 3.6f, 2) + "km/h";
        rpmText.text = Mathf.Round(drivetrain.engine.RPM).ToString();
    }

    public void ChangeRPMNeedle()
    {
        float angRPM = Mathf.Lerp(minNeedleAng, maxNeedleAng, Mathf.Abs(drivetrain.engine.RPM) / drivetrain.engine.RPMLimit);
        engineRPMNeedle.transform.eulerAngles = new Vector3(0, 0, angRPM);
    }
}
