﻿// -----------------------------------------------------------------------
//   <copyright file="SystemObjectSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class SystemObjectSerializer : ValueSerializer
    {
        public const byte Manifest = 1;
        public static readonly SystemObjectSerializer Instance = new SystemObjectSerializer();

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var span = stream.GetSpan(1);
            span[0] = Manifest;
            stream.Advance(1);
        }

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return new object();
        }

        public override Type GetElementType()
        {
            return typeof(object);
        }
    }
}