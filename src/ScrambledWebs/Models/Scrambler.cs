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

using ScrambledWebs.Parsers;
using ScrambledWebs.Utils;
using ScrambledWebs.Yml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace ScrambledWebs.Models
{
    public class Scrambler
    {
        public Scrambler(YmlConfiguration config)
        {
            this.Initialize(config);
        }

        private static readonly ReaderWriterLockSlim __fileLogObj = new ReaderWriterLockSlim();

        public static readonly Object __lockObj = new Object();

        private Random r = new Random();

        private string currentUri;
        public string CurrentUri 
        {
            get
            {
                if (string.IsNullOrEmpty(currentUri))
                    currentUri = string.Empty;

                lock (__lockObj)
                {
                    return currentUri;
                }
            }
            set
            {
                if (value == currentUri) return;
                lock (__lockObj)
                {
                    currentUri = value;
                }
            }
        }

        private string currentDomain;
        public string CurrentDomain
        {
            get
            {
                if (string.IsNullOrEmpty(currentDomain))
                    currentDomain = string.Empty;

                lock (__lockObj)
                {
                    return currentDomain;
                }
            }
            set
            {
                if(value == currentDomain) return;
                lock (__lockObj)
                {
                    currentDomain = value;
                }
            }
        }

        private int currentDepth;
        public int CurrentDepth
        {
            get
            {
                lock (__lockObj)
                {
                    return currentDepth;
                }
            }
            set
            {
                if(value == currentDepth) return;
                lock (__lockObj)
                {
                    currentDepth = value;
                }
            }
        }

        private int counterDepth { get; set; }
        public int CounterDepth
        {
            get 
            {
                lock (__lockObj)
                {
                    return counterDepth;
                }
            }
            set
            {
                if(value == counterDepth) return;
                lock (__lockObj)
                {
                    counterDepth = value;
                }
            }
        }

        public int MaxQueue { get; set; }
        public int MinWait { get; set; }
        public int MaxWait { get; set; }

        private int MinDepth { get; set; }
        private int MaxDepth { get; set; }

        private int scrambledEggs = 0;
        public int ScrambledEggs
        {
            get
            {
                lock (__lockObj)
                {
                    return scrambledEggs;
                }
            }
            set
            {
                if (value == scrambledEggs) return;
                lock (__lockObj)
                {
                    scrambledEggs = value;
                }
            }
        }

        private List<string> SeedUrls { get; set; }

        private List<string> BannedUrls { get; set; }

        private List<string> BannedKeywords { get; set; }

        private bool Logging = false; 

        private string LogPath { get; set; }

        private System.Timers.Timer Timer { get; set; }

        // thread-safe uri list containers 

        public ConcurrentDictionary<string, Guid> UriQueue = new ConcurrentDictionary<string, Guid>();

        public ConcurrentBag<string> UriCache = new ConcurrentBag<string>();

        

        /// <summary>
        /// Initialize scrambler
        /// </summary>
        /// <param name="config"></param>
        private void Initialize(YmlConfiguration config)
        {
            MinWait = config.Browser.MinWait;
            MaxWait = config.Browser.MaxWait;
            MinDepth = config.Browser.MinDepth;
            MaxDepth = config.Browser.MaxDepth;
            MaxQueue = config.Browser.MaxQueue;
            BannedUrls = new List<string>(config.Urls);
            BannedKeywords = new List<string>(config.BannedKeywords);
            SeedUrls = new List<string>(config.Urls);
            Logging = (bool)config.Logging;
            LogPath = (string.IsNullOrEmpty(config.LogPath)) ?
                    Constants.DefaultLogPath : config.LogPath;

            this.SetTimer(config.Duration);
        }

        /// <summary>
        /// Try getting next random uri in domain from queue
        /// </summary>
        /// <returns></returns>
        public string TryGetUri()
        {
            var uris = this.UriQueue.Where(
                x => LinqExtensions.TryParseUri(x) == this.currentDomain &&
                x.Key != this.CurrentUri);

            if (uris.Count() == 0)
                return string.Empty;

            return uris.ElementAt(r.Next(0, uris.Count())).Key;
        }

        /// <summary>
        /// Try adding uri to queue
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool TryAddUri(string uri)
        {          
            // check for max queue count
            if (HtmlParser.ValidateUri(uri)) return false;
            else if (this.UriQueue.Count > this.MaxQueue) return false;
            else if (BannedKeywords.Any(x => uri.Contains(x))) return false;
            // check if in cache history
            else if (this.UriCache.Contains(uri)) return false;
            else if (this.UriCache.Contains(uri)) return false;
            // check value uri & banned url
            else if ((!Uri.TryCreate(uri, UriKind.Absolute, out Uri? outUri) &&
                !(outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps)) ||
                (BannedUrls.Contains(outUri.Host))) return false;
            else
            {
                return this.UriQueue.TryAdd(uri, Guid.NewGuid());
            }
        }

        /// <summary>
        /// Remove stale domain links after depth reached
        /// </summary>
        /// <param name="uri"></param>
        public void RemoveDomainUris(string uri)
        {
            foreach (var item in this.UriQueue.Where(x => x.Key.Contains(uri)))
            {
                this.UriQueue.TryRemove(item);
            }
        }

        /// <summary>
        /// Reset random scrambler domain depth counter
        /// </summary>
        /// <param name="newUri"></param>
        public void ResetDepthCounter(string newUri)
        {
            this.CurrentUri = newUri;
            this.CurrentDomain = new Uri(newUri).Host;
            this.CurrentDepth = 0;
            this.ResetDepthCount();

            Console.WriteLine($"[{this.ScrambledEggs}] Depth ({this.CounterDepth}) : {this.CurrentDomain}");
        }

        /// <summary>
        /// Get random counter depth for next domain
        /// </summary>
        /// <returns></returns>
        public int ResetDepthCount()
        {
            this.CounterDepth = r.Next(this.MinDepth, this.MaxDepth);
            return this.CounterDepth;
        }

        /// <summary>
        /// Get a random uri from seed list
        /// </summary>
        /// <returns></returns>
        public string GetRandomSeedUri()
        {
            this.CurrentUri = this.SeedUrls[r.Next(0, this.SeedUrls.Count())];
            return this.CurrentUri;
        }

        /// <summary>
        /// Set campaign duration if set
        /// </summary>
        /// <param name="minutes"></param>
        public void SetTimer(int minutes)
        {
            if(minutes > 0)
            {
                this.Timer = new System.Timers.Timer();
                this.Timer.Interval = TimeSpan.FromMinutes(minutes).TotalMilliseconds;
                this.Timer.Elapsed += (s, e) =>
                {
                    this.LogOutput("[*] Campaign duration elapsed, exiting");
                };

                this.Timer.Enabled = true;
            }
        }

        /// <summary>
        /// Log visited uri to log file
        /// </summary>
        public void LogCurrentUri()
        {
            try
            {
                if (!this.Logging) return;

                try
                {
                    __fileLogObj.EnterWriteLock();

                    if (!File.Exists(this.LogPath))
                    {
                        File.Create(this.LogPath).Close();
                    }

                    using (StreamWriter w = File.AppendText(this.LogPath))
                    {
                        w.WriteLine($"{DateTime.Now} {this.CurrentUri}");
                        w.Flush();
                    }
                }
                finally
                {
                    __fileLogObj.ExitWriteLock();
                }    
            }
            catch
            {
                this.LogOutput($"[*] Could not find log file location : {this.LogPath}");
            }
        }

        /// <summary>
        /// Log to console
        /// </summary>
        /// <param name="output"></param>
        public void LogOutput(string output)
        {
            Console.WriteLine(output);
        }
    }
}
