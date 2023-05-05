using Godot;
using System;

public partial class Cow : RigidBody3D
{
	// are set int he spaceship
	[Export]
	public Area3D spaceShip;
	[Export]
	public SpaceShip spaceShipScript;
	[Export]
	public float speed;
	[Export]
	public float timeToResetVelocity = 4;

	public bool is_pulled = false;
	public bool follow_ship = false;

	public Vector3 destination;

	private Timer timer;

	public override void _Ready()
	{
		spaceShipScript = spaceShip as SpaceShip;

		spaceShip.BodyEntered += _on_space_ship_body_entered;
		spaceShip.BodyExited  += _on_space_ship_body_exited;

		this.MouseEntered 	+= _on_mouse_entered;
		this.MouseExited 	+= _on_mouse_exited;

		timer = new Timer(); 
		AddChild(timer);
		timer.Autostart 	= true;
		timer.OneShot 		= true;
		timer.Timeout 		+= OnTimerTimeout;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!is_pulled) {
			return;
		}
		

		//Vector3 direction = destination - this.GlobalPosition;
		bool isAtDestination = CheckPosition(this.GlobalPosition, destination, 0.1f, false);

		if (isAtDestination)
		{
			SetLockedMode(true);
			follow_ship = true;
		}
		else
		{		
			Vector3 direction = (destination - this.GlobalPosition) * 1; 
			this.LinearVelocity = direction;
			destination = new Vector3(spaceShip.GlobalPosition.X, spaceShip.GlobalPosition.Y, spaceShip.GlobalPosition.Z);
		}

		if (follow_ship)
		{
			this.Position = spaceShip.GlobalPosition - new Vector3(0,spaceShipScript.cowHoverBelow,0);
		}
	}

	private void OnTimerTimeout()
	{
		this.LinearVelocity 	= new Vector3(0,0,0);
		this.AngularVelocity 	= new Vector3(0,0,0);
		this.SetAxisVelocity(new Vector3(0,0,0));
	}

	private void _on_mouse_entered()
	{
		(spaceShipScript.player as Player).cowTarget = this;
	}

	private void _on_mouse_exited()
	{
		(spaceShipScript.player as Player).cowTarget = null;
	}


	private void _on_space_ship_body_entered(Node3D node)
	{
		if (node == this)
		{
			GD.Print("Moooo!");
			this.is_pulled 		= true;
		} 
	}

	private void _on_space_ship_body_exited(Node3D node)
	{
		if (node == this)
		{
			GD.Print("Moooo?");
			this.is_pulled = false;
			node = null;
		}
	}

	public void CowIsPulled(Vector3 location)
	{
		Vector3 forceDirection = (location - this.GlobalPosition);

		this.is_pulled 		= false;
		this.follow_ship 	= false;
		this.LinearVelocity = new Vector3(0,0,0);
		this.SetLockedMode(false);
		this.ApplyImpulse(new Vector3(forceDirection.X,6,forceDirection.Z)/2);
		timer.Start(timeToResetVelocity);
	}
	
	public void SetLockedMode(bool is_locked)
	{
		this.AxisLockAngularX 	= is_locked;
		this.AxisLockAngularY 	= is_locked;
		this.AxisLockAngularZ 	= is_locked;
		this.AxisLockLinearX 	= is_locked;
		this.AxisLockLinearY 	= is_locked;
		this.AxisLockLinearZ 	= is_locked;
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
		else 
		{ 
			return false;	
		}
		
		return true;
	}
}
