using Godot;
using System;

public partial class Main : Node2D
{
	[Export] public int NumberOfEnemies = 10;
	[Export] public PackedScene EnemyScene;
	public override void _Ready()
	{
		if (EnemyScene == null)
		{
			GD.PrintErr("EnemyScene not set.");
			return;
		}

		for (int i = 0; i < NumberOfEnemies; i++)
		{
			SpawnEnemy();
		}
	}

	private void SpawnEnemy()
	{
		Enemy boid = EnemyScene.Instantiate<Enemy>();
		boid.SetTarget(GetNode<RigidBody2D>("Player"));
		AddChild(boid);
	}
}
