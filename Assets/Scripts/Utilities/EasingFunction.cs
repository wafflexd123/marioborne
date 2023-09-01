using System;
using UnityEngine;

public static class EasingFunction
{
    public static readonly Func<float, float> Linear = (float percent) => percent;

    public static readonly Func<float, float> EaseInSine = (float percent) => 1 - Mathf.Cos((percent * Mathf.PI) / 2);
    public static readonly Func<float, float> EaseOutSine = (float percent) => 1 - Mathf.Sin((percent * Mathf.PI) / 2);
    public static readonly Func<float, float> EaseInOutSine = (float percent) => -(Mathf.Cos(Mathf.PI * percent) - 1) / 2;

    public static readonly Func<float, float> EaseInCubic = (float percent) => percent * percent * percent;
    public static readonly Func<float, float> EaseOutCubic = (float percent) => 1 - Mathf.Pow(1 - percent, 3);
    public static readonly Func<float, float> EaseInOutCubic = (float percent) => percent < 0.5 ? 4 * percent * percent * percent : 1 - Mathf.Pow(-2 * percent + 2, 3) / 2;

    public static readonly Func<float, float> EaseInBounce = (float percent) => 1 - EaseOutBounce(1 - percent);
    public static readonly Func<float, float> EaseOutBounce = (float percent) => percent < 1 / 2.75f ? 7.5625f * percent * percent : percent < 2 / 2.75f ? 7.5625f * (percent -= 1.5f / 2.75f) * percent + 0.75f : percent < 2.5 / 2.75f ? 7.5625f * (percent -= 2.25f / 2.75f) * percent + 0.9375f : 7.5625f * (percent -= 2.625f / 2.75f) * percent + 0.984375f;
    public static readonly Func<float, float> EaseInOutBounce = (float percent) => percent < 0.5 ? (1 - EaseOutBounce(1 - 2 * percent)) / 2 : (1 + EaseOutBounce(2 * percent - 1)) / 2;

	public static readonly Func<float, float> EaseInExpo = (float percent) => percent == 0 ? 0 : Mathf.Pow(2, 10 * percent - 10);
	public static readonly Func<float, float> EaseOutExpo = (float percent) => percent == 1 ? 1 : 1 - Mathf.Pow(2, -10 * percent);
	public static readonly Func<float, float> EaseInOutExpo = (float percent) => percent == 0 ? 0 : percent == 1 ? 1 : percent < 0.5 ? Mathf.Pow(2, 20 * percent - 10) / 2 : (2 - Mathf.Pow(2, -20 * percent + 10)) / 2;

	public enum Enum { Linear, EaseInSine, EaseOutSine, EaseInOutSine, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInExpo, EaseOutExpo, EaseInOutExpo }
	static Func<float, float>[] array = { Linear, EaseInSine, EaseOutSine, EaseInOutSine, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInExpo, EaseOutExpo, EaseInOutExpo };

	public static Func<float, float> Get(Enum e)
	{
		return array[(int)e];
	}
}