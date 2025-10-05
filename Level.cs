using Godot;
using System;

public partial class Level : Node2D
{
	[Export] public double EnemySpawnInterval = 5f;
	[Export] public double AsteroidSpawnInterval = 10f;
	[Export] public int StartingEnemies = 5;
	[Export] public int StartingAsteroids = 12;
	[Export] public double OffScreenSpawnMargin = 50f;
	[Export] public int ScoreRampUpThreshold = 1000;
	[Export] public double EnemyIntervalRampUp = 0.2f;
	[Export] public double MinEnemySpawnInterval = 1.0f;
	[Export] public double MinAsteroidVelocity = 10f;
	[Export] public double MaxAsteroidVelocity = 50f;
	[Export] public PackedScene EnemyScene;
	[Export] public PackedScene AsteroidScene;
	[Export] public int Score = 0;
	private Player _player;
	private Timer _enemyTimer;
	private Timer _asteroidTimer;
	private CanvasLayer _deathScreen;

	[Signal]
	public delegate void ScoreChangedEventHandler(int newScore);

	public override void _Ready()
	{
		_player = GetNode<Player>("Player");

		for (int i = 0; i < StartingEnemies; i++)
		{
			SpawnEnemy();
		}

		for (int i = 0; i < StartingAsteroids; i++)
		{
			SpawnAsteroid();
		}

		_enemyTimer = new Timer();
		_enemyTimer.WaitTime = EnemySpawnInterval;
		_enemyTimer.Autostart = true;
		_enemyTimer.Timeout += () =>
		{
			SpawnEnemy();
			_enemyTimer.Start();
		};
		AddChild(_enemyTimer);

		_asteroidTimer = new Timer();
		_asteroidTimer.WaitTime = AsteroidSpawnInterval;
		_asteroidTimer.Autostart = true;
		_asteroidTimer.Timeout += () =>
		{
			SpawnAsteroid();
			_asteroidTimer.Start();
		};
		AddChild(_asteroidTimer);

		_enemyTimer.Start();
		_asteroidTimer.Start();

		_deathScreen = GetNode<CanvasLayer>("DeathScreen");
		_deathScreen.Visible = false;

		var playerEntityManager = _player.GetNodeOrNull<EntityManager>("EntityManager");
		if (playerEntityManager != null)
		{
			playerEntityManager.Died += (Node entityDied) =>
			{
				var scoreLabel = _deathScreen.GetNode<Label>("ScoreLabel");
				scoreLabel.Text = $"Score: {Score}";

				_deathScreen.Visible = true;
				_enemyTimer.Stop();
				_asteroidTimer.Stop();
			};
		}
	}

	public void AddScore(int points)
	{
		Score += points;
		EmitSignal(SignalName.ScoreChanged, Score);

		int wave = Score / ScoreRampUpThreshold;
		EnemySpawnInterval = Math.Max(MinEnemySpawnInterval, EnemySpawnInterval - (EnemyIntervalRampUp * wave));
		_enemyTimer.WaitTime = EnemySpawnInterval;
	}

	public void SetScore(int newScore)
	{
		Score = newScore;
		EmitSignal(SignalName.ScoreChanged, Score);
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
		enemy.SetTarget(_player);

		var entityManager = enemy.GetNodeOrNull<EntityManager>("EntityManager");
		if (entityManager != null)
		{
			entityManager.Died += (Node entityDied) =>
			{
				if (entityDied is Enemy deadEnemy)
				{
					AddScore(deadEnemy.PointValue);
				}
			};
		}

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

		// Start the asteroid with very small velocity in random direction (assisted by copilot)
		asteroid.LinearVelocity = new Vector2((float)GD.RandRange(MinAsteroidVelocity, MaxAsteroidVelocity), 0)
			.Rotated(GD.Randf() * MathF.PI * 2);

		AddChild(asteroid);
	}

	private Vector2 GetRandomSpawnPosition()
	{
		var viewportRect = GetViewportRect();
		var randomX = GD.Randf() * (float)(viewportRect.Size.X + OffScreenSpawnMargin);
		var randomY = GD.Randf() * (float)(viewportRect.Size.Y + OffScreenSpawnMargin);
		return new Vector2(randomX, randomY);
	}
}
