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

using System;

namespace ScrambledWebs
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        // environment variables 
        private static string ProgramFiles86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");

        private static string ProgramFiles64 = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

        private static string LocalAppData = Environment.ExpandEnvironmentVariables("%LocalAppData%");

        private static string ProfileDesktop = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Desktop");

        // dependency paths
        public static readonly string DefaultCampaignPath = "campaign.yml";

        public static readonly string DefaultLogPath = $"{ProfileDesktop}\\scrambled_webs.logs";

        public static readonly string[] GeckDriverPaths = new[]
        {
            "C:\\tools\\selenium\\geckodriver.exe",
        };

        public static readonly string[] FirefoxBrowserPaths = new[]
        {
            $"{ProgramFiles64}\\Mozilla Firefox\\firefox.exe",
            $"{ProgramFiles86}\\Mozilla Firefox\\firefox.exe",
        };

        public static readonly string[] ChromeBrowserPaths = new[] 
        { 
            $"{ProgramFiles64}\\Google\\Chrome\\Application\\chrome.exe",
            $"{ProgramFiles86}\\Google\\Chrome\\Application\\chrome.exe",
            $"{LocalAppData}\\Google\\Chrome\\Application\\chrome.exe",
        };

        // javascript template commands
        public static readonly string JsWindowScrollCmd = @"window.scrollTo(0, document.documentElement.scrollHeight / {0})";
    }
}
