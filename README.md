# SCRAMBLED_WEBS
Network web traffic generator and aggragator driven by YML based campaign files

# Releases
[Release 1.0 Download](https://github.com/michaelrinderle/)

* Demo 
![Screenshot](demo.png)

# Privacy \ Research Tool 
	* Find data you didn't now existed
	* Generate web traffic for network stress testing
	* Obfuscate traffic from malicious eavesdropping
	* Extend for machine learning inputs

# Easy To Use
	* Use simple YML files to map your campaigns
	* Keyword based seeding for specific targeting
	* Written in .NetCore, easy to extend. 

# Features 
	* Run campaigns using browsers in headless mode
    * Logging support
	* Ability to run as startup service for privacy purposes
	* Proxy support
	* Written in .NetCore, easy to exend
	* Run as a start up service 

# Requirements 
    * .Net4  
	* Selenium
	* Firefox or Chrome 
	* More browsers to come

# Todo 
	* Bot campaign support for browserless campaigns
    * Multi-threading for multiple campaigns
    * Task based pipeline for easy custom user actions
	* Machine learning pipeline for mined data
	* Custom User Module Support 
	* More browser support

# Usage

Supply ScrambledWebs with the `-p` flag and the path to the campaign Yml file. `--verbose` is optional an defaults to false if your campaign setting for logging is not set. If a flag and path is not specificed, ScrambledWebs will look in current directory for `campaign.yml` 

Usage:
`scrambledwebs.exe [path, -p, --p] [verbose, --verbose]`
`scrambledwebs.exe -p campaign.yml -verbose `
`scrambledwebs.exe` 

# Configuration 
* Campaign Yml File 
You can map out the campaign to your own liking by creating a Yml campaign file.
`campaign.yml` is in the project directory as a template to use for your custom campaign.

```
Campaign: # (string) 
Author: # (string)
Duration: # (int) 0 means forever
Logging: # (bool) 
LogPath: # (string) path to log file
Browser:  # (browser)
    Type: # (enum) firefox | chrome
    HtmlOnly: # (bool) does not download images
    Headless: # (bool) run without browser window
    Incognito: # (bool)
    Agent: # (bool) user agent string 
    WindowHeight: # (int)
    WindowWidth: # (int)
    MaxDepth: # (int) max number of pages to visit from a site
    MinDepth: # (int) min number of pages to visit from a site
    MaxWait: # (int) max number of seconds to wait before next site
    MinWait: # (int) min number of seconds to wait before next site
    MaxQueue: # (int) number of links to store for scrambling
    Proxy:
        Enabled: # (bool)
        Host: # (string)
        Port: # (int)

Urls:  # urls to seed scrambler with
    - # (string)
    - https://www.msnbc.com
    - https://www.foxnews.com
    - https://www.nbc.com
    - https://www.vox.com/


Blacklist:  # sites to block aggragating
    - https://www.facebook.com
    - https://www.twitter.com

BannedKeywords: # banned keywords for uri parsing
    - mp3
    - mp4
    - zip
```