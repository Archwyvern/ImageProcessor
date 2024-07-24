using System;

namespace Archwyvern.Space2D.ImageProcessor;

public enum Easing
{
    Linear,
    InQuad,
    OutQuad,
    InOutQuad,
    InCubic,
    OutCubic,
    InOutCubic,
    InQuart,
    OutQuart,
    InOutQuart,
    InQuint,
    OutQuint,
    InOutQuint,
    InSine,
    OutSine,
    InOutSine,
    InExpo,
    OutExpo,
    InOutExpo,
    InCirc,
    OutCirc,
    InOutCirc,
    InElastic,
    OutElastic,
    InOutElastic,
    InBack,
    OutBack,
    InOutBack,
    InBounce,
    OutBounce,
    InOutBounce,
}

/// <summary>
/// https://gist.github.com/Kryzarel/bba64622057f21a1d6d44879f9cd7bd4
/// </summary>
public static class EasingFunctionsD
{
    public static double Ease(this Easing easing, double t)
    {
        ArgumentNullException.ThrowIfNull(easing, nameof(easing));

        return easing switch {
            Easing.Linear => Linear(t),
            Easing.InQuad => InQuad(t),
            Easing.OutQuad => OutQuad(t),
            Easing.InOutQuad => InOutQuad(t),
            Easing.InCubic => InCubic(t),
            Easing.OutCubic => OutCubic(t),
            Easing.InOutCubic => InOutCubic(t),
            Easing.InQuart => InQuart(t),
            Easing.OutQuart => OutQuart(t),
            Easing.InOutQuart => InOutQuart(t),
            Easing.InQuint => InQuint(t),
            Easing.OutQuint => OutQuint(t),
            Easing.InOutQuint => InOutQuint(t),
            Easing.InSine => InSine(t),
            Easing.OutSine => OutSine(t),
            Easing.InOutSine => InOutSine(t),
            Easing.InExpo => InExpo(t),
            Easing.OutExpo => OutExpo(t),
            Easing.InOutExpo => InOutExpo(t),
            Easing.InCirc => InCirc(t),
            Easing.OutCirc => OutCirc(t),
            Easing.InOutCirc => InOutCirc(t),
            Easing.InElastic => InElastic(t),
            Easing.OutElastic => OutElastic(t),
            Easing.InOutElastic => InOutElastic(t),
            Easing.InBack => InBack(t),
            Easing.OutBack => OutBack(t),
            Easing.InOutBack => InOutBack(t),
            Easing.InBounce => InBounce(t),
            Easing.OutBounce => OutBounce(t),
            Easing.InOutBounce => InOutBounce(t),
            _ => throw new ArgumentException(null, nameof(easing))
        };
    }

    public static double Linear(double t) => t;

    public static double InQuad(double t) => t * t;
    public static double OutQuad(double t) => 1 - InQuad(1 - t);
    public static double InOutQuad(double t)
    {
        if (t < 0.5) return InQuad(t * 2) / 2;
        return 1 - InQuad((1 - t) * 2) / 2;
    }

    public static double InCubic(double t) => t * t * t;
    public static double OutCubic(double t) => 1 - InCubic(1 - t);
    public static double InOutCubic(double t)
    {
        if (t < 0.5) return InCubic(t * 2) / 2;
        return 1 - InCubic((1 - t) * 2) / 2;
    }

    public static double InQuart(double t) => t * t * t * t;
    public static double OutQuart(double t) => 1 - InQuart(1 - t);
    public static double InOutQuart(double t)
    {
        if (t < 0.5) return InQuart(t * 2) / 2;
        return 1 - InQuart((1 - t) * 2) / 2;
    }

    public static double InQuint(double t) => t * t * t * t * t;
    public static double OutQuint(double t) => 1 - InQuint(1 - t);
    public static double InOutQuint(double t)
    {
        if (t < 0.5) return InQuint(t * 2) / 2;
        return 1 - InQuint((1 - t) * 2) / 2;
    }

    public static double InSine(double t) => -Math.Cos(t * Math.PI / 2);
    public static double OutSine(double t) => Math.Sin(t * Math.PI / 2);
    public static double InOutSine(double t) => (Math.Cos(t * Math.PI) - 1) / -2;

    public static double InExpo(double t) => Math.Pow(2, 10 * (t - 1));
    public static double OutExpo(double t) => 1 - InExpo(1 - t);
    public static double InOutExpo(double t)
    {
        if (t < 0.5) return InExpo(t * 2) / 2;
        return 1 - InExpo((1 - t) * 2) / 2;
    }

    public static double InCirc(double t) => -(Math.Sqrt(1 - t * t) - 1);
    public static double OutCirc(double t) => 1 - InCirc(1 - t);
    public static double InOutCirc(double t)
    {
        if (t < 0.5) return InCirc(t * 2) / 2;
        return 1 - InCirc((1 - t) * 2) / 2;
    }

    public static double InElastic(double t) => 1 - OutElastic(1 - t);
    public static double OutElastic(double t)
    {
        double p = 0.3;
        return Math.Pow(2, -10 * t) * Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
    }
    public static double InOutElastic(double t)
    {
        if (t < 0.5) return InElastic(t * 2) / 2;
        return 1 - InElastic((1 - t) * 2) / 2;
    }

    public static double InBack(double t)
    {
        double s = 1.70158;
        return t * t * ((s + 1) * t - s);
    }
    public static double OutBack(double t) => 1 - InBack(1 - t);
    public static double InOutBack(double t)
    {
        if (t < 0.5) return InBack(t * 2) / 2;
        return 1 - InBack((1 - t) * 2) / 2;
    }

    public static double InBounce(double t) => 1 - OutBounce(1 - t);
    public static double OutBounce(double t)
    {
        double div = 2.75f;
        double mult = 7.5625f;

        if (t < 1 / div)
        {
            return mult * t * t;
        }
        else if (t < 2 / div)
        {
            t -= 1.5f / div;
            return mult * t * t + 0.75f;
        }
        else if (t < 2.5 / div)
        {
            t -= 2.25f / div;
            return mult * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / div;
            return mult * t * t + 0.984375f;
        }
    }
    public static double InOutBounce(double t)
    {
        if (t < 0.5) return InBounce(t * 2) / 2;
        return 1 - InBounce((1 - t) * 2) / 2;
    }
}


/// <summary>
/// https://gist.github.com/Kryzarel/bba64622057f21a1d6d44879f9cd7bd4
/// </summary>
public static class EasingFunctionsF
{
    public static float Ease(this Easing easing, float t)
    {
        ArgumentNullException.ThrowIfNull(easing, nameof(easing));

        return easing switch {
            Easing.Linear => Linear(t),
            Easing.InQuad => InQuad(t),
            Easing.OutQuad => OutQuad(t),
            Easing.InOutQuad => InOutQuad(t),
            Easing.InCubic => InCubic(t),
            Easing.OutCubic => OutCubic(t),
            Easing.InOutCubic => InOutCubic(t),
            Easing.InQuart => InQuart(t),
            Easing.OutQuart => OutQuart(t),
            Easing.InOutQuart => InOutQuart(t),
            Easing.InQuint => InQuint(t),
            Easing.OutQuint => OutQuint(t),
            Easing.InOutQuint => InOutQuint(t),
            Easing.InSine => InSine(t),
            Easing.OutSine => OutSine(t),
            Easing.InOutSine => InOutSine(t),
            Easing.InExpo => InExpo(t),
            Easing.OutExpo => OutExpo(t),
            Easing.InOutExpo => InOutExpo(t),
            Easing.InCirc => InCirc(t),
            Easing.OutCirc => OutCirc(t),
            Easing.InOutCirc => InOutCirc(t),
            Easing.InElastic => InElastic(t),
            Easing.OutElastic => OutElastic(t),
            Easing.InOutElastic => InOutElastic(t),
            Easing.InBack => InBack(t),
            Easing.OutBack => OutBack(t),
            Easing.InOutBack => InOutBack(t),
            Easing.InBounce => InBounce(t),
            Easing.OutBounce => OutBounce(t),
            Easing.InOutBounce => InOutBounce(t),
            _ => throw new ArgumentException(null, nameof(easing))
        };
    }

    public static float Linear(float t) => t;

    public static float InQuad(float t) => t * t;
    public static float OutQuad(float t) => 1 - InQuad(1 - t);
    public static float InOutQuad(float t)
    {
        if (t < 0.5) return InQuad(t * 2) / 2;
        return 1 - InQuad((1 - t) * 2) / 2;
    }

    public static float InCubic(float t) => t * t * t;
    public static float OutCubic(float t) => 1 - InCubic(1 - t);
    public static float InOutCubic(float t)
    {
        if (t < 0.5) return InCubic(t * 2) / 2;
        return 1 - InCubic((1 - t) * 2) / 2;
    }

    public static float InQuart(float t) => t * t * t * t;
    public static float OutQuart(float t) => 1 - InQuart(1 - t);
    public static float InOutQuart(float t)
    {
        if (t < 0.5) return InQuart(t * 2) / 2;
        return 1 - InQuart((1 - t) * 2) / 2;
    }

    public static float InQuint(float t) => t * t * t * t * t;
    public static float OutQuint(float t) => 1 - InQuint(1 - t);
    public static float InOutQuint(float t)
    {
        if (t < 0.5) return InQuint(t * 2) / 2;
        return 1 - InQuint((1 - t) * 2) / 2;
    }

    public static float InSine(float t) => -MathF.Cos(t * MathF.PI / 2);
    public static float OutSine(float t) => MathF.Sin(t * MathF.PI / 2);
    public static float InOutSine(float t) => (MathF.Cos(t * MathF.PI) - 1) / -2;

    public static float InExpo(float t) => MathF.Pow(2, 10 * (t - 1));
    public static float OutExpo(float t) => 1 - InExpo(1 - t);
    public static float InOutExpo(float t)
    {
        if (t < 0.5) return InExpo(t * 2) / 2;
        return 1 - InExpo((1 - t) * 2) / 2;
    }

    public static float InCirc(float t) => -(MathF.Sqrt(1 - t * t) - 1);
    public static float OutCirc(float t) => 1 - InCirc(1 - t);
    public static float InOutCirc(float t)
    {
        if (t < 0.5) return InCirc(t * 2) / 2;
        return 1 - InCirc((1 - t) * 2) / 2;
    }

    public static float InElastic(float t) => 1 - OutElastic(1 - t);
    public static float OutElastic(float t)
    {
        float p = 0.3f;
        return MathF.Pow(2, -10 * t) * MathF.Sin((t - p / 4) * (2 * MathF.PI) / p) + 1;
    }
    public static float InOutElastic(float t)
    {
        if (t < 0.5) return InElastic(t * 2) / 2;
        return 1 - InElastic((1 - t) * 2) / 2;
    }

    public static float InBack(float t)
    {
        float s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }
    public static float OutBack(float t) => 1 - InBack(1 - t);
    public static float InOutBack(float t)
    {
        if (t < 0.5) return InBack(t * 2) / 2;
        return 1 - InBack((1 - t) * 2) / 2;
    }

    public static float InBounce(float t) => 1 - OutBounce(1 - t);
    public static float OutBounce(float t)
    {
        float div = 2.75f;
        float mult = 7.5625f;

        if (t < 1 / div)
        {
            return mult * t * t;
        }
        else if (t < 2 / div)
        {
            t -= 1.5f / div;
            return mult * t * t + 0.75f;
        }
        else if (t < 2.5 / div)
        {
            t -= 2.25f / div;
            return mult * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / div;
            return mult * t * t + 0.984375f;
        }
    }
    public static float InOutBounce(float t)
    {
        if (t < 0.5) return InBounce(t * 2) / 2;
        return 1 - InBounce((1 - t) * 2) / 2;
    }
}