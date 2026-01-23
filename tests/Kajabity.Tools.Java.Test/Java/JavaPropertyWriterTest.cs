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

using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.Tools.Java
{
    /// <summary>
    /// The following tests are validated by converting to equivalent JUnit tests - for example:
    ///
    /// <pre>
    ///    @Test
    ///    public void test() throws IOException
    ///    {
    ///        Properties properties = new Properties();
    ///        properties.put( "\\first", "anything" );
    ///
    ///        try(ByteArrayOutputStream outStream = new ByteArrayOutputStream() )
    ///        {
    ///            properties.store(outStream, null );
    ///            String actual = getAsString( outStream );
    ///
    ///            assertTrue( actual.contains( "\\\\first=anything" ) );
    ///        }
    ///    }
    ///
    ///    private String getAsString( ByteArrayOutputStream outStream )
    ///    {
    ///        ByteArrayInputStream inStream;
    ///        inStream = new ByteArrayInputStream( outStream.toByteArray() );
    ///        int inBytes = inStream.available();
    ///
    ///        System.out.println( "inStream has " + inBytes + " available bytes" );
    ///        byte inBuf[] = new byte[inBytes];
    ///        int bytesRead = inStream.read( inBuf, 0, inBytes );
    ///        String actual = new String( inBuf, StandardCharsets.ISO_8859_1 );
    ///        System.out.println( actual );
    ///        return actual;
    ///    }
    /// </pre>
    /// </summary>
    [TestFixture]
    public class JavaPropertyWriterTest
    {
        [Test]
        public void JavaPropertyWriter_shouldWriteComment()
        {
            const string comment = "a comment";
            var properties = new JavaProperties();

            using var stream = new MemoryStream();
            properties.Store(stream, comment);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain(comment));
        }

        [Test]
        public void JavaPropertyWriter_shouldWriteTimestampIfTrue()
        {
            var properties = new JavaProperties();

            using var stream = new MemoryStream();
            properties.Store(stream, null, null, true);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.StartWith("#"));
        }

        [Test]
        public void JavaPropertyWriter_shouldNotWriteTimestampIfFalse()
        {
            var properties = new JavaProperties();

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            ClassicAssert.IsEmpty(actual);
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeBackslash()
        {
            var properties = new JavaProperties
            {
                {"\\first", "anything"},
                {"sec\\ond", "nothing"},
                {"third\\", "something"},
                {"fourth", "some\\thing"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\\\first=anything"));
            Assert.That(actual, Does.Contain("sec\\\\ond=nothing"));
            Assert.That(actual, Does.Contain("third\\\\=something"));
            Assert.That(actual, Does.Contain("fourth=some\\\\thing"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeWhitespaceInKey()
        {
            var properties = new JavaProperties
            {
                {" first", "anything"},
                {"sec ond", "nothing"},
                {"third ", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\ first=anything"));
            Assert.That(actual, Does.Contain("sec\\ ond=nothing"));
            Assert.That(actual, Does.Contain("third\\ =something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeWhitespaceAtStartOfValue()
        {
            var properties = new JavaProperties
            {
                {"first", " anything"},
                {"second", "noth ing"},
                {"third", "something "}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\ anything"));
            Assert.That(actual, Does.Contain("second=noth ing"));
            Assert.That(actual, Does.Contain("third=something "));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeTabsInKey()
        {
            var properties = new JavaProperties
            {
                {"\tfirst", "anything"},
                {"sec\tond", "nothing"},
                {"third\t", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\tfirst=anything"));
            Assert.That(actual, Does.Contain("sec\\tond=nothing"));
            Assert.That(actual, Does.Contain("third\\t=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeTabsInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "\tanything"},
                {"second", "noth\ting"},
                {"third", "something\t"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\tanything"));
            Assert.That(actual, Does.Contain("second=noth\\ting"));
            Assert.That(actual, Does.Contain("third=something\\t"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeFormFeedInKey()
        {
            var properties = new JavaProperties
            {
                {"\ffirst", "anything"},
                {"sec\fond", "nothing"},
                {"third\f", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\ffirst=anything"));
            Assert.That(actual, Does.Contain("sec\\fond=nothing"));
            Assert.That(actual, Does.Contain("third\\f=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeFormFeedInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "\fanything"},
                {"second", "noth\fing"},
                {"third", "something\f"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\fanything"));
            Assert.That(actual, Does.Contain("second=noth\\fing"));
            Assert.That(actual, Does.Contain("third=something\\f"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeNewlineInKey()
        {
            var properties = new JavaProperties
            {
                {"\nfirst", "anything"},
                {"sec\nond", "nothing"},
                {"third\n", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\nfirst=anything"));
            Assert.That(actual, Does.Contain("sec\\nond=nothing"));
            Assert.That(actual, Does.Contain("third\\n=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeNewlineInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "\nanything"},
                {"second", "noth\ning"},
                {"third", "something\n"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\nanything"));
            Assert.That(actual, Does.Contain("second=noth\\ning"));
            Assert.That(actual, Does.Contain("third=something\\n"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeReturnInKey()
        {
            var properties = new JavaProperties
            {
                {"\rfirst", "anything"},
                {"sec\rond", "nothing"},
                {"third\r", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\rfirst=anything"));
            Assert.That(actual, Does.Contain("sec\\rond=nothing"));
            Assert.That(actual, Does.Contain("third\\r=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeReturnInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "\ranything"},
                {"second", "noth\ring"},
                {"third", "something\r"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\ranything"));
            Assert.That(actual, Does.Contain("second=noth\\ring"));
            Assert.That(actual, Does.Contain("third=something\\r"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeHashInKey()
        {
            var properties = new JavaProperties
            {
                {"#first", "anything"},
                {"sec#ond", "nothing"},
                {"third#", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\#first=anything"));
            Assert.That(actual, Does.Contain("sec\\#ond=nothing"));
            Assert.That(actual, Does.Contain("third\\#=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeHashInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "#anything"},
                {"second", "noth#ing"},
                {"third", "something#"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\#anything"));
            Assert.That(actual, Does.Contain("second=noth\\#ing"));
            Assert.That(actual, Does.Contain("third=something\\#"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapePlingInKey()
        {
            var properties = new JavaProperties
            {
                {"!first", "anything"},
                {"sec!ond", "nothing"},
                {"third!", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\!first=anything"));
            Assert.That(actual, Does.Contain("sec\\!ond=nothing"));
            Assert.That(actual, Does.Contain("third\\!=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapePlingInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "!anything"},
                {"second", "noth!ing"},
                {"third", "something!"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\!anything"));
            Assert.That(actual, Does.Contain("second=noth\\!ing"));
            Assert.That(actual, Does.Contain("third=something\\!"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeEqualsInKey()
        {
            var properties = new JavaProperties
            {
                {"=first", "anything"},
                {"sec=ond", "nothing"},
                {"third=", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\=first=anything"));
            Assert.That(actual, Does.Contain("sec\\=ond=nothing"));
            Assert.That(actual, Does.Contain("third\\==something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeEqualsInValue()
        {
            var properties = new JavaProperties
            {
                {"first", "=anything"},
                {"second", "noth=ing"},
                {"third", "something="}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\=anything"));
            Assert.That(actual, Does.Contain("second=noth\\=ing"));
            Assert.That(actual, Does.Contain("third=something\\="));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeColonInKey()
        {
            var properties = new JavaProperties
            {
                {":first", "anything"},
                {"sec:ond", "nothing"},
                {"third:", "something"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\:first=anything"));
            Assert.That(actual, Does.Contain("sec\\:ond=nothing"));
            Assert.That(actual, Does.Contain("third\\:=something"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeColonInValue()
        {
            var properties = new JavaProperties
            {
                {"first", ":anything"},
                {"second", "noth:ing"},
                {"third", "something:"}
            };

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("first=\\:anything"));
            Assert.That(actual, Does.Contain("second=noth\\:ing"));
            Assert.That(actual, Does.Contain("third=something\\:"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeExtendedCharsInKey()
        {
            var properties = new JavaProperties {{"Привет", "Greeting"}};

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("\\u041F\\u0440\\u0438\\u0432\\u0435\\u0442=Greeting"));
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeExtendedCharsInValue()
        {
            var properties = new JavaProperties {{"Greeting", "Привет"}};

            using var stream = new MemoryStream();
            properties.Store(stream);

            var actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
            Assert.That(actual, Does.Contain("Greeting=\\u041F\\u0440\\u0438\\u0432\\u0435\\u0442"));
        }

        //Key no value
        //No Key value.

        //Encoding - e.g.utf8 + one other.
    }
}
