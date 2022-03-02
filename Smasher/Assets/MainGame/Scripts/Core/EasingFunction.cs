using UnityEngine;
public static class EasingFunction
{
	public enum Ease
	{
		EaseInQuad = 0,
		EaseOutQuad,
		EaseInOutQuad,
		EaseInCubic,
		EaseOutCubic,
		EaseInOutCubic,
		EaseInQuart,
		EaseOutQuart,
		EaseInOutQuart,
		EaseInQuint,
		EaseOutQuint,
		EaseInOutQuint,
		EaseInSine,
		EaseOutSine,
		EaseInOutSine,
		EaseInExpo,
		EaseOutExpo,
		EaseInOutExpo,
		EaseInCirc,
		EaseOutCirc,
		EaseInOutCirc,
		Linear,
		Spring,
		EaseInBounce,
		EaseOutBounce,
		EaseInOutBounce,
		EaseInBack,
		EaseOutBack,
		EaseInOutBack,
		EaseInElastic,
		EaseOutElastic,
		EaseInOutElastic,
	}

	private const float c1 = 1.70158f;
	private const float c2 = c1 * 1.525f;
	private const float c3 = c1 + 1;
	private const float c4 = (2 * Mathf.PI) / 3f;
	private const float c5 = (2 * Mathf.PI) / 4.5f;

	public static float Linear(float value)
	{
		return value;
	}

	public static float EaseInQuad(float value)
	{
		return value * value;
	}

	public static float EaseOutQuad(float value)
	{
		return 1 - (1 - value) * (1 - value);
	}

	public static float EaseInOutQuad(float value)
	{
		return value < 0.5 ? 2 * value * value : 1 - Mathf.Pow(-2 * value + 2, 2) / 2;
	}

	public static float EaseInCubic(float value)
	{
		return value * value * value;
	}

	public static float EaseOutCubic(float value)
	{
		return 1 - Mathf.Pow(1 - value, 3);
	}

	public static float EaseInOutCubic(float value)
	{
		return value < 0.5f ? 4 * value * value * value : 1 - Mathf.Pow(-2 * value + 2, 3) / 2;
	}

	public static float EaseInQuart(float value)
	{
		return value * value * value * value;
	}

	public static float EaseOutQuart(float value)
	{
		return 1 - Mathf.Pow(1 - value, 4);
	}

	public static float EaseInOutQuart(float value)
	{
		return value < 0.5f ? 8 * value * value * value * value : 1 - Mathf.Pow(-2 * value + 2, 4) / 2;
	}

	public static float EaseInQuint(float value)
	{
		return value * value * value * value * value;
	}

	public static float EaseOutQuint(float value)
	{
		return 1 - Mathf.Pow(1 - value, 5);
	}

	public static float EaseInOutQuint(float value)
	{
		return value < 0.5 ? 16 * value * value * value * value * value : 1 - Mathf.Pow(-2 * value + 2, 5) / 2;
	}

	public static float EaseInSine(float value)
	{
		return 1 - Mathf.Cos((value * Mathf.PI) / 2);
	}

	public static float EaseOutSine(float value)
	{
		return Mathf.Sin((value * Mathf.PI) / 2);
	}

	public static float EaseInOutSine(float value)
	{
		return -(Mathf.Cos(Mathf.PI * value) - 1) / 2;
	}

	public static float EaseInExpo(float value)
	{
		return value == 0 ? 0 : Mathf.Pow(2, 10 * value - 10);
	}

	public static float EaseOutExpo(float value)
	{
		return value == 1 ? 1 : 1 - Mathf.Pow(2, -10 * value);
	}

	public static float EaseInOutExpo(float value)
	{
		return value == 0 ? 0 : value == 1 ? 1 : value < 0.5 ? Mathf.Pow(2, 20 * value - 10) / 2 : (2 - Mathf.Pow(2, -20 * value + 10)) / 2;
	}

	public static float EaseInCirc(float value)
	{
		return 1 - Mathf.Sqrt(1 - Mathf.Pow(value, 2));
	}

	public static float EaseOutCirc(float value)
	{
		return Mathf.Sqrt(1 - Mathf.Pow(value - 1, 2));
	}

	public static float EaseInOutCirc(float value)
	{
		return value < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * value, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * value + 2, 2)) + 1) / 2;
	}

	public static float EaseInBounce(float value)
	{
		return 1 - EaseOutBounce(1 - value);
	}

	public static float EaseOutBounce(float value)
	{
		float n1 = 7.5625f;
		float d1 = 2.75f;

		if (value < 1 / d1)
		{
			return n1 * value * value;
		}
		else if (value < 2 / d1)
		{
			return n1 * (value -= 1.5f / d1) * value + 0.75f;
		}
		else if (value < 2.5 / d1)
		{
			return n1 * (value -= 2.25f / d1) * value + 0.9375f;
		}
		else
		{
			return n1 * (value -= 2.625f / d1) * value + 0.984375f;
		}
	}

	public static float EaseInOutBounce(float value)
	{
		return value < 0.5 ? (1 - EaseOutBounce(1 - 2 * value)) / 2 : (1 + EaseOutBounce(2 * value - 1)) / 2;
	}

	public static float EaseInBack(float value)
	{
		return c3 * value * value * value - c1 * value * value;
	}

	public static float EaseOutBack(float value)
	{
		return 1 + c3 * Mathf.Pow(value - 1, 3) + c1 * Mathf.Pow(value - 1, 2);
	}

	public static float EaseInOutBack(float value)
	{
		return value < 0.5f ? (Mathf.Pow(2 * value, 2) * ((c2 + 1) * 2 * value - c2)) / 2 : (Mathf.Pow(2 * value - 2, 2) * ((c2 + 1) * (value * 2 - 2) + c2) + 2) / 2;
	}

	public static float EaseInElastic(float value)
	{
		return value == 0 ? 0 : value == 1 ? 1 : -Mathf.Pow(2, 10 * value - 10) * Mathf.Sin((value * 10f - 10.75f) * c4);
	}

	public static float EaseOutElastic(float value)
	{
		return value == 0 ? 0 : value == 1 ? 1 : Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 10f - 0.75f) * c4) + 1;
	}

	public static float EaseInOutElastic(float value)
	{
		return value == 0 ? 0 : value == 1 ? 1 : value < 0.5 ? -(Mathf.Pow(2, 20 * value - 10) * Mathf.Sin((20 * value - 11.125f) * c5)) / 2 : (Mathf.Pow(2, -20 * value + 10) * Mathf.Sin((20 * value - 11.125f) * c5)) / 2 + 1;
	}

	public delegate float Function(float percent);

	public static Function GetEasingFunction(Ease easingFunction)
	{
		if (easingFunction == Ease.EaseInQuad)
		{
			return EaseInQuad;
		}

		if (easingFunction == Ease.EaseOutQuad)
		{
			return EaseOutQuad;
		}

		if (easingFunction == Ease.EaseInOutQuad)
		{
			return EaseInOutQuad;
		}

		if (easingFunction == Ease.EaseInCubic)
		{
			return EaseInCubic;
		}

		if (easingFunction == Ease.EaseOutCubic)
		{
			return EaseOutCubic;
		}

		if (easingFunction == Ease.EaseInOutCubic)
		{
			return EaseInOutCubic;
		}

		if (easingFunction == Ease.EaseInQuart)
		{
			return EaseInQuart;
		}

		if (easingFunction == Ease.EaseOutQuart)
		{
			return EaseOutQuart;
		}

		if (easingFunction == Ease.EaseInOutQuart)
		{
			return EaseInOutQuart;
		}

		if (easingFunction == Ease.EaseInQuint)
		{
			return EaseInQuint;
		}

		if (easingFunction == Ease.EaseOutQuint)
		{
			return EaseOutQuint;
		}

		if (easingFunction == Ease.EaseInOutQuint)
		{
			return EaseInOutQuint;
		}

		if (easingFunction == Ease.EaseInSine)
		{
			return EaseInSine;
		}

		if (easingFunction == Ease.EaseOutSine)
		{
			return EaseOutSine;
		}

		if (easingFunction == Ease.EaseInOutSine)
		{
			return EaseInOutSine;
		}

		if (easingFunction == Ease.EaseInExpo)
		{
			return EaseInExpo;
		}

		if (easingFunction == Ease.EaseOutExpo)
		{
			return EaseOutExpo;
		}

		if (easingFunction == Ease.EaseInOutExpo)
		{
			return EaseInOutExpo;
		}

		if (easingFunction == Ease.EaseInCirc)
		{
			return EaseInCirc;
		}

		if (easingFunction == Ease.EaseOutCirc)
		{
			return EaseOutCirc;
		}

		if (easingFunction == Ease.EaseInOutCirc)
		{
			return EaseInOutCirc;
		}

		if (easingFunction == Ease.Linear)
		{
			return Linear;
		}

		if (easingFunction == Ease.EaseInBounce)
		{
			return EaseInBounce;
		}

		if (easingFunction == Ease.EaseOutBounce)
		{
			return EaseOutBounce;
		}

		if (easingFunction == Ease.EaseInOutBounce)
		{
			return EaseInOutBounce;
		}

		if (easingFunction == Ease.EaseInBack)
		{
			return EaseInBack;
		}

		if (easingFunction == Ease.EaseOutBack)
		{
			return EaseOutBack;
		}

		if (easingFunction == Ease.EaseInOutBack)
		{
			return EaseInOutBack;
		}

		if (easingFunction == Ease.EaseInElastic)
		{
			return EaseInElastic;
		}

		if (easingFunction == Ease.EaseOutElastic)
		{
			return EaseOutElastic;
		}

		if (easingFunction == Ease.EaseInOutElastic)
		{
			return EaseInOutElastic;
		}

		return null;
	}
}