﻿/*
 * Copyright 2009-20 Williams Technologies Limited.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Kajabity is a trademark of Williams Technologies Limited.
 *
 * http://www.kajabity.com
 */

using System;
using System.IO;
using NUnit.Framework;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Kajabity.Tools.Java
{
    [TestFixture]
    public class JavaPropertyReaderTest
    {
        private string EmptyTestFile;
        private string BlankTestFile;
        private string CommentsTestFile;
        private string DuplicateTestFile;
        private string LineBreaksTestFile;
        private string UrlsTestFile;
        private string SeparatorsTestFile;
        private string SpecialCharactersTestFile;
        private string NonAsciiSymbolsUtf8TestFile;
        private string NonAsciiSymbolsNativeToAsciiTestFile;
        private string Utf8WithBomFile;
        private string LineBreakWithUnicodeFile;

        /// <summary>
        /// The directory where a copy of the Java test data input files are placed.
        /// </summary>
        protected static string JavaTestDataDirectory;

        /// <summary>
        /// The directory where a copy of the Java test data input files are placed.
        /// </summary>
        protected static string JavaOutputDirectory;

        [OneTimeSetUp]
        public void SetUp()
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            string assemblyPath = Directory.GetParent(assem.Location).FullName;
            string testDataDirectory = Path.Combine(assemblyPath, "Test Data");
            string outputDirectory = Path.Combine(assemblyPath, "Output");

            JavaTestDataDirectory = Path.Combine(testDataDirectory, "Java");
            JavaOutputDirectory = Path.Combine(outputDirectory, "Java");

            if (!Directory.Exists(JavaOutputDirectory))
            {
                Console.WriteLine("Creating Java Properties output directory :" + JavaOutputDirectory);
                Directory.CreateDirectory(JavaOutputDirectory);
            }

            EmptyTestFile = Path.Combine(JavaTestDataDirectory, "empty.properties");
            BlankTestFile = Path.Combine(JavaTestDataDirectory, "blank.properties");
            CommentsTestFile = Path.Combine(JavaTestDataDirectory, "comments.properties");
            DuplicateTestFile = Path.Combine(JavaTestDataDirectory, "duplicate.properties");
            LineBreaksTestFile = Path.Combine(JavaTestDataDirectory, "line-breaks.properties");
            UrlsTestFile = Path.Combine(JavaTestDataDirectory, "urls.properties");
            SeparatorsTestFile = Path.Combine(JavaTestDataDirectory, "separators.properties");
            SpecialCharactersTestFile = Path.Combine(JavaTestDataDirectory, "special-characters.properties");
            NonAsciiSymbolsUtf8TestFile = Path.Combine(JavaTestDataDirectory, "non-ascii-symbols-utf8.properties");
            NonAsciiSymbolsNativeToAsciiTestFile = Path.Combine(JavaTestDataDirectory, "non-ascii-symbols-native2ascii.properties");
            Utf8WithBomFile = Path.Combine(JavaTestDataDirectory, "utf8-with-BOM.properties");
            LineBreakWithUnicodeFile = Path.Combine(JavaTestDataDirectory, "line-break-unicode.properties");
        }

        [Test]
        public void TestEmptyFile()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + EmptyTestFile);

                fileStream = new FileStream(EmptyTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Is.Empty);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        /// <summary>
        /// Check that empty lines are not interpreted as a value also.
        /// </summary>
        [Test]
        public void TestBlankFile()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + BlankTestFile);

                fileStream = new FileStream(BlankTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Is.Empty);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestComments()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + CommentsTestFile);

                fileStream = new FileStream(CommentsTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Has.Count.EqualTo(3));
                Assert.That(properties["name"], Is.EqualTo("value"));
                Assert.That(properties["key\nwith\nnewlines"], Is.EqualTo("value"));
                Assert.That(properties["key-no-newlines"], Is.EqualTo("Value\nwith\nnewlines."));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestDuplicates()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + DuplicateTestFile);

                fileStream = new FileStream(DuplicateTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Has.Count.EqualTo(1));
                Assert.That(properties["a"], Is.EqualTo("c"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestLineBreaks()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + LineBreaksTestFile);

                fileStream = new FileStream(LineBreaksTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Has.Count.EqualTo(5));
                Assert.That(properties["key\nwith\nnewlines"], Is.EqualTo("value"));
                Assert.That(properties["key-no-newlines"], Is.EqualTo("Value\nwith\nnewlines."));

                Assert.That(properties["fruits"], Is.EqualTo("apple, banana, pear, cantaloupe, watermelon, kiwi, mango"));
                Assert.That(properties["fruits2"], Is.EqualTo("apple, banana, pear, cantaloupe, watermelon, "));
                Assert.That(properties["kiwi,"], Is.EqualTo("mango"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestSeparators()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + SeparatorsTestFile);

                fileStream = new FileStream(SeparatorsTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Has.Count.EqualTo(8));

                Assert.That(properties["a"], Is.EqualTo("b"));
                Assert.That(properties["c"], Is.EqualTo("d"));

                Assert.That(properties["e"], Is.EqualTo("f"));
                Assert.That(properties["gh"], Is.EqualTo("ij klm"));

                Assert.That(properties["Truth1"], Is.EqualTo("Beauty 1"));
                Assert.That(properties["Truth3"], Is.EqualTo("Beauty 3"));
                Assert.That(properties["Truth2"], Is.EqualTo("Beauty 2"));

                Assert.That(properties["cheeses"], Is.EqualTo(""));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestSpecialCharacters()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + SpecialCharactersTestFile);

                fileStream = new FileStream(SpecialCharactersTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Has.Count.EqualTo(7));
                Assert.That(properties["key with spaces"], Is.EqualTo("value with spaces"));

                Assert.That(properties["anotherKey"], Is.EqualTo("unicode \\u0041='A'"));
                Assert.That(properties["\u0000\u001FUnicode-Key"], Is.EqualTo("\u0000\t\n\u001F\u4567Unicode Value"));

                Assert.That(properties["# key-not-comment"], Is.EqualTo(" value begins with\tspace."));

                Assert.That(properties["One"], Is.EqualTo("Two = Three Four"));
                Assert.That(properties["Five Six"], Is.EqualTo("Seven Eight"));
                Assert.That(properties["Nine"], Is.EqualTo("Ten "));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestUtf8NonAsciiSymbols()
        {
            FileStream utf8FileStream = null;
            FileStream isoFileStream = null;
            try
            {
                Console.WriteLine("Loading " + NonAsciiSymbolsUtf8TestFile);

                // A file containing non-ASCII characters, which is saved using the utf8 encoding
                utf8FileStream = new FileStream(NonAsciiSymbolsUtf8TestFile, FileMode.Open);

                Console.WriteLine("Loading " + NonAsciiSymbolsNativeToAsciiTestFile);

                // A file with the same data as above, but processed with the native2ascii tool from jdk 1.8, and stored in ISO-8859-1. This will work correctly.
                isoFileStream = new FileStream(NonAsciiSymbolsNativeToAsciiTestFile, FileMode.Open);

                // Java properties read from the utf8 file with correct encoding provided
                JavaProperties utf8PropertiesCorrect = [];

                // we explicitly specify the encoding, so that UTF8 characters are read using the UTF8 encoding and the data will be correct
                utf8PropertiesCorrect.Load(utf8FileStream, Encoding.UTF8);

                JavaProperties utf8PropertiesIncorrect = [];
                utf8FileStream.Seek(0, SeekOrigin.Begin);// reset the stream position from the previous loading.
                // we do not set the encoding, so the data will not appear correctly - UTF8 characters will be read usign the default ISO-8859-1 encoding
                // this is to ensure that setting the encoding makes a difference
                utf8PropertiesIncorrect.Load(utf8FileStream);


                JavaProperties isoProperties = [];
                isoProperties.Load(isoFileStream);

                foreach (var key in utf8PropertiesCorrect.Keys)
                {
                    // Asert the correct file is identical to its native2ascii version
                    Assert.That(isoProperties[key], Is.EqualTo(utf8PropertiesCorrect[key]));

                    if (key.Equals("AsciiText"))
                    {
                        // Assert that not-using the proper encoding will not corrupt pure ASCII data
                        Assert.That(utf8PropertiesIncorrect[key], Is.EqualTo(utf8PropertiesCorrect[key]));
                        Assert.That(isoProperties[key], Is.EqualTo(utf8PropertiesCorrect[key]));
                    }
                    else
                    {
                        // Assert that not-using the proper encoding will corrupt data
                        Assert.That(utf8PropertiesCorrect[key], Is.Not.EqualTo(utf8PropertiesIncorrect[key]));
                        Assert.That(isoProperties[key], Is.Not.EqualTo(utf8PropertiesIncorrect[key]));
                    }

                }
                isoFileStream.Close();

                //Assert.IsEmpty(properties);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                utf8FileStream?.Close();
                isoFileStream?.Close();
            }
        }

        [Test]
        public void TestUrls()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + UrlsTestFile);

                fileStream = new FileStream(UrlsTestFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Has.Count.EqualTo(3));

                Assert.That(properties["http://www.kajabity.com"], Is.EqualTo("my lovely web site."));
                Assert.That(properties["my-blog"], Is.EqualTo("http://www.kajabity.com"));
                Assert.That(properties["my-blog-2"], Is.EqualTo("{my-blog}"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestUtf8WithBom()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + Utf8WithBomFile);

                fileStream = new FileStream(Utf8WithBomFile, FileMode.Open);
                JavaProperties properties = [];

                properties.Load(fileStream, Encoding.UTF8);

                Assert.That(
                    properties.Keys.Single(), Is.EqualTo("key"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        [Test]
        public void TestLineBreakInUnicodeSequence()
        {
            FileStream fileStream = null;
            try
            {
                // Read the test properties file.
                Console.WriteLine("Loading " + LineBreakWithUnicodeFile);
                fileStream = new FileStream(LineBreakWithUnicodeFile, FileMode.Open);
                JavaProperties properties = [];
                properties.Load(fileStream);

                Assert.That(properties["AAAP"], Is.EqualTo("B"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }


        [Test]
        public void TestMissingProperty()
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("Loading " + EmptyTestFile);

                var defaults = new Dictionary<string, string>
                {
                    { "test", "value" }
                };

                fileStream = new FileStream(EmptyTestFile, FileMode.Open);
                JavaProperties properties = new(defaults);
                properties.Load(fileStream);
                fileStream.Close();

                Assert.That(properties, Is.Empty);
                Assert.That(properties.GetProperty("NonExistent"), Is.Null);
                Assert.That(properties.GetProperty("test"), Is.EqualTo(defaults["test"]));

                Assert.That(properties.SetProperty("NonExistent", "a new value"), Is.Null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }
    }
}
