/******************************************************************************
    CoAPSharp - C# Implementation of CoAP for .NET
    This library was originally written for .NET Micro framework. It is now
    migrated to nanoFramework.
    
    MIT License

    Copyright (c) [2023] [Idaax Inc., www.coapsharp.com]

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 *****************************************************************************/
using System;

namespace Idaax.CoAP.Exceptions
{
    /// <summary>
    /// Exception class that indicates a format exception.
    /// Not implementing ISerializable since that is not available in .NET micro framework
    /// and we do not intend to marshal exceptions in their serialized format in CoAP
    /// </summary>
    public class CoAPFormatException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoAPFormatException(){}
        /// <summary>
        /// Constructor with an error message
        /// </summary>
        /// <param name="message">Error message associated with the exception</param>
        public CoAPFormatException(string message) : base (message){}
        /// <summary>
        /// Constructor to wrap an exception
        /// </summary>
        /// <param name="message">Error message associated with the exception</param>
        /// <param name="inner">The actual exeption that is being wrapped by this exception</param>
        public CoAPFormatException(string message, Exception inner) : base(message, inner) { }
    }
}
