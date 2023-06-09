using Godot;
using Godot.Collections;
using System;

public partial class SpaceShip : Area3D
{
	[Export]
	public float cowHoverBelow = 1;
	[Export]
	public float startDelay = 12f;
	[Export]
	public float hoverSpeed = 0.75f;
	[Export]
	public float hoverAmount = 0.2f;
	
	public AudioStreamPlayer3D spaceHopSound;
	public AudioStreamPlayer3D spaceAwaySound;

	public String[] behaviour = {
		"wait_1",
		"between_5",
		"Wait_0.01",
		"between_5",
		"Wait_0.01",
		"between_5",
		"Wait_0.01",
		"cow_2",
		"wait_1",
		"end_5",
	};

	private Timer 	timer;
	private Timer 	timerGameOver;
	private Tween 	tweenHover;
	public int 		event_id 			= 0;
	public int 		currentNodeIndex    = 0;
	public bool 	isAtDestination     = false;
	public bool 	processNextEvent 	= false;
	public bool 	mustWait 			= true;
	public float 	wait_time    		= 2;
	public float 	speed 				= 1;
	public float 	cowPullForceMultiplier = 1;


	public Vector3  destination;
	public Node3D 	currentTarget;
	public Cow 		cowTarget;
	public Sprite3D sprite;
	public Label 	labelGameOver;

	public Node3D pathEndPoints;
	public Node3D pathBetweenPoints;
	public CharacterBody3D player;
	public Array<Node3D> cows 			= new Array<Node3D>();
	public Array<Node3D> cowsOriginal 	= new Array<Node3D>();
	public Array<Node3D> endPoints 		= new Array<Node3D>();
	public Array<Node3D> betweenPoints 	= new Array<Node3D>(); 

	
	[Signal]
	public delegate void OnPositionReachedEventHandler();	
	
	public override void _Ready()
	{
		spaceHopSound = this.GetNode<AudioStreamPlayer3D>("HopSound");
		spaceAwaySound = this.GetNode<AudioStreamPlayer3D>("AudioSpaceShipGoingAway");

		sprite = GetNode<Sprite3D>("SpriteSpaceShip");
		tweenHover = GetTree().CreateTween();
		tweenHover.SetLoops();
		tweenHover.SetEase(Tween.EaseType.Out);
		tweenHover.SetTrans(Tween.TransitionType.Quad);
		tweenHover.TweenProperty(sprite, "position", new Vector3(sprite.Position.X, sprite.Position.Y-hoverAmount, sprite.Position.Z), hoverSpeed);
		tweenHover.TweenProperty(sprite, "position", new Vector3(sprite.Position.X, sprite.Position.Y+hoverAmount, sprite.Position.Z), hoverSpeed);

		timer = new Timer(); 
		AddChild(timer);
		timer.Autostart 	= true;
		timer.OneShot 		= true;
		timer.Timeout 		+= OnTimerTimeout;
		
		timerGameOver = new Timer(); 
		AddChild(timerGameOver);
		timerGameOver.Autostart 	= true;
		timerGameOver.OneShot 		= true;
		timerGameOver.Timeout 		+= GameOver;

		this.Connect(SignalName.OnPositionReached, new Callable(this, MethodName.ShipReachedDestination));	

		labelGameOver.Visible = false;
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

	public void Init()
	{
		CalculateNextEndPoint(betweenPoints);
		this.GlobalPosition = endPoints[0].GlobalPosition;
		timer.Start(startDelay);
	}

	private void ShipReachedDestination()
	{
		timer.Start(wait_time);
	}
	
	private void OnTimerTimeout()
	{
		timer.Paused = true;
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
		String event_name_next  = behaviour[event_id_next].Split('_')[0];
		String event_name_prev  = behaviour[event_id_prev].Split('_')[0];
		String t_speed 		    = behaviour[event_id].Split('_')[1];
		speed 			        = (float) Convert.ToDouble(t_speed);
		GD.Print("Ship: Timer is done! ", event_name);
		
		mustWait = false;

		// look into the next event
		if (event_name == "cow" && event_name_next == "wait")
		{
			String time	    = behaviour[event_id_next].Split('_')[1];
			wait_time  	    = (float) Convert.ToDouble(time);
			cowPullForceMultiplier = 1-(wait_time/(this.GlobalPosition.Y-cowHoverBelow));
			GD.Print("Ship: next is <wait> event: cowPullForceMultiplier: ", cowPullForceMultiplier, " ----- ", wait_time);
		}

		// look into the previouse event
		if (event_name == "wait" && event_name_prev == "cow")
		{
			speed = speed + 1;	
		}

		// look into the previouse event
		event_name_prev = behaviour[event_id_prev].Split('_')[0];
		if (event_name == "wait" && event_name_prev == "end")
		{
			GD.Print("Ship: previous was <end> event, removing cow, if any");
			Array<Node3D> targets = FindCowsInRange(cows, 4);
			if (targets.Count != 0) 
			{
				foreach (Node3D c in targets)
				{
					Cow cow = c as Cow;

					cows.Remove(cow);

					cow.dumcowmode = true;
					cow.QueueFree();

					GD.Print("Ship: removed cow: ", cowTarget.Name);
				}
			}
			else
			{
				GD.Print("Ship: removing cow failed: ", cowTarget);
			}

			if (cows.Count < 2 )
			{
				timerGameOver.Start(4);
				timer.Stop();
				mustWait = true;
				labelGameOver.Visible = true;
			}
		}

		if (event_name == "start")
		{
			CalculateNextEndPoint(endPoints);
		}
		else if (event_name == "between")
		{
			CalculateNextEndPoint(betweenPoints);
			spaceHopSound.Play();
		}
		else if (event_name == "cow")
		{
			CalculateNextEndPoint(cows);
			cowTarget = currentTarget as Cow;
			destination = new Vector3(destination.X, this.GlobalPosition.Y, destination.Z);
			spaceHopSound.Play();
		}
		else if (event_name == "end")
		{
			CalculateNextEndPoint(endPoints);
			spaceAwaySound.Play();
		}
		else if (event_name == "wait")
		{
			GD.Print("Ship: Timer Start! ", wait_time, " with multiplier: ", cowPullForceMultiplier);

			mustWait = true;
			timer.Start(speed);
		}

		 
		event_id++;
		timer.Paused = false;
	}

	public void GameOver()
	{
		// goto end scene
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().ChangeSceneToFile("res://UI/Scenes/GameOver.tscn");
		// wait a few secodns
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

	public void InitMovePoints()
	{
		for (int i = 0; i < pathEndPoints.GetChildCount(); i++) {
			endPoints.Add(pathEndPoints.GetChild<Node3D>(i));
			endPoints[i].Position = new Vector3(endPoints[i].Position.X, this.GlobalPosition.Y, endPoints[i].Position.Z);
		}
		for (int i = 0; i < pathBetweenPoints.GetChildCount(); i++) 
		{
			betweenPoints.Add(pathBetweenPoints.GetChild<Node3D>(i));
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
