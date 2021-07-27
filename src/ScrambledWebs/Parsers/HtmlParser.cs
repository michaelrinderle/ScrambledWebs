/*
      __ _/| _/. _  ._/__ /
	_\/_// /_///_// / /_|/
				  _/
	copyright (c) sof digital 2021
	written by michael rinderle <michael@sofdigital.net>
    mit license
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace ScrambledWebs.Parsers
{
    public static class HtmlParser
    {
        /// <summary>
        /// Parse anchor tags from html source
        /// </summary>
        /// <param name="pageSource"></param>
        /// <returns></returns>
        public static List<string> ParseAnchorTags(string pageSource)
        {
            try
            {
                var result = new List<string>();
                var doc = new HtmlDocument();
                doc.LoadHtml(pageSource);
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    try
                    {
                        var uri = link.GetAttributeValue("href", string.Empty);
                        if (ValidateUri(uri)) result.Add(uri);
                    }
                    catch { continue; }
                }

                return result;
            }
            catch
            {               
                Console.WriteLine("[*] Error parsing html for anchor tags");
            }

            return null;
        }

        /// <summary>
        /// Validate uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool ValidateUri(string uri)
        {
            // check for max queue count
            if (string.IsNullOrEmpty(uri)) return false;
            // check for valid and absolute path
            else if(!uri.Contains("http") || !uri.Contains("https")) return false;
            else if (Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult)
                    && ((uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps))
                    && uriResult.IsAbsoluteUri)  return true;
         
            return true;
        }
    }
}