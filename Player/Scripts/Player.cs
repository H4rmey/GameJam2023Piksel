using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public  TextEdit 	textEdit;
	[Export]
	public SpaceShip spaceShip;
	[Export]
	public Lasso lasso;
	[Export]
	float lassoRate = 1f;
	[Export]
	float speed = 20f;
	[Export]
	float acceleration = 15f;
	[Export]
	float air_acceleration = 5f;
	[Export]
	float gravity = 0.98f;
	[Export]
	float max_terminal_velocity = 54f;
	[Export]
	float jump_power = 20f;
	[Export]
	float shoot_power = 20f;

	[Export(PropertyHint.Range, "0.1,1.0")]
	float mouse_sensitivity = 0.3f;
	[Export(PropertyHint.Range, "-90,0,1")]
	float min_pitch = -90f;
	[Export(PropertyHint.Range, "0,90,1")]
	float max_pitch = 90f;

	private Vector3 velocity;
	private float y_velocity;

	private Node3D camera_pivot;
	private Camera3D camera;

	public  Cow 		cowTarget;

	public int score;

	public Timer lassoTimer;
	public bool canLasso = true;

	public override void _Ready() {
		camera_pivot = GetNode<Node3D>("CameraPivot");
		camera = GetNode<Camera3D>("CameraPivot/CameraBoom/Camera");

		Input.MouseMode = Input.MouseModeEnum.Captured;

		lassoTimer = new Timer();
		AddChild(lassoTimer);
		lassoTimer.Autostart 	= true;
		lassoTimer.OneShot 		= true;
		lassoTimer.Timeout += OnLassoTimerTimeout;
	}

	public void OnLassoTimerTimeout()
	{
		canLasso = true;
		//lasso.Visible = false;
	}

	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("ui_cancel")) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
		handle_movement(delta);
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (@event is InputEventMouseMotion motionEvent) {
			Vector3 rotDeg = RotationDegrees;
			rotDeg.Y -= motionEvent.Relative.X * mouse_sensitivity;
			RotationDegrees = rotDeg;

			rotDeg = camera_pivot.RotationDegrees;
			rotDeg.X -= motionEvent.Relative.Y * mouse_sensitivity;
			rotDeg.X = Mathf.Clamp(rotDeg.X, min_pitch, max_pitch);
			camera_pivot.RotationDegrees = rotDeg;
		}
	}

	private async void handle_movement(double delta) {
		Vector3 direction = new Vector3(0,0,0);

		if (Input.IsActionPressed("move_forward")) {
			direction -= Transform.Basis.Z;
		}
		if (Input.IsActionPressed("move_backward")) {
			direction += Transform.Basis.Z;
		}
		if (Input.IsActionPressed("move_left")) {
			direction -= Transform.Basis.X;
		}
		if (Input.IsActionPressed("move_right")) {
			direction += Transform.Basis.X;
		}
		
		direction = direction.Normalized();

		double accel = IsOnFloor() ? acceleration : air_acceleration;
		velocity = velocity.Lerp(direction * speed, (float)(accel * delta));

		if (IsOnFloor()) {
			y_velocity = -0.01f; // apply a small amount of downward force if on floor
		}
		else {
			y_velocity = Mathf.Clamp(y_velocity-gravity, -max_terminal_velocity, max_terminal_velocity);
		}

		if (Input.IsActionJustPressed("open_debug")) {			
			textEdit.Visible = !textEdit.Visible;			
		}

		if (Input.IsActionJustPressed("jump") && !lasso.Visible) {			
			lasso.thingy();
		}
		
		velocity.Y = y_velocity;
		MoveAndSlide();
		this.Velocity = velocity;
	}
}
