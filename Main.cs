using Godot;
using System;

public partial class Main : Node2D
{
	[Export] public double EnemySpawnInterval = 2f;
	[Export] public double AsteroidSpawnInterval = 10f;
	[Export] public int StartingEnemies = 15;
	[Export] public int StartingAsteroids = 10;
    [Export] public PackedScene EnemyScene;
	[Export] public PackedScene AsteroidScene;

	public override void _Ready()
	{
		for (int i = 0; i < StartingEnemies; i++)
		{
			SpawnEnemy();
		}

		for (int i = 0; i < StartingAsteroids; i++)
		{
			SpawnAsteroid();
        }

        Timer enemyTimer = new Timer();
		enemyTimer.WaitTime = EnemySpawnInterval;
		enemyTimer.Autostart = true;
		enemyTimer.Timeout += () =>
		{
			SpawnEnemy();
			enemyTimer.Start();
		};
		AddChild(enemyTimer);

        Timer asteroidTimer = new Timer();
		asteroidTimer.WaitTime = AsteroidSpawnInterval;
		asteroidTimer.Autostart = true;
		asteroidTimer.Timeout += () =>
		{
			SpawnAsteroid();
			asteroidTimer.Start();
		};
		AddChild(asteroidTimer);

        enemyTimer.Start();
		asteroidTimer.Start();
    }

	private void SpawnEnemy()
	{
        if (EnemyScene == null)
        {
            GD.PrintErr("EnemyScene not set.");
            return;
        }

		var position = GetRandomSpawnPosition();
        Enemy enemy = EnemyScene.Instantiate<Enemy>();
        enemy.Position = position;
		enemy.SetTarget(GetNode<RigidBody2D>("Player"));
        AddChild(enemy);
    }

	private void SpawnAsteroid()
	{
		if (AsteroidScene == null)
		{
			GD.PrintErr("AsteroidScene not set.");
			return;
		}

        var position = GetRandomSpawnPosition();
        Asteroid asteroid = AsteroidScene.Instantiate<Asteroid>();
		asteroid.Position = position;
		AddChild(asteroid);
    }

	private Vector2 GetRandomSpawnPosition()
	{
		var viewportRect = GetViewportRect();
		var randomX = GD.Randf() * viewportRect.Size.X;
		var randomY = GD.Randf() * viewportRect.Size.Y;
		return new Vector2(randomX, randomY);
    }
}
