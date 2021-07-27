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

using ScrambledWebs.Enums;

namespace ScrambledWebs.Yml
{
    public partial class YmlConfiguration
    {
        public string Campaign { get; set; }
        public string Author { get; set; }
        public int Duration { get; set; }
        public bool? Logging { get; set; }
        public string LogPath { get; set; }
        public Browser Browser { get; set; }
        public string[] Urls { get; set; }
        public string[] Blacklist { get; set; }
        public string[] BannedKeywords { get; set; }
    }

    public partial class Browser
    {
        public BrowserType Type { get; set; }
        public bool? HtmlOnly { get; set; }
        public bool? Headless { get; set; }
        public bool? Incognito { get; set; }
        public string Agent { get; set; }
        public string WindowHeight { get; set; }
        public string WindowWidth { get; set; }
        public int MaxDepth { get; set; }
        public int MinDepth { get; set; }
        public int MaxWait { get; set; }
        public int MinWait { get; set; }
        public int MaxQueue { get; set; }
        public Proxy Proxy { get; set; }
    }

    public partial class Proxy
    {
        public bool Enabled { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
    }
}
