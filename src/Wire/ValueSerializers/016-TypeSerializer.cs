﻿// -----------------------------------------------------------------------
//   <copyright file="TypeSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class TypeSerializer : ValueSerializer
    {
        public const byte Manifest = 16;
        public static readonly TypeSerializer Instance = new TypeSerializer();

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            if (session.ShouldWriteTypeManifest(TypeEx.RuntimeType, out var typeIdentifier))
            {
                var span = stream.GetSpan(1);
                span[0] = Manifest;
                stream.Advance(1);
            }
            else
            {
                byte[] source = {ObjectSerializer.ManifestIndex};
                var destination = stream.GetSpan(source.Length);
                source.CopyTo(destination);
                stream.Advance(source.Length);
                UInt16Serializer.WriteValueImpl(stream, typeIdentifier);
            }
        }

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            if (value == null)
            {
                StringSerializer.WriteValueImpl(stream, null, session);
            }
            else
            {
                var type = (Type) value;
                if (session.Serializer.Options.PreserveObjectReferences &&
                    session.TryGetObjectId(type, out var existingId))
                {
                    ObjectReferenceSerializer.WriteManifestImpl(stream, session);
                    ObjectReferenceSerializer.WriteValueImpl(stream, existingId, session);
                }
                else
                {
                    if (session.Serializer.Options.PreserveObjectReferences) session.TrackSerializedObject(type);
                    //type was not written before, add it to the tacked object list
                    var name = type.GetShortAssemblyQualifiedName();
                    StringSerializer.WriteValueImpl(stream, name, session);
                }
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var shortname = stream.ReadString(session);
            if (shortname == null) return null;

            var name = TypeEx.ToQualifiedAssemblyName(shortname);
            var type = Type.GetType(name, true);

            //add the deserialized type to lookup
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackDeserializedObject(type);
            return type;
        }

        public override Type GetElementType()
        {
            return typeof(Type);
        }
    }
}