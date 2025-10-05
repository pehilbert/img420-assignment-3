using Godot;
using System;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		var PlayButton = GetNode<Button>("CanvasLayer/PlayButton");

		PlayButton.Pressed += () =>
		{
			GetTree().ChangeSceneToFile("res://Level.tscn");
		};
	}
}
