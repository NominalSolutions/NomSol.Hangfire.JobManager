namespace NomSol.Hangfire.JobManager.Core.Helpers
{
    public static class CronExpressionHelper
    {
        public static string GetCronInterval(Interval interval, int value)
        {
            return interval switch
            {
                Interval.Minutes => $"*/{value} * * * *",
                Interval.Hours => $"0 */{value} * * *",
                Interval.EveryHourMonFri => $"0 */{value} * * 1-5",
                _ => $"*/{value} * * * *",
            };
        }
        public enum Interval
        {
            Minutes,
            Hours,
            EveryHourMonFri
            //DayofMonth,
            //Month,
            //DayofWeek,
            //Year
        }
    }
}
