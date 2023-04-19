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

namespace Idaax.CoAP.Helpers
{
    /// <summary>
    /// CoAPSharp requires a logger class to work. If you forget to implement a logger, this
    /// library will fail. That is not good :-)
    /// Therefore, this default logger class is made to ensure, that if you forget to 
    /// provide your own logging class, then nothing fails, but all logging calls are
    /// silently ignored.
    /// You can argue that we could have used the "Debug" class from SPOT namespace, but
    /// we would like to keep the entire library free from any such namespace
    /// </summary>
    public class DefaultLogger : AbstractLogUtil
    {
        /// <summary>
        /// The only method that is required to be implemented.
        /// This mthod is a dummy implementation and thus all
        /// calls simply do nothing
        /// </summary>
        /// <param name="level">The log level</param>
        /// <param name="message">The message to log</param>
        protected override void LogIt(LogLevel level, string message)
        {
            //No Work :-)
        }
    }
}
