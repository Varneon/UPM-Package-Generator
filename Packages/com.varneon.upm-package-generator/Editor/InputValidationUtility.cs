using System;
using System.Text.RegularExpressions;

namespace Varneon.UPMPackageGenerator.Editor
{
    internal static class InputValidationUtility
    {
        internal enum InputType
        {
            UPMPackageName,
            Email
        }

        /// <summary>
        /// Regex for validating UPM package name
        /// </summary>
        private static Regex packageNameRegex = new Regex(@"^([a-z0-9-_]+\.){2}([a-z0-9-_]+)(\.[a-z0-9-_]+)?$", RegexOptions.Singleline);

        /// <summary>
        /// Regex used to validate email addresses from System.ComponentModel.DataAnnotations.EmailAddressAttribute.IsValid()
        /// </summary>
        private static Regex emailRegex = new Regex("^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        internal static bool ValidateInput(InputType type, string input)
        {
            switch (type)
            {
                case InputType.UPMPackageName:
                    return ValidateUPMPackageName(input);
                case InputType.Email:
                    return ValidateEmailAddress(input);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static bool ValidateUPMPackageName(string input)
        {
            return packageNameRegex.IsMatch(input);
        }

        internal static bool ValidateEmailAddress(string input)
        {
            return emailRegex.IsMatch(input);
        }
    }
}
