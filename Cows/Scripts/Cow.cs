using Godot;
using Godot.Collections;
using System;

public partial class Cow : RigidBody3D
{
	// are set int he spaceship
	[Export]
	public SpaceShip spaceShip;
	[Export]
	public float speed;
	[Export]
	public float timeToResetVelocity = 4;
	
	public bool dumcowmode 	= false;
	public bool is_pulled 	= false;
	public bool follow_ship = false;	
	public bool checkFloor 	= true;
	public bool is_taken    = false;

	private Timer 		timer;
	public  RayCast3D 	raycast;
	public  Vector3 	destination;

	public override void _Ready()
	{
		spaceShip.BodyEntered += _on_space_ship_body_entered;
		spaceShip.BodyExited  += _on_space_ship_body_exited;

		this.MouseEntered 	+= _on_mouse_entered;
		this.MouseExited 	+= _on_mouse_exited;

		timer = new Timer(); 
		AddChild(timer);
		timer.Autostart 	= true;
		timer.OneShot 		= true;
		timer.Timeout 		+= OnTimerTimeout;

		raycast = GetNode<RayCast3D>("RayCast3D");
	
		SetRandomCowSprite();
	}

	public void SetRandomCowSprite()
	{
		Sprite3D sprite = GetNode<Sprite3D>("SpriteCow");

		Array<Texture2D> textures = new Array<Texture2D>();
		textures.Add(ResourceLoader.Load<Texture2D>("res://Cows/Sprites/Cow1.png"));
		textures.Add(ResourceLoader.Load<Texture2D>("res://Cows/Sprites/Cow2.png"));

		RandomNumberGenerator random = new RandomNumberGenerator();
		int nextIndex = random.RandiRange(0, textures.Count-1);

		sprite.Texture = textures[nextIndex];
	}

	public override void _Process(double delta)
	{
		this.is_taken = this.is_pulled | this.follow_ship;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (dumcowmode){
			return;
		}
		
		if (raycast.GetCollider() as Node3D != null && checkFloor)
		{
			checkFloor = false;
			timer.Start(3);
		}		
		else
		{
			checkFloor = true;
		}


		if (!is_pulled) {
			return;
		}
		

		//Vector3 direction = destination - this.GlobalPosition;
		bool isAtDestination = (this.GlobalPosition.Y > spaceShip.GlobalPosition.Y-spaceShip.cowHoverBelow-0.1f); 
		
		if (isAtDestination)
		{
			SetLockedMode(true);
			follow_ship = true;
		}
		else
		{		
			Vector3 direction = (destination - this.GlobalPosition) * 1; 
			this.LinearVelocity = direction * spaceShip.cowPullForceMultiplier;
			destination = new Vector3(spaceShip.GlobalPosition.X, spaceShip.GlobalPosition.Y, spaceShip.GlobalPosition.Z);
		}

		if (follow_ship)
		{
			this.GlobalPosition = spaceShip.sprite.GlobalPosition - new Vector3(0,spaceShip.cowHoverBelow,0);
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
		(spaceShip.player as Player).cowTarget = this;
	}

	private void _on_mouse_exited()
	{
		(spaceShip.player as Player).cowTarget = null;
	}

	private void _on_space_ship_body_entered(Node3D node)
	{
		if (node == this)
		{
			GD.Print("Moooo!");
			this.is_pulled = true;
		} 
	}

	private void _on_space_ship_body_exited(Node3D node)
	{
		if (node == this)
		{
			GD.Print("Moooo?");
			this.is_pulled = false;
		}
	}

	public void CowIsPulled(Vector3 location)
	{
		Vector3 forceDirection = (location - this.GlobalPosition);

		spaceShip.cowTarget = null;

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
		if (v1.X > v2.X - sensitivity && v1.X < v2.X + sensitivity
		&&  v1.Z > v2.Z - sensitivity && v1.Z < v2.Z + sensitivity
		&&  v1.Z > v2.Z - sensitivity && v1.Z < v2.Z + sensitivity)
		{ 
			return true;	
		}
		
		return false;
	}
}
