
using System;
using System.Globalization;
using System.Windows.Controls;

namespace DispoAdmin.ViewModels
{
    /// <summary>
    /// Validates a text against a regular expression
    /// </summary>
    public class DateValidationRule : ValidationRule
    {
        private DateTime _termin;
        public int Mindate { get; set; }
        public int Maxdate { get; set; }

        //private Regex _regex;

        public DateTime Termin
        {
            get { return _termin; }
            set
            {
                _termin = value;
                //_regex = new Regex(_pattern, RegexOptions.IgnoreCase);
            }
        }

        public DateValidationRule()
        {
        }

        public override ValidationResult Validate(object date, CultureInfo cultureInfo)
        {
            int sensedate = 0;

            try
            {
                if (((string)date).Length > 0)
                    sensedate = int.Parse((string)date);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }


            if (sensedate < Mindate || sensedate > Maxdate)
            {
                return new ValidationResult(false, "The value is not a valid scheduling date !");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
