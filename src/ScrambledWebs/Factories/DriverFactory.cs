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

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using ScrambledWebs.Enums;
using ScrambledWebs.Interfaces;
using ScrambledWebs.Utils;
using ScrambledWebs.Yml;
using System;
using System.Diagnostics;

namespace ScrambledWebs.Campaign
{
    public class DriverFactory : IDriverFactory
    {
        /// <summary>
        /// Factory for creating web driver
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        public override IWebDriver CreateDriver(Browser browser)
        {
            switch (browser.Type)
            {
                // firefox driver
                case BrowserType.firefox:
                {
                    if(!DependencyCheck.CheckFireFox() ||
                       !DependencyCheck.CheckGeckoDriver())
                    {
                        Console.WriteLine("[*] Do not meet Firefox \\ Gecko driver requirements:");
                        Console.WriteLine(@"[*] Check : C:\Program Files\Mozilla Firefox\firefox.exe");
                        Console.WriteLine(@"[*] Check : C:\tools\selenium\geckodriver.exe");
                        Console.WriteLine("[*] Aborting");
                        Environment.Exit(1);
                    }

                    var firefoxOptions = new FirefoxOptions();

                    if ((bool)browser?.HtmlOnly)
                    {
                        firefoxOptions.SetPreference("permissions.default.image", 2);
                        firefoxOptions.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
                    }

                    if ((bool)browser?.Headless)
                        firefoxOptions.AddArguments("--headless");

                    if ((bool)browser?.Incognito)
                        firefoxOptions.SetPreference("browser.privatebrowsing.autostart", "true");

                    if (!string.IsNullOrEmpty(browser.WindowWidth))
                        firefoxOptions.AddArgument($"--width={browser.WindowWidth}");

                    if (!string.IsNullOrEmpty(browser.WindowHeight))
                        firefoxOptions.AddArgument($"--height={browser.WindowHeight}");

                    foreach (var process in Process.GetProcessesByName("firefox"))
                    {
                        process.Kill();
                    }

                    return new FirefoxDriver(firefoxOptions);
                }
                // chrome
                case BrowserType.chrome:
                {
                    var chromeOptions = new ChromeOptions();

                    if ((bool)browser?.HtmlOnly)
                    {
                        chromeOptions.AddExtension("Block-image_v1.0.crx");
                        chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
                        chromeOptions.AddArguments("--blink-settings=imagesEnabled=false");
                    }

                    if ((bool)browser?.Headless)
                        chromeOptions.AddArguments("--headless");

                    if ((bool)browser?.Incognito)
                        chromeOptions.AddArguments("--incognito");

                    if (!string.IsNullOrEmpty(browser.WindowWidth))
                        chromeOptions.AddArgument($"--width={browser.WindowWidth}");

                    if (!string.IsNullOrEmpty(browser.WindowHeight))
                        chromeOptions.AddArgument($"--height={browser.WindowHeight}");

                    foreach (var process in Process.GetProcessesByName("chrome"))
                    {
                        process.Kill();
                    }

                    return new ChromeDriver(chromeOptions);
                }
                default:
                    Console.WriteLine("[*] Configuration supplied incorrect browser type, aborting");
                    Environment.Exit(1);
                    return null;
            }
        }
    }
}
