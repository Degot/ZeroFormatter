﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter.DotNetCore.Internal;
using ZeroFormatter.DotNetCore.Segments;

namespace ZeroFormatter.DotNetCore.Formatters
{
    // Layout: FixedSize -> [count:int][t format...] -> if count = -1 is null
    // Layout: VariableSize -> [int byteSize][count:int][elementOffset:int...][t format...] -> if byteSize = -1 is null
    internal class ListFormatter<T> : Formatter<IList<T>>
    {
        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, IList<T> value)
        {
            if (value == null)
            {
                BinaryUtil.WriteInt32(ref bytes, offset, -1);
                return 4;
            }

            var segment = value as IZeroFormatterSegment;
            if (segment != null)
            {
                return segment.Serialize(ref bytes, offset);
            }

            var formatter = Formatter<T>.Default;
            var length = formatter.GetLength();
            if (length != null)
            {
                // FixedSize Array
                var writeSize = value.Count * length.Value + 4;
                if (bytes == null)
                {
                    bytes = new byte[writeSize];
                }

                offset += BinaryUtil.WriteInt32(ref bytes, offset, value.Count);
                foreach (var item in value)
                {
                    offset += formatter.Serialize(ref bytes, offset, item);
                }
                return writeSize;
            }
            else
            {
                var startoffset = offset;

                var count = 0;
                offset = (startoffset + 8) + (value.Count * 4);
                foreach (var item in value)
                {
                    var size = formatter.Serialize(ref bytes, offset, item);
                    BinaryUtil.WriteInt32(ref bytes, (startoffset + 8) + count * 4, offset);
                    offset += size;
                    count++;
                }
                BinaryUtil.WriteInt32(ref bytes, startoffset + 4, value.Count);

                var totalBytes = offset - startoffset;
                BinaryUtil.WriteInt32(ref bytes, startoffset, totalBytes);

                return totalBytes;
            }
        }

        public override IList<T> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            var formatter = Formatter<T>.Default;
            var length = formatter.GetLength();
            if (length != null)
            {
                return FixedListSegment<T>.Create(tracker, bytes, offset, out byteSize);
            }
            else
            {
                return VariableListSegment<T>.Create(tracker, bytes, offset, out byteSize);
            }
        }
    }

    internal class ReadOnlyListFormatter<T> : Formatter<IReadOnlyList<T>>
    {
        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, IReadOnlyList<T> value)
        {
            if (value == null)
            {
                BinaryUtil.WriteInt32(ref bytes, offset, -1);
                return 4;
            }

            var segment = value as IZeroFormatterSegment;
            if (segment != null)
            {
                return segment.Serialize(ref bytes, offset);
            }

            var formatter = Formatter<T>.Default;
            var length = formatter.GetLength();
            if (length != null)
            {
                // FixedSize Array
                var writeSize = value.Count * length.Value + 4;
                if (bytes == null)
                {
                    bytes = new byte[writeSize];
                }

                offset += BinaryUtil.WriteInt32(ref bytes, offset, value.Count);
                foreach (var item in value)
                {
                    offset += formatter.Serialize(ref bytes, offset, item);
                }
                return writeSize;
            }
            else
            {
                var startoffset = offset;

                var count = 0;
                offset = (startoffset + 8) + (value.Count * 4);
                foreach (var item in value)
                {
                    var size = formatter.Serialize(ref bytes, offset, item);
                    BinaryUtil.WriteInt32(ref bytes, (startoffset + 8) + count * 4, offset);
                    offset += size;
                    count++;
                }
                BinaryUtil.WriteInt32(ref bytes, startoffset + 4, value.Count);

                var totalBytes = offset - startoffset;
                BinaryUtil.WriteInt32(ref bytes, startoffset, totalBytes);

                return totalBytes;
            }
        }

        public override IReadOnlyList<T> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            var formatter = Formatter<T>.Default;
            var length = formatter.GetLength();
            if (length != null)
            {
                return FixedListSegment<T>.Create(tracker, bytes, offset, out byteSize);
            }
            else
            {
                return VariableListSegment<T>.Create(tracker, bytes, offset, out byteSize);
            }
        }
    }
}