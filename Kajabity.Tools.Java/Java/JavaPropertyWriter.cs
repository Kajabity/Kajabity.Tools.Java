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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kajabity.Tools.Java
{
    /// <summary>
    /// Use this class for writing a set of key value pair strings to an
    /// output stream using the Java properties format.
    /// </summary>
    public class JavaPropertyWriter
    {
        private const char CharCommentHash = '#';
        private const char CharCommentPling = '!';

        private const char CharEscape = '\\';
        private const char CharFormFeed = '\f';
        private const char CharHorizontalTab = '\t';
        private const char CharLineFeed = '\n';
        private const char CharCarriageReturn = '\r';

        private const char CodeFormFeed = 'f';
        private const char CodeHorizontalTab = 't';
        private const char CodeNewLine = 'n';
        private const char CodeCarriageReturn = 'r';
        private const char CodeUnicode = 'u';

        private const string TextCommentHash = "#";

        private const string EscapedEscape = "\\\\";
        private const string EscapedHash = "\\#";
        private const string EscapedPling = "\\!";
        private const string EscapedFormFeed = "\\f";
        private const string EscapedHorizontalTab = "\\t";
        private const string EscapedLineFeed = "\\n";
        private const string EscapedCarriageReturn = "\\r";
        private const string EscapedUnicode = "\\u";

        /// <summary>
        /// Gets or sets a flag indicating if the creation timestamp should be added as a comment at the top of the CSV file - default is true.
        /// </summary>
        public bool OutputTimestamp { get; set; } = true;


        // Converted to use Dictionary<TKey,TValue> in place of Hashtable when switched to .NET Standard.
        private readonly Dictionary<string, string> hashtable;

        /// <summary>
        /// Construct an instance of this class.
        /// </summary>
        /// <param name="hashtable">The Hashtable (or JavaProperties) instance
        /// whose values are to be written.</param>
        public JavaPropertyWriter(Dictionary<string, string> hashtable)
        {
            this.hashtable = hashtable;
        }

        /// <summary>
        /// Write the properties to the output stream.
        /// </summary>
        /// <param name="stream">The output stream where the properties are written.</param>
        /// <param name="comments">Optional comments that are placed at the beginning of the output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding">encoding</see> that is used to write the properties file stream.</param>
        public void Write(Stream stream, string comments = null, Encoding encoding = null)
        {
            //  Create a writer to output with the specified encoding.
            var writerEncoding = encoding ?? JavaProperties.DefaultEncoding;
            var writer = new StreamWriter(stream, writerEncoding);

            if (comments != null)
            {
                writer.WriteLine(CharCommentHash + " " + comments);
            }

            if (OutputTimestamp)
            {
                writer.WriteLine(CharCommentHash + " " + DateTime.Now.ToString());
            }

            for (IEnumerator e = hashtable.Keys.GetEnumerator(); e.MoveNext();)
            {
                var key = e.Current.ToString();
                var val = hashtable[key].ToString();

                writer.WriteLine(EscapeText(key, true) + "=" + EscapeText(val, false));
            }

            writer.Flush();
        }

        /// <summary>
        /// Escape the string as a Key with character set ISO-8859-1 -
        /// the characters 0-127 are US-ASCII and we will escape any others.  The passed string is Unicode which extends
        /// ISO-8859-1 - so all is well.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isKey">indicates if this is a key or value which affects space escaping.</param>
        /// <returns></returns>
        private string EscapeText(string s, bool isKey)
        {
            var buf = new StringBuilder();
            var first = true;
            foreach (var c in s)
            {
                switch (c)
                {
                    //  Avoid confusing with a comment if key starts with '!' (33) or '#' (35).
                    case CharCommentPling:
                        buf.Append(EscapedPling);
                        break;

                    case CharCommentHash:
                        buf.Append(EscapedHash);
                        break;

                    case CharHorizontalTab:  //  =09 U+0009  HORIZONTAL TABULATION   \t
                        buf.Append(EscapedHorizontalTab);
                        break;

                    case CharLineFeed:  //  =0A U+000A  LINE FEED               \n
                        buf.Append(EscapedLineFeed);
                        break;

                    case CharFormFeed:  //  =0C U+000C  FORM FEED               \f
                        buf.Append(EscapedFormFeed);
                        break;

                    case CharCarriageReturn:  //  =0D U+000D  CARRIAGE RETURN         \r
                        buf.Append(EscapedCarriageReturn);
                        break;

                    case ' ':   //  32: ' '
                        if (isKey || first)
                        {
                            buf.Append(CharEscape);
                        }
                        buf.Append(c);
                        break;

                    case ':':   //  58: ':'
                    case '=':   //  61: '='
                    case CharEscape:  //  92: '\'
                        buf.Append(CharEscape).Append(c);
                        break;

                    default:
                        if (c > 31 && c < 127)
                        {
                            buf.Append(c);
                        }
                        else
                        {
                            buf.Append(EscapedUnicode);
                            buf.Append(((int)c).ToString("X4"));
                        }
                        break;
                }

                first = false;
            }

            return buf.ToString();
        }
    }
}
