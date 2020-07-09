/*
 * Copyright 2009-17 Williams Technologies Limited.
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
 * Kajbity is a trademark of Williams Technologies Limited.
 *
 * http://www.kajabity.com
 */

using System;
using System.IO;
using System.Text;
using Kajabity.Tools.Test;
using NUnit.Framework;

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
    public class JavaPropertyWriterTest : KajabityToolsTest
    {
        [Test]
        public void JavaPropertyWriter_shouldWriteCommentWithTimestamp()
        {
            String comment = "a comment";
            JavaProperties properties = new JavaProperties();

            using (var stream = new MemoryStream())
            {
                properties.Store(stream, comment);

                string actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
                Assert.That(actual, Does.Match("# a comment\r\n# \\d{2}/\\d{2}/\\d{4} \\d{2}:\\d{2}:\\d{2}\r\n"));
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldWriteOnlyTimestamp()
        {
            JavaProperties properties = new JavaProperties();

            using (var stream = new MemoryStream())
            {
                properties.Store(stream, null);

                string actual = Encoding.GetEncoding("iso-8859-1").GetString(stream.ToArray());
                Assert.That(actual, Does.Match("# \\d{2}/\\d{2}/\\d{4} \\d{2}:\\d{2}:\\d{2}\r\n"));
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeBackslash()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "\\first", "anything" );
            properties.Add( "sec\\ond", "nothing" );
            properties.Add( "third\\", "something" );
            properties.Add( "fourth", "some\\thing" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\\\first=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\\\ond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\\\=something" ) );
                Assert.That( actual, Does.Contain( "fourth=some\\\\thing" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeWhitespaceInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( " first", "anything" );
            properties.Add( "sec ond", "nothing" );
            properties.Add( "third ", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\ first=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\ ond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\ =something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeWhitespaceAtStartOfValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", " anything" );
            properties.Add( "second", "noth ing" );
            properties.Add( "third", "something " );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\ anything" ) );
                Assert.That( actual, Does.Contain( "second=noth ing" ) );
                Assert.That( actual, Does.Contain( "third=something " ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeTabsInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "\tfirst", "anything" );
            properties.Add( "sec\tond", "nothing" );
            properties.Add( "third\t", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\tfirst=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\tond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\t=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeTabsInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "\tanything" );
            properties.Add( "second", "noth\ting" );
            properties.Add( "third", "something\t" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\tanything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\ting" ) );
                Assert.That( actual, Does.Contain( "third=something\\t" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeFormFeedInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "\ffirst", "anything" );
            properties.Add( "sec\fond", "nothing" );
            properties.Add( "third\f", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\ffirst=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\fond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\f=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeFormFeedInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "\fanything" );
            properties.Add( "second", "noth\fing" );
            properties.Add( "third", "something\f" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\fanything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\fing" ) );
                Assert.That( actual, Does.Contain( "third=something\\f" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeNewlineInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "\nfirst", "anything" );
            properties.Add( "sec\nond", "nothing" );
            properties.Add( "third\n", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\nfirst=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\nond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\n=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeNewlineInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "\nanything" );
            properties.Add( "second", "noth\ning" );
            properties.Add( "third", "something\n" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\nanything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\ning" ) );
                Assert.That( actual, Does.Contain( "third=something\\n" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeReturnInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "\rfirst", "anything" );
            properties.Add( "sec\rond", "nothing" );
            properties.Add( "third\r", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\rfirst=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\rond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\r=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeReturnInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "\ranything" );
            properties.Add( "second", "noth\ring" );
            properties.Add( "third", "something\r" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\ranything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\ring" ) );
                Assert.That( actual, Does.Contain( "third=something\\r" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeHashInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "#first", "anything" );
            properties.Add( "sec#ond", "nothing" );
            properties.Add( "third#", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\#first=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\#ond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\#=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeHashInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "#anything" );
            properties.Add( "second", "noth#ing" );
            properties.Add( "third", "something#" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\#anything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\#ing" ) );
                Assert.That( actual, Does.Contain( "third=something\\#" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapePlingInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "!first", "anything" );
            properties.Add( "sec!ond", "nothing" );
            properties.Add( "third!", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\!first=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\!ond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\!=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapePlingInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "!anything" );
            properties.Add( "second", "noth!ing" );
            properties.Add( "third", "something!" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\!anything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\!ing" ) );
                Assert.That( actual, Does.Contain( "third=something\\!" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeEqualsInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "=first", "anything" );
            properties.Add( "sec=ond", "nothing" );
            properties.Add( "third=", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\=first=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\=ond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\==something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeEqualsInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", "=anything" );
            properties.Add( "second", "noth=ing" );
            properties.Add( "third", "something=" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\=anything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\=ing" ) );
                Assert.That( actual, Does.Contain( "third=something\\=" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeColonInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( ":first", "anything" );
            properties.Add( "sec:ond", "nothing" );
            properties.Add( "third:", "something" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\:first=anything" ) );
                Assert.That( actual, Does.Contain( "sec\\:ond=nothing" ) );
                Assert.That( actual, Does.Contain( "third\\:=something" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeColonInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "first", ":anything" );
            properties.Add( "second", "noth:ing" );
            properties.Add( "third", "something:" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "first=\\:anything" ) );
                Assert.That( actual, Does.Contain( "second=noth\\:ing" ) );
                Assert.That( actual, Does.Contain( "third=something\\:" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeExtendedCharsInKey()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "Привет","Greeting" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "\\u041F\\u0440\\u0438\\u0432\\u0435\\u0442=Greeting" ) );
            }
        }

        [Test]
        public void JavaPropertyWriter_shouldEscapeExtendedCharsInValue()
        {
            JavaProperties properties = new JavaProperties();
            properties.Add( "Greeting", "Привет" );

            using( var stream = new MemoryStream() )
            {
                properties.Store( stream, null );

                string actual = Encoding.GetEncoding( "iso-8859-1" ).GetString( stream.ToArray() );
                Assert.That( actual, Does.Contain( "Greeting=\\u041F\\u0440\\u0438\\u0432\\u0435\\u0442" ) );
            }
        }

        //Key no value
        //No Key value.

        //Encoding - e.g.utf8 + one other.
    }
}
