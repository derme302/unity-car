using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Side {
    left,
    right
};

[System.Serializable]
public class WheelSet {

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
        if (steering) {
            if (side == Side.left)
                wheelLeftWc.steerAngle = max * amount;
            else
                wheelRightWc.steerAngle = max * amount;
        }
    }

    /// <summary>
    /// Store the handles for the wheel components
    /// </summary>
    public void Init() {
        wheelLeftWc = wheelLeftT.GetComponent<WheelCollider>();
        wheelRightWc = wheelRightT.GetComponent<WheelCollider>();
    }

    /// <summary>
    /// Behind the scenes magic (Called in Update)
    /// </summary>
    public void UpdateWheels() {
        // Update location of rendered model (dependent on collision meshes)
        wheelLeftT.position = wheelLeftWc.transform.position;
        wheelRightT.position = wheelRightWc.transform.position;

        // Update the (z component) rotation of the wheels (relative to current values)
        wheelLeftT.Rotate(wheelLeftWc.rpm * dps * Time.deltaTime, 0.0f, 0.0f);
        wheelRightT.Rotate(wheelRightWc.rpm * dps * Time.deltaTime, 0.0f, 0.0f);

        // Update the (y component) rotation of the wheels
        wheelLeftT.rotation = wheelLeftWc.transform.rotation;
        wheelRightT.rotation = wheelRightWc.transform.rotation;  
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

        // This code hasn't been updated to Unity 5, should be moved to the WheelSet class and make the steering wheels turn
        frontSet.wheelLeftT.localRotation = Quaternion.Euler(0.0f, steer * steerMax, 0.0f);
        frontSet.wheelRightT.localRotation = Quaternion.Euler(0.0f, steer * steerMax, 0.0f);

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
