namespace Regulas.MauiApp.Controls;

// The animated Leo sky. Twinkling runs on a dispatcher timer that always stops
// when the view leaves the tree, so no page keeps painting in the background.
// Tapping a star selects it and reports its name for the page to show.
public sealed class StarFieldView : GraphicsView
{
    private const double FrameSeconds = 0.05;
    private const double TapTolerance = 0.07;

    public static readonly BindableProperty AnimatedProperty =
        BindableProperty.Create(nameof(Animated), typeof(bool), typeof(StarFieldView), true, propertyChanged: OnAnimatedChanged);

    public static readonly BindableProperty SelectedStarNameProperty =
        BindableProperty.Create(nameof(SelectedStarName), typeof(string), typeof(StarFieldView), string.Empty);

    // Shown when nothing is selected, so the readout line is never blank.
    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(StarFieldView), string.Empty, propertyChanged: OnEmptyTextChanged);

    private readonly StarFieldDrawable _sky = new();
    private IDispatcherTimer? _timer;
    private double _seconds;
    private int? _selected;

    public StarFieldView()
    {
        Drawable = _sky;
        StartInteraction += OnStartInteraction;
        Loaded += (_, _) => Start();
        Unloaded += (_, _) => Stop();
    }

    public bool Animated
    {
        get => (bool)GetValue(AnimatedProperty);
        set => SetValue(AnimatedProperty, value);
    }

    public string SelectedStarName
    {
        get => (string)GetValue(SelectedStarNameProperty);
        private set => SetValue(SelectedStarNameProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    private static void OnEmptyTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (StarFieldView)bindable;
        view.Select(view._selected);
    }

    private static void OnAnimatedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (StarFieldView)bindable;
        if ((bool)newValue)
        {
            view.Start();
            return;
        }
        view.Stop();
    }

    private void Start()
    {
        if (_timer is not null || !Animated)
        {
            return;
        }
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(FrameSeconds);
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void Stop()
    {
        if (_timer is null)
        {
            return;
        }
        _timer.Tick -= OnTick;
        _timer.Stop();
        _timer = null;
    }

    private void OnTick(object? sender, EventArgs args)
    {
        _seconds += FrameSeconds;
        _sky.Seconds = _seconds;
        Invalidate();
    }

    // Taps are matched in the same box the figure is drawn in, so a star is
    // always where it looks.
    private void OnStartInteraction(object? sender, TouchEventArgs args)
    {
        var touch = args.Touches.FirstOrDefault();
        if (Width <= 0 || Height <= 0)
        {
            return;
        }
        var figure = SkyLayout.Figure(new RectF(0, 0, (float)Width, (float)Height));
        var (x, y) = SkyLayout.Normalize(figure, touch.X, touch.Y);
        Select(LeoConstellation.NearestIndex(x, y, TapTolerance));
    }

    // A tap that misses every star clears the selection instead of doing nothing.
    private void Select(int? index)
    {
        _selected = index;
        _sky.SelectedIndex = index;
        SelectedStarName = index is int found ? LeoConstellation.Stars[found].Name : EmptyText;
        Invalidate();
    }
}
