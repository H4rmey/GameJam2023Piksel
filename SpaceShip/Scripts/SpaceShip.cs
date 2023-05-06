using Godot;
using Godot.Collections;
using System;

public partial class SpaceShip : Area3D
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
	public float cowHoverBelow = 1;
    [Export]
	public float cowPullForceMultiplier = 1;
	
	public String[] behaviour = {
		"Wait_4",
		"between_3",
		"Wait_0.05",
		"between_3",
		"Wait_0.05",
		"between_3",
		"Wait_0.5",
		"cow_1",
		"wait_2",
		"end_1",
		"wait_2",
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
	public  Cow 	cowTarget;

    public CharacterBody3D player;
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

		timer.Start(0.1f);
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
			this.GlobalPosition = this.GlobalPosition.Slerp(destination, (float)delta*speed);
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
		int event_id_next = event_id + 1;
		int event_id_prev = event_id - 1;
		if (event_id_prev < 0 )
		{
			event_id_prev = behaviour.Length-1;
		}
		if (event_id >= behaviour.Length )
		{
			event_id=0;	
		}
		if (event_id_next >= behaviour.Length )
		{
			event_id_next = 0;
		}

		String event_name 		= behaviour[event_id].Split('_')[0];
		String t_speed 		    = behaviour[event_id].Split('_')[1];
		speed 			        = (float) Convert.ToDouble(t_speed);
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
			cowTarget = currentTarget as Cow;
			destination = new Vector3(destination.X, this.GlobalPosition.Y, destination.Z);
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

			GD.Print("Ship: Timer Start! ", wait_time, " with multiplier: ", cowPullForceMultiplier);

			timer.Start(wait_time);

			
		}

		// look into the next event
		String event_name_next = behaviour[event_id_next].Split('_')[0];
		if (event_name == "cow" && event_name_next == "wait")
		{
			String time	    = behaviour[event_id_next].Split('_')[1];
			wait_time  	    = (float) Convert.ToDouble(time);
			cowPullForceMultiplier = 1-(wait_time/(this.GlobalPosition.Y-cowHoverBelow));
			GD.Print("Ship: next is <wait> event: cowPullForceMultiplier: ", cowPullForceMultiplier, " ----- ", wait_time);
		}

		// look into the previouse event
		String event_name_prev = behaviour[event_id_prev].Split('_')[0];
		if (event_name == "wait" && event_name_prev == "end")
		{
			GD.Print("Ship: previous was <end> event, removing cow, if any");
			if (cowTarget != null)
			{
				Array<Node3D> targets = FindCowsInRange(cows, 4);
				foreach (Node3D c in targets)
				{
					Cow cow = c as Cow;

					cows.Remove(cowTarget);
						
					cow.dumcowmode = true;
					cow.QueueFree();

					GD.Print("Ship: removed cow: ", cowTarget.Name);
				}
			}
			else
			{
				GD.Print("Ship: removing cow failed: ", cowTarget);
			}

			if (cows.Count <= 0 )
			{
				GD.Print("GAME OVER, YOU ARE BAD...");
			}
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

	private Array<Node3D> FindCowsInRange(Array<Node3D> nodes, float range)
	{
		Array<Node3D> ret_nodes = new Array<Node3D>();
		foreach (Node3D n in nodes)
		{
			float l = this.GlobalPosition.DistanceTo(n.GlobalPosition);
			if (l <= range)
			{
				range = l;
				ret_nodes.Add(n);
			}
		}
		return ret_nodes;
	}	
}
