//*****************************************************************************
// Copyright © 2005, Bill Koukoutsis
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following disclaimer. 
//
// Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and/or other materials provided with the distribution. 
//
// Neither the name of the ORGANIZATION nor the names of its contributors may
// be used to endorse or promote products derived from this software without
// specific prior written permission. 
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//*****************************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Resources;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace BKSystem.IO
{
    /// <summary>
    ///		Creates a stream for reading and writing variable-length data.
    /// </summary>
    /// <remarks>
    ///		<b><font color="red">Notes to Callers:</font></b> Make sure to
    ///		include the "BitStream.resx" resource file in projects using the
    ///		<see cref="BitStream"/> class.<br></br>
    ///		<br></br>
    ///		[20051201]: Fixed problem with <c>public virtual void Write(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public virtual int Read(out ulong bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>public virtual void WriteTo(Stream bits);</c> to write
    ///		the contents of the current <b>bit</b> stream to another stream.<br></br>
    ///		<br></br>
    ///		[20051125]: Added the following implicit operators to allow type casting
    ///		instances of the <see cref="BitStream"/> class to and from other types
    ///		of <see cref="Stream"/> objects:<br></br>
    ///		<br></br>
    ///		<c>public static implicit operator BitStream(MemoryStream bits);</c><br></br>
    ///		<c>public static implicit operator MemoryStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator BufferedStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(NetworkStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(CryptoStream bits);</c><br></br>
    ///		<br></br>
    ///		[20051124]: Added <c>public virtual <see cref="byte"/> [] ToByteArray();</c> method.<br></br>
    ///		<br></br>
    ///		[20051124]: The <c>public override <see cref="int"/> ReadByte();</c> and
    ///		<c>public override void WriteByte(<see cref="byte"/> value)</c> method are now
    ///		supported by the <see cref="BitStream"/> class.<br></br>
    ///		<br></br>
    ///		[20051123]: Added <c>public BitStream(<see cref="Stream"/> bits);</c> contructor.<br></br>
    ///		<br></br>
    /// </remarks>
    /// <seealso cref="BitStream"/>
    /// <seealso cref="Stream"/>
    /// <seealso cref="int"/>
    /// <seealso cref="byte"/>
    public class BitStream : Stream
    {

        #region Nested Classes [20051116]

        #region private sealed class BitStreamResources [20051116]
        /// <summary>
        ///		Manages reading resources on behalf of the <see cref="BitStream"/>
        ///		class.
        /// </summary>
        /// <remarks>
        ///		<b><font color="red">Notes to Callers:</font></b> Make sure to
        ///		include the "BitStream.resx" resource file in projects using the
        ///		<see cref="BitStream"/> class.
        /// </remarks>
        private sealed class BitStreamResources
        {

            #region Fields [20051116]
            /// <summary>
            ///		The <see cref="ResourceManager"/> object.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static ResourceManager _resman;
            /// <summary>
            ///		An <see cref="Object"/> used to lock access to
            ///		<see cref="BitStream"/> resources while the current
            ///		<see cref="ResourceManager"/> is busy.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static object _oResManLock;
            /// <summary>
            ///		A <see cref="Boolean"/> value specifying whether a resource is
            ///		currently being loaded.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            /// <seealso cref="Boolean"/>
            private static bool _blnLoadingResource;

            #endregion


            #region Methods [20051116]
            /// <summary>
            ///		Initialises the resource manager.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static void InitialiseResourceManager()
            {
                if (_resman == null)
                {
                    lock (typeof(BitStreamResources))
                    {
                        if (_resman == null)
                        {
                            _oResManLock = new object();
                            _resman = new ResourceManager("BKSystem.IO.BitStream", typeof(BitStream).Assembly);
                        }
                    }
                }
            }
            /// <summary>
            ///		Gets the specified string resource.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            /// <param name="name">
            ///		A <see cref="String"/> representing the specified resource.
            /// </param>
            /// <returns>
            ///		A <see cref="String"/> representing the contents of the specified
            ///		resource.
            /// </returns>
            /// <seealso cref="String"/>
            public static string GetString(string name)
            {
                string str;
                if (_resman == null)
                    InitialiseResourceManager();

                lock (_oResManLock)
                {
                    if (_blnLoadingResource)
                        return ("The resource manager was unable to load the resource: " + name);

                    _blnLoadingResource = true;
                    str = _resman.GetString(name, null);
                    _blnLoadingResource = false;
                }
                return str;
            }

            #endregion

        }

        #endregion

        #endregion


        #region Constants [20051116]
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Byte"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Char"/>
        private const int SizeOfChar = 128;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="UInt16"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="UInt16"/>
        private const int SizeOfUInt16 = 16;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="UInt32"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="UInt32"/>
        private const int SizeOfUInt32 = 32;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Single"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Single"/>
        private const int SizeOfSingle = 32;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="UInt64"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="UInt64"/>
        private const int SizeOfUInt64 = 64;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Double"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Double"/>
        private const int SizeOfDouble = 64;
        /// <summary>
        ///		An <see cref="UInt32"/> value defining the number of bits
        ///		per element in the internal buffer.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private const uint BitBuffer_SizeOfElement = SizeOfUInt32;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bit
        ///		shifts equivalent to the number of bits per element in the
        ///		internal buffer.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        private const int BitBuffer_SizeOfElement_Shift = 5;
        /// <summary>
        ///		An <see cref="UInt32"/> value defining the equivalent of
        ///		a divisor in bitwise <b>AND</b> operations emulating
        ///		modulo calculations.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private const uint BitBuffer_SizeOfElement_Mod = 31;
        /// <summary>
        ///		An <see cref="UInt32"/> array defining a series of values
        ///		useful in generating bit masks in read and write operations.
        /// </summary>
        /// <remarks>
        ///		This field is static.
        /// </remarks>
        private static uint[] BitMaskHelperLUT = new uint[]
		{
			0x00000000, 
			0x00000001, 0x00000003, 0x00000007, 0x0000000F,
			0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF,
			0x000001FF, 0x000003FF, 0x000007FF, 0x00000FFF,
			0x00001FFF, 0x00003FFF, 0x00007FFF, 0x0000FFFF,
			0x0001FFFF, 0x0003FFFF, 0x0007FFFF, 0x000FFFFF,
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,
			0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF, 0x0FFFFFFF,
			0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF,
		};

        #endregion


        #region Fields [20051114]
        /// <summary>
        ///		A <see cref="Boolean"/> value specifying whether the current
        ///		stream is able to process data.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b>true</b> by default.
        /// </remarks>
        /// <seealso cref="Boolean"/>
        private bool _blnIsOpen = true;
        /// <summary>
        ///		An array of <see cref="UInt32"/> values specifying the internal
        ///		bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint[] _auiBitBuffer;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current length of the
        ///		internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Length;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current elemental index
        ///		of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current bit index for the
        ///		current element of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_BitIndex;
        /// <summary>
        ///		An <see cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
        /// <see cref="IFormatProvider"/>
        /// <see cref="CultureInfo.InvariantCulture"/>
        private static IFormatProvider _ifp = (IFormatProvider)CultureInfo.InvariantCulture;

        #endregion


        #region Properties [20051116]
        /// <summary>
        ///		Gets the length of this stream in <b>bits</b>.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the length of this stream in
        ///		<b>bits</b>.
        /// </value>
        /// <seealso cref="Int64"/>
        public override long Length
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)_uiBitBuffer_Length;
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///		// to read the entire stream into a <see cref="Byte"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">byte</font> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(abytBuffer, 0, (<font color="blue">int</font>)bstrm.Length8);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>8-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(byte [], int, int)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">short</font> [] asBuffer = <font color="blue">new short</font> [bstrm.Length16];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(asBuffer, 0, (<font color="blue">int</font>)bstrm.Length16);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>16-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(short [], int, int)"/>
        /// <seealso cref="Int16"/>
        /// <seealso cref="Int64"/>
        public virtual long Length16
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 4) + (long)((_uiBitBuffer_Length & 15) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>32-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(int [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int32"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">int</font> [] aiBuffer = <font color="blue">new int</font> [bstrm.Length32];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(aiBuffer, 0, (<font color="blue">int</font>)bstrm.Length32);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>32-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(int [], int, int)"/>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Int64"/>
        public virtual long Length32
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 5) + (long)((_uiBitBuffer_Length & 31) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>64-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(long [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int64"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">long</font> [] alBuffer = <font color="blue">new long</font> [bstrm.Length64];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(alBuffer, 0, (<font color="blue">int</font>)bstrm.Length64);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>64-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(long [], int, int)"/>
        /// <seealso cref="Int64"/>
        public virtual long Length64
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 6) + (long)((_uiBitBuffer_Length & 63) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the number of <b>bits</b> allocatated to this stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the number of <b>bits</b>
        ///		allocated to this stream.
        /// </value>
        public virtual long Capacity
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return ((long)_auiBitBuffer.Length) << BitBuffer_SizeOfElement_Shift;
            }
        }
        /// <summary>
        ///		Gets or sets the current <b>bit</b> position within this stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        ///	<exception cref="System.ArgumentOutOfRangeException">
        ///		The position is set to a negative value or position + 1 is geater than
        ///		<see cref="Length"/>.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the current <b>position</b>
        ///		within this stream.
        /// </value>
        /// <seealso cref="Length"/>
        /// <seealso cref="Int64"/>
        public override long Position
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                uint uiPosition = (_uiBitBuffer_Index << BitBuffer_SizeOfElement_Shift) + _uiBitBuffer_BitIndex;
                return (long)uiPosition;
            }
            set
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", BitStreamResources.GetString("ArgumentOutOfRange_NegativePosition"));

                uint uiRequestedPosition = (uint)value;

                if (_uiBitBuffer_Length < uiRequestedPosition + 1)
                    throw new ArgumentOutOfRangeException("value", BitStreamResources.GetString("ArgumentOutOfRange_InvalidPosition"));

                _uiBitBuffer_Index = uiRequestedPosition >> BitBuffer_SizeOfElement_Shift;
                if ((uiRequestedPosition & BitBuffer_SizeOfElement_Mod) > 0)
                    _uiBitBuffer_BitIndex = (uiRequestedPosition & BitBuffer_SizeOfElement_Mod);
                else
                    _uiBitBuffer_BitIndex = 0;
            }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports reading.
        /// </value>
        /// <seealso cref="Boolean"/>
        public override bool CanRead
        {
            get { return _blnIsOpen; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <remarks>
        ///		This method always returns <b>false</b>. To set the position within
        ///		the current stream use the <see cref="Position"/> property instead.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports seeking.
        /// </value>
        /// <seealso cref="Position"/>
        /// <seealso cref="Boolean"/>
        public override bool CanSeek
        {
            get { return false; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports writing.
        /// </value>
        /// <seealso cref="Boolean"/>
        public override bool CanWrite
        {
            get { return _blnIsOpen; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports setting
        ///		its length.
        /// </summary>
        /// <remarks>
        ///		This field always returns <b>false</b>. All write operations at the
        ///		end of the <b>BitStream</b> expand the <b>BitStream</b> automatically.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports setting its length.
        /// </value>
        /// <see cref="Boolean"/>
        public static bool CanSetLength
        {
            get { return false; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports the flush
        ///		operation.
        /// </summary>
        /// <remarks>
        ///		This field always returns <b>false</b>. Since any data written to a
        ///		<b>BitStream</b> is written into RAM, flush operations become
        ///		redundant.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports the flush operation.
        /// </value>
        /// <seealso cref="Boolean"/>
        public static bool CanFlush
        {
            get { return false; }
        }

        #endregion


        #region ctors/dtors [20051123]
        /// <summary>
        ///		Initialises a new instance of the <see cref="BitStream"/> class
        ///		with an expandable capacity initialised to one.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="BitStream"/>
        public BitStream()
        {
            // Initialise the bit buffer with 1 UInt32
            _auiBitBuffer = new uint[1];
        }
        /// <summary>
        ///		Initialises a new instance of the <see cref="BitStream"/> class
        ///		with an expandable capacity initialised to the specified capacity in
        ///		<b>bits</b>.
        /// </summary>
        ///	<exception cref="System.ArgumentOutOfRangeException">
        ///		<i>capacity</i> is negative or zero.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <param name="capacity">
        ///		An <see cref="Int64"/> specifying the initial size of the internal
        ///		bit buffer in <b>bits</b>.
        /// </param>
        /// <seealso cref="BitStream"/>
        public BitStream(long capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(BitStreamResources.GetString("ArgumentOutOfRange_NegativeOrZeroCapacity"));

            _auiBitBuffer = new uint[(capacity >> BitBuffer_SizeOfElement_Shift) + ((capacity & BitBuffer_SizeOfElement_Mod) > 0 ? 1 : 0)];
        }
        /// <summary>
        ///		Initialises a new instance of the <see cref="BitStream"/> class
        ///		with the <b>bits</b> provided by the specified <see cref="Stream"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		Added [20051122].
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Stream"/> object containing the specified <b>bits</b>.
        /// </param>
        /// <seealso cref="BitStream"/>
        /// <seealso cref="Stream"/>
        public BitStream(Stream bits)
            : this()
        {
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            // Write the stream to the internal buffer using a temporary byte buffer
            byte[] abytBits = new byte[bits.Length];


            long lCurrentPos = bits.Position;
            bits.Position = 0;

            bits.Read(abytBits, 0, (int)bits.Length);

            bits.Position = lCurrentPos;


            Write(abytBits, 0, (int)bits.Length);
        }

        #endregion


        #region Methods [20051201]

        #region Write [20051201]

        #region Generic Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt32"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="UInt32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="UInt32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt32"/>
        private void Write(ref uint bits, ref uint bitIndex, ref uint count)
        {
            // Calculate the current position
            uint uiBitBuffer_Position = (_uiBitBuffer_Index << BitBuffer_SizeOfElement_Shift) + _uiBitBuffer_BitIndex;
            // Detemine the last element in the bit buffer
            uint uiBitBuffer_LastElementIndex = (_uiBitBuffer_Length >> BitBuffer_SizeOfElement_Shift);
            // Clalculate this values end index
            uint uiValue_EndIndex = bitIndex + count;

            // Clear out unwanted bits in value
            int iValue_BitsToShift = (int)bitIndex;
            uint uiValue_BitMask = (BitMaskHelperLUT[count] << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
            uint uiBitBuffer_FreeBits = BitBuffer_SizeOfElement - _uiBitBuffer_BitIndex;
            iValue_BitsToShift = (int)(uiBitBuffer_FreeBits - uiValue_EndIndex);
            uint uiValue_Indexed = 0;
            if (iValue_BitsToShift < 0)
                uiValue_Indexed = bits >> Math.Abs(iValue_BitsToShift);
            else
                uiValue_Indexed = bits << iValue_BitsToShift;

            // Clear current bits in bit buffer that are at same indices
            // (only if overwriting)
            if (_uiBitBuffer_Length >= (uiBitBuffer_Position + 1))
            {
                int iBitBuffer_BitsToShift = (int)(uiBitBuffer_FreeBits - count);
                uint uiBitBuffer_BitMask = 0;
                if (iBitBuffer_BitsToShift < 0)
                    uiBitBuffer_BitMask = uint.MaxValue ^ (BitMaskHelperLUT[count] >> Math.Abs(iBitBuffer_BitsToShift));
                else
                    uiBitBuffer_BitMask = uint.MaxValue ^ (BitMaskHelperLUT[count] << iBitBuffer_BitsToShift);
                _auiBitBuffer[_uiBitBuffer_Index] &= uiBitBuffer_BitMask;

                // Is this the last element of the bit buffer?
                if (uiBitBuffer_LastElementIndex == _uiBitBuffer_Index)
                {
                    uint uiBitBuffer_NewLength = 0;
                    if (uiBitBuffer_FreeBits >= count)
                        uiBitBuffer_NewLength = uiBitBuffer_Position + count;
                    else
                        uiBitBuffer_NewLength = uiBitBuffer_Position + uiBitBuffer_FreeBits;
                    if (uiBitBuffer_NewLength > _uiBitBuffer_Length)
                    {
                        uint uiBitBuffer_ExtraBits = uiBitBuffer_NewLength - _uiBitBuffer_Length;
                        UpdateLengthForWrite(uiBitBuffer_ExtraBits);
                    }
                }
            }
            else // Not overwrinting any bits: _uiBitBuffer_Length < (uiBitBuffer_Position + 1)
            {
                if (uiBitBuffer_FreeBits >= count)
                    UpdateLengthForWrite(count);
                else
                    UpdateLengthForWrite(uiBitBuffer_FreeBits);
            }

            // Write value
            _auiBitBuffer[_uiBitBuffer_Index] |= uiValue_Indexed;

            if (uiBitBuffer_FreeBits >= count)
                UpdateIndicesForWrite(count);
            else // Some bits in value did not fit
            // in current bit buffer element
            {
                UpdateIndicesForWrite(uiBitBuffer_FreeBits);

                uint uiValue_RemainingBits = count - uiBitBuffer_FreeBits;
                uint uiValue_StartIndex = bitIndex;
                Write(ref bits, ref uiValue_StartIndex, ref uiValue_RemainingBits);
            }
        }

        #endregion


        #region 1-Bit Writes [20051116]
        /// <summary>
        ///		Writes the <b>bit</b> represented by a <see cref="Boolean"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bit">
        ///		A <see cref="Boolean"/> value representing the <b>bit</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Boolean"/>
        public virtual void Write(bool bit)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

            // Convert the bool to UInt32
            uint uiBit = (uint)(bit ? 1 : 0);
            uint uiBitIndex = 0;
            uint uiCount = 1;

            Write(ref uiBit, ref uiBitIndex, ref uiCount);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Boolean"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Boolean"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Boolean"/>
        public virtual void Write(bool[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Boolean"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Boolean"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Boolean"/>
        ///		offset to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Boolean"/> values to write.
        /// </param>
        /// <seealso cref="Boolean"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(bool[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iBitCounter = offset; iBitCounter < iEndIndex; iBitCounter++)
                Write(bits[iBitCounter]);
        }

        #endregion


        #region 8-Bit Writes [20051124]
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Byte"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Byte"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Byte"/>
        public virtual void Write(byte bits)
        {
            Write(bits, 0, SizeOfByte);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Byte"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="Byte"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Byte"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(byte bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (SizeOfByte - bitIndex))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_Byte"));

            uint uiBits = (uint)bits;
            uint uiBitIndex = (uint)bitIndex;
            uint uiCount = (uint)count;

            Write(ref uiBits, ref uiBitIndex, ref uiCount);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Byte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Byte"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Byte"/>
        public virtual void Write(byte[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Byte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Byte"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Byte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Byte"/> values to write.
        /// </param>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int32"/>
        public override void Write(byte[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iByteCounter = offset; iByteCounter < iEndIndex; iByteCounter++)
                Write(bits[iByteCounter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="SByte"/>
        
        public virtual void Write(sbyte bits)
        {
            Write(bits, 0, SizeOfByte);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="SByte"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(sbyte bits, int bitIndex, int count)
        {
            // Convert the value to a byte
            byte bytBits = (byte)bits;

            Write(bytBits, bitIndex, count);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="SByte"/>
        
        public virtual void Write(sbyte[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to write.
        /// </param>
        /// <seealso cref="SByte"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(sbyte[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            byte[] abytBits = new byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);

            Write(abytBits, 0, count);
        }
        /// <summary>
        ///		Writes a byte to the current stream at the current position.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.<br></br>
        ///		<br></br>
        ///		Modified [20051124]
        /// </remarks>
        /// <param name="value">
        ///		The byte to write.
        /// </param>
        public override void WriteByte(byte value)
        {
            Write(value);
        }

        #endregion


        #region 16-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Char"/>
        
        public virtual void Write(char bits)
        {
            Write(bits, 0, SizeOfChar);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="Char"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Char"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(char bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (SizeOfChar - bitIndex))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_Char"));

            uint uiBits = (uint)bits;
            uint uiBitIndex = (uint)bitIndex;
            uint uiCount = (uint)count;

            Write(ref uiBits, ref uiBitIndex, ref uiCount);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Char"/>
        
        public virtual void Write(char[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Char"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Char"/> values to write.
        /// </param>
        /// <seealso cref="Char"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(char[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iCharCounter = offset; iCharCounter < iEndIndex; iCharCounter++)
                Write(bits[iCharCounter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="UInt16"/>
        
        public virtual void Write(ushort bits)
        {
            Write(bits, 0, SizeOfUInt16);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt16"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ushort bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (SizeOfUInt16 - bitIndex))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt16"));

            uint uiBits = (uint)bits;
            uint uiBitIndex = (uint)bitIndex;
            uint uiCount = (uint)count;

            Write(ref uiBits, ref uiBitIndex, ref uiCount);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="UInt16"/>
        
        public virtual void Write(ushort[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="UInt16"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="UInt16"/> values to write.
        /// </param>
        /// <seealso cref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ushort[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iUInt16Counter = offset; iUInt16Counter < iEndIndex; iUInt16Counter++)
                Write(bits[iUInt16Counter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int16"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Int16"/>
        public virtual void Write(short bits)
        {
            Write(bits, 0, SizeOfUInt16);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int16"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Int16"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(short bits, int bitIndex, int count)
        {
            // Convert the value to an UInt16
            ushort usBits = (ushort)bits;

            Write(usBits, bitIndex, count);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int16"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Int16"/>
        public virtual void Write(short[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int16"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Int16"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Int16"/> values to write.
        /// </param>
        /// <seealso cref="Int16"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(short[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            ushort[] ausBits = new ushort[count];
            Buffer.BlockCopy(bits, offset << 1, ausBits, 0, count << 1);

            Write(ausBits, 0, count);
        }

        #endregion


        #region 32-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="UInt32"/>
        
        public virtual void Write(uint bits)
        {
            Write(bits, 0, SizeOfUInt32);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt32"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt32"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(uint bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (SizeOfUInt32 - bitIndex))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt32"));

            uint uiBitIndex = (uint)bitIndex;
            uint uiCount = (uint)count;

            Write(ref bits, ref uiBitIndex, ref uiCount);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt32"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="UInt32"/>
        
        public virtual void Write(uint[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt32"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="UInt32"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="UInt32"/> values to write.
        /// </param>
        /// <seealso cref="UInt32"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(uint[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iUInt32Counter = offset; iUInt32Counter < iEndIndex; iUInt32Counter++)
                Write(bits[iUInt32Counter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int32"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Int32"/>
        public virtual void Write(int bits)
        {
            Write(bits, 0, SizeOfUInt32);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int32"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Int32"/>
        public virtual void Write(int bits, int bitIndex, int count)
        {
            // Convert the value to an UInt32
            uint uiBits = (uint)bits;

            Write(uiBits, bitIndex, count);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int32"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int32"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Int32"/>
        public virtual void Write(int[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int32"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int32"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Int32"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Int32"/> values to write.
        /// </param>
        /// <seealso cref="Int32"/>
        public virtual void Write(int[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            uint[] auiBits = new uint[count];
            Buffer.BlockCopy(bits, offset << 2, auiBits, 0, count << 2);

            Write(auiBits, 0, count);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Single"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Single"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Single"/>
        public virtual void Write(float bits)
        {
            Write(bits, 0, SizeOfSingle);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Single"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Single"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Single"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(float bits, int bitIndex, int count)
        {
            byte[] abytBits = BitConverter.GetBytes(bits);
            uint uiBits = (uint)abytBits[0] | ((uint)abytBits[1]) << 8 | ((uint)abytBits[2]) << 16 | ((uint)abytBits[3]) << 24;
            Write(uiBits, bitIndex, count);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Single"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Single"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Single"/>
        public virtual void Write(float[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Single"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Single"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Single"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Single"/> values to write.
        /// </param>
        /// <seealso cref="Single"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(float[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iSingleCounter = offset; iSingleCounter < iEndIndex; iSingleCounter++)
                Write(bits[iSingleCounter]);
        }

        #endregion


        #region 64-Bit Writes [20051201]
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt64"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt64"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="UInt64"/>
        
        public virtual void Write(ulong bits)
        {
            Write(bits, 0, SizeOfUInt64);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt64"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt64"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.<br></br>
        ///		<br></br>
        ///		Fixed [20051201].
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt64"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt64"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ulong bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (SizeOfUInt64 - bitIndex))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrBitIndex_UInt64"));

            int iBitIndex1 = (bitIndex >> 5) < 1 ? bitIndex : -1;
            int iBitIndex2 = (bitIndex + count) > 32 ? (iBitIndex1 < 0 ? bitIndex - 32 : 0) : -1;
            int iCount1 = iBitIndex1 > -1 ? (iBitIndex1 + count > 32 ? 32 - iBitIndex1 : count) : 0;
            int iCount2 = iBitIndex2 > -1 ? (iCount1 == 0 ? count : count - iCount1) : 0;

            if (iCount1 > 0)
            {
                uint uiBits1 = (uint)bits;
                uint uiBitIndex1 = (uint)iBitIndex1;
                uint uiCount1 = (uint)iCount1;
                Write(ref uiBits1, ref uiBitIndex1, ref uiCount1);
            }
            if (iCount2 > 0)
            {
                uint uiBits2 = (uint)(bits >> 32);
                uint uiBitIndex2 = (uint)iBitIndex2;
                uint uiCount2 = (uint)iCount2;
                Write(ref uiBits2, ref uiBitIndex2, ref uiCount2);
            }
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt64"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt64"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="UInt64"/>
        
        public virtual void Write(ulong[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt64"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt64"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="UInt64"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="UInt64"/> values to write.
        /// </param>
        /// <seealso cref="UInt64"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ulong[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iUInt64Counter = offset; iUInt64Counter < iEndIndex; iUInt64Counter++)
                Write(bits[iUInt64Counter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int64"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int64"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Int64"/>
        public virtual void Write(long bits)
        {
            Write(bits, 0, SizeOfUInt64);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int64"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Int64"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(long bits, int bitIndex, int count)
        {
            // Convert the value to an UInt64
            ulong ulBits = (ulong)bits;

            Write(ulBits, bitIndex, count);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int64"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int64"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Int64"/>
        public virtual void Write(long[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="Int64"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int64"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Int64"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Int64"/> values to write.
        /// </param>
        /// <seealso cref="Int64"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(long[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
 ***************throw new ArgumentException(BitStreamResources.Ge****ing("********_InvalidCountOrO*****"));
*************ulong[] aulBits =*****
//
//csis
];*************Buffer.BlockCopy�//**,******* << 4,Redistri, 0, e in modifrights reserved.Write(ation, are permit)ource and }*********/// <summary>/ Redistribu		 folls the <b>//**</b> contained in a <see cref="Double"/> value toource code musn thcurrent s*****./ Redistributi/ons of source code m <remarkssource code musAll wfoll opera****s atin thend ofin the a********* copyexpandin tource code musons and the foll/ Redistributionreproduce the above  <param name="//**"ce the above copthis
// list of conditions aspecifyingin the above copytoight nodataource code musfro// Redistributionhe diprovided with tseealso// list of condi*********public virtual voide follodof co //************{ided that the follo witho0, SizeOf of core met:
//
// Redistributions of source code must retain the above copyright notice, nthis
// list of conditions and the following disclaimer. 
//
// Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and/or other materials provided with the distribution. 
//
// Neither the name of the ORGANIZATION nor the names of its contributors may
// be used to endorse or promote products derived from the distributionIndex 
//
// Neither tMITED TO, THEInt32ANIZATION nor the names olittle-endiIMIT abov copource code musiY THibutbeginight  namS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND Oe in EORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY,maximum number ofaimer in the docuts contributors me or promote products derived from this software without
// specificrom this software w// CONTR specific prior written permission. 
//
// THI, int/ THNY THion;
uons ar SOFTWARE IS PROVIDED Bbyte/ RebytstributBitConverterpyriBytes COPYre met:
//
ved.
//
/ distribut(
//
/)n;
using[0] | (amespace BKSyste1])modi8IO
{
    /// <summar2>
    16IO
{
    /// <summar3>
    24 |*****************{
    /// <summar4>
    32IO
{
    /// <summar5>
    40IO
{
    /// <summar6lor="re///		Creates a stream7>
    56ovided that the follotion, arsing Systeons are met:
//
// Redistributions of source code must retain the above copyright notice, this
// list of conditby forand the following disclaimer. 
//
// Redistributions in binary form must e********// listSystem.ObjectDisposed********* 
//
// Neither Tdisclaimer. 
//
/ is clrtua/ Redistributionnt)</c>
 tIndex, int count)</c>
    ///		and <c********Nulll int Read(out ulong bits<i*******i>t coa null reference (<b>Nothing copyin Visten Basic)c> methods.<br></br>
    ///		<br></br>
reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and/or other materials provided with the distribution. 
//
// Neither the name of the ORGANIZarray nor the names oxed probltors may
//NY WAY OUT OF THE USE OF THIS SOFTWARE, Ehis software without
// specific prior written permission. 
//
/[]/ THIS SOFTWARE IS PROVIDED Bif (!_blnIsOpen****************************>public virtual int Rea*************
// Copyright © 2>public virtua_*********Cnt)</l rig operator Memory//** ==to wr************************************ublic virtual(tion. ,yste*********
// Copyright © 2005, Bilubli;</cry forl rights reserved.Y THE COPYRIGHT//*********jects using the
    ///		<see cref="BitStream"/> class.<br></br>
    ///		<br></br>
    ///		[20051201]: Fixed problem with <c>public virtual void Write(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public virtual int Read(out ulong bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>public virtual void WriteTo(Stream bits);</c> to write
    ///		the contents of the current <b>bit</b> stream to another stream.<br></    ///		[20051127]: Added <c>pOutOfRangec virtual void WriteTo(Strea******ts);<or <i>e in ts);</c>negativ*****

using Syst/br>
    ///		<br></br>
    ///		[20051127]: Added <c>pm"/> class.<br></br>
    ///		<br></br>subtractedNY WAbr></br>
   l******is lesain IMIT ///		[2005>bit</b> stream to another stream.<br></br>
    ///		<br></br>
    ///		[20051125]: Added the following implicit operators to allow type casting
    ///		instances of the <see cref="BitStream"/> class to and from other types
    ///		of <see cref="Stream"/> objects:<br></br>
    ///		<br></br>
    ///		<c>public static implicit opehe distribut******EORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY,his
// list of condit****** the following // ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//******************************************************is
// list of conditions s*************

using System;
using Systes);</c><br></br>
    ///		<c>public staion;
u******tem.ComponentModel;
using System.RemoryStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator BufferedStream(B<see cref="Resouut
// mo 0************************************="BitStream"/> clas(ss.
       ///		<c>public static implicit operat="BitStrea_N: AddedPe diet           ///		<see crermittetream"/> resources while the current
            ///		<see ED OF esourceManager"/> is busy.
            /// </summary>
            /// <remarks>
            ///﻿//*****************************************************************************
// Copyright © 2005, Bill Koukoutsis
//
// All rights reserved.n;
uiEndNY TH =out
// m+in pro      ///		<sefor (  /// of cotsis
erks>
     ;ean"/>
         <// </remarte static bool _++******************Y THE COPY[an"/>
        ]re met:
//
//ghts reser#endregion           /<summa Miscellaneous/ Redistributions of source code must retain thentireyrighents condiisLUDING NEGLdex, intto another void Write(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public virtual int Read(out ulong bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>public virtual void WriteTo(Stream bits);</c> to write
    ///		the contents of the current <b>bit</b> stream to another stream.<br></br>
    ///		<br></br>
dded [20051127]<br></brvoid WriteTo(Str      _resman = new Repyright notice,
// this list of conditions and the following disclaimer in the documentation
// and/or other 	aterials provided with the distribution. 
//
// Neither the name of t******otes publi nor the names o      ///	ght no<rem********************           ///e or promote products derived from this software wecified s          #region Fields [20051116To(******c static implicit operator MemoryStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator Buecified rights reserved.flection;
using SyTo;
usAam"/(tatic string GetS/*** follow;
usingRIGHTstr;
    ///		<c>public stat         /// <summary>
        // <summary>
            ///Rea        201]y>
            ///Generic     s        15]/ Redistributions of source code mus   {
 n the above copyright notice, disclaimer. 
//
////		</remarks>
        privatU// CONTRACT, Srite(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public virtual int Read(out ulong bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>p="BitStream"/> class.<br></br>
    ///sing Sys</br>
    ///		[20051123]: Added <c>public BitStream(<see cref="Stream"/> bits);</c> contructor.<br></br>
    ///		<br></br>
    //
        }

 >
    /// <seealso c*********he above copyce, able to load the resource: " + name"/>
    /// <seealso cref="Stream"/>
    /// <seealso cref="int"/>
    /// <seealso cre, in<b>     copymethod returns zeroResolist of conditiclaimer. 
//
/IGENCE OR OTHERs reached. In all 
     cases,/ </remarks>
alway"/>
 dthis least onlaimer in the docuremarks>seealso curce manager wabefore  /// <ingand/or other materials provided with the distribution. 
//
// Neither Whee reis
        /// <s in ght n      nor thiedfont color=": " + namource code musions awith                 betweenurce file g diatic"/>
        / - 1ntModel;
ue musreplaced by                     f="Int32"/> value definiR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
: " + name);

  nor the names of its  ///		i>
   t whichand the following ARISI    of bits
        /// OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAin a <see cref="UInt16"/> value*****************he above copllers:</font></b>         ///		This field is constant.
   /// <ss>
        /// <seealso cref="Int32"/>
        /// <seealso nt16"/>
        private const int Sght ten   /oref=");

   T
   can be>
    /// <its
        ///		in a <see cref="UInt32reques// < crefater of bits
        ///	are not/> valuely available,har"/>
       oreealso cref="Int32"/>
        /// <see    >
     ng the nany**************************    OfUInt16 = 16;
          ///		An <see crperator BitStreatant.
        /// private un;
u    (ref//		in    //ee cref="Sing Systeee cref="mponentModel;
using System.Re// Calcul  //32"/> value posi****hts reserved.
n;
uuifferedStr_Pd is co

na_/ </remarks>"/>
  <<ysteemarks> HOLDEEle BillShift) +  <seealso creBtype.
 ights reserved.// D  //mineo cref=eOfUrary>ough"Singcref="Int3)
// Aar = 128;
     xed prnstant.
        /// Acttentsis
 =     /// <seealso crmory <seealso cre*******< (/ </remarks>
        +e number of bi******************* number of bits
cref="UInt64"/> valu-// </remarks>
       e const int SizeOfGe/ <seeals"Sin/br>
   eeealso      nstant.
        /// Vons a= _a/ </remarks[ <seealso cref="Insource and bina  ///    p   psTocref=

naint)****        /// <seealso -e cref="UInt64"  privatummary>
        ///		.
           f (ry>
        ///		An		.
            / IS PROVIDED Bemarks>
lear out unwan// <ary>
inealso cref="UInt64"/ <suy>
        ///		An <Math.Abs> value type.
            ///		<set64"/>
        pof bMask

nae"/>
  HelperLUT[ number of bi] >>/ </remarks>
              /// <seealso     pr&=ef="Double"/>
  mmary>
        ///		An <se<<=/ </remarks>
      ights reserved.       /// Remai of of bits
(   /)its
        ///		per <remarks>
        /// </rremarks>0// <remarks>
        ///	    pToApp of ld is/ <remarks>
     UpdateIndicesFor a <s number of bit     buffer.
     <see cref="DoubleOfU a <see cre</remarks>
         /	This fi       /BitBuffer_SizeOfElement = SizeOfUI		An <se| cref="UIarks>
  // <remarks>
 // Redistrthe els cref="UInt64"        /// <remarks>
        ///		This field is constant.
        ///o cref="Double"/>
        private const int SizeOfDouble<< 64;
        /// <summary>
        ///		An <see cref="UInt32"/> value defining the number >> bits
        ///		per element in the i="UInt32"/>
        private const        ///		<sel)
                  cref="UIt.
 see c type.
   /// <seealso  /// <    ///		This man == null)
                    Initialise    ///1-Biin a <
         6          if (_blnLoadingResource)
                       return ("The resource manager was unable to load the resourceBooleanname);

                    _blnLoadingResourced is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.        /// </summary>
        /// <remarks>
        ///		This field is cons/>
            /// </remarks>
        /// <sealso cref="Int32"/>
        /// <seealso cref="Char"/>
        private const int SizOfChar = 128;
        /// <summary>
        ///		An <see cref="Int32"     ///		An <see cref="Int32"/> vale defining the number of bits
        ///		in a <see cref="UInt32"/> value type.
        /// </summar1>
  ealso cref="Int32"/>
 har"/>
       
        private const int SizeOfalue type.
                 ///		An <see cref="Int32"/> value defining the000001, 0x00000003, 0xllections.Specialized;
using System.Resources;
u		in a <s   /bool fie   ///		The <see cref="ResourceManager"/> object.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
       nstant.
        /// </ris field is constant.
    ///	of bits
1n"/>
        private     lean"/>
        private          =Int32;
     Bitry>
        ///		An <see_SizeOfElement = Size/>
 = m.Net.S.To/>
    type.
>
        /// <swise <bsee c/		An arrae met:
//
// Redistributions of source code mus                      return ("The resource manager was unable to load the resource/>
        xed prrite(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public virtual int Read(out ulong bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>public virtual void WriteTo(Stream bits);</c> to write
    ///		the contents of the current <b>bit</b> stream to another stream.<br></br>
    ///		<br></br> /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is cons000001, 0x00000003, 0x00eam"/>        /lass Bicref="Inut
// m     
            /ef="C  private>
        /// <onst int SizeOfChar = 128;
        /// <summary>
        ///		An <see cref="Int32"		0x0001FFFF, 0x0003FFFF, 0x0007FFFF, 0x000FFFFF,
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,
			0x01FFFFFF, 0x /// <s /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="UInt32"/>
        private const int SizeOfUInt32 = 32;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the
        ///		A <see cref="Boolean"/> value specifying whether the current
    ///	lic static implicit operator MemoryStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator BufferedStream(BitStream bitswise <bfo.Invbr></br>
    ///		<c>public static implicit operator BitStream(Netw </remarks>
        /// <seealso cref="UInt32"/>
        private uint[] _auiBitBuffer;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current length of the
        ///		internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Length;
        /// <summary>
  ///		supported by the <see cref="BitStream"/> class.<br></br>
    ///		<br></br>
    ///		[20051123]: Added <c>public BitStream(<see cref="Stream"/> bits);</c> contructor.<br></br>
    ///		<br></br>
    /// </remarks>
    /// <seealso cref="BitStream"/>
    /// <seealso cref="Stream"/>
    /// <seealso cref="int"/>
    /// <seealso cref="UInt32"/> value specifying the current elemental index
        ///		of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current bit index for the
        ///		current element of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
           ///		class.
        /// </summary>
        /// <remarks>
        ///		<b><font color="/>
         to Callers:</font></ </summary/ ARISIks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref=//		<see cref="BitStream"/> class.
        /// </remarks>
        privat/>
        privaBitStOfUInt16 = 16;
        /// <summary>
        ///		An <see cref="Int32"/> valcref="UInt32"/>
        private uint _uiBitBuffer_BitIndex;
        /// <summary>
        ///		An <see cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
        /// <see cref="IFormatProvider"/>
        /// <see cref="CultureInfo.InvariantCult/// <summary>
            ///		The <see cref="ResourceManager"/> object.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static ResourceManager _resman;
            /// <summary>
            ///		An <see cref="Object"/> used to lock access to
            ///		<see cref="BitStream"/> resources while the current
            ///		<see cref="ResourceManager"/> is busy.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static object _oResManLock;
            /// <summary>
            ///		A <see cref="Boolean"/> value specifying whether a resource is
            ///		currently being loaded.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            /// <seealso cr  ///	An array o is constant.
 ef="Booleastem.         private or="blue">nlnLoadingResouor="blue">     #endregion


       ///		+ of <se    /   #ror="blue"> [20m.ObjectDisposedExceummary>
        /// <         /// <summary>
            ///8        /// </summ24          if (_blnLoadingResource)
                        return ("The resource manager was unable to load the resource;
us    private const uint BitBuffer_SizeOfElement_Mod = 31;
        /// <summary>
        ///		An <see cref="UInt32"/> array defining a series of values
        ///		useful in generating bit masks in read and write operations.
        /// </summary>
        /// <remarks>
        ///		This field is static.
        /// </remarks>
        priBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> valuer>
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Char"/>
        private const int SizeOfChar = 128;
        /// <summary>
        ///		An <see cref="Int32"		0x0001FFFF, 0x0003FFFF, 0x0007FFFF, 0x000FFFFF,
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,
			0x01FFFFFF, 0x03FFFFFF, 0x07FFFFF cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
      {
           ref="Boolean"/> value specifying whether the current
        /yt// THIS SOFTWARE IS PROVIDED BsedException    ///	RIGHT HOLDE<remcurrent stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
 r>
        ///		:<br></br>
        ///		</font>
   = true;
                    str = _resman.GetString(name, null);
                    _blnLoadingResource = false;
                }
                return str;
            }

            #endregion

        }

        #endregion

        #endregion


        #region Constants [20051116]
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Byte"/> valuo read t  /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///	     {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

      CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY,n <see cref    /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref=Int32"/>
        /// <seealso cref="UInt16"/>
        private const int SizeOfUInt16 = 16;
        /// <summary>
        ///		An <see cref="Int32"/> vale defining the number of bits
        ///		in a <see cref="UInt32"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="UInt32"/>
        private const int SizeOfUInt32 = 32;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the<remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font ion;
using System.ComponentModel;
using System.RemoryStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public staticf="Int3tream"/> resources while the current
            ///		<see N ANY THEesourceManager"/> is busy.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static object _oResManLock;
            /// <summary>
            ///		A <see cref="Boolean"/> value specifying wen">
      -urce file***********************************************************
// Copyright © 2005, Bill Koukoutsis
//cref="Bo_      rights reserved.
  ///		This field/// </  ///		a d/>
        private bool _bl/// </    /// <seealso cr     publiequi<summary>
        ///		An array of <see cref="Ugle"/> va values specifying the internal
        ;

na/		</summarrent stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint[] _auiBitBuffer>
     /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current length of the
        ///		internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Length;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current elemental index
        ///		of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> value     {
            g bit index for the
        ///		current element of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_BitIndex;
        /// <summary>
        ///		An <see cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
 remarks>
        ///		.
        /// </remarks>
        /// <example>
      flectioCulture"/>
        private static IFormatProvider _ifp = (IFormatProvider)CultureInfo.InvariantCulture;

        #endregion


        #region Properties [20051116]
        /// <summary>
        ///		Gets the length of this stream in <b>bits</b>.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		Gets the number of <b>bits</b> allocatated to this stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		An <see cref="Int64"/> valuetString("ObjectDisposed_BitStreamClosed"));

                return (long)_uiBitBuffer_Length;
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///	er.Length) << BitBuffer_SizeOfElement_Shift;
            }
        }
        /// <summary>
        ///		Gets or sets the current <b>bit</b> position within this stream.
        /// </summ<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">r>
    > [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(abytBuffer, 0, (<font color="blue">int</font>)bstrm.Length8);<r>
        /  ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>8-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(byte [], int, int)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
            get
            {
                if (remarks>
        ///		.
        /// </remarks>
        ///overrideurrent <b>position</bString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font coloyt            private /remarks>
  <br></br>
     remarks>
 <br></br>
        ///		:<br></br>
        ///		<fremarks>
 ="blue">short</font> [] asBuffer = <font colom is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
So read the entire stream into an <see cref="Int32"/edException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        /// false; 
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 4) + (long)((_uiBitBuffer_Length & 15) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>32-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        ///      ///		A <see crellections.Specialized;
using Systecifying whether the current
        s/		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(int [], int, int)"/> method<br></br>
        ///		// t false; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports the flush
        ///		operation.
        /// </summary>
        /// <remarks>
        ///		This field always returns <b>false</b>. Since any data written to a
        ///		<b>BitStream</b> is written into RAM, flush operations become
        ///		redundant.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports the flush operation.
        /// </value>
        /// <seealso cref="Boolean"/>
        public sta  return (long)(_uiBitBuffer_Length >> 5) + (long)((_uiBitBuffer_Length & 31) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>64-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(long [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int64"/> array.<br        public BitStream()
        {
            // Initialise the bit buffer with 1 UInt32
        ion;
using System.ComponentModel;
using System.Reflec//		(!_blnIsOpen)
        
        ///		Bi       ///		 resource file in projects using("ObjectDisp2
   )is()
  4
        {
  ting its length.
        /// </value>
        /// <see cref="Boolean"/>
        public static bool CanSetLength
        {onstant.
 capacity initialised to t /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current length of the
        ///		internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Length;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current elemental index
        ///		of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> valu     ///		A <see cref= bit index for the
        ///		current element of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_BitIndex;
        /// <summary>
        ///		An <see cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
        public BitStream()
        {
            // Initialise the bit buffer with 1 U2
   tion</b>
        ///		within this stream.
        /// </value>
        /// <seealso cref="Length"/>
        /// <seealso cref="Int64"/>
        public override long Position
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                uint uiPosition = (_uiBitBuffer_Index << BitBuffer_SizeOfElement_Shift) + he internal buffer using a temporary byte buffer
            byte[] abytBits = new byte[bits.Length];


            long lCurrentPos = bits.Position;
            bits.Position = 0;

            bits.Read(abytBits, 0, (int)bits.Length);

            bits.Position = lCurrentPos;


            Write(abytBits, 0, (int)bits.Length);
        }

        #endregion


        #retString("ObjectDisposed_BitStreamClosed"));

                return (long)_uiBitBuffer_Length;
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///      /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exce   ///		class.
        /// </summary>
        /// <remarks>
        ///		<b><font color=" false; }> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(abytBuffer, 0, (<font color="blue">int</font>)bstrm.Length8); false; }
      ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>8-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(byte [], int, int)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
            get
            {
                if Int32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
  String("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font col fals"blue">new</font> B>= count)
   lnLoadingResou>= count)
  <br></br>
        ///		:<br></br>
        ///		<>= count)
  ///		supports setting its length.
        /// </value>
        /// <see cref="Boolean"a//		<fseealso c privatg diadvanctain thld is coindexhe resource code musdefiningyry>
//		<,>
   /// <se-1    is list of conditi 
//
// Redistributions in binary form must reproduce the above coModhis fir>
        ///		<font coterials provided with t     ///		An <see cref, inunsignoti/		<fcmmar//		. 0 ?// CO cops;
        uint uiValue_Starts = count - uiBitBu      ///		An <see cref="Int32"/>o cref="Boolean"/>
     am
 (<seealso cref="Stream"/>
        pubBitStream(Stream bits)
            : this()
  <see cref="Double"/> 	An array =tream"/> resources wwise <b-nIsOpen = true;umber of bits per.
        /// </.
        /// </s// </value>
        /// <see cref="ions in binary form must reproduce the above cop             4]and/or other materials provided with tee cref="Int32"/> valuresented by a <see cref="written flectio)
           ary>
        /// <remarks> folly>
    
//
/'ld iternolear>
    //a*****/		<fo2"/> value defininyptoglClaimerPoequi>
  .rks>
     **************on(BitStrea      /// <seealso flection;
using Sy)
      [Strin******8source and binaStrin a <sstr;
        see cConvert the <see cref="Double"/>String("Object!=ctDisposedEx********************ring("ObjectDitDisposedExblue">short</font> [] str;
   ations emulating
        ///		modulo calculations.
6    s    {
                    if (_blnLoadingResource)
                        return ("The resource manager was unable to load the resourceChare; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports the flush
        ///		operation.
        /// </summary>
        /// <remarks>
        ///		This field always returns <b>false</b>. Since any data written to a
        ///		<b>BitStream</b> is written into RAM, flush operations become
        ///		redundant.
        /// </remarks>
        /// <value>
   eption 
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 4) + (long)((_uiBitBuffer_Length & 15) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>32-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// /// </remarks>
    ref="Boolean"/> value specifying whether the current
        charfont face="Courier New">
        ///		<font color="green">
 epticurrent stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
eption cref="System.ObjectDisposedException">
      = true;
                    str = _resman.GetString(name, null);
                    _blnLoadingResource = false;
                }
                return str;
            }

            #endregion

        }

        #endregion

        #endregion


        #region Constants [20051116]
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Byte"/> valueption c  /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ////// </remarks>
        /// <param name="bits">
        ///		A <see cref="Boolean"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Boolean"/>
        return (long)(_uiBitBuffer_Length >> 5) + (long)((_uiBitBuffer_Length & 31) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>64-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(long [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int64"/> array.<br    ///		Writes the <b>bits</b> contained in a <see cref="Boolean"/> buffer to
        //color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">long</font> [] alBuffer = <font color="blue">new long</font> [bstrm.Length64];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(alBuffer, 0, (<font color="blue">int</font>)bstrm.Length64);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>64-bit</b> valueseptiuired to store this stream.
        /// </value>
        /// <seealso cref="Read(long [], int, int)"/>
        /// <seeaeptioref="Int64"/>
        public virtual long Length64
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisp    _BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 6) + (long)((_uiBitBuffer_Length & 63) > 0 ? 1 : 0);
            }
       he internal buffer using aeption c /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current length of the
        ///		internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Length;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current elemental index
        ///		of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> valu/// </remarks>
       bit index for the
        ///		current element of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_BitIndex;
        /// <summary>
        ///		An <see cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
    ///		Writes the <b>bits</b> contained in a <see cref="Boolean"/> buffer     ram>
        /// <param name="bitIndex">
        ///		An <see cref="UInt32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="UInt32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt32"/>
        private void Write(ref uint bits, ref uint bitIndex, ref uint count)
        {
       eam.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgtString("ObjectDisposed_BitStreamClosed"));

                return (long)_uiBitBuffer_Length;
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///     ///		A <see cref="Byte"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/   ///		class.
        /// </summary>
        /// <remarks>
        ///		<b><font color="eption c> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(abytBuffer, 0, (<font color="blue">int</font>)bstrm.Length8);eption cref="  ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>8-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(byte [], int, int)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
            get
            {
                if        {
            if (!_blnIsOpen)
                throw new ObjectDisposedExceptionString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font colepti"blue">new</font> Bt offset, inlnLoadingResout offset, i<br></br>
        ///		:<br></br>
        ///		<t offset, i///		supports setting its length.
        /// </value>
        /// <see cref="Boolean"/>
        public static bool CanSetLength
        {
            get { return: " 16e; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports the flush
        ///		operation.
        /// </summary>
        /// <remarks>
        ///		This field always returns <b>false</b>. Since any data written to a
        ///		<b>BitStream</b> is written into RAM, flush operations become
        ///		redundant.
        /// </remarks>
        /// <value>
   tString("
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 4) + (long)((_uiBitBuffer_Length & 15) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>32-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        ///    throw new Argumentream()
        {
            // Initialise the bit buffer with 1 UInt3ushor  ///	face="Courier New">
        ///		<font color="green">
 tStrincurrent stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
tString("ArgumentNull_BitBuffer"));
            if (o = true;
                    str = _resman.GetString(name, null);
                    _blnLoadingResource = false;
                }
                return str;
            }

            #endregion

        }

        #endregion

        #endregion


        #region Constants [20051116]
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Byte"/> value tying("A      /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constanthrow new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iByteCounter = offset; iByteCounter <   return (long)(_uiBitBuffer_Length >> 5) + (long)((_uiBitBuffer_Length & 31) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>64-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(long [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int64"/> array.<brme="bits">
        ///		An <see cref="SByte"/> value specifying the <b>bits</b> to write data
        color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">long</font> [] alBuffer = <font color="blue">new long</font> [bstrm.Length64];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(alBuffer, 0, (<font color="blue">int</font>)bstrm.Length64);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>64-bit</b> valueste(sbyuired to store this stream.
        /// </value>
        /// <seealso cref="Read(long [], int, int)"/>
        /// <seeate(sbytref="Int64"/>
        public virtual long Length64
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDispta
           Write(bits, 0, SizeOfByte);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Byte"/> value to
        ///		the current straram name= /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current length of the
        ///		internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Length;
        /// <summary>
        ///		An <see cref="UInt32"/> value specifying the current elemental index
        ///		of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_Index;
        /// <summary>
        ///		An <see cref="UInt32"/> valute(sbyte bits, int bitI bit index for the
        ///		current element of the internal bit buffer for the current stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint _uiBitBuffer_BitIndex;
        /// <summary>
        ///		An <see cref="IFormatProvider"/> object specifying the format specifier
        ///		for the current stream.
        /// </summary>
        /// <remarks>
        ///		This field is set to <b><see cref="CultureInfo.InvariantCulture"/></b>
        ///		by default.
        /// </remarks>
me="bits">
        ///		An <see cref="SByte"/> value specifying the <b>bits</b> to writta
   ram>
        /// <param name="bitIndex">
        ///		An <see cref="UInt32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="UInt32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt32"/>
        private void Write(ref uint bits, ref uint bitIndex, ref uint count)
        {
             /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset"tString("ObjectDisposed_BitStreamClosed"));

                return (long)_uiBitBuffer_Length;
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        //// <seealso cref="Int32"/>
        
        public virtual void Write(sbyte[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedExcep   ///		class.
        /// </summary>
        /// <remarks>
        ///		<b><font color="aram name=> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(abytBuffer, 0, (<font color="blue">int</font>)bstrm.Length8);tString("Argume  ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <see cref="Int64"/> value specifying the maximum number of
        ///		<b>8-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(byte [], int, int)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
            get
            {
                if   throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NeString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font colte(sby"blue">new</font> B"));
         lnLoadingResou"));
        <br></br>
        ///		:<br></br>
        ///		<"));
        ///		supports setting its length.
        /// </value>
        /// <see cref="Boolean"/>
        public static bool CanSetLength
        {
            get { returnString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
             e(sbyte bits, int bitIndex, int count)
        {
            // Convert the value to a byte
            byte bytBits = (byte)bits;

            Write(bytBits, bitIndex, count);
        }
        ///  ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(long [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int64"/> array.<bre="bits">
        ///		An <see cref="SByte"/> value s the bit buffer with 1 UInt32
        ///		from.
        /// </param>
        /// <seealso cref="SByte"/>
        
        public virtual void Write(sbyte bits)
        {
            Write(bits, 0, SizeOfByte);
        }
        /nt;

            Write(ref uiBits, ref uiBitIndex, ref uiCount);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b>   return (long)(_uiBitBuffer_Length >> 5) + (long)((_uiBitBuffer_Length & 31) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>64-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(long [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int64"/> array.<brtDisposed_BitStreamClosed"));
            if (bits == null)
                throw new Argumion;
using System.ComponentModel;
using System.Reta
    us public BitStream(Stream bits)
            : th      ource file in projec if (bits == null)
   gative     /		supports setting its length.
        /// </value>
        /// <see cref="Boolean"/>
        public static bool CanSetLength
        {he internal buffer using a     /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to write.
        /// </param>
        /// <seealso cref="SByte"/>
        // <seealso cref="Int32"/>
        
        public virtual void Write(sbyte[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
              tDisposed_BitStreamClosed"));
            if (bits == null)
                tutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            byte[] abytBits = new byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);

            Write(abytBits, 0, count);
        }
        /// <summary>
        ///		Writes a byte to the current stream at the current position.
        /// </summary>
        /// <remark>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.<br></br>
        ///		<br></br>
        ///		Modified [20051124]
        /// </remarks>
        /// <param name="value">
        ///		The byte to write.
        /// </param>
        public override void WriteByte(byte value)
        {
            Write(value);
        }

        #endregion


        #region 16-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Char"/>
        
        public virtual void Write(char bits)
        {
            Write(its, 0, SizeOfChar);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stram is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exceptio cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="Char"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///	ummary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> value toiBitBuffer_Position + count;
                    else
                        uiBitBuffer_NewLength = uiBitBuffer_Position + uiBitBuffer_FreeBits;
                    if (uiBitBuffer_NewLength > _uiBitBuffer_Length)
                    {
                        uint uiBitBuffer_ExtraBits = uiBitBuffer_NewLength - _uiBitBuffer_Length;
                        UpdateLengthForWrite(uiBitBuffer_ExtraBits);
                    }
                }
            }
            else // Not overwrinting any bits: _uiBitBuffer_Length < (uiBitBuffer_Position + 1)
            {
                if (uiBitBuffer_FreeBits >= count)
                    UpdateLengthForWrite(count);
                else
                    UpdateLengthForWrite(uiBitBuffer_FreeBits);
            }

            // Write value
            _auiBitBuffer[_uiBitBuffer_Index] |= uiValue_Indexed;

            if (uiBitBuffer_FreeBits >
   unt)
               > contained esForWrite(coun> contained     else // Some bits in value did not fit
     > contained="blue">short</font> [] asBuffer = <font color="blue">new short</font> [bstrm.Lengt32        /// </summa           if (_blnLoadingResource)
                        return ("The resource manager was unable to load the resource: " + name);

                    _blnLoadingResourced is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Char"/>
        private const int SizeOfChar = 128;
        /// <summary>
        ///		An <see cref="Int32"on">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <seeals.
        /// </r/		An <see cref="SByte"/> value specifying the <b>bits</b> to write datn;
usin ///		from.
        /// </param>
        /// <seealso cref="32yte"/>
        
        public virtual void Write(sbyte bits)
        {
            Write(bits, 0, SizeOfByte);
        }
        /// <+ name);

                    _blnLoadingResource = true;
                    str = _resman.GetString(name, null);
                    _blnLoadingResource = false;
                }
                return str;
            }

            #endregion

        }

        #endregion

        #endregion


        #region Constants [20051116]
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Byte"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Char"/>
        private const int SizeOfChar = 128;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		tained in an <see cref="SByte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealsref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(us (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException32
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullExcepti32">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> ise.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
      CONTRA</exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to write.
        /// </param>
        /// <seealso cref="SByte"/>
        /// <s.
        /// </rem/>
        
        public virtual void Write(sbyte[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                thref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual  Wriange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            byte[] abytBits = new byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);

            Write(abytBits, 0, count);
        }
        /// <summary>
        ///		Writes a byte to the current stream at the current position.
        /// </summary>
        /// <remarks>
 e operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Int16"/>
        public virtual void Write(short[] bits)
        {

            Write(value);
        }

        #endregion


        #region 16-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Char"/>
        
        public virtual void Write(char bits)
        {
            Write(bits		Writes the <b>bits</b> contained in an <see cref="Int16"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException       /// <exception cref="System.ObjectDisposedException">
        ///		The current stream CONTRAsed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception crCONTRACT, SArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="Char"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		froumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Char"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(char bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));32           if (count >  ///		An <r - bitIndex))
    ///		An     throw new ArgumentException(BitStreamResources.G ///		An "Argument_InvalidCountOrBitIndex_Char"));

            uint uiBits = (uint)bits;
            uint uiBitIndex = (uint)bitIndex;
            uint uiCount = (uint)count;</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operatios at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="UInt16"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="UInt16"/> values to write.
        /// </param>
        /// <seeal      if (bits == null)
                throwWrite(ushort[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
     /// </param>
        /// <seealso cref="UInt32"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(uint bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOt</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Char"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Char"/> values to write.
        /// </param>
        /// <seeained in an <see cref="UInt32"/> buffer to
        id Write(char[] bits, int offset, int count)
        if (!_blnIsOpen)
        en)
                throw DisposeectDisposedException(BitStreamResources/ </summarObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_Be operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> array specifying the buffer to write data from.
        /// </param>
        /// <seealso cref="Int16"/>
        public virtual void Write(short[] bits)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
       ///		Writes the <b>bits</b> contained in an <see cref="Int16"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System="System.ArgumentOutOfRangeException">
  s than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int16"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Int16"/> offset
   exception>
        /// <exception  cref="UInt16"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ushort bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClhrow new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            ushort[] ausBits = new ushort[count];
            Buffer.BlockCopy(bits, offset << 1, ausBits, 0, count << 1);

            Write(ausBits, 0, count);
        }

        #edregion


        #region 32-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt32"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="UInt32"/>
        
        public virtual void Write(uint bits)
        {
            Write(bits, 0, SizeOfUInt32);
        }
        //veParameter"));
            if (count > (bits.LengtString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font col    ///		An <see cref="It32"/> value specifying thelittle-endian <b>bit</b>
        ///		index to begin writingfrom.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
     Singconditions / Redistributions in binary form must reproduce the above coue
            uint uiBitBuffer_FreeBits = BitBuffer_SizeOfElement - _uiBitBuffer_BitIndex;
            iValue_BitsToShift = (int)(uiBitBuffer_FreeBits - uiValue_EndIndex);
            uint uiValue_Indexed = 0;
            if (iValue_BitsToShift < 0)
                uiValue_Indexed = bits >> Math.Abs(iValue_BitsToShift);
            else
       >offset<		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports the flush operation.
        /// </value>
        /// <seealso cref="Boolean"/>
        public static bool CanFlush
        {
            get { return false; }
        }

        #endregion


        #region ctors/dtors [20051123]
        /// <summary>
        ///		Initialises a new instance of the <see cref="BitStream"/> class
        ///		with an expandable capacity initialised to one.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="BitStream"/>
 >
        ///		An <se <b>bits</b> contained in an <see cref="UInt32"/> buffer to
floaite(ushort[] bits, int offset, int count)
        {
               mary>
        ///		Initialises a new instance of the <see cref="BitStream"/> class
        ///		with an expandable capacity initiali>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Int32"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the tOfRangeException(BitStreamResources.GetString("ArgumentOutOfRange_NegativeOrZeroCapacity"));

            _auiBitBuffer = new uint[(capacity >> BitBuffer_SizeOfElement_Shift) + ((capacity & BitBuffer_SizeOfElement_Mod) > 0 ? 1 : 0)];
        }
        /// <summary>
        ///		Initialises a new instance of the <see cref="BitStream"/> class
        ///		with the <b>bits</b> provided by the specified <see cref="Stream"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		Added [20051122].
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Stre      throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStrcolor="blue">new</font> BitStream();<br></br>
    // </exception>
        /// <ex/		An array of <setem.ArgumentException">
        ///		<i>offset</i>stem.Net.SockTo   ///   pm.Net.Sockets;
usinArgume), 0rrent stream.
        /// </summary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <seealso cref="UInt32"/>
        private uint[] _auiBitBuff         ty byte buffer
            byte[] abytBits = new byte[bits.Length];


            long lCurrentPos = bits.Position;
            bits.Position = 0;

            bits.Read(abytBits, 0, (int)bits.Length);

            bits.Position = lCurrentPos;


            Write(abytBits, 0, (int)bits.Length);
        }

        #endregion


        #region Methods [20051201]

        #region Write [20051201]

        #region Generic Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
   >
        ///		An <see
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt32"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UI      throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDsed_Btion</b>
        ///		within this stream.
        /// </value>
        /// <seealso cref="Length"/>
        /// <seealso cref="Int64"/>
        public override long Position
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                uint uiPosition = (_uiBitBuffer_Index << BitBuffer_SizeOfElement_Shift) + _uiBitBuffer_BitIndex;
  dex to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Single"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(float bits, int bitIndex, intthis values end index
            uint uiValue_EndIndex = bitIndex + count;

            // Clear out unwanted bits in value
            int iValue_BitsToShift = (int)bitIndex;
            uint uiValue_BitMask = (BitMaskHelperLUT[count] << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
            uint uiBitBuffer_FreeBits = BitBuffer_SizeOfElement - _uiBitBuffer_BitIndex;
            iValue_BitsToShift = (int)(uiBitBuffer_FreeBits - uiValue_EndIndex);
            uint uiValue_Indexed = 0;
            if (iValue_BitsToShift < 0)
                uiValue_Indexed = bits >> Math.Abs(iValue_BitsToShift);
            else
         ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /Buffer_Length >= (uiBitBuffer_Position + 1))
            {
                int iBitBuffer_>offset</ift = (int)(uiBitBuffer_FreeBits - count);
                uint uiBitBuffer_BitMask = 0;
                if (iBitBuffer_BitsToShift < 0)
                    uiBitBuffer_BitMask = uint.MaxValue ^ (BitMa>offset</i> orount] >> Math.Abs(iBitBuffer_BitsToShift));
                else
                    uiBitBuffer_BitMask = uint.MaxValue ^ (BitMaskHelperLUT[count] << iBitBuffer_BitsToShift);
                _auiBitBuffer[_uiBitBuffer_Index] &= uiBitBuffer_BitMask;

                // Is this the last element of the bit buffer?
                if (uiBitBuffer_LastElementIndex == _uiBitBuffer_Index)
                {
                    uint uiBitBuffer_NewLength = 0;
       ta from.
        /// </param>
        /// <seealso cref="Single"/>
        public virtualiBitBuffer_Position + count;
                    else
                        uiBitBuffer_NewLength = uiBitBuffer_Position + uiBitBuffer_FreeBits;
                    if (uiBitBuffer_NewLength > _uiBitBuffer_Length)
                    {
                        uint uiBitBuffer_ExtraBits = uiBitBuffer_NewLength - _uiBitBuffer_Length;
                        UpdateLengthForWrite(uiBitBuffer_ExtraBits);
                    }
                }
            }
            else // Not overwrinting any bits: _uiBitBuffer_Length < (uiBitBuffer_Position + 1)
            {
                if (uiBitBuffer_FreeBits >= count)
                    UpdateLengthForWrite(count);
                else
                    UpdateLengthForWrite(uiBitBuffer_FreeBits);
            }

            // Write value
            _auiBitBuffer[_uiBitBuffer_Index] |= uiValue_Indexed;

            if (uiBitBuffer_FreeBits >>offsunt)
               ref="UInt64"/esForWrite(counref="UInt64"     else // Some bits in value did not fit
     ref="UInt64"="blue">short</font> [] asBuffer = <font color="blue">new short</font> [bstrm.Lengt64-/>
    /// </sumk (_o        if (_blnLoadingResource)
                        return ("The resource manager was unable to load the resource: " 64et</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations        _resman = new ResourceManager("BKSystemFix, ref uiVk (_bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant      end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt16"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="UInt16"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="UInt16"/> values to write.
        /// </param>
        /// <seealso ctions at the end /		An <see cref="SByte"/> value specifying the <b>bits</b> to write datyptoge(ushort[] bits, int offset, int count)
        {
            i6prov"/>
        
        public virtual void Write(sbyte bits)
        {
            Write(bits, 0, SizeOfByte);
        }
        /// <       ///		The current stream is closed.
        = true;
                    str = _resman.GetString(name, null);
                    _blnLoadingResource = false;
                }
                return str;
            }

            #endregion

        }

        #endregion

        #endregion


        #region Constants [20051116]
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Byte"/> value ty            /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Byte"/>
        private const int SizeOfByte = 8;
        /// <summary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Char"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constanttions at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.<br></br>
        ///		<br></br>
        ///		Fixed [20051201].
        /// </remarks>
        /// <param name="b <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <seeals       ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt64"/>
      (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="SByte"/> buffer to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException64
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullExcepti64 ///		.
            ///cref="Bo1Dispoth64];<b>> 5) < 1 ?nt32"/>
  :</b> expand the
 ;
        }
 2      /// <su      /) >ks>
?perati  }
   < 0      ///		W-ks>
: 0)Writes the <b>bits</b> c"blue          }
   >    buffer to
    f the in4"/> buitInd      }
   :f="UInt6:ion>
        /// <exc"blue in contained ieption crry>
    =   //the in    ///ceptry>
  		The cummary>
        ///		An array o<summary>
        ///		An     <summary>
        ///		An  in on>
        /// "/> ry>
   > /// </summary>
        /// <remar     public vir     // </ser to
   // <remarks>
        ///	ry>
     fRangeEry>
  // <remarks>
            throw new ObjectDispo1sedException(Bit/ <except="Syste		shifts equivalent to the n /// </exc/ </on>
        /// <exception cref="System.Argument in aRangeException2>
        ///		<i>offset</i> ocount</i>.
// <re negative.
        /// </excebr>
    bjectDispo2sedException(BititStream<// <reef="UInt32"/> value defining the equi{
    //in Visuaarks>)IO
{marks>
     1     /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
            </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="SByte"/> array specifying the buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to write.
        /// </param>
        /// <seealso cref="SByte"/>
        /// <stions at the end of/>
        
        public virtual void Write(sbyte[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                th       ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="U
//
// Re_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            byte[] abytBits = new byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);

            Write(abytBits, 0, count);
        }
        /// <summary>
        ///		Writes a byte to the current stream at the current position.
        /// </summary>
        /// <remarks>
 32"/> value specifying the <see cref="UInt64"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="UInt64"/> values to write.
        /// </param>
        /// <seealso cref="UInt64"/>
        /// <se
            Write(value);
        }

        #endregion


        #region 16-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="Char"/>
        
        public virtual void Write(char bits)
        {
            Write(bitsrgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter")       /// <exception cref="System.ObjectDisposedException">
        ///		The current stream       sed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cr       ///	ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="Char"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="Char"/> value specifying the <b>bits</b> to write data
        ///		fronter < iEndIndex; iUInt64Counter++)
                Write(bits[iUInt64Counter]);
        }
    value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="Char"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(char bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));64           if (count >     /// </r - bitIndex))
       /// <    throw new ArgumentException(BitStreamResources.G    /// <"Argument_InvalidCountOrBitIndex_Char"));

            uint uiBits = (uint)bits;
            uint uiBitIndex = (uint)bitIndex;
            uint uiCount = (uint)count;       ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>ref="UInt32"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(uint[] bits, int offset, int count)
  tions at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.<br></br>
        ///		<br></br>
        ///		Fixed [20051201].
        /// </remarks>
        /// <param name="bits">
        ///		An <see cref="UInt64"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number / <seealso cref="Int64"/>
        public virtual voidwhether the current
        "/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ulong bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedExd the
        ///		<    /// </summary>
﻿/﻿//****remarks*************		The <b>Read</b> method returns zero if the end oight �current stream**************is reached. In all other cases,*************alway reseds at least one**************<b>bit*****from, Bill Koukoutsis
/ before******
ing.**************/******************** <param name="bits"***************When this*************
/, containsght �specified <see cref="Int64"/***************value withght �t
// ms*****between bitIndex and (in binary+ count - 1)**************replaced by. 
//
// Redistriand ication, are permitted pfollowing conditdistre met:
//
// Redistributions obinar source code musAnt of conditions32"/> disclais
// ying. 
//
// Re*****iinary t which to**************beginnditihe following conditdocumentation
// and/or other maduce provided with the distribution. 
//
// Neither the name of tmaximum number005,
// Redistr**************tonditi/ be used to endorse or promote prod*****
/rovided with the distribution. 
//
// Neither the name of tTHIS SOFTWARE IS PROVIDED BY THE COwritten intoght �discl. Tin tcan be lesce, aetaiED TO, THE
// IMPLIED WARRANTIES OF MrequestedyrightatD TO, THE
// IMPLIED W are notll Koukoly available,**************or/ Copyright © 2005, Bill Koukoutsis
/ s reservedrovided anywith or without
// mUTORS BE
/IGHT HOLDERS AND CON AND ANY EXPRESS OR t of alsoconditions and the followinCES; LOSS OF USE, D//
//*********public virtualABIL ****(out long in s, ON Aterials LITY, duce above copy{*********TY, u OF LulBits = 0; LIABILITY, OON AiINCL**** =ANY THEORY (INCL, WHETHER INCONTRA;
 LIABILITY, OIABI = ( OF ) (INCL OF THIS SOFTWA*****
THERWISE)
G NEGLIGEN}OR PROFITS; OR *******************		****ce, th LIMITED TO,ht noti COPPURPOSY, OR
// CONSEQto anwith or withoutof conditions and  bufferfollowing condit**********************exceptionconditiSystem.ObjectDisposedEstem.Res source code mus****Y, OR
// CONSEQUENclbalifollowing conditystem.Resalized;
using System.Resources;
using ArgumentNullzation;
using System.Comp<iIMITED i>QUENa null reference (<b>NothingANIZATI VisAND Basic)lection;
using System.Net.Sockets;
using**************************************************
// Copyright © 2005, Bill Koukoutsis
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following arrayaimer. 
//disclsibutions offsety formnt)</c>roduce the a  notice,s of its contri/ this list of conditions and the following disclaimer in the documentation
// and AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY ANsing SyESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY TH OF []ARE, ACT, STRICT LIABILITY, Oif (!_blnIsOpenabove copym(FileStthrow new System.Globalization;
u(BitSsis
/Resources.Gepubling("System.Globali_c>public Cm.Ref"))G NEGLIGENCE ORfrm mus ==  /// BitStream(FileStream bits);graphy;

namespace BK(ns of , c>public static implicit operagraphy;

namBuffBing Sits);F THE
// POSSIBILITYNY THIABILI0WAY Os.Lengths);</c><br>*****************************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Resources;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace BKSystem.IO
{
    /// <summary>
    ///		Creates a stream for reading and writing variable-length data.
    /// </summary> System.Security.Cryptography;
OutOfRangeespace BKSystem.IO
{
    ///nt)</cmmaryor <i>duce mmary>
 negativelection;
using System.Net.Sockets;
using System.Security.Cryptography;
ic BitStream(<see cref="Stream"/> bits)subtracE COcation, asing S lr></bQUENTICULAR PUc> contructovariable-length data.
    /// </summary>
    /// <remarks>
    ///		<b><font color="red">Notes to Callers:</font></b> Make sure to
    ///		include the "BitStream.resx" resource file in projects using the
    ///		<see cref="BitStream"/> class.<br></br>
    ///		<br></br>
    ///		[20051201]: Fixed problem with <c>public virtual void Write(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public virtual int Read(out ulong bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>public virtual void WriteTo(Stream bits);</c> to write
    ///distributionnt)</cprovided with the distribution. 
//
// Neither the name of thsing System.Text;
unt)</ct Read(out ulonr the name ibutors may
// be used to endorse or promote products derived from this software without
// specific prior written permission. 
//
// THIS SOFTng System.IO;
using System.Text;
utIndex,PYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY ANded the following implicit operators to allow type casting
    ///		instances of the <see cref="BitStream"/> class to and from other types
    ///		of <see cref="Stream"/> objects:<br></br>
    ///		<br></br>
    ///		<c>public static implicit operator BitStream(MemoryStream bits);</c><br></br>
    ///		<c>public static implicit operator MemoryStream(BitStream bits);</c><br></br>
    LITY, nt)</cIN
// CONTRACT, STRICT LIABILITY, Oplicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator BufferedStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operaeam(FileStr crefnt)</c>< 0blic static implicit operator Buffered <c>public BitStr(   #regiits);</c><br></br>
    ///		<c>public sted <c>publ_N></br>
Pistret         ///		.
       duce t/ </remarks>
            private static void InitialiseResou from ger()
            {
                if (_resman == null)
                {
                    >    //<br></b -  /// <)blic static implicit operator Buffer    ///		<c>public static implicit operagraphy;
_InvalidCuce OrO #regiperator BitStreamR OTHEndbinary=unt)</c>roduce G NEGLIGENCE OR OTHERWISE)
// NG NEGLIGENCE Ofor (R OTHons aStreaer
        ; <summary>
    <         }///		Gets the s++ BitStream(FileStrHERWISE)
/+/ ARISING I</c>[<summary>
   ]erator BitStream(Networ OF SUCH DAMAGE.
//*****************************************************************************

using System;
usng System.IO;
using SysteDouble
// Neithystem.Collections.Specialized;
using S**************************************************
// Copyright © 2005, Bill Koukoutsis
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this
// list of conditi/// </parthe following disclaimer. 
//
// Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USEk (_oResManLock)
    OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORYd// </
    ///		<c>public static im(NetworkStre/// <remits);SizeOf/// </r>
    ///		<c>public static implicit operator BitStream(CryptoStream bits);</c><br></br>
    ///		<br></bed resource.
            /// </param>
            /// <returns>
            ///		A <see cref="String"/> representing the contents of the specified
            ///		resource.
            /// </returns>
            /// <seealso cref="String"/>
            public static string GetString(string name)
            {
                string str;
                if (_resman == null)
                    InitialiseResourceManager();

                lock (_oResManLock)
                {
                    if (_blnLoadingResource)
                        return ("The resource manager was unable to load the resource: " + name);

                    /or other materials provided with the distribution. 
//
// Neither the name of the ORGANIZATION nor the names of its contributors may
// be used to endorse or promote products derived from this software without
// specific prior written permission. 
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USEa <see cref="Byte"/> value type.
        /// </summary>
        /// <remarks>
        ///		ThLITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEBitConverter.To/// </<c>p     ///		GetBytes( (INCL), 0   ///		.
            /// </remarks>
            /// <param name="name">
            ///		A <see cref="String"/> representing the specified resource.
            /// </parasing System.Collections.Specialized;
using System.Resources;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace BKSystem.IO
{
    /// <summary>
    ///		Creates a stream for reading and writing variable-length data.
    /// </summary>
    /// <remarks>
    ///		<b><font color="red">Notes to Callers:</font></b> Make sure to
    ///		include the "BitStream.resx" resource file in projects using the
    ///		<see cref="BitStream"/> class.<br></br>
    ///		<br></br>
    ///		[20051201]: Fixed problem with <c>public virtual void Write(ulong bits, int bitIndex, int count)</c>
    ///		and <c>public vik (_oResManLock)
      ng bits, int bitIndex, int count)</c> methods.<br></br>
    ///		<br></br>
    ///		[20051127]: Added <c>public virtual void WriteTo(Stream bits);</c> to write
    ///		the contents of the current <b>bit</b> stream to another stream.<br></br>
    ///		<br></br>
    ///		[20051125]: Added the following implicit operators to allow type casting
    ///		instances of the <see cref="BitStream"/> class to and from other types
    ///		of <see cref="Stream"/> objects:<br></br>
    ///		<br></br>
    ///		<c>public static implicit operator BitStream(MemoryStream bits);</c><br></br>
   a <see cref="Byte"/> value type.
        /// </summary>
        /// <remarks>
    //r>
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator BufferedStream(BitStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(NetworkStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(CryptoStream bits);</c><br></br>
    ///		<br></bsummary>
        ///		An <see cref="Int32"/> value defining the number of bits
        ///		in a <see cref="Double"/> value type.
        /// </summary>
        /// <remarks>
        ///		This field is constant.
        /// </remarks>
        /// <seealso cref="Int32"/>
        /// <seealso cref="Double"/>
        private const int SizeOfDouble = 64;
        /// <summary></br>
    ///		[20051123]: Added <c>public BitStream(<see cref="Stream"/> bits);</c> contructor.<br></br>
    ///		<br></br>
    /// </remarks>
    /// <seealso cref="BitStream"/>
    /// <seealso cref="Stream"/>
    /// <seealso cref="int"/>
    /// <seealso cref="byte"/>
    public class BitStream : Stream
    {

        #region Nested Classes [20051116]

        #region private sealed class BitStreamResources [20051116]
        /// <summary>
        ///		Manages reading resources on behalf of the <see cref="BitStream"/>
        ///		class.
        /// </summary>
        /// <remarks>
        ///		<b><font color="red">Notes to Callers:</font></b> Make sure to
        ///		 <see cref="Int32"/> value defining the number of bit
        ///		shifts equivalent to the number of bits per element in the
        ///		internal buffer.
        /// </summary>
        ///

            #region Fields [20051116]
            /// <summary>
            ///		The <s/// </paraResourceManager"/> object.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static ResourceManager _resman;
            /// <summa/// </param>
    ///		An <see cref="Object"/> used to lock access to
            ///		<see cref="BitStream"/> resources while the current
            ///		<see cref="ResourceManager"/> is busy.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static object _oResManLock;
            /// <summary>
            ///		A <see cref="Boolean"/> value specifying whether a resource is
            ///		currently bei	This field is constant.
        /// </remarks>
        /// <seealso cref="UInt32"/>
             /// </remarks>
            /// <seealso cref="Boolean"/>
            private static bool _blnLoadingResource;

            #endregion


            #region Methods [20051116]
            /// <summary>
            ///		Initialises the resource manager.
            /// </summary>
            /// <remarks>
            ///		.
            /// </remarks>
            private static void InitialiseResourceManager()
            {
                if (_resman == null)
                {
                    lock (typeof(BitStreamResources))
                    {
                        if (_resman == null)
                        {
                            _oResManLock = new object();
                            _resman = new ResourceManager("BKSystem.IO.BitStream", typeof(BitStream).Assembly);
                        }
                    }
                }
            }
            /// </// </ry>
            ///he length of tecified stringhe length of            /// </summary>
            /// <remarhe length of     ///		.
            /// </remarks>
          ///		.#endregiont
            {
      
         {
     Logical Opera.Ress [20051115]************************************Perform
   bitwis*****AND*****o throw n oPURPOS LIMITED TO,********************Y, OR
// CONSEQagotict******orresponay
/               retuhis
// lis**************ing disclaimer in the 			0x000001FF, 0x000003FF, 0x000007FF, 0x00000FFF,
			0x00001FFF, 0x00003FFF, 0x00007FFF, 0x0000FFFF,
			0x0001FFFF, 0x0003FFFF, 0x0007FFFF, 0x000FFFFF,
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,
			0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF, 0x0FFFFFFF,
			0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF,
		};

        #endregion


        #regiozation;
using System.Componen CONSEQis
// list[2005112// <summarydistr    y for*****

usin ///		Gets the maxim do// LIhavemmary>amED TO, THE
// IMPLIED Wvariable-length data.
    /// </summary>
    /// <remarks>
   following conditions are met:
//
// Redistributions of source code musA ///		The <sc>public "UIntystemaimer.ct.
     pStringeManagectDiswith or without
/d_BitStreamClosedce: " + name);

                    _blnLoadingResource = ttStream bstrm = <font color="bluht notiame of tresul   /am();<br></br>
        ///		:<br></br>
       "));

                retuntModel;
using S
      IDED BY THE COP    }
        }
        /// <summary>
       wing disclaimer in the ITUTE GOODS OR SERVICES; LOSS OF USEm = <font coION) HOWEVER CAUSED AND c>public  And<c>public 
    ///		<c>public static implicit operator BitStream(FileStream bits);</c><br></br>
    ///		<c>public static implicit operator BitStream(BufferedStream bits);</c><br></br>
    ///		<c>public static implicit operator BufferedStream(BitStream bits);</c><br></br>
    ///		<c>public static imp <font ts);</c><br></br>
    //= new ob!= _uHERWlicit _br></br   ///		.
                _resman = new ResourceManager("BKSystem.IO.BitStream", typDing Sent        {br></bsbly);
            // Creat)"/> mts);        {   ///		.
   ref="Int64"strmNew =    return (lo(           if (!_blnrator BitStreamuON AuiWholeUNTERRosed_Bi              if (!_bl >>ffer     if    //Ele, typShif     }
               ry>
      0rator BitStream     ues required tlues requir<    }
        }
     .
        /           /// </sum >> 3) +._a          i[ues requi]   /Exception">
        ///	&;</c><he current stream is cloreamClosed"));

  Ar)"/>  (INCL furdistr  ///*******sing S?   ///		.
       uffer_Length & 7) >  &///		Gets the maximum numMod) > </remarks>
    T LIABILITY, Ot</b> valueBitMask =b> va.MaxViscla<<  ///)<c>p	Gets the maximum nu - le>
        ///		<font face="Courier New">
             ///		.
   ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
   &/ This pro    ///		.
   et
        .
        / >> 3) +>
    ///		<c>public static implicit operator BtString("ObjectDisposeORitStreamClosed"));

                return (long)_uiBitBuffer_Length;
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///		// to read the entire stream into a <see cref="Byte"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:    ///		:<br></ ///		:<br></br>
        ///		<font color="blue">byte</font> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
          ///		:<br></br>
        ///		<font colead = Read(abytBuffer, 0, (<font color="blue">int</font>)bstrm.Length8);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <sOrcref="Int64"/> value specifying the maximum number of
        ///		<b>8-bit</b> values required to store this stream.
        /// </value>
        /// <seealso cref="Read(byte [], int, int)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
            get
            {
                if (!_blnIsOpen)
                    throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is clos|d.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
       /// <value>
        ///	/		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
 e<b>X****clusiv
        ///		:<br></br>
   UDING, BUT NOT LIMITED TO,iBitsRead = Read(abytBuffer,       }
        }
        /// /
// All rightsh8);<br></br>
        ///		:<br></br>
8-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(byte [], int, int)"/> method<br></br>
        ///		// to read the entire stream into a <see cref="Byte"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///       return (long)(_uiBitBuffer_Le ///		:<br></br>
        ///		<font color="blue">byte</font> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
          return (long)(_uiBitBuffer_Length >> number of <b>32-bit</b> vaee cref="Read(byte []
            }
        }
        /// <summary>
              ///		:<br></br>
        ///		</font>
        /// </example>
        /// <value>
        ///		An <sXo    ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(int [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int32"/> array.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
        ///		:<br></br>
        ///		<font color="blue">int</font> [] aiBuffer = <font color="blue">new int</font> [bstrm.Length32];<br></br>
        ///		<font color="blue">int</font> iBitsRead = Read(aiBuffer, 0, (<font color="blue">int</font>)bstrm.Length32);<br></br>
        ///		:<br></br>
        ///		</font>
        /// </example>
 ^d.
        ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="Int16"/> array.<br></br>
      /// <value>
        ///		/		:<br></br>
        ///		BitStream bstrm = <font color="blue">new</font> BitStream();<br></br>
        ///		:<br></br>
    NOTitStreamClosed"));

                return (long)_uiBitBuffer_Lengthmaximum number of <b>64-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
ee cref="Byte"/> array.<br></br>
        ///		</font>
      color="blue">byte</font> [] abytBuffer = <font color="blue">new byte</font> [bstrm.Length8];<br></br>
      e current <b>bit</b> position within this she following disclaimer in the         ///		</font>
        /// </example>
        /// <value>
        ///		An <sNot(           /// <seealso cref="Boolean"/>
            private static bool _blnLoadingResource;

            #endregion


            #region Methods [2005111amClosed"));

                return (long)(_uiBitBuffer_Length >> 3) + (long)((_uiBitBuffer_Length & 7) > 0 ? 1 : 0);
            }
        }
        /// <summary>
        ///		Gets the maximum number of <b>16-bit</b> values required to store this
        ///		stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedException">
        ///		T~     ///	</exception>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font face="Courier New">
        ///		<font color="green">
        ///		// This property can be used in conjunction with the <see cref="Read(short [], int, int)"/> method<br></br>
        ///		// to read the entire stream into an <see cref="      }
            set
 /		:<br></br>
        ///		BitStream bstrm = <font color="blue">new<           if (!_blnIsOpen)
          Bit ber o }
  ObjectD6sposedException(BitStreamResources.GeMovm.
 = Read(alBufferXEMPLARY, OR
// CONSEQLITY ANlefolor="blue">int[200511is
// listTHIS SOotices>
        ///	<exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        ///	</exception>
        ///	<exception cref="System.ArgumentOutOfRangeException">
        ///		The position is sucts derived from this software without
// spec    ext;
uIGHT representce, this
// listTHIS SOFTWBuffer_Index = uiReque
        ///		<font color="blue">byte</font> [] abytBuffer = <font color="blue">new byte</font> [bstnval ser o INDIRECT, INCIont color="blue">int</font> iBitsRead = Read(abytemarks>
        /// <value>
     value>
        ///		An <sber oLeftr></brks>
            /// <seealso cref="Boolean"/>
            private static bool _blnLoadingResource;

            #endregion


            #region Methods [2005111amClosed"));

         a copy005, Bill Koukoutsis
//
// All tBuffer_Length >> 3) + (lain .Copy(0 ? 1 : 0);
           StreaEVEN    )         }
               /		<fonnt streactDisposebr></b);
             f    ///		 >erty (!_blnIsOpen)
     green">
        //
   lear EORY// R</c>///		// to read the entirePosi.Resoed to store this
 is
        	// This///		stream.
  y instead.
/ </ /// <r    /// </rem           /// </sumObjectDisposeWrite(falsealso cref="///		<c>publicurreelse the      lobr></bs returns <b>false</b>. To set bool bln    = alue  ///		// to read tn"/> property instead.
        /// </remarks>
      -the curr   /// <value>
        ///		A <seegreen">
        //he current stream use thhe curre+    /// </rem ///		// to read tObjectDispose     /// <>
     ///		// to read treturn false; }
        }
/ <summary>
        ///		Gets a value indn"/> vhether the current stream	BitStream bstrset the position ge_Informduce tin
        ///		the cn"/> property instead.
  n"/>
        public override booarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whetheF THIS SOFTWARrrent stream use the <see cref="Posm = <font color="blue">new</font> BitStream();<br></br>
       BitStreamResources.GetString("ArgumentOutOfRange_IrighlidPosition"));

                _uiBitBuffer_Index = uiRequestedPosition >> BitBuffer_SizeOfElement_Shift;
                if ((uiRequestedPosition & BitBuffer_SizeOfElement_Mod) > 0)
                    _uiBitBuffer_BitIndex = (uiRequestedPosition & BitBuffer_SizeOfElement_Mod);
                else
                    _uiBitBuffer_BitIndex = 0;
            }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports reading.
        /// </ream ary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicReam g whether the current stream
        ///		supports reading.
        /// </value>
        /// <seealso cref="Boolean"/>
        public override bool CanRead
        {
            get { return _blnIsOpen; }
        }
        /// <summary>
        ///		Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <remarks>
        ///		This method always returns <b>false</b>. To set the position within
        ///		the current stream use the <see cref="Position"/> property instead.
        /// </remarks>
        /// <value>
        ///		A <see cref="Boolean"/> value indicating whether the current stream
        ///		supports seeking.
        /// </value>
        /// <seealso cref="Position"/>
        /// <seealso cref="Boolean"/>
        public override bool CanSeek
        {
            get { return false; }
        }
/ <summary>
        ///		Gets a value indicating whether the current stream supports writing.
        /// </sum   //publicary>
        /// <remarks>
        ///		.
        /// </remarks>
        /// <value>
        ///	fir <see cref="Boolean"/> value iCanWrite
        {
    seealso cref="Position"/>
        /// <seealso cref="Boolean"public override bool CanSeek
        {
seealso cref="Boolean"/>
        public override bool CanWrite
        {
            get { return _blnIsOpen; }
                if (!_blnIsOpen)
          Toit ope thro new ArgumentOutOfRangeException("value",R****
//a ///		The <sit opeBitBuffer_BitIndex = 0;
l Koukoutsis
//
// All rightss innary// L/		:<br></br>
       ********************************************ption">
        ///		The position is set to a negative value or position emarks>
  or="blu_BitIndexame of t /// </remarks>
        /// <param name="capacity">
        //ITUTE GOODS OR SERVICES; LOSS OF USEemarks>
 //		A <see cref=overrid   //aciti>capaciion</b>
        ///		with        }
        }
        /// <summary>
        /5f <b>16-bit</b> values required toNEGLIGENCE OR OTHERWauiBitBuffer = new uint[(propert1 = 1
            geit opeBuilder sb (long)tBuffer_SizeO( conjffer_Length & 7) > 0 ? 1 : 0);
           ///		stream.
        /// </summary>
        ///	<exception cref="System.green">
        //sb.Append("[" {
       /		Thit oper_ifp) + "]:{"  ///		// to read t     /ity >> BitBuf31;acity >> BitB>eam.
/summary>
 --CanSeek
        {
            get { retu/		// This property1d in        ///		y initialised to the xample              uint uiRequestedPosit)/		<   /// <rey initialised to the he <b>bits</b>'1' the current stream supt st ///		Added [20051122].
        /// 0/remarks>
        //her the currenhe <b>bits</b> }\r\n"Stream"/>.
     override bool rks>
        ///		.
        /// </remarks>
        /// <example>
        ///		<font 31 ///		<font color="green">
        //b>bits</b> provided by the specified <see cref="Stream"/>.
        capacity >> BitMie th conju32ef="Read(short [], int, in31 {
            ge    /// </summary>
        /// <exception w ArgumentNullref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks>
        ///		Added [20051122].
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Stream"/> object constance of the <see cr/summary>
    w ArgumentNullE-    /// <exception cref="System.ArgumentNullException].
        /// ./remontaining the specified <b>bits</b>.
        /// </param>
      IBILITYsbe specifier>
    ///		<c>public static implicit operator Bition>
        /// <remarks>
        ///		.
       >
        ///		Gets thediscla<param name="capacity">
        ///		An <see cref="Int64"/> specifying the initial size of the internal
        ///	distributions o>
        ///		BitStream bstrmoolean
// Neithe     /// <seealsois
// listm>
            /// <r
        ///		<font color="blue">byte</font> [] abytBuf /// </param>
        /// <seealsotion">
        am"/>
        public BitStream(long capacity)
        {
            if (capacity <= 0)
               nt>
        /// </exf="System//		A <see cref=s"cap>.
 tOutOfRangeExc/valueiRACT, STRICT LIABILITY, O  /// <st     is less{rovieam  ? 1 : 0ee cr}"r = new uint[(/// <sumty>
        tes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exceptir>
        ///		BitStream bstrmyt/param>
  entOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt32"/> ilue span <i>count</i>.
        /// </exceptionyt//		This field is constant.
  tBuffer_SizeOfElement_Mod) > 0 ? 1 : 8     ///		.
   nt_Shift) + ((capacity & Bib>bits</b> eeal="Stream"/>.
         /// <ity >> BitBuf7  /// <exception cref="System.ArgumentNullExcepgreen">
        ///		// This propertyerence (<b>Nothing</b> in Visual Baxampl  ///     /// <remarks>
        ///		Added [20051122       /// </remarks>
        //aram name="bits">
       		A <see cref="Stream"/> objher the currencified <b>bi"Str115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt32"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exceptir>
        ///		BitStream bstrSalue specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="UInt32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <sBuffer_F = new uian <i>count</i>.
        /// </exceptiosite(ref uint bits, ref uint bitInite(reyObje EVENite()s in((capacity & BitBuffer_SizeOfElement_Mod) > 0 ? 1 :       // Calculate the current position
            uiBuffeiBitBuffer_Position = (_uiBitBuffer_Index << BitBuffer_SizeOfElement_Shift) + _uiBitBuffer_BitIndex;
            // Detemine the last element in the bit buffer
            /  uint uiBitBuffer_LastElementIndex = (_uiBitBuffer_Length >> BitBuffer_SizeOfElement_Shift);
            // Clalculate this values end index
            uint uiValue_EndIndex = bitIndex + count;

            // Clear out unwanted bits in value
            int iValue_BitsToShift = (int)bitIndex;
            uint uiValue_BitMask = (BitMaskHelperLUT[count] << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
            uint uiBitChar specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="UInt32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <sstElemean <i>count</i>.
        /// </exceptiocha     /int bits, ref uint bitIndex, ref uint count)
        {
      16     // Calculate the current position
            uistEliBitBuffer_Position = (_uiBitBuffer_Inde15 << BitBuffer_SizeOfElement_Shift) + _uiBitBuffer_BitIndex;
            // Detemine the last element in the bit buffer
            uint uiBitBuffer_LastElementIndex = (_uiBitBuffer_Length >> BitBuffer_SizeOfElement_Shift);
            // Clalculate this values end index
            uint uiValue_EndIndex = bitIndex + count;

            // Clear out unwanted bits in value
            int iValue_BitsToShift = (int)bitIndex;
            uint uiValue_BitMask = (BitMaskHelperLUT[count] << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
            uint uiBit    16 specifying the little-endian <b>bit</b>
        ///		index to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="UInt32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <sount)
   ;
            else
                uiValue_Indexushorref="B   /// <remarks>
        x;
  s    // C, ref current bits in bit buffer that are at same indices
    h > _uiBitBuffer_Length)
                    {
        ount)
            uint uiBitBuffer_ExtraBits = uiBitBuffer_NewLength - _uiBitBuffer_Length;
                        UpdateLengthForWrite(uiBitBuffer_ExtraBits);
           uiValu  uint uiBitBuffer_LastElementIndex = (_uiBitBuffer_Length >> BitBuffer_SizeOfElement_Shift);
            // Clalculate this values end index
            uint uiValue_EndIndex = bitIndex + count;

            // Clear out unwanted bits in value
            int iValue_BitsToShift = (int)bitIndex;
            uint uiValue_BitMask = (BitMaskHelperLUT[count] << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
            uint uiBitunt)
                UpdateIndicesForWrite(count);
            else // Some bits in value did not fit
            // in current bit buffer element
            {
                UpdateIndicesForWrite(uiBitBuffer_FreeBits);

                uint uiValue_RemainingBits = count - uiBitBuffer_reeBits;
        
                uiValue_Indexex;
                Write(ref bitsmainingBits);
            }
        }

        #endregion


        #region 1-Bit Writes [2051116]
        /// <summary>
        ///		Writes the <b>bit</b> represented by a <see cref="Boolean"/> value to
        ///		the current stream.
        /// </summary>
            }
                }
            }
            else // Not overwrinting any bits: _uiBitBuffer_Length < (uiBitBuffer_Position + 1)
            {
                if (uiBitBuffer_FreeBits >= count)
                    UpdateLengthForWrite(count);
                else
                    UpdateLengthForWrite(uiBitBuffer_FreeBits);
            }

            // Write value
            _auiBitBuffer[_uiBitBuffer_Index] |= uiValue_Indexed;

            if (uiBitBuffer_FreeBits >= count//
// Neithe      UpdateIndicesForWrite(count);
            else // Some bits in value did not fit
            // in current bit buffer element
            {
                UpdateIndicesForWrite(uiBitBuffer_FreeBits);

                uint uiValue_RemainingBits = count - uiBitBuffer_FreeRRUPTION) HOWE        uint uiValue_StartIndex = bitIndTY, WHEtBuffer_FreeBits;
                    if (uiBitBuffer_NewLengt32        #endregion


        #region 1-Bit Writes [20051132iBitBuffer_Position = (_uiBitBuffer_Inde    /// <exception cref="System.ArgumentNullExceper_BitIndex;
            // Detemine the last element in the bit buffer
            uint uiBitBuffer_LastElementIndex = (_uiBitBuffer_Length >> BitBuffer_SizeOfElement_Shift);
            // Clalculate this values end index
            uint uiValue_EndIndex = bitIndex + count;

            // Clear out unwanted bits in value
            int iValue_BitsToShift = (int)bitIndex;
            uint uiValue_BitMask = (BitMaskHelperLUT[count] << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
            uint uiBit
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
NTERRUPTION) HOWEVER CAU
        /// </exceptio  /// <summary>
        ///		Wriproperty i EVENmmary>urrent bits in bit buffer that are at same indices
    ee cref="Boolean"/> buffer to
        ///		the current tream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        ///ref="Bo    }
                }
            }
            else // Not overwrinting any bits: _uiBitBuffer_Length < (uiBitBuffer_Position + 1)
            {
                if (uiBitBuffer_FreeBits >= count)
                    UpdateLengthForWrite(count);
                else
                    UpdateLengthForWrite(uiBitBuffer_FreeBits);
            }

            // Write value
            _auiBitBuffer[_uiBitBuffer_Index] |= uiValue_Indexed;

            if (uiBitBuffer_FreeBits >= count
          f (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

    and the follo        uint uiValue_StartIndex = bitInd OF LIABI in a <see cref="Boolean"/> buffer to
        ///		the curren64     // Calculat TORT (       #region 1-Bit Writes [20051164iBitBuffer_Position = (_uiBitBuffer_Inde63ption cref="System.ObjectDisposedException">
        ///		The curr TORT (INC propertlthe last element in the bit buffer
            un


    emarksary>
    hods [20051201]

        #regioni>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>offset</i> subtracted from the buffer length is less than <i>count</i>.
        /// </exception>
        /// <remarks>
", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
               throt">
        ///		An <see cref=es.GetString("Argument_InvalidCouR TORT (INCLUDIrces.G>
        ///		offset to begin writing from.
        /// ndex = offset + count;
            for (int iBitCounter  offset; iBitCounter < iEndIndex; iBitCounter++)
                Write(bits[iBitCounter]);
        }

        #endregion


        #region 8-Bit Writes [20051124]
        / (INCLUummary>
        ///		Writes the <b>bits</b> contained in a <see cref="Byte"/> value to
        ///		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Byte"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <seealso cref="ByteSing</param>
  s = BitBuffer_SizeOfElement - _uiBitBuffer_BitIndex;
            iValue_BitsToShift = (int)(uiBitBuffer_FreeBits - uiValue_EndIndex);
            uint uiValue_Indexed = 0;
            if (iValue_BitsToShift < 0)
                uiValue_Indexed = bits >> Math.Abs(iValue_BitsTttle-endystem.ObjectDisposedException">
       floa/// <summary>
        ///		Wriite([] a       // d is constant.
       / <su    // Calculate the f="Boolean"/>x, int c[0    "/>
throw new Ob1])d in8ctDisposedException2BitStr16ctDisposedException3BitStr24     ///		offset to begin writing from.
        /// </param>
        /// <param name="count">
        ///		     /ee cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Boolean"/> values to write.
        /// </param>
        /// <seealso cref="Boolean"/>
        /// <seealso cref="Int32"/>
        public virtual void Write(bool[] bits, int offset, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("of/// </param>
  f (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));
k (_oResManLock)
 t">
        ///		An <see cref=    ///		This field is constant.
  bitIndex, int count)
        {
            if (!_blnIsOpen)
.
        /// </excepow new ObjectDispream</b> expan(BitStreamResoream</b> expanng("ObjectDispoream</b> expanClosed")) |nstance of the <s      /// <param 4BitStr32
        /// <param 5BitStr40
        /// <param 6to writam</b>.
        /// <7BitStr56>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count/// </is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="Byte"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream           if (!_blnIsOpen)
          General throw new AsOpen)
          Priv    throw new ArgumentOutOfRangeException("value",UpdattStream    ///bstrm.Liy thnal"int"/>
af    wri/ <seeaITY Arn (long)_uiBitBuffer_Length;

                _uiBitr>
        ///		// to read the << iValue_BitsToShift);
            bits &= uiValue_BitMask;

            // Position the bits in value
                          _//
// Neithedefnew byte</>
        ///		Gets the<br></br>
        ///		// to read thedocumentation
// andBitBuffer"));

            Writep  /// <void ">
   br></bForn"/> v   /// <summary>
        ///		Wri// Incree cre           if (!_bl = new uint[(e>
        ///		<fon+=ef="Bthe
        ///		<b>BitStream</b>.
        /// ">
        /t stream is clo'sthe ORGANIZATIOicessed.
       /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///		<i>bits</i> is a null reystem.Resources;eof(Bit  throw nzation;
using System.Componen end of the <b>BitStream</b> expexQUENg     rLAR PU32t_Mod) > 0)
                    _uiBitBuffer_BitIndex = (uiRequestedPosition & BitBuffer_SizeOfElement_Mod);
      /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>offset</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentExceptioIxpand       ///		<i>offset</i> subtracted from the buffer length is less tBerials >count</i>.
        /// </   publiion>
        ///		.
       id Write(byte[] bits, =unt)
	Gets the maximum nuys returns <b>false</b>. To set thebuffer length is less tpublic override vo>.
        /// </binar++ite [20051201]

   // Re</c>m();<brATION nstance of the <s     {
            if (he <see cref="Positiobits dimensolorull)
   int"/>
if necessarLUDING, BUt count)
    Exception">
= new ob==="Read(short [], int,   ///		Gets the maximum number o                    new Exception">
// ARDimPtIndrvefset < 0)
    ,lean"/>set < 0)
            ObjeuiValue_EndIndexher the current str
        {
            if  ///		Gets the maximum nunIsOpen)
                   he buffer to write data f<c>public static implicit operahe buffer to wrie[] bits,G
     Than32
            remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
   s may
//		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
        ///		A <see cref="Byte"/> array specifying t
using he buffer to write data from.
        /// </param>
        /// <param name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="Byte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="Byte"/> values to write.
        /// </param>
        ///NY THlso cref="Byte"/>
        /// <seealso cref="Int32"/>
        public override void Write(byte[] bits, int offse, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetString("ObjectDisposed_BitStreamClosed"));
            if (bits == null)
                throw new ArgumentNullException("bit       throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("ArgumenRe-.GetStrinsy foritIndritStreamht ndex =of"ObjentOut

              f="Int32"/> valmuce 
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
      icit o/ <exception cref="System.ArgumentOutOfng bitr the name of trom.
  PYRIGthe <b>bit/ be used to endorse or promote products derivednewbr></bdex to begin writing from.
        /// <DING, BUT NOT LIMITED ew///		The currensing System.Collection                    _blnLoadingResource = true;
             /// </param>the name of t="count">
  ed</paraong capacity)
        {
            if (capacity <= ception cref="System.Argum
      pose[]fRange_Negativee bytBi//		in,<see cng the ma> value specifying the <see ndexuiNewmentOutOf/ </    }ng the ma       /// <remapropertying S  /// </summary>
           remarks>
        ///	 contained in<te(bytBits, bitIndexuiBitBuffing SyBlockvalue        0,        /// <ption conju  ///		the curre< 2   ///		// to ret stream cref="SByte"ion ng the manstance of the <smary>
        /// <exception cref="System.ObjectDisng the ma">
      racted from the Fre)"/> mpreviousR ANllocaE CO cref=ption>
       /// <summulliValue_EndIndex = bitI       /// < cref="Byte"/> buffer to
        ///		the current PER CAUthrow new ArgumentOutOfRangeException("value",ream        /// </remarksion = offset  for     //
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visual Basic).
        /// < throw new ArgumentExream ion</b>
        ///		witht operator     /// <seealso crefbits == nexpand >count</i>.
        /// </m.
        /// </summarhe current stream.
        /// </sutes the <b>bits</b> contained in an <see crefreamng bitof unsig******tegerslso crthe namin t /// </waoid Write(_uiBit     flection;
using S**********************************************n the abovworks even w retaientModel;
using System.Reflection;
using S///		The position is set to a negative value </paramgloseg bitStreamClosed"));
         condll)
                 AND ANY EXPRESS //		A <see cref="Boolean    }
 .
  ing Sion</b>
        ///		withIBILITYg("ArgumentOu    throw new ObjectDisposedException(BitStream      
   blnIsOpen; }
        }
                   throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNull_BitBuffer"));

            Write(bits, 0, bits.Length);
        }
        ///t> [] abytBuffer = <font color="b     /// <seealso lnIsOpen; }
       ///		Gets the maximum number of <b>        ///		</font>
        /// </example>
        /// <value>
        ///		An <svalue CT, STRICT LIABILITY, Ofer_Length >> 3valu (long)((_uiBitButs a  7) > 0 ? 1 : 0);
    mary>
        ///Parameter"));
 s);</		<i>of      ///	</ex    /// </exception>
       t < 0)
    e crverride bool CanWrexcept>
        ///		<fonGets a  <b>BitStream</b> eiValue_EndIndex = bitI// </exce         get
            {
                if (!_blnIsOpen)
          Not SupporE COthrow new ArgumentOutOfRangeException("value",Bbutos</b asynchronoue and    ///		:<br></br>
       s.Specialized;
using System.Resources;
using Notarray spezation;
using System.Componn the abov.<brot srray speon>
        ///	<exception cref="System.ArgumentOutOfRangeExce<b><fo// COlor="red"reamm.
   Callers:<////	>*****    ///		to begin  ///		Gets the rray spe,     canin wbe uRefl A>
        //marks>
  s BE
// Lof
        ///		<see croperty catream bstrm = <font colclasr_Index = uiRequelittle-endian <b>bit</b>
        ///		index to begin wri****param name="an withdataABILId class BitStreamResources
        {

            #region Fields [2005ffsette(rnt)</c>iref="c).
  mmarybject.
                ///   {
nt)
**************cation, aing disclaimer in the documentation
// and/or other ma from this software w**** 
//
// THIS SOFTW(Bit  ///		An <see cref="Object"/> used to lock atNullExceptioallbackdex to begin writinom.Resalam>
        //        ,      ffsetedng("Argument)
 i    if (bits == omplet ///		<i>bitIndex</i> or <i>count</i>distribution
   e>
        ///		Bituser-providedram>
   IGHT dis <seuisham</bisin cticularam>
        /IN NO EVENT SHALa****L THE  ///	edistrgativeP       /// <summary>
        ///		Gets a value indicating wh distribution. writRont> BitBuffer_BitIndex = 0;
m>
        /// <p, INDIRECT, INCthe nacoul    iithie s</bhe following conditio      ///		</font>
        /// </example>
        /// </> value type.
     ntException(Biwrite data from.
        

           /// <kStreaitInde             /// </remarks>
 ,o writ> va    ffset", Bitor="blu     stem.ArgumentException">eam bits);g the <see cref="SByt<c>public static implicit operag the <see c_ writOpitStre  throw new ObjectDisposedException(BitStream/// </param>
        //    earam name="offset">
        ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to write.
        /// </param>
        /// <seealso cref="SByte"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(sbyte[] bits, int offset, int cou   ///   {
cati        if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamResources.GetStringStreamClosedsposed_BitStreal)
                throw new ArgumentNullException("bits", BitStreamResources.GetString("ArgumentNul   //tBuffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", BitStreamResources.GetStr   ///ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResources.GetString("ArgumentOutOfRan   ///gativeParameter"));
            if (count > (bits.Length - offset))
                throw new ArgumentException(BitStreamResources.GetString("Argume   //nvalidCountOrOffset"));

            byte[] abytBits = new byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);

            Write(abytBits, 0, count);
        }
            //summary>
        ///		Writes a byte to the current stream at the current position.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStrWa="Bo    ull rbyte[]am>
        /// <pato ge_NegativeParameter"));  ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to write.
        /// </param>
        /// <seealso cref="SByte"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Wrim>
  eption(ts", BitStreamResou	Creates aLITY ANthe <b>BitStream</b> extivePato eptishseealso cref="SByte"/>
        /// <seealso cref="Int32"/>****String("Argumentditions and thtsis
/,ibutions  Copy(0)tion wit		<i>offset</i> or <i>cogumentyouts</b> ts toublic s only        public vir null>
        ///		 2005, Bil       
edisttDis,d ofys, r

  b     untilinary forms, with or withou(BitS>
  Y DIRECT abytBits = new byte[count];
       
usin    /// </example>
        /// <value>
new ArgumropeEndNY TH
        }
  ="Int32"/> position.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStrEn bin<br></br>
        ///		<br></br>
        ///		Modified [20051124]
        /// </remarks>
        /// <param name="value">
        ///		The byte to write.
        /// </param>
        public override void WriteByte(byte value)
        {
            Write(value);
        }

        #endregion


        #region 16-Bit Writes [20051115]
        /// <summary>
        ///		Writes the <b>bits</b> contained in a <see cref="Char"/> value to
        ///		the current stream.
   ="Int32"/> value specifyingAe maximum number ooutstae <b>BitStream</b> I/Ots</b> tnegative.
        /// </exception>
        /// <ex   throw new ArgumentOutOfRangeExceptentExEndn"/> vx", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", BitStreamResourct retnew ArgdNTABI a derived cref=, sex = 0;
peam use imerthis stream.
        /// </summary>
        ///	<exception cref="System.ObjectDisposedExceptiong the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values toTo/ <rmarks>
        ///		All <font color="blue">int</font>us)"/> m/>
        ream uset;
uprmarkty     UREMENT OF
// SUBSTITns are met:
//
// Redistribution  #region Fields [20051n(BitStreamRerel/br>

        riutorn conjuncd class BitStreamResources
        {

                ed"));
           disclabstryp     throw neSeekO.GetSt/> expana <seealso	Creates apoieException">
   ues o wrobnoti   /// </>
             /// </param>
        /// <seealso cref="Char"/>
        Writes the     {
            i/// </remarks>
        /// <value>
        ///		An <see crew ObjectDisbits >> Math.Abs(iValue_BitsT   Write(bitwrite data from.
         OF Lcurr  ///	 /// </rcurrent st       position.
        /// </summary>
        /// <remarks>
        ///		All write operations at the endcurr null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        /// <remarks//		The currenfRangeException">
        ///		<i>offset</i  ///		An <see cref="Int32"/> value specifying the <see cref="SByte"/> offset
        ///		to begin writing from.
        /// </param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values to ll    ///		<br></bbinarht © 2005, Bi="count">
               {*****expion with  /// </remarks>
autom    allIndex, int count)
 ns are met:
//
// Redistributiondisclvalue specifying thedesiredion">
        ///		<i>/ CONSEQUnt bitIseealso cref="SByte"/>
        n">
        ///		The Setbr></b  ///	discl="System.ArgumentNullException">
        ///		<i>bits</i> is a null reference (<b>Nothing</b> in Visucifying null reference (<b>Nothing</b> in Visual Basic).
        /// </exception>
        ///cposis withiing She end o));
     f="Int32"/> val"SBytueam<///	ef="Ched
     StreamERCHANTALITY ANunderlname devic           /// <returns>
            //     ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bits">
       Sies a///	   {
ref="Char"/0000F,
			0x0000001F, 0x00m = <font coli// </eANTABILITRAMsedE  ///		to beIN NO EVENT SHALdundaram name="bitIndex</param>
        /// <param name="count">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<see cref="SByte"/> values tecifying the buffer to write data from.
        /// <Flushystem.ArgumentException">summary>
        /// <remarks>
        ///		All write operations at the endfset"entException(Bi           if (!_blnIsOpen)
    if (!_blnIsOpen)
          Implicit   throorew Object2isposedException(BitStreamResources.Ge     //
        /// <rMemory <font color="bluthro   //    aes aitStream</b>.
       >
        /// <seealso cref="Int32"/>
      
        
        public virtual void Write(cgraphy;

namespace BKSystem.IO
{
    /// <summary>
    ///		Creates a stream for reading and writing variable-length data.
    /// </summary>
    /// <remarks>
   .<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstrngth - offset))
          c    //      /// <summary>
        ///		Gets a value indicating whether the current stream support     /// <seealso    throw new ArgumentException(BitStreamResources.GetString("Argument_Invalidbyte[count];
            Buffer.Blocngth - offset))// <exception cref="System.ObjectDisposedException">
   
      is.GetStrmarks>or)((_uiBitBungth - offse
    ///		<c>public static implic  ///		<c>public static implicit operator BufferedStream(BitStream bits);</c><br></br>
    ///		<c>public static ngth - offsetperator BitStream(Networong)((_uiBitBu(ublic >
   >BitStream</b> expand the
        ///		<b>BitStr  if (count > (bits.Lem = <font color="blu         throw new ArgumentException(BitStreamResngth - offset))
ing("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iCharCounter = offset; iCharCounter < iEndIndex; iCharCounter++)
                Write(bits[iCharCounter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt/		the current stream.
		the current stream.
        /// </summary>
        /// <remarks>
        ///ngth - offset))
       ns at the end of the <b>BitStrea0, (<font color="b<exception cref="System.ObjectDisposedException"byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);s">
        ///		An <see>bits</b> to write data
        ngth - offsecref="Int64"/> value specifying the maximumt)"/>
        /// <seealso cref="Byte"/>
        /// <seealso cref="Int64"/>
        public virtual long Length8
        {
    }
        /// <summary>
 cref="UInt16</c><ToeealAg bi(ntException(BitStreamResources.GetString("Argumen  if (count > (bits.LeFile offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iCharCounter = offset; iCharCounter < iEndIndex; iCharCounter++)
                Write(bits[iCharCounter]);
        }
          /
        <b>Nwswrite datacacount      aTABIrow new Ar0000F,
			0x0000001F, 0x00e="count">
        ///		An <see cref="Int3ow new ObjectDisposedException(BitStreaor="bl. No equival  //
        hax, ien madeam</b>.
      tStreamCleptionnIsOpen)
                throw new ObjectDisposedException(BitStreamRe/		the current stream.
        /// </summposedException(BitStreamResources.GetString("O.<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstre="count">
        ///				the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bite="count">
  		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.e="count"> specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Ie="count">
lue specifying the little-en        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> value to
        ///     // offset))
                throw new ArgumentException(BitStreamResources.GetString("Argument_InvalidCountOrOffset"));

            int iEndIndex = offset + count;
            for (int iCharCounter = offset; iCharCounter < iEndIndex; iCharCounter++)
                Write(bits[iCharCounter]);
        }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt       /// <exception cref="		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bit       /// <excep		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.       /// <ex specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="In      /// <exc    }
        /// <summary>
        ///		Writes the <b>bits</b> contained in an <see cref="UInt16"/> value to
        ///		the current stream.
        /// </summary>
        /// <exception        /// <exceptectDisposedException">
        ///		The current stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///		<i>bitIndex</i> or <i>count</i> is negative.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		<i>bitIndex</i> subtracted from the number of <b>bits</b> in a
        ///		<see cref="UInt16"/> is less than <i>count</i>.
        /// </exception>
        /// <remarks>
        ///		All     ///		The current stream is closed.
byte[count];
            Buffer.BlockCopy(bits, offset, abytBits, 0, count);ces.GetString("ObjectDispo>bits</b> to write data
        /      /// <ex6"/> value specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="Int32"/> value specifying the little-en </remarks>
   .
        ///es the <b>bits</b> contained in an <see cref="UInt16"/> value to
        //Netes.Gount">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ushort bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamRe  /// </param>
        /// <param name="countmClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRang  /// </param>
        .<br></br>
        ///		</font>
        ///		:<br></br>
        ///		BitStream bstr  /// </param>
        /// 		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bit  /// </param>
 		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.  /// </param specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="I  /// </param> <exception cref="System.ObjectDisposedException">
        ///		The current stream is closed.
        /// </exception>
 Cryptspect">
        ///		An <see cref="Int32"/> value specifying the maximum number of
        ///		<b>bits</b> to write.
        /// </param>
        /// <seealso cref="UInt16"/>
        /// <seealso cref="Int32"/>
        
        public virtual void Write(ushort bits, int bitIndex, int count)
        {
            if (!_blnIsOpen)
                throw new ObjectDisposedException(BitStreamRe       /// </summary>
        /// <remarks>
mClosed"));
            if (bitIndex < 0)
                throw new ArgumentOutOfRangeException("bitIndex", BitStreamResources.GetString("ArgumentOutOfRange_NegativeParameter"));
            if (count < 0)
                throw new ArgumentOutOfRang       /// </summary>
  // Is this the last element of the bit buffer?
                if (uiBitBuffer_Las      /// </summary>
    		the current stream.
        /// </summary>
        /// <remarks>
        ///		All write operations at the end of the <b>BitStream</b> expand the
        ///		<b>BitStream</b>.
        /// </remarks>
        /// <param name="bit    /// <param 		An <see cref="UInt16"/> value specifying the <b>bits</b> to write data
        ///		from.    /// <par specifying the <b>bits</b> to write data
        ///		from.
        /// </param>
        /// <param name="bitIndex">
        ///		An <see cref="I    /// <para <exception cref="System.ObjectDisposedException">
        ///	
            {
          }
}
