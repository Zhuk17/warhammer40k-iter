using Godot;
using System;

public partial class SettingsPanel : Panel
{
    [Export] private CheckBox _fullscreenCheckbox;
    [Export] private OptionButton _resolutionDropdown;
    [Export] private HSlider _volumeSlider;
    [Export] private Button _exitButton;

    private Vector2I[] _resolutions = {
        new Vector2I(1920, 1080),
        new Vector2I(1366, 768),
        new Vector2I(1280, 720)
    };



    public override void _Ready()
    {
        InitResolutionDropdown();
        _exitButton.Pressed += OnExitPressed;

        // Настройка Tooltip
        _volumeSlider.TooltipText = "Громкость: " + (_volumeSlider.Value / 10) + "%";

        // Динамическое обновление
        _volumeSlider.ValueChanged += (value) =>
        {
            // Обновление звука
            AudioServer.SetBusVolumeDb(
                AudioServer.GetBusIndex("Master"),
                Mathf.LinearToDb((float)value / 1000f)
            );

            // Обновление Tooltip
            _volumeSlider.TooltipText = "Громкость: " + (value / 10) + "%";
        };

        // Создаем Label для отображения значения громкости
        var volumeLabel = new Label();
        volumeLabel.Name = "VolumeValue";
        volumeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        volumeLabel.VerticalAlignment = VerticalAlignment.Center;

        // После создания volumeLabel
        CallDeferred("UpdateVolumeLabelPosition");

        // Настраиваем стиль
        volumeLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        volumeLabel.AddThemeFontSizeOverride("font_size", 16);

        // Добавляем Label к HSlider
        _volumeSlider.AddChild(volumeLabel);

        // Позиционируем над ползунком
        volumeLabel.Position = new Vector2(
            _volumeSlider.Size.X / 2 - volumeLabel.Size.X / 2,
            -25
        );

        // Обновляем обработчик
        _volumeSlider.ValueChanged += (value) =>
        {
            // Обновляем звук
            AudioServer.SetBusVolumeDb(
                AudioServer.GetBusIndex("Master"),
                Mathf.LinearToDb((float)value / 1000f)
            );

            // Обновляем Label
            volumeLabel.Text = $"{value / 10}%";

            // Обновляем Tooltip
            _volumeSlider.TooltipText = $"Текущая громкость: {value / 10}%";
        };

        // Инициализируем начальное значение
        volumeLabel.Text = $"{_volumeSlider.Value / 10}%";

        _exitButton.Pressed += OnExitPressed;

        LoadSettings();
    }

    // И новый метод
    private void UpdateVolumeLabelPosition()
    {
        var label = _volumeSlider.GetNode<Label>("VolumeValue");
        label.Position = new Vector2(
            _volumeSlider.Size.X / 2 - label.Size.X / 2,
            -25
        );
    }



    private void InitResolutionDropdown()
    {
        _resolutionDropdown.Clear();

        // Получаем список поддерживаемых разрешений
        var supportedResolutions = GetSupportedResolutions();

        // Добавляем только поддерживаемые
        foreach (var res in supportedResolutions)
        {
            _resolutionDropdown.AddItem($"{res.X}x{res.Y}");
        }

        // Выбираем текущее разрешение
        var currentRes = DisplayServer.WindowGetSize();
        for (int i = 0; i < supportedResolutions.Length; i++)
        {
            if (supportedResolutions[i] == currentRes)
            {
                _resolutionDropdown.Select(i);
                break;
            }
        }
    }

    private Vector2I[] GetSupportedResolutions()
    {
        // Здесь можно реализовать реальную проверку через DisplayServer
        // Пока используем базовый список с фильтрацией
        var baseResolutions = new Vector2I[] {
        new Vector2I(1920, 1080),
        new Vector2I(1600, 900),
        new Vector2I(1366, 768),
        new Vector2I(1280, 720),
        new Vector2I(1024, 768)
    };

        // Фильтруем слишком большие разрешения
        var maxRes = DisplayServer.ScreenGetSize();
        var result = new System.Collections.Generic.List<Vector2I>();

        foreach (var res in baseResolutions)
        {
            if (res.X <= maxRes.X && res.Y <= maxRes.Y)
            {
                result.Add(res);
            }
        }

        return result.ToArray();
    }

    private void LoadSettings()
    {
        var config = new ConfigFile();
        var err = config.Load("user://settings.cfg");

        if (err == Error.Ok)
        {
            // Видео
            _fullscreenCheckbox.ButtonPressed = (bool)config.GetValue("video", "fullscreen", true);

            // Звук
            var volume = (float)config.GetValue("audio", "volume", 0.8f);
            _volumeSlider.Value = volume * 1000;

            // Разрешение
            var resIndex = (int)config.GetValue("video", "resolution", 0);
            if (resIndex < _resolutionDropdown.ItemCount)
            {
                _resolutionDropdown.Select(resIndex);
            }
        }
        else
        {
            // Настройки по умолчанию
            _fullscreenCheckbox.ButtonPressed = true;
            _volumeSlider.Value = 800; // 80%
        }

        // Применяем настройки звука сразу
        AudioServer.SetBusVolumeDb(
            AudioServer.GetBusIndex("Master"),
            Mathf.LinearToDb((float)_volumeSlider.Value / 1000f)
        );
    }

    private void SaveSettings()
    {
        var config = new ConfigFile();

        // Видео
        config.SetValue("video", "fullscreen", _fullscreenCheckbox.ButtonPressed);
        config.SetValue("video", "resolution", _resolutionDropdown.Selected);

        // Аудио
        config.SetValue("audio", "volume", _volumeSlider.Value / 1000f);

        config.Save("user://settings.cfg");

        // Применение изменений
        ApplySettings();
    }

    private void ApplySettings()
    {
        // Видео
        DisplayServer.WindowSetMode(
            _fullscreenCheckbox.ButtonPressed ?
                DisplayServer.WindowMode.Fullscreen :
                DisplayServer.WindowMode.Windowed
        );
        var selectedRes = GetSupportedResolutions()[_resolutionDropdown.Selected];
        DisplayServer.WindowSetSize(selectedRes);

        // Аудио (пример для главного микшера)
        AudioServer.SetBusVolumeDb(
            AudioServer.GetBusIndex("Master"),
            Mathf.LinearToDb((float)_volumeSlider.Value / 1000f)
        );
    }


private void OnExitPressed()
{
    SaveSettings();

    if (HasNode("PanelAnimator"))
    {
        var animator = GetNode<AnimationPlayer>("PanelAnimator");
        animator.Play("hide");
        animator.AnimationFinished += (_) => Hide();
    }
    else
    {
        Hide();
    }
}

}