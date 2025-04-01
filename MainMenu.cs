using Godot;
using System;

public partial class MainMenu : CanvasLayer
{
	// Экспортируемые ноды (убедитесь, что привязаны в редакторе)
	[Export] private Button _newGameButton;
	[Export] private Button _loadGameButton;
	[Export] private Button _settingsButton;
	[Export] private Button _galleryButton;
	[Export] private Button _quitButton;
	[Export] private Panel _settingsPanel; // Убедитесь, что это ваша панель настроек

	public override void _Ready()
	{
		// Подключаем сигналы кнопок
		_newGameButton.Pressed += OnNewGamePressed;
		_loadGameButton.Pressed += OnLoadGamePressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_galleryButton.Pressed += OnGalleryPressed;
		_quitButton.Pressed += OnQuitPressed;
		
		// Изначально скрываем панель настроек
		_settingsPanel.Hide();
		SetupSettingsPanelTheme();
	}

	private void SetupSettingsPanelTheme()
{
    var theme = new Theme();
    
    // Шрифт
    var font = new FontFile();
    font.LoadDynamicFont("res://fonts/Roboto-Regular.ttf");
    theme.DefaultFont = font;
    theme.DefaultFontSize = 24;
    
    // Стиль панели
    var panelStyle = new StyleBoxFlat();
    panelStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    panelStyle.BorderColor = new Color(0.5f, 0.5f, 0.5f);
    panelStyle.BorderWidthTop = 2;
    panelStyle.CornerRadiusBottomLeft = 10;
	panelStyle.CornerRadiusBottomRight = 10;
    theme.SetStylebox("panel", "Panel", panelStyle);
    
    // Применяем тему
    _settingsPanel.Theme = theme;
    
    // Центрируем текст
    foreach (Label label in _settingsPanel.GetTree().GetNodesInGroup("settings_labels"))
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }
}
private void OnSettingsPressed()
{
    _settingsPanel.Visible = !_settingsPanel.Visible;
    if (_settingsPanel.HasNode("PanelAnimator"))
    {
        var animator = _settingsPanel.GetNode<AnimationPlayer>("PanelAnimator");
        animator.Play(_settingsPanel.Visible ? "show" : "hide");
    }
    else
    {
        GD.PrintErr("AnimationPlayer 'PanelAnimator' не найден!");
    }
}

	private void OnNewGamePressed()
	{
		GD.Print("Нажата кнопка Новая игра");
		GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
	}

	private void OnLoadGamePressed()
	{
		GD.Print("Нажата кнопка Загрузить");
		// Здесь будет загрузка игры
	}

	private void OnGalleryPressed()
	{
		GD.Print("Нажата кнопка Галерея");
		// Здесь будет открытие галереи
	}

	private void OnQuitPressed()
	{
		GD.Print("Нажата кнопка Выход");
		GetTree().Quit();
	}
}
