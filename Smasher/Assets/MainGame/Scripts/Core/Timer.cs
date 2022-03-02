public class Timer
{
	EasingFunction.Ease mEase = EasingFunction.Ease.Linear;
	float mTimer = 0;
	float mDuration = 0;
	float mOverhead = 0;
	bool mIsDone = true;

	public void SetDuration(float time)
	{
		mTimer = time;
		mDuration = time;
		mOverhead = 0;
		mIsDone = false;
	}

	public void SetTimerDone()
	{
		mTimer = 0f;
		mOverhead = -mDuration;
	}

	public void SetTime(float time)
	{
		mTimer = time;
	}

	public void SetEase(EasingFunction.Ease ease)
	{
		mEase = ease;
	}

	public float GetTime()
	{
		return mTimer;
	}

	public float GetDuration()
	{
		return mDuration;
	}

	public float GetOverhead()
	{
		return mOverhead;
	}

	public float GetTimePercent()
	{
		if (mDuration == 0)
		{
			return 0;
		}

		if (mTimer < 0)
		{
			mTimer = 0;
		}

		float percent = mTimer / mDuration;
		return EasingFunction.GetEasingFunction(mEase)(percent);
	}

	public void Reset()
	{
		mTimer = mDuration;
		mOverhead = 0;
		mIsDone = false;
	}

	public bool IsDone()
	{
		return mTimer == 0;
	}

	public bool JustFinished()
	{
		if (mTimer > 0)
		{
			return false;
		}
		if (mIsDone)
		{
			return false;
		}

		mIsDone = true;
		return true;
	}

	public void Update(float deltaTime)
	{
		if (mTimer == 0)
		{
			return;
		}

		mTimer -= deltaTime;
		if (mTimer < 0)
		{
			mOverhead = -mTimer;
			mTimer = 0;
		}
	}
}