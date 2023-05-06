using Godot;
using Godot.Collections;
using System;

public partial class CowHandler : Node3D
{
	[Export]
	public TextEdit text;
	[Export]
	public Node3D pathCows;
	[Export]
	public Node3D pathSpawnpoints;
	[Export]
	public Node3D pathEndPoints;
	[Export]
	public Node3D pathBetweenPoints;
	[Export]
	public Player player;
	[Export]
	public Vector3 spaceShipSpawnPoint;
	[Export]
	public int level;
	[Export]
	public int nofCows = 3;
	[Export]
	public int nofCowsMin = 2;
	[Export]
	public int levelUpTime = 3;

	private Array<Node3D> cows = new Array<Node3D>();
	private Array<Node3D> spwns = new Array<Node3D>();
	
	private String debugText;
	private Timer timer;
	public SpaceShip spaceShip;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		level--;

		timer = new Timer(); 
		AddChild(timer);
		timer.Autostart 	= true;
		timer.OneShot 		= true;
		timer.Timeout 		+= LeveUp;

		for (int i = 0; i < pathSpawnpoints.GetChildCount(); i++) 
		{
			spwns.Add(pathSpawnpoints.GetChild<Node3D>(i));
 		}

		LeveUp();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		update_debug_information();

		if (player.score == nofCows-nofCowsMin)
		{
			timer.Start(levelUpTime);
		}
	}

	private void LeveUp()
	{
		level++;
		if (spaceShip != null) {
			spaceShip.QueueFree();
			RemoveCows();
		}

		CreateSpaceShip();
		SpawnCows();
		InitSpaceShip();

		player.score = 0;
	}

	private void CreateSpaceShip()
	{
		// create spaceship
		PackedScene shipScene 	= ResourceLoader.Load<PackedScene>("res://SpaceShip/Scenes/SpaceShip.tscn");
		this.spaceShip			= (SpaceShip)shipScene.Instantiate();

		// set some variables
		this.spaceShip.player 				= this.player;
		this.spaceShip.pathEndPoints 		= this.pathEndPoints;
		this.spaceShip.pathBetweenPoints 	= this.pathBetweenPoints;

		//get main scene and add spaceship
		this.GetNode(".").AddChild(spaceShip);
		this.spaceShip.GlobalPosition = spaceShipSpawnPoint;
		this.spaceShip.InitMovePoints();
	}

	private void InitSpaceShip()
	{
		spaceShip.cows = this.cows;
		spaceShip.Init();
	}

	private void RemoveCows()
	{
		foreach (Cow cow in spaceShip.cows)
		{
			cow.QueueFree();
		}
		spaceShip.cows.Clear();
		this.cows.Clear();
	}

	private void SpawnCows()
	{
		nofCows = 3 + level * nofCowsMin;

		if (nofCows >= spwns.Count)
		{
			nofCows = spwns.Count-1;
		}

		if (nofCows <= 0)
		{
			nofCows = 3;
		}
		
		RandomNumberGenerator random = new RandomNumberGenerator();
		cows.Clear();
		for (int i = 0; i < nofCows; i++) 
		{
			float randomHeight = random.RandfRange(3, 6);
			
			PackedScene cowScene 	= ResourceLoader.Load<PackedScene>("res://Cows/Scenes/Cow.tscn");
			Cow cow 				= (Cow)cowScene.Instantiate();
			cow.Name 				= "Cow_" + i;
			cow.spaceShip 			= this.spaceShip;
			cows.Add(cow);
			pathCows.AddChild(cow);		
			cow.GlobalPosition 		= new Vector3(spwns[i].GlobalPosition.X, randomHeight, spwns[i].GlobalPosition.Z);	
		}
	}
	

	public void update_debug_information()
	{
		debugText = "";

		//add_to_debug_text("CowHandler - Level: " + this.level);
		//add_to_debug_text("CowHandler - nofCows: " + this.nofCows);
		//add_to_debug_text("Player - position: " + this.player.GlobalPosition);
		//add_to_debug_text("Player - score: " + this.player.score);
		//add_to_debug_text("Cows - amount: " + this.spaceShip.cows.Count);
		//add_to_debug_text("Cows - cowTarget: " + ((this.spaceShip.cowTarget != null) ? this.spaceShip.cowTarget.Name : "null"));

		text.Text = debugText;
	}

	public void add_to_debug_text(String a_text)
	{
		debugText = debugText + "\n" + a_text;
	}
}
