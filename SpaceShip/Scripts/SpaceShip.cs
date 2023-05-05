using Godot;
using Godot.Collections;
using System;

public partial class SpaceShip : Node3D
{
	[Export]
	public NodePath pathPlayer;
	[Export]
	public NodePath pathCows;
	[Export]
	public NodePath pathEndPoints;
	[Export]
	public NodePath pathBetweenPoints;
    [Export]
	public double speedToEnd = 5;
    [Export]
	public double speedToCow = 3;
    [Export]
	public double speedToBetween = 0.5f;

    [Export]
	public double holdTimeCow = 5;
    [Export]
	public double holdTimeEnd = 3;
    [Export]
	public double holdTimeBetween = 0.5f;
    [Export]
	public float cowHoverBelow = 1;
	
	public String[] behaviour = {
		"start_2",
		"wait_0",
		"cow_1",
		"wait_3",
		"end_1.2",
		"wait_8",
	};

	private Timer 	timer;
	private int 	event_id 			= 0;
	private bool 	isAtDestination     = false;
	private bool 	processNextEvent 	= false;
	private bool 	mustWait 			= true;
	private float 	wait_time    		= 2;
	private float 	speed 				= 1;
	private int 	currentNodeIndex;


	private Vector3 destination;
	private Node3D 	currentTarget;
	private Node3D 	cowTarget;

    private CharacterBody3D player;
    private Array<Node3D> cows 				= new Array<Node3D>();
    private Array<Node3D> cowsOriginal 		= new Array<Node3D>();
    private Array<Node3D> endPoints 		= new Array<Node3D>();
    private Array<Node3D> betweenPoints 	= new Array<Node3D>(); 
	
	[Signal]
	public delegate void OnPositionReachedEventHandler();	
	
    public override void _Ready()
    {
        player = GetNode<CharacterBody3D>(pathPlayer);

		timer = new Timer(); 
		AddChild(timer);
		timer.Autostart 	= true;
		timer.OneShot 		= true;
		timer.Timeout 		+= OnTimerTimeout;

		InitMovePoints();
		CalculateNextEndPoint(betweenPoints);

		this.GlobalPosition = endPoints[0].GlobalPosition;
		
		this.Connect(SignalName.OnPositionReached, new Callable(this, MethodName.ShipReachedDestination));	

		timer.Start(2);
    }
	

    public override void _Process(double delta)
    {
		if (mustWait) {
			return;
		}

		isAtDestination = CheckPosition(this.Position, destination, 0.1f,false);

		if (isAtDestination) 
		{
			processNextEvent = true;
			GD.Print("Ship: Reached destination");
		}
		else if (!isAtDestination)
 		{
			Position = Position.Lerp(destination, (float)delta*speed);
		}

		if (processNextEvent)
		{
			processNextEvent = false;
			mustWait = true;
			OnTimerTimeout();
		}
    }

	private void ShipReachedDestination()
	{
		timer.Start(wait_time);
	}
	
	private void OnTimerTimeout()
	{
		if (event_id >= behaviour.Length )
		{
			event_id=0;	
		}

		String event_name 	= behaviour[event_id].Split('_')[0];
		String t_speed 		= behaviour[event_id].Split('_')[1];
		speed 			    = (float) Convert.ToDouble(t_speed);
		GD.Print("Ship: Timer is done! ", event_name);
		
		mustWait = false;

		if (event_name == "start")
		{
			CalculateNextEndPoint(endPoints);
		}
		else if (event_name == "between")
		{
			CalculateNextEndPoint(betweenPoints);
		}
		else if (event_name == "cow")
		{
			CalculateNextEndPoint(cows);
			destination = new Vector3(destination.X, this.GlobalPosition.Y, destination.Z);

			Cow cow = currentTarget as Cow;
			cow.destination = new Vector3(this.GlobalPosition.X, this.GlobalPosition.Y-cowHoverBelow, this.GlobalPosition.Z);
			cow.mustWait = false;
			cow.SetLockedMode(true);

			GD.Print("Ship: Abducting Cow! ", wait_time);
		}
		else if (event_name == "end")
		{
			CalculateNextEndPoint(endPoints);
		}
		else if (event_name == "wait")
		{
			String time	    = behaviour[event_id].Split('_')[1];
			wait_time  	    = (float) Convert.ToDouble(time);
			mustWait 		= true;

			GD.Print("Ship: Timer Start! ", wait_time);

			timer.Start(wait_time);
		}

		event_id++;
	}

	private void CalculateNextEndPoint(Array<Node3D> nodes)
	{
		// Generate the next index
		RandomNumberGenerator random = new RandomNumberGenerator();
		int nextIndex = random.RandiRange(0, nodes.Count-1);

		if (currentNodeIndex == nextIndex )
		{
			int m = random.RandiRange(0, 1);
			nextIndex = (m == 0) ? (nextIndex-1)%nodes.Count : (nextIndex+1)%nodes.Count;
		}

		currentNodeIndex 	= nextIndex;	
		currentTarget 		= nodes[nextIndex];
		destination 		= nodes[nextIndex].GlobalPosition;
	}

	private void InitMovePoints()
	{
		for (int i = 0; i < GetNode<Node3D>(pathCows).GetChildCount(); i++) 
		{
        	cows.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
        	cowsOriginal.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
			SetCowScript(i);
		}
		for (int i = 0; i < GetNode<Node3D>(pathEndPoints).GetChildCount(); i++) {
			endPoints.Add(GetNode<Node3D>(pathEndPoints).GetChild<Node3D>(i));
			endPoints[i].Position = new Vector3(endPoints[i].Position.X, this.GlobalPosition.Y, endPoints[i].Position.Z);
		}
		for (int i = 0; i < GetNode<Node3D>(pathBetweenPoints).GetChildCount(); i++) 
		{
			betweenPoints.Add(GetNode<Node3D>(pathBetweenPoints).GetChild<Node3D>(i));
			betweenPoints[i].Position = new Vector3(betweenPoints[i].Position.X, this.GlobalPosition.Y, betweenPoints[i].Position.Z);
		}
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

	private void SetCowScript(int i)
	{
		//set cow script
		Cow cowScript = cows[i] as Cow;
		cowScript.spaceShipScript = this as SpaceShip;
		cowScript.spaceShip = this;
	}
	
}
