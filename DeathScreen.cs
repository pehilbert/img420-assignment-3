using Godot;
using System;

public partial class DeathScreen : CanvasLayer
{
	public override void _Ready()
	{
		var PlayAgainButton = GetNode<Button>("PlayButton");
		var MainMenuButton = GetNode<Button>("MainMenu");

		PlayAgainButton.Pressed += () =>
		{
			GetTree().ChangeSceneToFile("res://Level.tscn");
		};

		MainMenuButton.Pressed += () =>
		{
			GetTree().ChangeSceneToFile("res://main.tscn");
		};
	}
}
