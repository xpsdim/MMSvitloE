using System;

namespace MMSvitloE
{
	internal static class StaticUtils
	{
		public static string TimespanToReadableStr(this TimeSpan period)
		{
			var periodStr = string.Empty;
			if (period.TotalMinutes > 5)
			{
				var d = period.Days;
				var h = period.Hours;
				var m = period.Minutes;
				periodStr = $"{Environment.NewLine}вже {(d > 0 ? $"{period.Days} дн" : string.Empty)} {(h > 0 ? $"{period.Hours} год" : string.Empty)} {(m > 0 ? $"{period.Minutes} хв" : string.Empty)}";
			}

			return periodStr;
		}
	}
}
