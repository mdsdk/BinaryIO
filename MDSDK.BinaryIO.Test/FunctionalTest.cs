﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;

namespace MDSDK.BinaryIO.Test
{
    class FunctionalTest
    {
        private const byte TestByte = 123;
        private const short TestShort = -12345;
        private const ushort TestUShort = 54321;
        private const int TestInt = -123456789;
        private const uint TestUInt = 987654321;
        private const long TestLong = -12345678987654321;
        private const ulong TestULong = 98765432123456789;
        private const float TestFloat = -123456789876.54321f;
        private const double TestDouble = 98765.432123456789;

        private delegate void WriteSpan<T>(ReadOnlySpan<T> data);

        private static void TestWriteArray<T>(int n, T testValue, WriteSpan<T> writeSpan)
        {
            var array = new T[n];
            for (var i = 0; i < n; i++)
            {
                array[i] = testValue;
            }
            writeSpan(array);
        }

        private delegate T Read<T>();

        private static void TestRead<T>(Read<T> read, T testValue) where T : struct, IEquatable<T>
        {
            var datum = read();
            Trace.Assert(datum.Equals(testValue));
        }

        private delegate void ReadSpan<T>(Span<T> data);

        private static void TestReadArray<T>(int n, ReadSpan<T> readSpan, T testValue) where T : struct, IEquatable<T>
        {
            var array = new T[n];
            readSpan(array);
            for (var i = 0; i < n; i++)
            {
                Trace.Assert(array[i].Equals(testValue));
            }
        }

        private static void TestWrite(ByteOrder byteOrder, Stream stream)
        {
            var output = new BinaryStreamWriter(byteOrder, stream);

            Trace.Assert(stream.Position == 0);

            output.Write(TestByte);
            output.Flush(FlushMode.Shallow);

            Trace.Assert(stream.Position == 1);

            output.Write(TestShort);
            output.Write(TestUShort);

            Trace.Assert(stream.Position == 1);

            output.Flush(FlushMode.Shallow);

            Trace.Assert(stream.Position == 5);

            output.Write(TestInt);
            output.Write(TestUInt);
            output.Write(TestLong);
            output.Write(TestULong);
            output.Write(TestFloat);
            output.Write(TestDouble);

            Trace.Assert(stream.Position == 5);

            output.Flush(FlushMode.Shallow);

            Trace.Assert(stream.Position == 41);

            TestWriteArray(9 * ushort.MaxValue, TestByte, output.WriteBytes);
            TestWriteArray(8 * ushort.MaxValue, TestShort, output.Write);
            TestWriteArray(7 * ushort.MaxValue, TestUShort, output.Write);
            TestWriteArray(6 * ushort.MaxValue, TestInt, output.Write);
            TestWriteArray(5 * ushort.MaxValue, TestUInt, output.Write);
            TestWriteArray(4 * ushort.MaxValue, TestLong, output.Write);
            TestWriteArray(3 * ushort.MaxValue, TestULong, output.Write);
            TestWriteArray(2 * ushort.MaxValue, TestFloat, output.Write);
            TestWriteArray(1 * ushort.MaxValue, TestDouble, output.Write);

            output.Flush(FlushMode.Deep);
        }

        private static void TestRead(ByteOrder byteOrder, Stream stream)
        {
            var input = new BinaryStreamReader(byteOrder, stream);

            TestRead(input.ReadByte, TestByte);
            TestRead(input.Read<Int16>, TestShort);
            TestRead(input.Read<UInt16>, TestUShort);
            TestRead(input.Read<Int32>, TestInt);
            TestRead(input.Read<UInt32>, TestUInt);
            TestRead(input.Read<Int64>, TestLong);
            TestRead(input.Read<UInt64>, TestULong);
            TestRead(input.Read<Single>, TestFloat);
            TestRead(input.Read<Double>, TestDouble);

            TestReadArray(9 * ushort.MaxValue, input.Read, TestByte);
            TestReadArray(8 * ushort.MaxValue, input.Read, TestShort);
            TestReadArray(7 * ushort.MaxValue, input.Read, TestUShort);
            TestReadArray(6 * ushort.MaxValue, input.Read, TestInt);
            TestReadArray(5 * ushort.MaxValue, input.Read, TestUInt);
            TestReadArray(4 * ushort.MaxValue, input.Read, TestLong);
            TestReadArray(3 * ushort.MaxValue, input.Read, TestULong);
            TestReadArray(2 * ushort.MaxValue, input.Read, TestFloat);
            TestReadArray(1 * ushort.MaxValue, input.Read, TestDouble);
        }

        private static void Test(ByteOrder byteOrder)
        {
            Console.WriteLine("Testing " + byteOrder);
            using var stream = new MemoryStream();

            TestWrite(byteOrder, stream);
            Console.WriteLine("    Write OK");

            stream.Seek(0, SeekOrigin.Begin);
            TestRead(byteOrder, stream);
            Console.WriteLine("    Read OK");
        }

        public static void Run()
        {
            Test(ByteOrder.LittleEndian);
            Test(ByteOrder.BigEndian);
        }
    }
}
