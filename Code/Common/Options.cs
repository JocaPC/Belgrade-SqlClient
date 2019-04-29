﻿//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license.
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
using System.Text;

namespace Belgrade.SqlClient
{
    public class Options
    {
        public string Prefix;
        public string Suffix;
        public object DefaultOutput;
        public Encoding OutputEncoding = Encoding.UTF8;
    }
}