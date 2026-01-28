using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using System;
using System.Diagnostics;

namespace LumenRGB.UI.Avalonia.Mobile
{
    public class SwipeBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<bool> IsMenuOpenProperty =
            AvaloniaProperty.Register<SwipeBehavior, bool>(nameof(IsMenuOpen));

        public bool IsMenuOpen
        {
            get => GetValue(IsMenuOpenProperty);
            set => SetValue(IsMenuOpenProperty, value);
        }

        public static readonly StyledProperty<object> CommandProperty =
            AvaloniaProperty.Register<SwipeBehavior, object>(nameof(Command));

        public object Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        private Point _start;
        private bool _tracking;
        private bool _isDragging;
        private readonly Stopwatch _timer = new();

        private const double SwipeDistance = 70;
        private const double VerticalTolerance = 25;
        private const double TapTolerance = 15;
        private const int MaxTapTimeMs = 200;
        private const double MinSwipeVelocity = 0.35;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is null)
                return;

            AssociatedObject.PointerPressed += OnPressed;
            AssociatedObject.PointerMoved += OnMoved;
            AssociatedObject.PointerReleased += OnReleased;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is null)
                return;

            AssociatedObject.PointerPressed -= OnPressed;
            AssociatedObject.PointerMoved -= OnMoved;
            AssociatedObject.PointerReleased -= OnReleased;
        }

        private void OnPressed(object? sender, PointerPressedEventArgs e)
        {
            _start = e.GetPosition(AssociatedObject!);

            // Only start swipe-to-open if touch begins at left edge
            if (!IsMenuOpen && _start.X > 30)
            {
                _tracking = false;
                return;
            }

            _tracking = true;
            _isDragging = false;
            _timer.Restart();
        }

        private void OnMoved(object? sender, PointerEventArgs e)
        {
            if (!_tracking)
                return;

            var pos = e.GetPosition(AssociatedObject!);
            var deltaX = pos.X - _start.X;
            var deltaY = pos.Y - _start.Y;

            if (Math.Abs(deltaX) < TapTolerance && Math.Abs(deltaY) < TapTolerance)
                return;

            if (Math.Abs(deltaY) > VerticalTolerance)
            {
                _tracking = false;
                return;
            }

            _isDragging = true;

            var time = _timer.ElapsedMilliseconds;
            if (time == 0)
                return;

            var velocity = Math.Abs(deltaX) / time;

            // Swipe right to open
            if (!IsMenuOpen && deltaX > SwipeDistance && velocity > MinSwipeVelocity)
            {
                TriggerCommand();
                _tracking = false;
                return;
            }

            // Swipe left to close
            if (IsMenuOpen && deltaX < -SwipeDistance && velocity > MinSwipeVelocity)
            {
                TriggerCommand();
                _tracking = false;
                return;
            }
        }

        private void OnReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isDragging && _timer.ElapsedMilliseconds < MaxTapTimeMs)
            {
                _tracking = false;
                return;
            }

            _tracking = false;
            _timer.Stop();
        }

        private void TriggerCommand()
        {
            if (Command is System.Windows.Input.ICommand cmd && cmd.CanExecute(null))
                cmd.Execute(null);
        }
    }
}
