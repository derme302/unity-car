using UnityEngine;
using System.Collections;

/*
 * Originally written in JS
 * http://carpe.com.au/slawia/2009/08/unity-wheel-collider-part-2/
 * 
 * To do:
 * - Add pause between forward and reverse?
*/

public class CarControllerAdvanced : MonoBehaviour {

    const int dps = 6; // Conversion from rpm to degrees per second (was -6)

    public GameObject wheelColFrontLeft;
    public GameObject wheelColFrontRight;
    public GameObject wheelColBackLeft;
    public GameObject wheelColBackRight;

    public Transform wheelModFrontLeft;
    public Transform wheelModFrontRight;
    public Transform wheelModBackLeft;
    public Transform wheelModBackRight;

    public float steerMax; // Max angle wheels rotate (In degrees)
    public float motorMax; // Max torque of motors (In N*m)
    public float brakeMax; // Max braking (In N*m)

    public Vector3 objectCentreOfMass; // Allows the centre of mass to be offset (relative, in metres)

    public bool debugDisplay = true; // Whether or not debug information should be displayed

    Quaternion angleCorrection = Quaternion.Euler(0.0f, 0.0f, 90.0f); // Create a Quaternion that is rotated 90 degrees in the z direction

    float steer = 0.0f;
    float forward = 0.0f;
    float back = 0.0f;
    float speed = 0.0f; // The net velocity of the object (In m/s)
    float motor = 0.0f;
    float brake = 0.0f;

    bool reverse = false;

    WheelCollider wheelFrontLeft;
    WheelCollider wheelFrontRight;
    WheelCollider wheelBackLeft;
    WheelCollider wheelBackRight;
   
	// Use this for initialization
	void Start() {
        // Set centre of mass to what is defined in the inspector
        GetComponent<Rigidbody>().centerOfMass += objectCentreOfMass;

        // Store the handles for the wheel components
        wheelFrontLeft = wheelColFrontLeft.GetComponent<WheelCollider>();
        wheelFrontRight = wheelColFrontRight.GetComponent<WheelCollider>();
        wheelBackLeft = wheelColBackLeft.GetComponent<WheelCollider>();
        wheelBackRight = wheelColBackRight.GetComponent<WheelCollider>();
	}

    void OnGUI() {
        // Debug log
        if (debugDisplay) {
            GUI.Label(new Rect(10.0f, 10.0f, 100.0f, 20.0f), "Speed: " + speed.ToString());
            GUI.Label(new Rect(10.0f, 30.0f, 100.0f, 20.0f), "Steer: " + steer.ToString());
            GUI.Label(new Rect(10.0f, 50.0f, 100.0f, 20.0f), "Motor: " + motor.ToString());
            GUI.Label(new Rect(10.0f, 70.0f, 100.0f, 20.0f), "Brake: " + brake.ToString());
            GUI.Label(new Rect(10.0f, 90.0f, 500.0f, 20.0f), "Left Motor Torque: " + wheelBackLeft.motorTorque.ToString());
            GUI.Label(new Rect(10.0f, 110.0f, 500.0f, 20.0f), "Right Motor Torque: " + wheelBackRight.motorTorque.ToString());
        }
    }

    void Update() {
        // Retrieve Input
        steer = Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1);
        forward = Mathf.Clamp(Input.GetAxis("Vertical"), 0, 1);
        back = -1 * Mathf.Clamp(Input.GetAxis("Vertical"), -1, 0);

        // Update location of rendered model (dependent on collision meshes)
        wheelModFrontLeft.position = wheelColFrontLeft.transform.position;
        wheelModFrontRight.position = wheelColFrontRight.transform.position;
        wheelModBackLeft.position = wheelColBackLeft.transform.position;
        wheelModBackRight.position = wheelColBackRight.transform.position;

        // Update the (y component) rotation of the wheels
        wheelModFrontLeft.localRotation = Quaternion.Euler(0.0f, wheelFrontLeft.steerAngle, 0.0f) * angleCorrection;
        wheelModFrontRight.localRotation = Quaternion.Euler(0.0f, wheelFrontRight.steerAngle, 0.0f) * angleCorrection;
        wheelModBackLeft.rotation = wheelColBackLeft.transform.rotation * angleCorrection;
        wheelModBackRight.rotation = wheelColBackRight.transform.rotation * angleCorrection;
        
        // Update the (z component) rotation of the wheels (relative to current values)
        wheelModFrontLeft.Rotate(0.0f, wheelFrontLeft.rpm * dps * Time.deltaTime, 0.0f);
        wheelModFrontRight.Rotate(0.0f, wheelFrontRight.rpm * dps * Time.deltaTime, 0.0f);
        wheelModBackLeft.Rotate(0.0f, wheelBackLeft.rpm * dps * Time.deltaTime, 0.0f);
        wheelModBackRight.Rotate(0.0f, wheelBackRight.rpm * dps * Time.deltaTime, 0.0f);        

        // Calculate the speed of the 
        speed = GetComponent<Rigidbody>().velocity.sqrMagnitude;

        if ((int)speed == 0) { // Cast as an (int) due to the accuracy of floating point and the physics setup, speed will never be exactly zero
            if (back > 0)
                reverse = true;
            if (forward > 0)
                reverse = false;
        }

        if (reverse) { 
            motor = -1 * back;
            brake = forward;
        }
        else {
            motor = forward;
            brake = back;
        }
    }

	// Update is called once per frame
	void FixedUpdate() {


        // The code here is for a rear steering system (That doesn't work)
        //if (steer >= 0.15f || steer <= -0.15f) {
        //    wheelBackLeft.motorTorque = -1 * motorMax * Mathf.Clamp((steer), -1.0f, 1.0f);
        //    wheelBackRight.motorTorque = -1 * motorMax * Mathf.Clamp((steer), -1.0f, 1.0f);
        //}
        //else {

            wheelBackLeft.motorTorque = -1 * motorMax * motor;
            wheelBackRight.motorTorque = -1 * motorMax * motor;

        //}

        // Brakes
        wheelBackLeft.brakeTorque = brakeMax * brake;
        wheelBackRight.brakeTorque = brakeMax * brake;

        // Steering (This needs to be changed to be dependent on rear motors)
        wheelFrontLeft.steerAngle = steerMax * steer;
        wheelFrontRight.steerAngle = steerMax * steer;

	}
}
