using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StatisticsListExentsion {
	public static float Mean (this List<float> values)
	{
		return values.Count == 0 ? 0 : values.Mean (0, values.Count);
	}

	public static float Mean (this List<float> values, int start, int end)
	{
		float s = 0;

		for (int i = start; i < end; i++) {
			s += values [i];
		}

		return s / (end - start);
	}

	public static float Variance (this List<float> values)
	{
		return values.Variance (values.Mean(), 0, values.Count);
	}

	public static float Variance (this List<float> values, float mean)
	{
		return values.Variance (mean, 0, values.Count);
	}

	public static float Variance (this List<float> values, float mean, int start, int end)
	{
		float variance = 0;

		for (int i = start; i < end; i++) {
			variance += Mathf.Pow ((values [i] - mean), 2);
		}

		int n = end - start;
		if (start > 0) n -= 1;

		return variance/(n);
	}

	public static float StandardDeviation (this List<float> values)
	{
		return values.Count == 0 ? 0 : values.StandardDeviation (0, values.Count);
	}

	public static float StandardDeviation (this List<float> values, int start, int end)
	{
		float mean = values.Mean (start, end);
		float variance = values.Variance (mean, start, end);

		return Mathf.Sqrt(variance);
	}
}
