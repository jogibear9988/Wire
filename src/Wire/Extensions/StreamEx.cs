﻿// -----------------------------------------------------------------------
//   <copyright file="StreamEx.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using JetBrains.Annotations;

namespace Wire.Extensions
{
    [PublicAPI]
    public static class StreamEx
    {
        public static uint ReadVarint32(this Stream stream)
        {
            var result = 0;
            var offset = 0;

            for (; offset < 32; offset += 7)
            {
                var b = stream.ReadByte();
                if (b == -1) throw new EndOfStreamException();

                result |= (b & 0x7f) << offset;

                if ((b & 0x80) == 0) return (uint) result;
            }

            throw new InvalidDataException();
        }

        public static void WriteVarint32(this Stream stream, uint value)
        {
            for (; value >= 0x80u; value >>= 7) stream.WriteByte((byte) (value | 0x80u));

            stream.WriteByte((byte) value);
        }

        public static ulong ReadVarint64(this Stream stream)
        {
            long result = 0;
            var offset = 0;

            for (; offset < 64; offset += 7)
            {
                var b = stream.ReadByte();
                if (b == -1) throw new EndOfStreamException();

                result |= (long) (b & 0x7f) << offset;

                if ((b & 0x80) == 0) return (ulong) result;
            }

            throw new InvalidDataException();
        }

        public static void WriteVarint64(this Stream stream, ulong value)
        {
            for (; value >= 0x80u; value >>= 7) stream.WriteByte((byte) (value | 0x80u));

            stream.WriteByte((byte) value);
        }

        public static uint ReadUInt16(this Stream self, DeserializerSession session)
        {
            var buffer = session.GetBuffer(2);
            self.Read(buffer, 0, 2);
            var res = BitConverter.ToUInt16(buffer, 0);
            return res;
        }

        public static int ReadInt32(this Stream self, DeserializerSession session)
        {
            var buffer = session.GetBuffer(4);
            self.Read(buffer, 0, 4);
            var res = BitConverter.ToInt32(buffer, 0);
            return res;
        }

        public static byte[] ReadLengthEncodedByteArray(this Stream self, DeserializerSession session)
        {
            var length = self.ReadInt32(session);
            var buffer = new byte[length];
            self.Read(buffer, 0, length);
            return buffer;
        }

       

        public static object ReadObject(this Stream stream, DeserializerSession session)
        {
            var s = session.Serializer.GetDeserializerByManifest(stream, session);
            var value = s.ReadValue(stream, session); //read the element value
            return value!;
        }
        
        public static T ReadObjectTyped<T>(this Stream stream, DeserializerSession session)
        {
            var s = session.Serializer.GetDeserializerByManifest(stream, session);
            var value = s.ReadValue(stream, session); //read the element value
            return (T)value!;
        }

        public static string? ReadString(this Stream stream, DeserializerSession session)
        {
            var length = stream.ReadByte();
            switch (length)
            {
                case 0:
                    return null;
                case 255:
                    length = stream.ReadInt32(session);
                    break;
                default:
                    length--;
                    break;
            }

            var buffer = session.GetBuffer(length);

            stream.Read(buffer, 0, length);
            var res = StringEx.FromUtf8Bytes(buffer, 0, length);
            return res;
        }
    }
}