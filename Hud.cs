using Godot;
using System;

public partial class Hud : CanvasLayer
{
	private Level _level;
	private Player _player;
	private EntityManager _playerEntityManager;
	private Label _scoreLabel;
	private Label _healthLabel;

	public override void _Ready()
	{
		base._Ready();

		_level = GetNode<Level>("/root/Level");
		_player = GetNode<Player>("/root/Level/Player");
		_playerEntityManager = GetNode<EntityManager>("/root/Level/Player/EntityManager");
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_healthLabel = GetNode<Label>("HealthLabel");

		_level.ScoreChanged += (int newScore) => {
			_scoreLabel.Text = $"Score: {newScore}";
		};

		_playerEntityManager.HealthChanged += (double currentHealth, double maxHealth) => {
			_healthLabel.Text = $"Health: {currentHealth} / {maxHealth}";
		};
	}
}
