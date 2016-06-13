#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

#endregion

namespace CustomBinding.Models
{
    /// <summary>
    /// Date time model binder.
    /// </summary>
    public class DateTimeModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the DateTime property in the Model with valueResult.
        /// </summary>
        /// <param name="controllerContext">Controller context of the page.</param>
        /// <param name="bindingContext">Binding context of the page.</param>
        /// <returns>Returns Datetime value.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var modelState = new ModelState { Value = valueResult };

            DateTime actualValue = DateTime.Now;
            if (valueResult.AttemptedValue == string.Empty)
                return string.Empty;
            try
            {
                    actualValue = GetDateTimeFromString("dd-mm-yyyy", "hh:mm", valueResult.AttemptedValue);
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }

        /// <summary>
        /// Gets the DateTime from value with given date and time format.
        /// </summary>
        /// <param name="dateformat">Date format for the value to convert.</param>
        /// <param name="timeformat">Tome format for the value to convert.</param>
        /// <param name="value">Attempted value.</param>
        /// <returns>Raeturn date time with given date andtime formats.</returns>
        public static DateTime GetDateTimeFromString(string dateformat, string timeformat, string value)
        {
            DateTime returnvalue = DateTime.Now;
            if (timeformat.Contains("tt"))
                timeformat = timeformat.Substring(0, timeformat.IndexOf(" "));
            else
                timeformat = timeformat.Replace("h", "H");
            dateformat = dateformat.Replace("m", "M");

            // Possible date and time formats
            string[] validDateFormats = new string[26];
            validDateFormats[0] = dateformat;
            validDateFormats[1] = dateformat + " " + timeformat;
            validDateFormats[2] = dateformat + " " + timeformat + " tt";
            validDateFormats[3] = dateformat + " " + timeformat + ":ss";
            validDateFormats[4] = dateformat + " " + timeformat + ":ss tt";
            validDateFormats[5] = dateformat + " " + timeformat.Replace(":ss", "");
            validDateFormats[6] = dateformat + " " + timeformat.Replace(":ss", "") + " tt";
            validDateFormats[7] = timeformat;
            validDateFormats[8] = timeformat + " tt";
            validDateFormats[9] = timeformat + ":ss";
            validDateFormats[10] = timeformat + ":ss tt";
            validDateFormats[11] = timeformat.Replace(":ss", "");
            validDateFormats[12] = timeformat.Replace(":ss", "") + " tt";
            validDateFormats[13] = "yyyy-MM";
            validDateFormats[14] = dateformat + " " + timeformat + ".fff";
            validDateFormats[15] = dateformat + " " + timeformat + ".fff tt";
            validDateFormats[16] = dateformat + " " + timeformat + ":ss.fff";
            validDateFormats[17] = dateformat + " " + timeformat + ":ss.fff tt";
            validDateFormats[18] = dateformat + " " + timeformat.Replace(":ss", "") + ".fff";
            validDateFormats[19] = dateformat + " " + timeformat.Replace(":ss", "") + ".fff tt";
            validDateFormats[20] = timeformat + ".fff";
            validDateFormats[21] = timeformat + ".fff tt";
            validDateFormats[22] = timeformat + ":ss.fff";
            validDateFormats[23] = timeformat + ":ss.fff tt";
            validDateFormats[24] = timeformat.Replace(":ss", "") + ".fff";
            validDateFormats[25] = timeformat.Replace(":ss", "") + ".fff tt";
            returnvalue = DateTime.ParseExact(value, validDateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal);
            return returnvalue;
        }
    }
}