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

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kajabity.Tools.Java
{
    /// <summary>
    /// This class reads Java style properties from an input stream.
    /// </summary>
    public class JavaPropertyReader
    {
        private const int MatchEndOfInput = 1;
        private const int MatchTerminator = 2;
        private const int MatchWhitespace = 3;
        private const int MatchAny = 4;

        private const int ActionAddToKey = 1;
        private const int ActionAddToValue = 2;
        private const int ActionStoreProperty = 3;
        private const int ActionEscape = 4;
        private const int ActionIgnore = 5;

        private const int StateStart = 0;
        private const int StateComment = 1;
        private const int StateKey = 2;
        private const int StateKeyEscape = 3;
        private const int StateKeyWs = 4;
        private const int StateBeforeSeparator = 5;
        private const int StateAfterSeparator = 6;
        private const int StateValue = 7;
        private const int StateValueEscape = 8;
        private const int StateValueWs = 9;
        private const int StateFinish = 10;

        private static string[] _stateNames = new string[]
        { nameof(StateStart), nameof(StateComment), nameof(StateKey), nameof(StateKeyEscape), nameof(StateKeyWs),
            nameof(StateBeforeSeparator), nameof(StateAfterSeparator), nameof(StateValue), nameof(StateValueEscape),
            nameof(StateValueWs), nameof(StateFinish) };

        private static readonly int[][] States = new int[][] {
            new int[]{//StateStart
                MatchEndOfInput, StateFinish,           ActionIgnore,
                MatchTerminator, StateStart,            ActionIgnore,
                '#',             StateComment,          ActionIgnore,
                '!',             StateComment,          ActionIgnore,
                MatchWhitespace, StateStart,            ActionIgnore,
                '\\',            StateKeyEscape,        ActionEscape,
                ':',             StateAfterSeparator,   ActionIgnore,
                '=',             StateAfterSeparator,   ActionIgnore,
                MatchAny,        StateKey,              ActionAddToKey,
            },
            new int[]{//StateComment
                MatchEndOfInput, StateFinish,           ActionIgnore,
                MatchTerminator, StateStart,            ActionIgnore,
                MatchAny,        StateComment,          ActionIgnore,
            },
            new int[]{//StateKey
                MatchEndOfInput, StateFinish,           ActionStoreProperty,
                MatchTerminator, StateStart,            ActionStoreProperty,
                MatchWhitespace, StateBeforeSeparator,  ActionIgnore,
                '\\',            StateKeyEscape,        ActionEscape,
                ':',             StateAfterSeparator,   ActionIgnore,
                '=',             StateAfterSeparator,   ActionIgnore,
                MatchAny,        StateKey,              ActionAddToKey,
            },
            new int[]{//StateKeyEscape
                MatchTerminator, StateKeyWs,            ActionIgnore,
                MatchAny,        StateKey,              ActionAddToKey,
            },
            new int[]{//StateKeyWs
                MatchEndOfInput, StateFinish,           ActionStoreProperty,
                MatchTerminator, StateStart,            ActionStoreProperty,
                MatchWhitespace, StateKeyWs,            ActionIgnore,
                '\\',            StateKeyEscape,        ActionEscape,
                ':',             StateAfterSeparator,   ActionIgnore,
                '=',             StateAfterSeparator,   ActionIgnore,
                MatchAny,        StateKey,              ActionAddToKey,
            },
            new int[]{//StateBeforeSeparator
                MatchEndOfInput, StateFinish,           ActionStoreProperty,
                MatchTerminator, StateStart,            ActionStoreProperty,
                MatchWhitespace, StateBeforeSeparator,  ActionIgnore,
                '\\',            StateValueEscape,      ActionEscape,
                ':',             StateAfterSeparator,   ActionIgnore,
                '=',             StateAfterSeparator,   ActionIgnore,
                MatchAny,        StateValue,            ActionAddToValue,
            },
            new int[]{//StateAfterSeparator
                MatchEndOfInput, StateFinish,           ActionStoreProperty,
                MatchTerminator, StateStart,            ActionStoreProperty,
                MatchWhitespace, StateAfterSeparator,   ActionIgnore,
                '\\',            StateValueEscape,      ActionEscape,
                MatchAny,        StateValue,            ActionAddToValue,
            },
            new int[]{//StateValue
                MatchEndOfInput, StateFinish,           ActionStoreProperty,
                MatchTerminator, StateStart,            ActionStoreProperty,
                '\\',            StateValueEscape,      ActionEscape,
                MatchAny,        StateValue,            ActionAddToValue,
            },
            new int[]{//StateValueEscape
                MatchTerminator, StateValueWs,          ActionIgnore,
                MatchAny,        StateValue,            ActionAddToValue
            },
            new int[]{//StateValueWs
                MatchEndOfInput, StateFinish,           ActionStoreProperty,
                MatchTerminator, StateStart,            ActionStoreProperty,
                MatchWhitespace, StateValueWs,          ActionIgnore,
                '\\',            StateValueEscape,      ActionEscape,
                MatchAny,        StateValue,            ActionAddToValue,
            }
        };

        // Converted to use Dictionary<TKey,TValue> in place of Hashtable when switched to .NET Standard.
        private readonly Dictionary<string, string> hashtable;

        private bool escaped;
        private readonly StringBuilder keyBuilder = new StringBuilder();
        private readonly StringBuilder valueBuilder = new StringBuilder();

        /// <summary>
        /// Configures the way how duplicate keys will be handled.
        /// </summary>
        public DuplicateKeyResolution DuplicateKeyResolution { get; set; } = DuplicateKeyResolution.Overwrite;

        /// <summary>
        /// Construct a reader passing a reference to a Hashtable (or JavaProperties) instance
        /// where the keys are to be stored.
        /// </summary>
        /// <param name="hashtable">A reference to a hashtable where the key-value pairs can be stored.</param>
        public JavaPropertyReader(Dictionary<string, string> hashtable)
        {
            this.hashtable = hashtable;
        }

        /// <summary>
        /// <para>Load key value pairs (properties) from an input Stream.
        /// The input stream (usually reading from a ".properties" file) consists of a series of lines (terminated
        /// by \r, \n or \r\n) each a key value pair, a comment or a blank line.</para>
        ///
        /// <para>Leading whitespace (spaces, tabs, formfeeds) are ignored at the start of any line - and a line that is empty or
        /// contains only whitespace is blank and ignored.</para>
        ///
        /// <para>A line with the first non-whitespace character is a '#' or '!' is a comment line and the rest of the line is
        /// ignored.</para>
        ///
        /// <para>If the first non-whitespace character is not '#' or '!' then it is the start of a key.  A key is all the
        /// characters up to the first whitespace or a key/value separator - '=' or ':'.</para>
        ///
        /// <para>The separator is optional.  Any whitespace after the key or after the separator (if present) is ignored.</para>
        ///
        /// <para>The first non-whitespace character after the separator (or after the key if no separator) begins the value.
        /// The value may include whitespace, separators, or comment characters.</para>
        ///
        /// <para>Any unicode character may be included in either key or value by using escapes preceded by the escape
        /// character '\'.</para>
        ///
        /// <para>The following special cases are defined:</para>
        /// <code>
        /// 	'\t' - horizontal tab.
        /// 	'\f' - form feed.
        /// 	'\r' - return
        /// 	'\n' - new line
        /// 	'\\' - add escape character.
        ///
        /// 	'\ ' - add space in a key or at the start of a value.
        /// 	'\!', '\#' - add comment markers at the start of a key.
        /// 	'\=', '\:' - add a separator in a key.
        /// </code>
        ///
        /// <para>Any unicode character using the following escape:</para>
        /// <code>
        /// 	'\uXXXX' - where XXXX represents the unicode character code as 4 hexadecimal digits.
        /// </code>
        ///
        /// <para>Finally, longer lines can be broken by putting an escape at the very end of the line.  Any leading space
        /// (unless escaped) is skipped at the beginning of the following line.</para>
        ///
        /// Examples
        /// <code>
        /// 	a-key = a-value
        /// 	a-key : a-value
        /// 	a-key=a-value
        /// 	a-key a-value
        /// </code>
        ///
        /// <para>All the above will result in the same key/value pair - key "a-key" and value "a-value".</para>
        /// <code>
        /// 	! comment...
        /// 	# another comment...
        /// </code>
        ///
        /// <para>The above are two examples of comments.</para>
        /// <code>
        /// 	Honk\ Kong = Near China
        /// </code>
        ///
        /// <para>The above shows how to embed a space in a key - key is "Hong Kong", value is "Near China".</para>
        /// <code>
        /// 	a-longer-key-example = a really long value that is \
        /// 			split over two lines.
        /// </code>
        ///
        /// <para>An example of a long line split into two.</para>
        /// </summary>
        /// <param name="stream">The input stream that the properties are read from.</param>
        /// <param name="encoding">The <see cref="Encoding">encoding</see> that is used to read the properties file stream.</param>
        public void Parse(Stream stream, Encoding encoding = null)
        {
            // The default encoding will be used if we do not pass explicitly different encoding
            var parserEncoding = encoding ?? JavaProperties.DefaultEncoding;
            reader = new StreamReader(stream, parserEncoding);

            var state = StateStart;
            do
            {
                var ch = NextChar();

                var matched = false;

                for (var s = 0; s < States[state].Length; s += 3)
                {
                    if (!Matches(States[state][s], ch))
                    {
                        continue;
                    }

                    //Debug.WriteLine( stateNames[ state ] + ", " + (s/3) + ", " + ch + (ch>20?" (" + (char) ch + ")" : "") );
                    matched = true;
                    DoAction(States[state][s + 2], ch);

                    state = States[state][s + 1];
                    break;
                }

                if (!matched)
                {
                    throw new ParseException("Unexpected character at " + 1 + ": <<<" + ch + ">>>");
                }
            } while (state != StateFinish);
        }

        private bool Matches(int match, int ch)
        {
            switch (match)
            {
                case MatchEndOfInput:
                    return ch == -1;

                case MatchTerminator:
                    return IsTerminator(ch);

                case MatchWhitespace:
                    return ch == ' ' || ch == '\t' || ch == '\f';

                case MatchAny:
                    return true;

                default:
                    return ch == match;
            }
        }

        private void DoAction(int action, int ch)
        {
            switch (action)
            {
                case ActionAddToKey:
                    keyBuilder.Append(EscapedChar(ch));
                    escaped = false;
                    break;

                case ActionAddToValue:
                    valueBuilder.Append(EscapedChar(ch));
                    escaped = false;
                    break;

                case ActionStoreProperty:
                    //Debug.WriteLine( keyBuilder.ToString() + "=" + valueBuilder.ToString() );

                    var propToAdd = keyBuilder.ToString();
                    if (DuplicateKeyResolution == DuplicateKeyResolution.Throw
                        && hashtable.ContainsKey(propToAdd))
                    {
                        throw new ParseException($"Duplicate key: <<<{propToAdd}>>>.");
                    }
                    // Corrected to avoid duplicate entry errors - thanks to David Tanner.
                    hashtable[propToAdd] = valueBuilder.ToString();

                    keyBuilder.Length = 0;
                    valueBuilder.Length = 0;
                    escaped = false;
                    break;

                case ActionEscape:
                    escaped = true;
                    break;

                //case ACTION_ignore:
                default:
                    escaped = false;
                    break;
            }
        }

        private char EscapedChar(int ch)
        {
            if (!escaped)
            {
                return (char) ch;
            }

            switch (ch)
            {
                case 't':
                    return '\t';
                case 'r':
                    return '\r';
                case 'n':
                    return '\n';
                case 'f':
                    return '\f';
                case 'u':
                    var uch = 0;
                    var i = 0;
                    do
                    {
                        ch = NextChar();
                        if (ch == '\\' && IsTerminator(NextChar()))
                        {
                            // Skip past it.
                        }
                        else if (ch >= '0' && ch <= '9')
                        {
                            uch = (uch << 4) + ch - '0';
                            i++;
                        }
                        else if (ch >= 'a' && ch <= 'z')
                        {
                            uch = (uch << 4) + ch - 'a' + 10;
                            i++;
                        }
                        else if (ch >= 'A' && ch <= 'Z')
                        {
                            uch = (uch << 4) + ch - 'A' + 10;
                            i++;
                        }
                        else
                        {
                            throw new ParseException("Invalid Unicode character.");
                        }
                    }
                    while (i < 4);

                    return (char)uch;
            }

            return (char)ch;
        }

        private bool IsTerminator(int ch)
        {
            switch (ch)
            {
                case '\r':
                {
                    if (PeekChar() == '\n')
                    {
                        saved = false;
                    }
                    return true;
                }
                case '\n':
                    return true;
                default:
                    return false;
            }
        }

        // we now use a StreamReader, which supports encodings
        private StreamReader reader;
        private int savedChar;
        private bool saved;

        private int NextChar()
        {
            if (saved)
            {
                saved = false;
                return savedChar;
            }

            return ReadCharSafe();
        }

        private int PeekChar()
        {
            if (saved)
            {
                return savedChar;
            }

            saved = true;
            return savedChar = ReadCharSafe();
        }

        /// <summary>
        /// A method to substitute calls to <c>stream.ReadByte()</c>.
        /// The <see cref="JavaPropertyReader" /> now uses a <see cref="StreamReader"/> to read properties.
        /// <para>
        /// If the stream is already processed to the end, return <c>-1</c>.
        /// </para>
        /// </summary>
        /// <returns>A character code or <c>-1</c> if no more characters are available.</returns>
        private int ReadCharSafe()
        {
            // reader.Read() will take into account the encoding.
            return reader.Read();
        }
    }
}
