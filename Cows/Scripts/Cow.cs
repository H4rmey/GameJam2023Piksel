using Godot;
using System;

public partial class Cow : RigidBody3D
{
	// are set int he spaceship
	[Export]
	public Node3D spaceShip;
	[Export]
	public SpaceShip spaceShipScript;
	[Export]
	public float speed;
	

	public bool mustWait = true;
	public Vector3 destination;

	public override void _Ready()
	{
		spaceShipScript = spaceShip as SpaceShip;
	}

	public override void _Process(double delta)
	{
		if (spaceShip == null) {
			GD.Print("Error, spaceShip not found for cow: ", this);
			return; 
		}
		if (spaceShipScript == null) {
			GD.Print("Error, spaceShipScript not found for cow: ", this);
			return; 
		}
		if (mustWait) {
			return;
		}

		bool isAtDestination = CheckPosition(this.Position, destination, 0.1f,false);

		if (isAtDestination) 
		{
			GD.Print("Cow: Reached destination");
		}
		else if (!isAtDestination)
 		{
			Position = Position.Lerp(destination, (float)delta*speed);
		}

	}
	
	public void SetLockedMode(bool is_locked)
	{
		this.AxisLockAngularX = is_locked;
		this.AxisLockAngularY = is_locked;
		this.AxisLockAngularZ = is_locked;
		this.AxisLockLinearX = is_locked;
		this.AxisLockLinearY = is_locked;
		this.AxisLockLinearZ = is_locked;
	}

	private bool CheckPosition(Vector3 v1, Vector3 v2, float sensitivity,bool enable_hight = true)
	{
		if (v1.X > v2.X - sensitivity && v1.X < v2.X + sensitivity) { }
		else 
		{
			return false;	
		}
		if (v1.Z > v2.Z - sensitivity && v1.Z < v2.Z + sensitivity) { }
		else 
		{
			return false;	
		}
		if (v1.Z > v2.Z - sensitivity && v1.Z < v2.Z + sensitivity && !enable_hight) { }
		else { return false;	}
		
		return true;
	}
}
