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
using ScrambledWebs.Interfaces;
using ScrambledWebs.Models;
using ScrambledWebs.Parsers;
using ScrambledWebs.Yml;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScrambledWebs.Campaign
{
    public class BrowserCampaign : ICampaign
    {
        public IWebDriver driver { get; set; }

        private Scrambler scrambler { get; set; }

        private Random r = new Random();
        
        // cancellation tokens

        private CancellationTokenSource quitTokenSource = new CancellationTokenSource();
        
        private CancellationTokenSource scrollTokenSource = new CancellationTokenSource();

        public BrowserCampaign(YmlConfiguration configuration) 
            : base(configuration)
        {
            driver = new DriverFactory().CreateDriver(this.configuration.Browser);       
            scrambler = new Scrambler(configuration);
        }

        /// <summary>
        /// Run scramble sequence 
        /// </summary>
        public override void Scramble()
        {
            try
            {
                // press q to quit console
                var cancelTask = Task.Run(() =>
                {
                    while (Console.ReadKey().Key == ConsoleKey.C)
                    {
                        quitTokenSource.Cancel();
                    }
                }, quitTokenSource.Token);

                // wait for q or campaign duration
                Task.WhenAny(new[] { cancelTask, ScrambleTask() }).Wait();
            }
            catch
            {
                Console.WriteLine("[*] Fatal error, aborting");
            }
        }

        /// <summary>
        /// Scramble sequence task
        /// </summary>
        /// <returns></returns>
        private async Task ScrambleTask()
        {
            // scramble initialize
            scrambler.LogOutput("\n[*] Scrambling up some webs benedict in a jiffy. (press ctrl+c to quit)");

            // seed scrambler
            scrambler.GetRandomSeedUri();
            scrambler.CurrentDomain = new Uri(scrambler.CurrentUri).Host;

            scrambler.LogOutput($"\n[*] Campaign {this.configuration.Campaign}");
            scrambler.LogOutput($"[*] Depth ({scrambler.ResetDepthCount()}) : {new Uri(scrambler.CurrentUri).Host}");

            // scramble loop
            int eggIntervalCounter = 0;
            int retry = 0;
            int maxRetry = 5;
            while (true)
            {
                try
                {
                    if (!this.ProcessPage()) continue;

                    if (scrambler.CurrentDepth < scrambler.CounterDepth)
                        this.ProcessNextPage();
                    else
                        this.ProcessNextSite();

                    scrambler.ScrambledEggs++;
                    if(eggIntervalCounter > 15)
                    {
                        eggIntervalCounter = 0;
                        scrambler.LogOutput($"Scrambled up {scrambler.ScrambledEggs} eggs so far");
                    }
                }
                catch
                {
                    scrambler.LogOutput("[*] Error in campaign loop");
                    if (retry > maxRetry) break;
                    retry++;
                }  
            }

            this.Close();
        }

        /// <summary>
        /// Navigate to page and process anchor tags
        /// </summary>
        /// <returns></returns>
        private bool ProcessPage()
        {
            try
            {
                // random wait per campaign yml settings
                Task.Delay(TimeSpan.FromSeconds(
                r.Next(this.configuration.Browser.MinWait,
                        this.configuration.Browser.MaxWait))).Wait();

                // fail on go to current uri
                if (!this.GoTo(scrambler.CurrentUri))
                {
                    // check uri queue for 
                    bool reseed = false;
                    if (scrambler.UriQueue.Count <= 0)
                    {
                        // try get same domain uri
                        scrambler.CurrentUri = scrambler.TryGetUri();
                        if (string.IsNullOrEmpty(scrambler.CurrentUri))
                            reseed = true;
                        else
                            this.ProcessPage();
                    }
                    else
                        reseed = true;

                    // go to next domain
                    if (reseed)
                    {
                        scrambler.LogOutput($"[*] No paths in cache, reseeding a new domain");
                        scrambler.GetRandomSeedUri();
                        scrambler.ResetDepthCount();
                        return false;
                    }          
                }

                var path = new Uri(scrambler.CurrentUri).PathAndQuery;
                if(path.Length > 75)
                    path = path.Substring(0, 75);

                scrambler.LogOutput($"[*]\t{path}");

                this.ExecuteJS(StringTemplate.Parse(Constants.JsWindowScrollCmd, "8"));

                scrambler.LogCurrentUri();

                this.ExecuteJS(StringTemplate.Parse(Constants.JsWindowScrollCmd, "6"));

                scrambler.UriCache.Add(scrambler.CurrentUri);

                this.ExecuteJS(StringTemplate.Parse(Constants.JsWindowScrollCmd, "4"));

                var links = HtmlParser.ParseAnchorTags(this.driver.PageSource);
                if(links != null)
                {
                    foreach (var item in links)
                        scrambler.UriQueue.TryAdd(item, Guid.NewGuid());
                }

                this.ExecuteJS(StringTemplate.Parse(Constants.JsWindowScrollCmd, "2"));
                this.ExecuteJS(StringTemplate.Parse(Constants.JsWindowScrollCmd, "1"));

                return true;
            }
            catch
            {
                scrambler.LogOutput("[*] Error parsing page");
            }

            return false;
        }

        /// <summary>
        /// Process next page navigation
        /// </summary>
        /// <returns></returns>
        private bool ProcessNextPage()
        {
            try
            {
                string nextPage = scrambler.TryGetUri();

                if (string.IsNullOrEmpty(nextPage))
                {
                    if (scrambler.UriQueue.Count <= 0)
                        nextPage = scrambler.UriQueue.First().Key;
                    else
                        nextPage = scrambler.GetRandomSeedUri();

                    scrambler.ResetDepthCounter(nextPage);

                    scrollTokenSource.Cancel();
                    return true;
                }

                scrambler.UriQueue.TryRemove(scrambler.UriQueue.First(x => x.Key == nextPage));

                scrambler.CurrentUri = nextPage;
                scrambler.CurrentDepth++;

                scrollTokenSource.Cancel();
                return true;
            }
            catch(Exception ex)
            {
                scrollTokenSource.Cancel();
                scrambler.LogOutput("[*] Error parsing next page");
            }

            return false;
        }

        /// <summary>
        /// Process next site navigation
        /// </summary>
        /// <returns></returns>
        private bool ProcessNextSite()
        {
            try
            {
                var current = new Uri(scrambler.CurrentUri).Host;
                var nextSite = scrambler.UriQueue.First(x => new Uri(x.Key).Host != current).Key;
                scrambler.ResetDepthCounter(nextSite);
                scrambler.RemoveDomainUris(current);
                                
                scrollTokenSource.Cancel();
                return true;
            }
            catch
            {
                scrollTokenSource.Cancel();
                scrambler.LogOutput("[*] Error processing next site");
            }

            return false;
        }

        /// <summary>
        /// Navigate driver to uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public override bool GoTo(string uri)
        {
            try
            {
                this.scrambler.CurrentUri = uri;
                this.driver.Url = uri;
                return true;
            }
            catch (WebDriverException ex)
            {
                scrambler.LogOutput($"[*] Timeout : {uri}");
            }
            catch
            {
                scrambler.LogOutput($"[*] Error nagivating to {uri}");
            }

            return false;
        }

        /// <summary>
        /// Navigate web driver back one uri
        /// </summary>
        public override void GoBack()
        {
            try
            {
                this.driver.Navigate().Back();
            }
            catch
            {
                scrambler.LogOutput($"[*] Error closing web driver");
            }
        }

        /// <summary>
        /// Close web driver
        /// </summary>
        public override void Close()
        {
            try
            {
                this.driver.Close();
            }
            catch
            {
                scrambler.LogOutput($"[*] Error closing web driver");
            }
        }

        /// <summary>
        /// Execute javascript in browser 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override object ExecuteJS(string command)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)this.driver;
                return js.ExecuteScript(command);
            }
            catch
            {
                scrambler.LogOutput($"[*] Error executing javascript");
            }

            return null;
        }        
    }
}
