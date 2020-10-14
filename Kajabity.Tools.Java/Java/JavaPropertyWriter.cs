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
        private const char CHAR_COMMENT_HASH = '#';
        private const char CHAR_COMMENT_PLING = '!';

        private const char CHAR_ESCAPE = '\\';
        private const char CHAR_FORM_FEED = '\f';
        private const char CHAR_HORIZONTAL_TAB = '\t';
        private const char CHAR_LINE_FEED = '\n';
        private const char CHAR_CARRIAGE_RETURN = '\r';

        private const char CODE_FORM_FEED = 'f';
        private const char CODE_HORIZONTAL_TAB = 't';
        private const char CODE_NEW_LINE = 'n';
        private const char CODE_CARRIAGE_RETURN = 'r';
        private const char CODE_UNICODE = 'u';

        private const string TEXT_COMMENT_HASH = "#";

        private const string ESCAPED_ESCAPE = "\\\\";
        private const string ESCAPED_HASH = "\\#";
        private const string ESCAPED_PLING = "\\!";
        private const string ESCAPED_FORM_FEED = "\\f";
        private const string ESCAPED_HORIZONTAL_TAB = "\\t";
        private const string ESCAPED_LINE_FEED = "\\n";
        private const string ESCAPED_CARRIAGE_RETURN = "\\r";
        private const string ESCAPED_UNICODE = "\\u";

        /// <summary>
        /// Whether or not to output the creation timestamp as a comment at the top of the CSV file.
        /// </summary>
        private bool outputTimestamp = true;

        /// <summary>
        /// Gets or sets a flag indicating if the creation timestamp should be added as a comment at the top of the CSV file - default is true.
        /// </summary>
        public bool OutputTimestamp
        {
            get => outputTimestamp;
            set => outputTimestamp = value;
        }


        // Converted to use Dictionary<TKey,TValue> in place of Hashtable when switched to .NET Standard.
        private Dictionary<string, string> hashtable;

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
        /// Write the properties to the output stream with the default encoding.
        /// </summary>
        /// <param name="stream">The output stream where the properties are written.</param>
        /// <param name="comments">Optional comments that are placed at the beginning of the output.</param>
        public void Write(Stream stream, string comments)
        {
            Write(stream, comments, null);
        }

        /// <summary>
        /// Write the properties to the output stream.
        /// </summary>
        /// <param name="stream">The output stream where the properties are written.</param>
        /// <param name="comments">Optional comments that are placed at the beginning of the output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding">encoding</see> that is used to write the properies file stream.</param>
        public void Write(Stream stream, string comments, Encoding encoding)
        {
            //  Create a writer to output with the specified encoding.
            var writerEncoding = encoding ?? JavaProperties.DefaultEncoding;
            StreamWriter writer = new StreamWriter(stream, writerEncoding);

            if (comments != null)
            {
                writer.WriteLine(CHAR_COMMENT_HASH + " " + comments);
            }

            if (outputTimestamp)
            {
                writer.WriteLine(CHAR_COMMENT_HASH + " " + DateTime.Now.ToString());
            }

            for (IEnumerator e = hashtable.Keys.GetEnumerator(); e.MoveNext();)
            {
                string key = e.Current.ToString();
                string val = hashtable[key].ToString();

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
            StringBuilder buf = new StringBuilder();
            bool first = true;
            foreach (char c in s)
            {

                switch (c)
                {
                    //  Avoid confusing with a comment if key starts with '!' (33) or '#' (35).
                    case CHAR_COMMENT_PLING:
                        buf.Append(ESCAPED_PLING);
                        break;

                    case CHAR_COMMENT_HASH:
                        buf.Append(ESCAPED_HASH);
                        break;

                    case CHAR_HORIZONTAL_TAB:  //  =09 U+0009  HORIZONTAL TABULATION   \t
                        buf.Append(ESCAPED_HORIZONTAL_TAB);
                        break;

                    case CHAR_LINE_FEED:  //  =0A U+000A  LINE FEED               \n
                        buf.Append(ESCAPED_LINE_FEED);
                        break;

                    case CHAR_FORM_FEED:  //  =0C U+000C  FORM FEED               \f
                        buf.Append(ESCAPED_FORM_FEED);
                        break;

                    case CHAR_CARRIAGE_RETURN:  //  =0D U+000D  CARRIAGE RETURN         \r
                        buf.Append(ESCAPED_CARRIAGE_RETURN);
                        break;

                    case ' ':   //  32: ' '
                        if (isKey || first)
                        {
                            buf.Append(CHAR_ESCAPE);
                        }
                        buf.Append(c);
                        break;

                    case ':':   //  58: ':'
                    case '=':   //  61: '='
                    case CHAR_ESCAPE:  //  92: '\'
                        buf.Append(CHAR_ESCAPE).Append(c);
                        break;

                    default:
                        if (c > 31 && c < 127)
                        {
                            buf.Append(c);
                        }
                        else
                        {
                            buf.Append(ESCAPED_UNICODE);
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
