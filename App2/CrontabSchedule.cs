using System;
using System.Collections.Generic;

namespace App2
{
    public class CrontabSchedule
    {
        private string _schedule;
        private Dictionary<CrontabField, string[]> crontabFields;

        public string Schedule
        {
            get { return _schedule; }
            set { _schedule = value; }
        }

        public CrontabSchedule()
        {
            this.Schedule = "";
            this.crontabFields = new Dictionary<CrontabField, string[]>();
        }

        public void SetCronFields(string sSchedule)
        {
            string[] fieldExpr = sSchedule.Split();
            if (fieldExpr.Length < 5)
            {
                AddOrUpdateCronField(CrontabField.Minute, new string[] { "0" });
                AddOrUpdateCronField(CrontabField.Hour, new string[] { "12" });
                AddOrUpdateCronField(CrontabField.Day, new string[] { "*" });
                AddOrUpdateCronField(CrontabField.Month, new string[] { "*" });
                AddOrUpdateCronField(CrontabField.DayOfWeek, new string[] { "*" });
            }
            else
            {
                this.Schedule = sSchedule;
                AddOrUpdateCronField(CrontabField.Minute, GetSortedExpr(fieldExpr[(int)CrontabField.Minute]));
                AddOrUpdateCronField(CrontabField.Hour, GetSortedExpr(fieldExpr[(int)CrontabField.Hour]));
                AddOrUpdateCronField(CrontabField.Day, GetSortedExpr(fieldExpr[(int)CrontabField.Day]));
                AddOrUpdateCronField(CrontabField.Month, GetSortedExpr(fieldExpr[(int)CrontabField.Month]));
                AddOrUpdateCronField(CrontabField.DayOfWeek, GetSortedExpr(fieldExpr[(int)CrontabField.DayOfWeek]));
            }
        }

        public DateTime GetNextScheduledTime(DateTime aTime)
        {
            DateTime scheduledTime = aTime;
            try
            {
                scheduledTime = scheduledTime.AddSeconds(-scheduledTime.Second);// set second to 0
                scheduledTime = HandleMinuteField(scheduledTime);
                scheduledTime = HandleHourField(scheduledTime);
                scheduledTime = HandleDayField(scheduledTime);
                scheduledTime = HandleMonthField(scheduledTime);
                scheduledTime = HandleDayOfWeekField(scheduledTime);
            }
            catch (Exception)
            {
                return aTime + new TimeSpan(12, 0, 0);// error in getting next time: int parse exception or Datetime construct exception
            }
            return scheduledTime;
        }

        public TimeSpan GetSleepInterval(DateTime aTime, DateTime nextScheduledTime)
        {
            return nextScheduledTime - aTime;
        }

        private string[] GetSortedExpr(string fieldExpr)
        {
            string[] sortedExpr = fieldExpr.Split(',');
            Array.Sort(sortedExpr, (x, y) => int.Parse(x).CompareTo(int.Parse(y)));
            return sortedExpr;
        }

        private void AddOrUpdateCronField(CrontabField cName, string[] filters)
        {
            if (this.crontabFields.ContainsKey(cName))
            {
                this.crontabFields[cName] = filters;
            }
            else
            {
                this.crontabFields.Add(cName, filters);
            }
        }

        private int GetMinimumFromField(CrontabField fieldName)
        {
            string[] expressions;
            if (this.crontabFields.TryGetValue(fieldName, out expressions))
            {
                if (expressions[0].Equals("*"))
                {
                    if (fieldName == CrontabField.Minute || fieldName == CrontabField.Hour)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return int.Parse(expressions[0]);
                }
            }
            else
            {
                return 0;
            }
        }

        private DateTime HandleMinuteField(DateTime aTime)
        {
            string[] expressions;
            if (this.crontabFields.TryGetValue(CrontabField.Minute, out expressions))
            {
                if (expressions[0].Equals("*"))
                {
                    return aTime.AddMinutes(1);
                }
                else
                {
                    foreach (string expr in expressions)
                    {
                        if (aTime.Minute < int.Parse(expr))
                        {
                            return new DateTime(aTime.Year, aTime.Month, aTime.Day, aTime.Hour, int.Parse(expr), 0);
                        }
                    }
                    return new DateTime(aTime.Year, aTime.Month, aTime.Day, aTime.Hour, int.Parse(expressions[0]), 0);
                }
            }
            else
            {
                return aTime;
            }
        }

        private DateTime HandleHourField(DateTime aTime)
        {
            string[] expressions;
            if (this.crontabFields.TryGetValue(CrontabField.Hour, out expressions))
            {
                if (expressions[0].Equals("*"))
                {
                    if (aTime > DateTime.Now)// The time is later than now, no need to add hour
                    {
                        return aTime;
                    }
                    else
                    {
                        DateTime dummy = new DateTime(aTime.Year, aTime.Month, aTime.Day, aTime.Hour, this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                        return dummy.AddHours(1);
                    }
                }
                else
                {
                    foreach (string expr in expressions)
                    {
                        if (aTime.Hour < int.Parse(expr))
                        {
                            return new DateTime(aTime.Year, aTime.Month, aTime.Day, int.Parse(expr), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                        }
                    }
                    DateTime dummy = new DateTime(aTime.Year, aTime.Month, aTime.Day, int.Parse(expressions[0]), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                    if (dummy < aTime)
                    {
                        return new DateTime(aTime.Year, aTime.Month, aTime.Day, int.Parse(expressions[0]), aTime.Minute, aTime.Second);
                    }
                    else
                    {
                        return dummy;
                    }
                }
            }
            else
            {
                return aTime;
            }
        }

        private DateTime HandleDayField(DateTime aTime)
        {
            string[] expressions;
            if (this.crontabFields.TryGetValue(CrontabField.Day, out expressions))
            {
                if (expressions[0].Equals("*"))
                {
                    if (aTime > DateTime.Now)
                    {
                        return aTime;
                    }
                    else
                    {
                        DateTime dummy = new DateTime(aTime.Year, aTime.Month, aTime.Day, this.GetMinimumFromField(CrontabField.Hour), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                        return dummy.AddDays(1);                    }
                }
                else
                {
                    foreach (string expr in expressions)
                    {
                        if (aTime.Day < int.Parse(expr))
                        {
                            return new DateTime(aTime.Year, aTime.Month, int.Parse(expr), this.GetMinimumFromField(CrontabField.Hour), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                        }
                    }
                    DateTime dummy = new DateTime(aTime.Year, aTime.Month, int.Parse(expressions[0]), this.GetMinimumFromField(CrontabField.Hour), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                    if (dummy < aTime)
                    {
                        return new DateTime(aTime.Year, aTime.Month, int.Parse(expressions[0]), aTime.Hour, aTime.Minute, aTime.Second);
                    }
                    else
                    {
                        return dummy;
                    }
                }
            }
            else
            {
                return aTime;
            }
        }

        private DateTime HandleMonthField(DateTime aTime)
        {
            string[] expressions;
            if (this.crontabFields.TryGetValue(CrontabField.Month, out expressions))
            {
                if (expressions[0].Equals("*"))
                {
                    if (aTime > DateTime.Now)
                    {
                        return aTime;
                    }
                    else
                    {
                        DateTime dummy = new DateTime(aTime.Year, aTime.Month, this.GetMinimumFromField(CrontabField.Day), this.GetMinimumFromField(CrontabField.Hour), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                        return dummy.AddMonths(1);
                    }
                }
                else
                {
                    foreach (string expr in expressions)
                    {
                        if (aTime.Month < int.Parse(expr))
                        {
                            return new DateTime(aTime.Year, int.Parse(expr), this.GetMinimumFromField(CrontabField.Day), this.GetMinimumFromField(CrontabField.Hour), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                        }
                    }
                    DateTime dummy = new DateTime(aTime.Year, int.Parse(expressions[0]), this.GetMinimumFromField(CrontabField.Day), this.GetMinimumFromField(CrontabField.Hour), this.GetMinimumFromField(CrontabField.Minute), aTime.Second);
                    return dummy.AddYears(1);
                }
            }
            else
            {
                return aTime;
            }
        }

        private DateTime HandleDayOfWeekField(DateTime aTime)
        {
            return aTime;
        }
    }
}
