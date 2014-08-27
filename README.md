CssProfiler
===========

Scans markup and css for style usage and issues. Right now this application is a glorified Windows Search.

The goal of the CSS Profiler application is to analyze a project's CSS usage for any inconsistincies, such as:

* Unused selectors
* Duplicated CSS
* Deprecated CSS
* CSS errors
* Inline styling - inline styling will be compared to CSS classes to determine if one of those could be used instead
* More TBD

This application is very early in its lifecycle. Right now you can only enter a root directory, 
search term, and file extension to scan for. Related results are displayed in the listbox. Selecting a result opens it in
your default application for that file type.
