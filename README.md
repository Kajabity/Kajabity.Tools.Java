Kajabity.Tools.Java - Java Properties Utilities
===============================================

[![CI](https://github.com/kajabity/Kajabity.Tools.Java/actions/workflows/ci.yml/badge.svg)](https://github.com/kajabity/Kajabity.Tools.Java/actions/workflows/ci.yml)
[![CodeQL](https://github.com/kajabity/Kajabity.Tools.Java/actions/workflows/codeql.yml/badge.svg)](https://github.com/kajabity/Kajabity.Tools.Java/actions/workflows/codeql.yml)
[![NuGet](https://img.shields.io/nuget/v/Kajabity.Tools.Java.svg)](https://www.nuget.org/packages/Kajabity.Tools.Java/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Kajabity.Tools.Java.svg)](https://www.nuget.org/packages/Kajabity.Tools.Java/)
[![GitHub Release](https://img.shields.io/github/v/release/kajabity/Kajabity.Tools.Java.svg)](https://github.com/kajabity/Kajabity.Tools.Java/releases)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE.txt)

Overview
--------

Kajabity.Tools.Java is a collection of utility classes to read and write Java style “.properties”
files in .NET applications.

This repository contains several sub-projects:

- **Kajabity.Tools.Java** - a DLL project providing the JavaProperties classes for .NET projects.
- **Kajabity.Tools.Java.Test** - NUnit tests for the Kajabity.Tools.Java classes.

See the Releases section on GitHub to download copies of code, DLL exe's and NuGets.

Kajabity.Tools.Java DLL is a strongly named assembly and is available from nuget.org as
Kajabity.Tools.Java.

Features
--------

The JavaProperties class wraps a Dictionary<string, string> class to provide the following additional features:

- Load Java properties from a Stream into the Dictionary<string, string>
- Load Java properties from a Stream with an alternate (e.g. Unicode) encoding – easier to support languages in alternate character sets.
- Store Java properties to a Stream from the Dictionary<string, string>.
- Support default properties (as with Java Properties class) using the 2nd constructor which provides a Dictionary<string, string> of defaults.
- Coerce the property keys and values to strings (as they are for Java properties).

Usage
-----

Full documentation is available at [https://www.kajabity.com/kajabity-tools/java-properties-classes/](https://www.kajabity.com/kajabity-tools/java-properties-classes/).

Releases
--------

- **Latest GitHub Release:** [View on GitHub](https://github.com/kajabity/Kajabity.Tools.Java/releases/latest)
- **Latest NuGet Package:** [View on NuGet.org](https://www.nuget.org/packages/Kajabity.Tools.Java/)

Feedback & Contributions
-------------------------

Contributions and feedback are welcome if you notice anything that could be improved.

