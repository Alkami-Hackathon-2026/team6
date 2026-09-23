using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public class Timings
    {
        internal List<TimeSpan> Raw;

        public Timings(List<TimeSpan> raw)
        {
            Raw = raw;
        }

        /// <summary>
        /// Gets the percentile
        /// </summary>
        /// <param name="excelPercentile"></param>
        /// <see href="https://en.wikipedia.org/wiki/Percentile"/>
        /// <returns></returns>
        private TimeSpan Percentile(double excelPercentile)
        {
            var totalMilliseconds = 100d;

            try
            {
                if (Raw.Any())
                {
                    var sequence = Raw.Select(x => x.TotalMilliseconds).ToArray();

                    Array.Sort(sequence);

                    int length = sequence.Length;
                    double index = (length - 1) * excelPercentile + 1;

                    if (index == 1d)
                    {
                        totalMilliseconds = sequence[0];
                    }
                    else if (index == length)
                    {
                        totalMilliseconds = sequence[length - 1];
                    }
                    else
                    {
                        totalMilliseconds = sequence[(int)index];
                    }
                }
            }
            catch
            {
                totalMilliseconds = 100d;
            }

            return TimeSpan.FromMilliseconds((int) totalMilliseconds);
        }

        private TimeSpan CalculateStdDev()
        {
            try
            {
                double ret = 0;
                var values = Raw.Select(x => x.TotalMilliseconds).ToList();
                if (values.Count > 1)
                {
                    //Compute the Average
                    double avg = values.Average();
                    //Perform the Sum of (value-avg)_2_2
                    double sum = values.Sum(d => Math.Pow(d - avg, 2));
                    //Put it all together
                    ret = Math.Sqrt((sum) / (values.Count() - 1));
                }
                return TimeSpan.FromMilliseconds(ret);
            }
            catch
            {
                return new TimeSpan();
            }
        }

        public TimeSpan Average
        {
            get
            {
                if (!Raw.Any())
                    return new TimeSpan();

                return Percentile(0.5);
            }
        }

        public TimeSpan NintyNinthPercentile
        {
            get
            {
                return Percentile(0.99);
            }
        }

        public TimeSpan NinetiethPercentile
        {
            get
            {
                return Percentile(0.90);
            }
        }

        public TimeSpan StandardDeviation
        {
            get { return CalculateStdDev(); }
        }
    }
}