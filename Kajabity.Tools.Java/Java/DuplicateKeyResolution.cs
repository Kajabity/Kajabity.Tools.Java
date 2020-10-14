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

namespace Kajabity.Tools.Java
{
    /// <summary>
    /// Defines the strategy how to handle duplicate keys when reading the property file.
    /// </summary>
    public enum DuplicateKeyResolution
    {
        /// <summary>
        /// Overwrite the key with a new value.
        /// 
        /// Last one wins.
        /// </summary>
        Overwrite = 1,

        /// <summary>
        /// Throw an exception.
        /// </summary>
        Throw = 2
    }
}
