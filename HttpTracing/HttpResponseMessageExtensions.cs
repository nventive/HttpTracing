using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HttpTracing
{
    /// <summary>
    /// <see cref="HttpResponseMessage"/> extension methods.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Returns all headers as a standard HTTP headers string.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The headers as HTTP headers string.</returns>
        public static string AllHeadersAsString(this HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            return string.Join(
                Environment.NewLine,
                response.Headers
                    .Concat(response.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                    .Select(x => $"{x.Key}: {string.Join(" ", x.Value)}"));
        }
    }
}
