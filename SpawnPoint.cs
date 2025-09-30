using Godot;
using System;

public partial class SpawnPoint : Area2D
{
	public bool Enabled = true;
	private int _bodyCount = 0;

	public override void _Ready()
	{
		BodyEntered += (Node2D body) =>
		{
			_bodyCount++;
			Enabled = false;
		};

		BodyExited += (Node2D body) =>
		{
			_bodyCount--;
			if (_bodyCount <= 0)
			{
				_bodyCount = 0;
				Enabled = true;
			}
		};
	}
}
