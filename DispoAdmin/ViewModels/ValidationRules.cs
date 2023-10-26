using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DispoAdmin.ViewModels
{
    /// <summary>
    /// Validates a text against a regular expression
    /// </summary>
    public class DateValidationRule : ValidationRule
    {
        private DateTime _termin;
        public int mindate { get; set; }
        public int maxdate { get; set; }

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
                    sensedate = Int32.Parse((String)date);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }


            if (sensedate < mindate || sensedate > maxdate)
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
