using System;
using Newtonsoft.Json.Linq;

namespace AwesomeAssertions.Json
{
    /// <summary>
    /// Provides access to the global configuration and options to customize the behavior of AwesomeAssertions.Json.
    /// </summary>
    public static class JsonAssertionConfiguration
    {
        private static Func<string, JToken> _parser = JToken.Parse;

        /// <summary>
        /// Sets the function used to parse a json string into <see cref="JToken"/>.
        /// </summary>
        public static Func<string, JToken> ParseFunction
        {
            get
            {
                return _parser;
            }

            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(ParseFunction));
                _parser = value;
            }
        }
    }
}
