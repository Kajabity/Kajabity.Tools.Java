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
using System.Collections;
using System.Text;
using System.Collections.Generic;

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
        private const string ESCAPED_FORM_FEED = "\\f";
        private const string ESCAPED_HORIZONTAL_TAB = "\\t";
        private const string ESCAPED_LINE_FEED = "\\n";
        private const string ESCAPED_CARRIAGE_RETURN = "\\r";
        private const string ESCAPED_UNICODE = "\\u";

        // Converted to use Dictionary<TKey,TValue> in place of Hashtable when switched to .NET Standard.
        private Dictionary<string, string> hashtable;

        /// <summary>
        /// Construct an instance of this class.
        /// </summary>
        /// <param name="hashtable">The Hashtable (or JavaProperties) instance
        /// whose values are to be written.</param>
        public JavaPropertyWriter( Dictionary<string, string> hashtable )
        {
            this.hashtable = hashtable;
        }

        /// <summary>
        /// Write the properties to the output stream.
        /// </summary>
        /// <param name="stream">The output stream where the properties are written.</param>
        /// <param name="comments">Optional comments that are placed at the beginning of the output.</param>
        public void Write( Stream stream, string comments )
        {
            //  Create a writer to output to an ISO-8859-1 encoding (code page 28592).
            StreamWriter writer = new StreamWriter( stream, System.Text.Encoding.GetEncoding( "iso-8859-2" ) );

            //TODO: Confirm correct codepage:
            //  28592              iso-8859-2                   Central European (ISO)
            //  28591              iso-8859-1                   Western European (ISO)
            //  from http://msdn.microsoft.com/en-us/library/system.text.encodinginfo.getencoding.aspx

            if( comments != null )
            {
                writer.WriteLine( CHAR_COMMENT_HASH + " " + comments );
            }

            writer.WriteLine( CHAR_COMMENT_HASH + " " + DateTime.Now.ToString() );

            for( IEnumerator e = hashtable.Keys.GetEnumerator(); e.MoveNext(); )
            {
                string key = e.Current.ToString();
                string val = hashtable[ key ].ToString();

                writer.WriteLine( escapeKey( key ) + "=" + escapeValue( val ) );
            }

            writer.Flush();
        }

        /// <summary>
        /// Escape the string as a Key with character set ISO-8859-1 -
        /// the characters 0-127 are US-ASCII and we will escape any others.  The passed string is Unicode which extends
        /// ISO-8859-1 - so all is well.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string escapeKey( string s )
        {
            StringBuilder buf = new StringBuilder();
            bool first = true;

            foreach( char c in s )
            {
                //  Avoid confusing with a comment if key starts with '!' (33) or '#' (35).
                if( first )
                {
                    first = false;
                    if( c == CHAR_COMMENT_PLING || c == CHAR_COMMENT_HASH )
                    {
                        buf.Append( CHAR_ESCAPE );
                    }
                }

                switch( c )
                {
                    case CHAR_HORIZONTAL_TAB:  //  =09 U+0009  HORIZONTAL TABULATION   \t
                        buf.Append( ESCAPED_HORIZONTAL_TAB );
                        break;
                    case CHAR_LINE_FEED:  //  =0A U+000A  LINE FEED               \n
                        buf.Append( ESCAPED_LINE_FEED );
                        break;
                    case CHAR_FORM_FEED:  //  =0C U+000C  FORM FEED               \f
                        buf.Append( ESCAPED_FORM_FEED );
                        break;
                    case CHAR_CARRIAGE_RETURN:  //  =0D U+000D  CARRIAGE RETURN         \r
                        buf.Append( ESCAPED_CARRIAGE_RETURN );
                        break;

                    case ' ':   //  32: ' '
                    case ':':   //  58: ':'
                    case '=':   //  61: '='
                    case CHAR_ESCAPE:  //  92: '\'
                        buf.Append( CHAR_ESCAPE ).Append( c );
                        break;

                    default:
                        if( c > 31 && c < 127 )
                        {
                            buf.Append( c );
                        }
                        else
                        {
                            buf.Append( ESCAPED_UNICODE );
                            buf.Append( ((int) c).ToString( "X4" ) );
                        }
                        break;
                }
            }

            return buf.ToString();
        }

        private string escapeValue( string s )
        {
            StringBuilder buf = new StringBuilder();
            bool first = true;

            foreach( char c in s )
            {
                //  Handle value starting with whitespace.
                if( first )
                {
                    first = false;
                    if( c == ' ' || c == CHAR_HORIZONTAL_TAB )
                    {
                        buf.Append( CHAR_ESCAPE ).Append( c );
                        continue;
                    }
                }

                switch( c )
                {
                    case CHAR_HORIZONTAL_TAB:  //  =09 U+0009  HORIZONTAL TABULATION   \t
                        buf.Append( c );  //OK after first position.
                        break;
                    case CHAR_LINE_FEED:  //  =0A U+000A  LINE FEED               \n
                        buf.Append( ESCAPED_LINE_FEED );
                        break;
                    case CHAR_FORM_FEED:  //  =0C U+000C  FORM FEED               \f
                        buf.Append( ESCAPED_FORM_FEED );
                        break;
                    case CHAR_CARRIAGE_RETURN:  //  =0D U+000D  CARRIAGE RETURN         \r
                        buf.Append( ESCAPED_CARRIAGE_RETURN );
                        break;
                    case CHAR_ESCAPE:  //  92: '\'
                        buf.Append( CHAR_ESCAPE ).Append( c );
                        break;

                    default:
                        if( c > 31 && c < 127 )
                        {
                            buf.Append( c );
                        }
                        else
                        {
                            buf.Append( ESCAPED_UNICODE );
                            buf.Append( ((int) c).ToString( "X4" ) );
                        }
                        break;
                }
            }

            return buf.ToString();
        }
    }
}
