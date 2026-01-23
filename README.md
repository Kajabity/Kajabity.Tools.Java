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

Kajabity.Tools.Java is a collection of utility classes for reading and writing Java-style 
`.properties` files in .NET applications.

This repository contains several sub-projects:

- Kajabity.Tools.Java – a DLL project providing the JavaProperties classes for .NET projects.

- Kajabity.Tools.Java.Test – NUnit tests for the Kajabity.Tools.Java classes.

See the Releases section on GitHub to download source code, DLLs, executables, and NuGet packages.

The Kajabity.Tools.Java DLL is a strongly named assembly and is available from nuget.org as Kajabity.Tools.Java.

Features
--------

The JavaProperties class wraps a `Dictionary<string, string>` to provide the following additional features:

- Load Java properties from a Stream into a `Dictionary<string, string>`.

- Load Java properties from a Stream using an alternate encoding (e.g. Unicode), making it easier to 
  support languages with non-ASCII character sets.

- Store Java properties from a `Dictionary<string, string>` to a Stream.

- Support default properties (as with the Java Properties class) using the second constructor, which 
accepts a `Dictionary<string, string>` of defaults.

- Coerce property keys and values to strings, matching Java properties behaviour.

Usage
-----

Full documentation is available at:
https://www.kajabity.com/kajabity-tools/java-properties-classes/

Releases
--------

Latest GitHub Release: View on GitHub

Latest NuGet Package: View on NuGet.org

Feedback & Contributions
------------------------

Contributions and feedback are welcome if you notice anything that could be improved.