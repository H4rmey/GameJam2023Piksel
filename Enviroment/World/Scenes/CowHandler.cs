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
	public NodePath pathSpawnpoints;
	[Export]
	public Area3D spaceShip;
	[Export]
	public SpaceShip spaceShipScript;
	[Export]
	public Player player;
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
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		timer = new Timer(); 
		AddChild(timer);
		timer.Autostart 	= true;
		timer.OneShot 		= true;
		timer.Timeout 		+= OnTimerTimeout;

		for (int i = 0; i < GetNode<Node3D>(pathSpawnpoints).GetChildCount(); i++) 
		{
			spwns.Add(GetNode<Node3D>(pathSpawnpoints).GetChild<Node3D>(i));
 		}

		SpawnCows();
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

	private void OnTimerTimeout()
	{
		LeveUp();
	}

	private void LeveUp()
	{
		level++;
		RemoveCows();
		SpawnCows();
		spaceShipScript.Reset();
		player.score = 0;
	}

	private void RemoveCows()
	{
		foreach (Cow cow in spaceShipScript.cows)
		{
			cow.QueueFree();
		}
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

		for (int i = 0; i < nofCows; i++) 
		{
			float randomHeight = random.RandfRange(3, 6);
			
			PackedScene cowScene 	= ResourceLoader.Load<PackedScene>("res://Cows/Scenes/Cow.tscn");
			Cow cow 				= (Cow)cowScene.Instantiate();
			cow.Name 				= "Cow_" + i;
			cow.spaceShipScript 	= spaceShip as SpaceShip;
			cow.spaceShip 			= spaceShip;
			pathCows.AddChild(cow);		
			cow.GlobalPosition 		= new Vector3(spwns[i].GlobalPosition.X, randomHeight, spwns[i].GlobalPosition.Z);	
		}
	}
	

	public void update_debug_information()
	{
		debugText = "";

		add_to_debug_text("CowHandler - Level: " + this.level);
		add_to_debug_text("CowHandler - nofCows: " + this.nofCows);
		add_to_debug_text("Player - position: " + this.player.GlobalPosition);
		add_to_debug_text("Player - score: " + this.player.score);
		add_to_debug_text("Cows - amount: " + this.spaceShipScript.cows.Count);
		add_to_debug_text("Cows - cowTarget: " + ((this.spaceShipScript.cowTarget != null) ? this.spaceShipScript.cowTarget.Name : "null"));

		text.Text = debugText;
	}

	public void add_to_debug_text(String a_text)
	{
		debugText = debugText + "\n" + a_text;
	}
	
	private void SetCowScript(int i)
	{
		//set cow script
		Cow cowScript = cows[i] as Cow;
		cowScript.spaceShipScript = spaceShip as SpaceShip;
		cowScript.spaceShip = spaceShip;
	}	
}
