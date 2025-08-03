using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace vj0.Extensions.Animations.Properties;

public static class GradientAnimationService
{
    public static readonly AttachedProperty<bool> IsAnimatedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsAnimated", typeof(GradientAnimationService), inherits: true);

    public static readonly AttachedProperty<Color> AnimationStartColorProperty =
        AvaloniaProperty.RegisterAttached<Control, Color>("AnimationStartColor", typeof(GradientAnimationService), Colors.Black, inherits: true);

    public static readonly AttachedProperty<Color> AnimationEndColorProperty =
        AvaloniaProperty.RegisterAttached<Control, Color>("AnimationEndColor", typeof(GradientAnimationService), Colors.White, inherits: true);

    public static readonly AttachedProperty<double> AnimationDurationProperty =
        AvaloniaProperty.RegisterAttached<Control, double>("AnimationDuration", typeof(GradientAnimationService), 0.5, inherits: true);

    public static readonly AttachedProperty<int> AnimationStepsProperty =
        AvaloniaProperty.RegisterAttached<Control, int>("AnimationSteps", typeof(GradientAnimationService), 100, inherits: true);
    
    private static readonly AttachedProperty<GradientAnimator?> AnimatorInstanceProperty =
        AvaloniaProperty.RegisterAttached<Control, GradientAnimator?>("AnimatorInstance", typeof(GradientAnimationService));

    static GradientAnimationService()
    {
        IsAnimatedProperty.Changed.Subscribe(OnIsAnimatedChanged);
        AnimationStartColorProperty.Changed.Subscribe(OnAnimationStartColorChanged);
        AnimationEndColorProperty.Changed.Subscribe(OnAnimationEndColorChanged);
        AnimationDurationProperty.Changed.Subscribe(OnAnimationDurationChanged);
        AnimationStepsProperty.Changed.Subscribe(OnAnimationStepsChanged);
    }

    public static bool GetIsAnimated(Control control) => control.GetValue(IsAnimatedProperty);
    public static void SetIsAnimated(Control control, bool value) => control.SetValue(IsAnimatedProperty, value);

    public static Color GetAnimationStartColor(Control control) => control.GetValue(AnimationStartColorProperty);
    public static void SetAnimationStartColor(Control control, Color value) => control.SetValue(AnimationStartColorProperty, value);

    public static Color GetAnimationEndColor(Control control) => control.GetValue(AnimationEndColorProperty);
    public static void SetAnimationEndColor(Control control, Color value) => control.SetValue(AnimationEndColorProperty, value);

    public static double GetAnimationDuration(Control control) => control.GetValue(AnimationDurationProperty);
    public static void SetAnimationDuration(Control control, double value) => control.SetValue(AnimationDurationProperty, value);
    
    public static int GetAnimationSteps(Control control) => control.GetValue(AnimationStepsProperty);
    public static void SetAnimationSteps(Control control, int value) => control.SetValue(AnimationStepsProperty, value);

    private static GradientAnimator? GetAnimatorInstance(Control control) => control.GetValue(AnimatorInstanceProperty);
    private static void SetAnimatorInstance(Control control, GradientAnimator? value) => control.SetValue(AnimatorInstanceProperty, value);

    private static void OnIsAnimatedChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not Control control) return;
        
        if (e.GetNewValue<bool>())
        {
            EnsureAnimator(control);
        }
        else
        {
            StopAndDisposeAnimator(control);
        }
    }

    private static void OnAnimationStartColorChanged(AvaloniaPropertyChangedEventArgs<Color> e)
    {
        if (e.Sender is not Control control || !GetIsAnimated(control)) return;
        
        StopAndDisposeAnimator(control);
        EnsureAnimator(control);
    }

    private static void OnAnimationEndColorChanged(AvaloniaPropertyChangedEventArgs<Color> e)
    {
        if (e.Sender is not Control control || !GetIsAnimated(control)) return;
        
        StopAndDisposeAnimator(control);
        EnsureAnimator(control);
    }

    private static void OnAnimationDurationChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is not Control control || !GetIsAnimated(control)) return;
        
        StopAndDisposeAnimator(control);
        EnsureAnimator(control);
    }
    
    private static void OnAnimationStepsChanged(AvaloniaPropertyChangedEventArgs<int> e)
    {
        if (e.Sender is not Control control || !GetIsAnimated(control)) return;
        
        StopAndDisposeAnimator(control);
        EnsureAnimator(control);
    }

    private static void EnsureAnimator(Control control)
    {
        if (GetAnimatorInstance(control) != null) return;

        LinearGradientBrush? targetBrush = null;

        if (control is TextBlock textBlock && textBlock.Foreground is LinearGradientBrush fgBrush)
        {
            targetBrush = fgBrush;
        }
        else if (control is TemplatedControl templated && templated.Background is LinearGradientBrush bgBrush)
        {
            targetBrush = bgBrush;
        }

        if (targetBrush != null)
        {
            var startColor = GetAnimationStartColor(control);
            var endColor = GetAnimationEndColor(control);
            var duration = GetAnimationDuration(control);
            var steps = GetAnimationSteps(control);

            var animator = new GradientAnimator(targetBrush, startColor, endColor, duration, steps);
            SetAnimatorInstance(control, animator);

            control.AttachedToVisualTree += Control_AttachedToVisualTree;
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;

            if (control.GetVisualRoot() != null)
            {
                animator.StartAnimation();
            }
        }
    }

    private static void StopAndDisposeAnimator(Control control)
    {
        var animator = GetAnimatorInstance(control);
        if (animator != null)
        {
            animator.Dispose();
            SetAnimatorInstance(control, null);

            control.AttachedToVisualTree -= Control_AttachedToVisualTree;
            control.DetachedFromVisualTree -= Control_DetachedFromVisualTree;
        }
    }

    private static void Control_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            GetAnimatorInstance(control)?.StartAnimation();
        }
    }

    private static void Control_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            GetAnimatorInstance(control)?.StopAnimation();
        }
    }
}
