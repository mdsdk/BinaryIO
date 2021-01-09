﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace MDSDK.BinaryIO
{
    public sealed class BinaryStreamReader
    {
        public Stream Stream { get; }

        public ByteOrder ByteOrder { get; }

        private readonly byte[] _buffer;

        private int _bufferReadPointer;

        private int _bufferedDataLength;

        private long _position;

        private long _endPosition;

        public BinaryStreamReader(Stream stream, ByteOrder byteOrder)
        {
            Stream = stream;

            ByteOrder = byteOrder;

            _buffer = new byte[4096];
            _bufferReadPointer = 0;
            _bufferedDataLength = 0;

            _position = 0;
            _endPosition = stream.CanSeek ? stream.Length : long.MaxValue;
        }

        public long Position
        {
            get { return _position; }
            private set
            {
                if (value > _endPosition)
                {
                    throw new IOException($"Attempt to consume {value - _endPosition} more bytes than available");
                }
                _position = value;
            }
        }

        public long BytesRemaining => _endPosition - _position;

        public bool AtEnd => _position == _endPosition;

        private void BeginReadFromBuffer(int count)
        {
            if (_bufferedDataLength < count)
            {
                if (_bufferReadPointer + count > _buffer.Length)
                {
                    _buffer.AsSpan(_bufferReadPointer, _bufferedDataLength).CopyTo(_buffer);
                    _bufferReadPointer = 0;
                }

                do
                {
                    var bytesRead = Stream.Read(_buffer.AsSpan(_bufferReadPointer + _bufferedDataLength));
                    if (bytesRead == 0)
                    {
                        throw new IOException("Unexpected end of stream");
                    }
                    _bufferedDataLength += bytesRead;
                }
                while (_bufferedDataLength < count);
            }
        }

        private void EndReadFromBuffer(int count)
        {
            _bufferedDataLength -= count;
            _bufferReadPointer = (_bufferedDataLength == 0) ? 0 : _bufferReadPointer + count;
        }

        public byte ReadByte()
        {
            Position++;
            BeginReadFromBuffer(1);
            var datum = _buffer[_bufferReadPointer];
            EndReadFromBuffer(1);
            return datum;
        }

        public sbyte ReadSByte() => (sbyte)ReadByte();

        public void Read(out byte datum) => datum = ReadByte();

        public void Read(out sbyte datum) => datum = ReadSByte();

        private Span<byte> GetBufferReadSpan(int count)
        {
            BeginReadFromBuffer(count);
            var readSpan = _buffer.AsSpan(_bufferReadPointer, count);
            EndReadFromBuffer(count);
            return readSpan;
        }

        public T ReadPrimitive<T>() where T : struct
        {
            Debug.Assert(BinaryIOUtils.IsPrimitiveType(typeof(T)));

            var datumSize = Unsafe.SizeOf<T>();

            Debug.Assert((datumSize > 1) && (datumSize <= 8));

            Position += datumSize;

            var bufferReadSpan = GetBufferReadSpan(datumSize);

            if (ByteOrder != BinaryIOUtils.NativeByteOrder)
            {
                bufferReadSpan.Reverse();
            }

            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(bufferReadSpan));
        }

        public void Read(out short datum) => datum = ReadPrimitive<short>();

        public void Read(out ushort datum) => datum = ReadPrimitive<ushort>();

        public void Read(out int datum) => datum = ReadPrimitive<int>();

        public void Read(out uint datum) => datum = ReadPrimitive<uint>();

        public void Read(out long datum) => datum = ReadPrimitive<long>();

        public void Read(out ulong datum) => datum = ReadPrimitive<ulong>();

        public void Read(out float datum) => datum = ReadPrimitive<float>();

        public void Read(out double datum) => datum = ReadPrimitive<double>();

        public void Read(Span<byte> data)
        {
            Position += data.Length;

            if (data.Length <= _bufferedDataLength)
            {
                var bufferReadSpan = GetBufferReadSpan(data.Length);
                bufferReadSpan.CopyTo(data);
            }
            else
            {
                if (_bufferedDataLength > 0)
                {
                    var n = _bufferedDataLength;
                    var bufferReadSpan = GetBufferReadSpan(n);
                    bufferReadSpan.CopyTo(data);
                    data = data[n..];
                }

                while (data.Length > 0)
                {
                    var n = Stream.Read(data);
                    if (n == 0)
                    {
                        throw new IOException("Unexpected end of stream");
                    }
                    data = data[n..];
                }
            }
        }

        public void Read(Span<short> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<ushort> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<int> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<uint> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<long> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<ulong> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<float> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void Read(Span<double> data)
        {
            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes(data));
            }
            else
            {
                for (var i = 0; i < data.Length; i++)
                {
                    Read(out data[i]);
                }
            }
        }

        public void ReadNonPrimitive<T>(Span<T> data) where T : struct
        {
            Debug.Assert(!BinaryIOUtils.IsPrimitiveType(typeof(T)));

            if (ByteOrder != BinaryIOUtils.NativeByteOrder)
            {
                throw new NotSupportedException("Non-primitive data types can only be read in native byte order");
            }

            Read(MemoryMarshal.AsBytes(data));
        }

        public void ReadNonPrimitive<T>(ref T datum) where T : struct
        {
            ReadNonPrimitive(MemoryMarshal.CreateSpan(ref datum, 1));
        }

        public T ReadNonPrimitive<T>() where T : struct
        {
            var datum = default(T);
            ReadNonPrimitive(ref datum);
            return datum;
        }

        public T[] ReadArray<T>(int length) where T : struct
        {
            var array = new T[length];

            if (ByteOrder == BinaryIOUtils.NativeByteOrder)
            {
                Read(MemoryMarshal.AsBytes<T>(array));
            }
            else if (BinaryIOUtils.IsPrimitiveType(typeof(T)))
            {
                for (var i = 0; i < length; i++)
                {
                    array[i] = ReadPrimitive<T>();
                }
            }
            else
            {
                throw new NotSupportedException("Non-primitive data types can only be read in native byte order");
            }

            return array;
        }

        private class Disposable : IDisposable
        {
            private readonly Action _dispose;

            public Disposable(Action dispose) => _dispose = dispose;

            public void Dispose() => _dispose.Invoke();
        }

        public IDisposable Window(long length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var newEndPosition = Position + length;
            if (newEndPosition > _endPosition)
            {
                throw new IOException($"Window exceeds stream by {newEndPosition - _endPosition} bytes");
            }

            var originalEndPosition = _endPosition;

            _endPosition = newEndPosition;

            return new Disposable(() => _endPosition = originalEndPosition);
        }

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            Read(bytes);
            return bytes;
        }

        public byte[] ReadRemainingBytes()
        {
            return ReadBytes(checked((int)BytesRemaining));
        }

        public void SkipBytes(long count)
        {
            Position += count;

            if (count <= _bufferedDataLength)
            {
                EndReadFromBuffer((int)count);
            }
            else
            {
                count -= _bufferedDataLength;
                EndReadFromBuffer(_bufferedDataLength);

                if (Stream.CanSeek)
                {
                    Stream.Seek(count, SeekOrigin.Current);
                }
                else
                {
                    while (count > 0)
                    {
                        var n = (int)Math.Min(count, _buffer.Length);
                        var bytesRead = Stream.Read(_buffer, 0, n);
                        if (bytesRead == 0)
                        {
                            throw new IOException("Unexpected end of stream");
                        }
                        count -= bytesRead;
                    }
                }
            }
        }

        public void SkipRemainingBytes()
        {
            SkipBytes(BytesRemaining);
        }
    }
}
