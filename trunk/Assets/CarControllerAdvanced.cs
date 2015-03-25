using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Side {
    left,
    right
};

[System.Serializable]
public class WheelSet {

    Quaternion angleCorrection = Quaternion.Euler(0.0f, 0.0f, 90.0f); // Create a Quaternion that is rotated 90 degrees in the z direction
    const int dps = 6; // Conversion from rpm to degrees per second (was -6)

    public GameObject wheelLeftGo;
    public Transform wheelLeftT;
    WheelCollider wheelLeftWc;
    
    public GameObject wheelRightGo;
    public Transform wheelRightT;
    WheelCollider wheelRightWc;
    
    public bool drive; // Is this wheel attached to a drive?
    public bool brakes; // Does it have brakes?
    public bool steering; // Does this wheel apply steer angle?

    float steerAngle;
    Vector3 wheelRightRotationCurrent;
    Vector3 wheelLeftRotationCurrent;
    Vector3 tempRotation = new Vector3(0.0f, 0.0f, 0.0f);
    
    /// <summary>
    /// Makes a wheel turn (Called in FixedUpdate)
    /// </summary>
    /// <param name="side">Which side of the wheel set should turn</param>
    /// <param name="motor">How fast the wheel should turn</param>
    /// <param name="max">Max speed of the wheel</param>
    public void Throttle(Side side, float motor, float max) {
        if (drive) {
            if (side == Side.left)
                wheelLeftWc.motorTorque = -1 * max * motor;
            else
                wheelRightWc.motorTorque = -1 * max * motor;
        }
    }

    /// <summary>
    /// Makes a wheel stop (Called in FixedUpdate)
    /// </summary>
    /// <param name="side">Which side of the wheel set should stop</param>
    /// <param name="motor">How fast the wheel should stop</param>
    /// <param name="max">Max amount of brakes</param>
    public void Brake(Side side, float brake, float max) {
        if (brakes) {
            if (side == Side.left)
                wheelLeftWc.brakeTorque = max * brake;
            else
                wheelRightWc.brakeTorque = max * brake;
        }
    }

    public void Steer(Side side, float amount, float max) {
        steerAngle = amount * max;

        if (steering) {
            if (side == Side.left)
                wheelLeftWc.steerAngle = steerAngle;
            else
                wheelRightWc.steerAngle = steerAngle;
        }
    }

    /// <summary>
    /// Store the handles for the wheel components
    /// </summary>
    public void Init() {
        wheelLeftWc = wheelLeftT.GetComponent<WheelCollider>();
        wheelRightWc = wheelRightT.GetComponent<WheelCollider>();

        wheelLeftRotationCurrent = wheelLeftGo.transform.eulerAngles;
        wheelRightRotationCurrent = wheelLeftGo.transform.eulerAngles;
    }

    /// <summary>
    /// Behind the scenes magic (Called in Update)
    /// </summary>
    public void UpdateWheels() {
        // Update location of rendered model (dependent on collision meshes)
        wheelLeftGo.transform.position = wheelLeftWc.transform.position;
        wheelRightGo.transform.position = wheelRightWc.transform.position;

        // Update the (y component) rotation of the wheels
        if (steering) {
            tempRotation.y = Mathf.Lerp(tempRotation.y, steerAngle, 0.2f);
            wheelRightRotationCurrent.y = tempRotation.y;
            wheelLeftRotationCurrent.y = tempRotation.y;
            wheelLeftGo.transform.localEulerAngles = wheelLeftRotationCurrent;
            wheelRightGo.transform.localEulerAngles = wheelRightRotationCurrent;
        }
        else {
            wheelLeftGo.transform.rotation = wheelLeftWc.transform.rotation * angleCorrection;
            wheelRightGo.transform.rotation = wheelRightWc.transform.rotation * angleCorrection;
        }

        // Update the (z component) rotation of the wheels (relative to current values)
        wheelLeftGo.transform.Rotate(0.0f, wheelLeftWc.rpm * dps * Time.deltaTime, 0.0f);
        wheelRightGo.transform.Rotate(0.0f, wheelRightWc.rpm * dps * Time.deltaTime, 0.0f);
    }

    /// <summary>
    /// Get the ammount of Torque of a wheel
    /// </summary>
    /// <param name="side">Which side of the wheel you want the torque of</param>
    /// <returns>The ammount of Torque the wheel is supplying</returns>
    public string GetTorque(Side side) {
        if (side == Side.left)
            return wheelLeftWc.motorTorque.ToString();
        else
            return wheelRightWc.motorTorque.ToString();
    }
}

public class CarControllerAdvanced : MonoBehaviour {

    public WheelSet frontSet;
    public WheelSet backSet;

    public float steerMax; // Max angle wheels rotate (In degrees)
    public float motorMax; // Max torque of motors (In N*m)
    public float brakeMax; // Max braking (In N*m)

    public Vector3 objectCentreOfMass; // Allows the centre of mass to be offset (relative, in metres)

    public bool debugDisplay = true; // Whether or not debug information should be displayed

    float steer = 0.0f;
    float forward = 0.0f;
    float back = 0.0f;
    float speed = 0.0f; // The net velocity of the object (In m/s)
    float motor = 0.0f;
    float brake = 0.0f;

    bool reverse = false;
   
	// Use this for initialization
	void Start() {
        // Set centre of mass to what is defined in the inspector
        GetComponent<Rigidbody>().centerOfMass += objectCentreOfMass;

        // Setup the wheel sets
        frontSet.Init();
        backSet.Init();
	}

    void OnGUI() {
        // Debug log
        if (debugDisplay) {
            GUI.Label(new Rect(10.0f, 10.0f, 100.0f, 20.0f), "Speed: " + speed.ToString());
            GUI.Label(new Rect(10.0f, 30.0f, 100.0f, 20.0f), "Steer: " + steer.ToString());
            GUI.Label(new Rect(10.0f, 50.0f, 100.0f, 20.0f), "Motor: " + motor.ToString());
            GUI.Label(new Rect(10.0f, 70.0f, 100.0f, 20.0f), "Brake: " + brake.ToString());
            GUI.Label(new Rect(10.0f, 90.0f, 500.0f, 20.0f), "Left Motor Torque: " + backSet.GetTorque(Side.left));
            GUI.Label(new Rect(10.0f, 110.0f, 500.0f, 20.0f), "Right Motor Torque: " + backSet.GetTorque(Side.right));
        }
    }

    void Update() {
        // Retrieve Input
        steer = Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1);
        forward = Mathf.Clamp(Input.GetAxis("Vertical"), 0, 1);
        back = -1 * Mathf.Clamp(Input.GetAxis("Vertical"), -1, 0);

        frontSet.UpdateWheels();
        backSet.UpdateWheels();

        // Calculate the speed of the 
        speed = GetComponent<Rigidbody>().velocity.magnitude;

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

        // Throttle
        frontSet.Throttle(Side.left, motor, motorMax);
        frontSet.Throttle(Side.right, motor, motorMax);
        backSet.Throttle(Side.left, motor, motorMax);
        backSet.Throttle(Side.right, motor, motorMax);
        
        // Brakes
        frontSet.Brake(Side.left, brake, brakeMax);
        frontSet.Brake(Side.right, brake, brakeMax);
        backSet.Brake(Side.left, brake, brakeMax);
        backSet.Brake(Side.right, brake, brakeMax);

        // Steering (This needs to be changed to be dependent on rear motors)
        frontSet.Steer(Side.left, steer, steerMax);
        frontSet.Steer(Side.right, steer, steerMax);
        backSet.Steer(Side.left, steer, steerMax);
        backSet.Steer(Side.right, steer, steerMax);
	}
}
