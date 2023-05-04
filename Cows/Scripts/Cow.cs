using Godot;
using System;

public partial class Cow : Node3D
{
	// are set int he spaceship
	[Export]
	public Node3D spaceShip;
	[Export]
	public SpaceShip spaceShipScript;

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
		
	}
	
}
