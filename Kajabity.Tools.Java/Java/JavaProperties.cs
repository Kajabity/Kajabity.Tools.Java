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
    /// Hold Java style properties as key-value pairs and allow them to be loaded from or
    /// saved to a ".properties" file. By default, the file is stored with character set ISO-8859-1 which extends US-ASCII
    /// (the characters 0-127 are the same) and forms the first part of the Unicode character set.  Within the
    /// application <see cref="string"/> are Unicode - but all values outside the basic US-ASCII set are escaped.
    /// </summary>
    public class JavaProperties : Dictionary<string, string>
    {
        /// <summary>
        /// Gets a reference to the ISO-8859-1 encoding (code page 28591). This is the Java standard for .properties files.
        /// </summary>
        internal static Encoding DefaultEncoding => Encoding.GetEncoding("iso-8859-1");

        //TODO: Confirm correct codepage:
        //  28591              iso-8859-1                   Western European (ISO)
        //  28592              iso-8859-2                   Central European (ISO)
        //  from http://msdn.microsoft.com/en-us/library/system.text.encodinginfo.getencoding.aspx

        /// <summary>
        /// A reference to an optional set of default properties - these values are returned
        /// if the value has not been loaded from a ".properties" file or set programatically.
        /// </summary>
        protected Dictionary<string, string> Defaults;

        /// <summary>
        /// An empty constructor that doesn't set the defaults.
        /// </summary>
        public JavaProperties()
        {
        }

        /// <summary>
        /// Use this constructor to provide a set of default values.  The default values are kept separate
        /// to the ones in this instant.
        /// </summary>
        /// <param name="defaults">A Hashtable that holds a set of defafult key value pairs to
        /// return when the requested key has not been set.</param>
        public JavaProperties(Dictionary<string, string> defaults)
        {
            this.Defaults = defaults;
        }

        /// <summary>
        /// Load Java Properties from a stream expecting the format as described in <see cref="JavaPropertyReader"/>.
        /// </summary>
        /// <param name="streamIn">An input stream to read properties from.</param>
        /// <exception cref="ParseException">If the stream source is invalid.</exception>
        public void Load(Stream streamIn)
        {
            JavaPropertyReader reader = new JavaPropertyReader(this);
            reader.Parse(streamIn);
        }

        /// <summary>
        /// Load Java Properties from a stream with the specified encoding and 
        /// expecting the format as described in <see cref="JavaPropertyReader"/>.
        /// </summary>
        /// <param name="streamIn">An input stream to read properties from.</param>
        /// <param name="encoding">The stream's encoding.</param>
        public void Load(Stream streamIn, Encoding encoding)
        {
            JavaPropertyReader reader = new JavaPropertyReader(this);
            reader.Parse(streamIn, encoding);
        }

        /// <summary>
        /// Store the contents of this collection of properties to the stream in the format
        /// used for Java ".properties" files using an instance of <see cref="JavaPropertyWriter"/>.
        /// The keys and values will be minimally escaped to ensure special characters are read back
        /// in properly.  Keys are not sorted.  The file will begin with a comment identifying the
        /// date - and an additional comment may be included.
        /// </summary>
        /// <param name="streamOut">An output stream to write the properties to.</param>
        /// <param name="comments">Optional additional comment to include at the head of the output (null to omit).</param>
        public void Store(Stream streamOut, string comments)
        {
            JavaPropertyWriter writer = new JavaPropertyWriter(this);
            writer.Write(streamOut, comments);
        }

        /// <summary>
        /// Store the contents of this collection of properties to the stream in the format
        /// used for Java ".properties" files using an instance of <see cref="JavaPropertyWriter"/>.
        /// The keys and values will be minimally escaped to ensure special characters are read back
        /// in properly.  Keys are not sorted.  The file may begin with a comment identifying the
        /// date.
        /// </summary>
        /// <param name="streamOut">An output stream to write the properties to.</param>
        /// <param name="outputTimestamp">Indicate that a comment with a timestamp should be output (true) or not (false).</param>
        public void Store(Stream streamOut, bool outputTimestamp)
        {
            JavaPropertyWriter writer = new JavaPropertyWriter(this);
            writer.OutputTimestamp = outputTimestamp;
            writer.Write(streamOut, null);
        }

        /// <summary>
        /// Store the contents of this collection of properties to the stream in the format
        /// used for Java ".properties" files using an instance of <see cref="JavaPropertyWriter"/>.
        /// The keys and values will be minimally escaped to ensure special characters are read back
        /// in properly.  Keys are not sorted.  The file may begin with a comment identifying the
        /// date - and an additional comment may be included.
        /// </summary>
        /// <param name="streamOut">An output stream to write the properties to.</param>
        /// <param name="comments">Optional additional comment to include at the head of the output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding">encoding</see> that is used to write the properies file stream.</param>
        /// <param name="outputTimestamp">Indicate that a comment with a timestamp should be output (true) or not (false).</param>
        public void Store(Stream streamOut, string comments, Encoding encoding, bool outputTimestamp)
        {
            JavaPropertyWriter writer = new JavaPropertyWriter(this);
            writer.OutputTimestamp = outputTimestamp;
            writer.Write(streamOut, comments, encoding);
        }

        /// <summary>
        /// Get the value for the specified key value.  If the key is not found, then return the
        /// default value - and if still not found, return null.
        /// </summary>
        /// <param name="key">The key whose value should be returned.</param>
        /// <returns>The value corresponding to the key - or null if not found.</returns>
        public string GetProperty(string key)
        {
            Object objectValue = this[key];
            if (objectValue != null)
            {
                return AsString(objectValue);
            }
            else if (Defaults != null)
            {
                return AsString(Defaults[key]);
            }

            return null;
        }

        /// <summary>
        /// Get the value for the specified key value.  If the key is not found, then return the
        /// default value - and if still not found, return <c>defaultValue</c>.
        /// </summary>
        /// <param name="key">The key whose value should be returned.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value corresponding to the key - or null if not found.</returns>
        public string GetProperty(string key, string defaultValue)
        {
            string val = GetProperty(key);
            return val ?? defaultValue;
        }

        /// <summary>
        /// Set the value for a property key.  The old value is returned - if any.
        /// </summary>
        /// <param name="key">The key whose value is to be set.</param>
        /// <param name="newValue">The new value off the key.</param>
        /// <returns>The original value of the key - as a string.</returns>
        public string SetProperty(string key, string newValue)
        {
            string oldValue = AsString(this[key]);
            this[key] = newValue;
            return oldValue;
        }

        /// <summary>
        /// Returns an enumerator of all the properties available in this instance - including the
        /// defaults.
        /// </summary>
        /// <returns>An enumarator for all of the keys including defaults.</returns>
        public IEnumerator PropertyNames()
        {
            Dictionary<string, string> combined;
            if (Defaults != null)
            {
                combined = new Dictionary<string, string>(Defaults);

                for (IEnumerator e = Keys.GetEnumerator(); e.MoveNext();)
                {
                    string key = AsString(e.Current);
                    combined.Add(key, this[key]);
                }
            }
            else
            {
                combined = new Dictionary<string, string>(this);
            }

            return combined.Keys.GetEnumerator();
        }

        /// <summary>
        /// A utility method to safely convert an <c>Object</c> to a <c>string</c>.
        /// </summary>
        /// <param name="o">An Object or null to be returned as a string.</param>
        /// <returns>string value of the object - or null.</returns>
        private string AsString(Object o)
        {
            if (o == null)
            {
                return null;
            }

            return o.ToString();
        }
    }
}
