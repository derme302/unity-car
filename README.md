# Unity - Simple Car Demo
*Built with Unity 2021.3.6f1*

This project is a simple car demo for [Unity](http://unity3d.com/) which tries to make it as simple as possible to get a basic vehicle up and running.

## How it works
I'll write up more later about the specifics, but basically it shows how you implement Unity's WheelCollider component to get a simple car moving.

## What can the project do right now?
The extension has two parts, the first is a really basic CarControllerSimple class, which has the bare bones of what you need to get a car moving. (Not yet working with Unity 2021)

However where the project really gets interesting is the CarAdvancedController, which makes use of the addition of the WheelSet class (A fancy wrapper for WheelColiders), this class has the functions listed below. This class does a lot of the heavy lifting for you and lets you focus on just getting the car moving how you want it to.

```csharp
public void Init();
public void Throttle(Side side, float motor, float max); // Makes a wheel turn (Called in FixedUpdate)
public void Brake(Side side, float brake, float max); // Makes a wheel stop (Called in FixedUpdate)
public void Steer(Side side, float amount, float max);
public void UpdateWheels(); // Updates the visual models for the wheels (Called in Update)
public string GetTorque(Side side); // Get the ammount of Torque of a wheel
```

## Authors and Contributors
This project was originally built on code by [Slawia](http://carpe.com.au/slawia/2009/08/unity-wheel-collider-part-2), however Unity 5 introduces a new wheel collider and has broken older projects. The current code base has been completely re-written from scratch by me (<a class="user-mention" style="font-style: inherit; color: #2879d0;" href="https://github.com/derme302">@derme302</a>).

A special thanks to [@barlo695](https://twitter.com/barlo695) who recently went though and helped fixed a number of issues with the project.

If you would like to add something feel free to do so!

The project is currently under the MIT License (MIT)

## Support or Contact
Having trouble getting the car demo? Either try me ([@derme302](https://twitter.com/derme302)) or [@barlo695](https://twitter.com/barlo695) on Twitter.
