﻿using System.Collections;
using UnityEngine;

public class RearWheelDrive : MonoBehaviour {

	private WheelCollider[] wheels;

	public float maxAngle = 30;
	public float maxTorque = 300;
	public float maxBrake = 300;
	public GameObject wheelShape;

	// here we find all the WheelColliders down in the hierarchy
	public void Start () {
		wheels = GetComponentsInChildren<WheelCollider> ();

		for (int i = 0; i < wheels.Length; ++i) {
			var wheel = wheels[i];

			// create wheel shapes only when needed
			if (wheelShape != null) {
				var ws = GameObject.Instantiate (wheelShape);
				ws.transform.parent = wheel.transform;
			}
		}
	}

	// this is a really simple approach to updating wheels
	// here we simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero
	// this helps us to figure our which wheels are front ones and which are rear
	public void Update () {
		float angle = maxAngle * Input.GetAxis ("HorizontalJoy");
		float torque = maxTorque * Input.GetAxis ("Accelerator");
		float brake = maxBrake * Input.GetAxis ("Brake");

		foreach (WheelCollider wheel in wheels) {
			// a simple car where front wheels steer while rear ones drive
			if (wheel.transform.localPosition.z > 0)
				wheel.steerAngle = angle;

			if (wheel.transform.localPosition.z < 0)
				wheel.motorTorque = torque;

			wheel.brakeTorque = brake;

			// update visual wheels if any
			if (wheelShape) {
				Quaternion q;
				Vector3 p;
				wheel.GetWorldPose (out p, out q);

				// assume that the only child of the wheelcollider is the wheel shape
				Transform shapeTransform = wheel.transform.GetChild (0);
				shapeTransform.position = p;
				shapeTransform.rotation = q;
			}

		}
	}
}