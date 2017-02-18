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

using System.Text;
using NUnit.Framework;

namespace Kajabity.Tools.Test
{
    /// <summary>
    /// A base test class providing utility methods.
    /// </summary>
    [SetUpFixture]
    public class KajabityToolsTest
    {
        /// <summary>
        /// Convert an array of strings to text.
        /// </summary>
        /// <param name="strings">an array of strings</param>
        /// <returns>a string representation of the string array</returns>
        public static string ToString( string[] strings )
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "{" );

            if( strings != null )
            {
                for( int i = 0; i < strings.Length; i++ )
                {
                    if( i > 0 )
                    {
                        builder.Append( ", " );
                    }
                    builder.Append( "\"" );
                    builder.Append( NoNull( strings[ i ] ) );
                    builder.Append( "\"" );
                }
            }

            builder.Append( "}" );
            return builder.ToString();
        }

        /// <summary>
        /// Return the string, or "" if the string is null.
        /// </summary>
        /// <param name="s">the string to check</param>
        /// <returns>a string that is not null</returns>
        public static string NoNull( string s )
        {
            if( s == null )
            {
                return "";
            }

            return s;
        }

        /// <summary>
        /// Returns true if two string arrays contain the same values.
        /// </summary>
        /// <param name="a">first string array to compare</param>
        /// <param name="b">second string array to compare</param>
        /// <returns>true if both string arrays are equal</returns>
        public bool CompareStringArray( string[] a, string[] b )
        {
            if( a == null )
            {
                return b == null;
            }

            if( b == null || a.Length != b.Length )
            {
                return false;
            }

            for( int i = 0; i < a.Length; i++ )
            {
                if( !a[ i ].Equals( b[ i ] ) )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
