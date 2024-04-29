/*
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
using NUnit.Framework.Legacy;

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
            var assem = Assembly.GetExecutingAssembly();
            var assemblyPath = Directory.GetParent(assem.Location).FullName;
            var testDataDirectory = Path.Combine(assemblyPath, "Test Data");
            var outputDirectory = Path.Combine(assemblyPath, "Output");

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
            try
            {
                Console.WriteLine("Loading " + EmptyTestFile);

                using var fileStream = new FileStream(EmptyTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.IsEmpty(properties);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Check that empty lines are not interpreted as a value also.
        /// </summary>
        [Test]
        public void TestBlankFile()
        {
            try
            {
                Console.WriteLine("Loading " + BlankTestFile);

                using var fileStream = new FileStream(BlankTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.IsEmpty(properties);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestComments()
        {
            try
            {
                Console.WriteLine("Loading " + CommentsTestFile);

                using var fileStream = new FileStream(CommentsTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual(3, properties.Count);
                ClassicAssert.AreEqual("value", properties["name"]);
                ClassicAssert.AreEqual("value", properties["key\nwith\nnewlines"]);
                ClassicAssert.AreEqual("Value\nwith\nnewlines.", properties["key-no-newlines"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestDuplicates()
        {
            try
            {
                Console.WriteLine("Loading " + DuplicateTestFile);

                var fileStream = new FileStream(DuplicateTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual(1, properties.Count);
                ClassicAssert.AreEqual("c", properties["a"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestLineBreaks()
        {
            try
            {
                Console.WriteLine("Loading " + LineBreaksTestFile);

                using var fileStream = new FileStream(LineBreaksTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual(5, properties.Count);
                ClassicAssert.AreEqual("value", properties["key\nwith\nnewlines"]);
                ClassicAssert.AreEqual("Value\nwith\nnewlines.", properties["key-no-newlines"]);

                ClassicAssert.AreEqual("apple, banana, pear, cantaloupe, watermelon, kiwi, mango", properties["fruits"]);
                ClassicAssert.AreEqual("apple, banana, pear, cantaloupe, watermelon, ", properties["fruits2"]);
                ClassicAssert.AreEqual("mango", properties["kiwi,"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestSeparators()
        {
            try
            {
                Console.WriteLine("Loading " + SeparatorsTestFile);

                using var fileStream = new FileStream(SeparatorsTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual(8, properties.Count);

                ClassicAssert.AreEqual("b", properties["a"]);
                ClassicAssert.AreEqual("d", properties["c"]);

                ClassicAssert.AreEqual("f", properties["e"]);
                ClassicAssert.AreEqual("ij klm", properties["gh"]);

                ClassicAssert.AreEqual("Beauty 1", properties["Truth1"]);
                ClassicAssert.AreEqual("Beauty 3", properties["Truth3"]);
                ClassicAssert.AreEqual("Beauty 2", properties["Truth2"]);

                ClassicAssert.AreEqual(string.Empty, properties["cheeses"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestSpecialCharacters()
        {
            try
            {
                Console.WriteLine("Loading " + SpecialCharactersTestFile);

                using var fileStream = new FileStream(SpecialCharactersTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual(7, properties.Count);
                ClassicAssert.AreEqual("value with spaces", properties["key with spaces"]);

                ClassicAssert.AreEqual("unicode \\u0041='A'", properties["anotherKey"]);
                ClassicAssert.AreEqual("\u0000\t\n\u001F\u4567Unicode Value", properties["\u0000\u001FUnicode-Key"]);

                ClassicAssert.AreEqual(" value begins with\tspace.", properties["# key-not-comment"]);

                ClassicAssert.AreEqual("Two = Three Four", properties["One"]);
                ClassicAssert.AreEqual("Seven Eight", properties["Five Six"]);
                ClassicAssert.AreEqual("Ten ", properties["Nine"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestUtf8NonAsciiSymbols()
        {
            try
            {
                Console.WriteLine("Loading " + NonAsciiSymbolsUtf8TestFile);

                // A file containing non-ASCII characters, which is saved using the utf8 encoding
                using var utf8FileStream = new FileStream(NonAsciiSymbolsUtf8TestFile, FileMode.Open);

                Console.WriteLine("Loading " + NonAsciiSymbolsNativeToAsciiTestFile);

                // A file with the same data as above, but processed with the native2ascii tool from jdk 1.8, and stored in ISO-8859-1. This will work correctly.
                using var isoFileStream = new FileStream(NonAsciiSymbolsNativeToAsciiTestFile, FileMode.Open);

                // Java properties read from the utf8 file with correct encoding provided
                var utf8PropertiesCorrect = new JavaProperties();

                // we explicitly specify the encoding, so that UTF8 characters are read using the UTF8 encoding and the data will be correct
                utf8PropertiesCorrect.Load(utf8FileStream, Encoding.UTF8);

                var utf8PropertiesIncorrect = new JavaProperties();
                utf8FileStream.Seek(0, SeekOrigin.Begin);// reset the stream position from the previous loading.
                // we do not set the encoding, so the data will not appear correctly - UTF8 characters will be read usign the default ISO-8859-1 encoding
                // this is to ensure that setting the encoding makes a difference
                utf8PropertiesIncorrect.Load(utf8FileStream);


                var isoProperties = new JavaProperties();
                isoProperties.Load(isoFileStream);

                foreach (var key in utf8PropertiesCorrect.Keys)
                {
                    // Assert the correct file is identical to its native2ascii version
                    ClassicAssert.AreEqual(utf8PropertiesCorrect[key], isoProperties[key]);

                    if (key.Equals("AsciiText"))
                    {
                        // Assert that not-using the proper encoding will not corrupt pure ASCII data
                        ClassicAssert.AreEqual(utf8PropertiesCorrect[key], utf8PropertiesIncorrect[key]);
                        ClassicAssert.AreEqual(utf8PropertiesCorrect[key], isoProperties[key]);
                    }
                    else
                    {
                        // Assert that not-using the proper encoding will corrupt data
                        ClassicAssert.AreNotEqual(utf8PropertiesIncorrect[key], utf8PropertiesCorrect[key]);
                        ClassicAssert.AreNotEqual(utf8PropertiesIncorrect[key], isoProperties[key]);
                    }

                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestUrls()
        {
            try
            {
                Console.WriteLine("Loading " + UrlsTestFile);

                using var fileStream = new FileStream(UrlsTestFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual(3, properties.Count);

                ClassicAssert.AreEqual("my lovely web site.", properties["http://www.kajabity.com"]);
                ClassicAssert.AreEqual("http://www.kajabity.com", properties["my-blog"]);
                ClassicAssert.AreEqual("{my-blog}", properties["my-blog-2"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestUtf8WithBom()
        {
            try
            {
                Console.WriteLine("Loading " + Utf8WithBomFile);

                using var fileStream = new FileStream(Utf8WithBomFile, FileMode.Open);
                var properties = new JavaProperties();

                properties.Load(fileStream, Encoding.UTF8);

                ClassicAssert.AreEqual(
                    "key",
                    properties.Keys.Single());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestLineBreakInUnicodeSequence()
        {
            try
            {
                // Read the test properties file.
                Console.WriteLine("Loading " + LineBreakWithUnicodeFile);
                using var fileStream = new FileStream(LineBreakWithUnicodeFile, FileMode.Open);
                var properties = new JavaProperties();
                properties.Load(fileStream);

                ClassicAssert.AreEqual("B", properties["AAAP"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


        [Test]
        public void TestMissingProperty()
        {
            try
            {
                Console.WriteLine("Loading " + EmptyTestFile);

                var defaults = new Dictionary<string, string>
                {
                    { "test", "value" }
                };

                using var fileStream = new FileStream(EmptyTestFile, FileMode.Open);
                var properties = new JavaProperties(defaults);
                properties.Load(fileStream);

                ClassicAssert.IsEmpty(properties);
                ClassicAssert.IsNull(properties.GetProperty("NonExistent"));
                ClassicAssert.AreEqual(defaults["test"], properties.GetProperty("test"));

                ClassicAssert.IsNull(properties.SetProperty("NonExistent", "a new value"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
