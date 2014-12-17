using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LessGravity.Common
{
    public partial class DataStream
    {
        private Encoding _stringEncoding;

        public int GetVariantIntLength(int value)
        {
            var internalValue = (uint)value;
            var length = 0;
            while (true)
            {
                length++;
                if ((internalValue & 0xFFFFFF80u) == 0)
                {
                    break;
                }
                internalValue >>= 7;
            }
            return length;
        }


        #region Read

        public byte ReadUInt8()
        {
            var value = BaseStream.ReadByte();
            if (value == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)value;
        }

        public sbyte ReadInt8()
        {
            return (sbyte)ReadUInt8();
        }

        public string ReadString()
        {
            var length = ReadVariableInt();
            if (length == 0)
            {
                return string.Empty;
            }
            var data = ReadUInt8Array(length);
            return _stringEncoding.GetString(data);
        }

        public Int32 ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public Int64 ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public byte[] ReadUInt8Array(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be bigger than 0.", "length");
            }
            var result = new byte[length];
            var bytesRead = length;
            while (true)
            {
                bytesRead -= Read(result, length - bytesRead, bytesRead);
                if (bytesRead == 0)
                {
                    break;
                }
                Thread.Sleep(1);
            }
            return result;
        }

        public UInt32 ReadUInt32()
        {
            return (uint)(
                (ReadUInt8() << 24) |
                (ReadUInt8() << 16) |
                (ReadUInt8() << 8) |
                 ReadUInt8());
        }

        public UInt64 ReadUInt64()
        {
            return unchecked(
                   ((UInt64)ReadUInt8() << 56) |
                   ((UInt64)ReadUInt8() << 48) |
                   ((UInt64)ReadUInt8() << 40) |
                   ((UInt64)ReadUInt8() << 32) |
                   ((UInt64)ReadUInt8() << 24) |
                   ((UInt64)ReadUInt8() << 16) |
                   ((UInt64)ReadUInt8() << 8) |
                    ReadUInt8());
        }

        public int ReadVariableInt()
        {
            int length;
            return ReadVariableInt(out length);
        }

        public int ReadVariableInt(out int length)
        {
            var result = 0U;
            length = 0;
            while (true)
            {
                var current = ReadUInt8();
                result |= (current & 0x7FU) << length++ * 7;
                if (length > 5)
                {
                    throw new InvalidDataException("Variable Integer may not be longer than 28bits.");
                }
                if ((current & 0x80) != 0x80)
                {
                    break;
                }
            }
            return (int)result;
           
        }

        #endregion Read
        #region Write

        public void WriteString(string value)
        {
            WriteVariableInt(_stringEncoding.GetByteCount(value));
            if (value.Length > 0)
            {
                WriteUInt8Array(_stringEncoding.GetBytes(value));
            }
        }

        public void WriteUInt8(byte value)
        {
            WriteByte(value);
        }

        public void WriteUInt8Array(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        public void WriteInt32(Int32 value)
        {
            WriteUInt32((UInt32)value);
        }

        public void WriteInt64(Int64 value)
        {
            WriteUInt64((UInt64)value);
        }

        public void WriteUInt32(UInt32 value)
        {
            Write(new[]
            {
                (byte) ((value & 0xFF000000) >> 24),
                (byte) ((value & 0xFF0000) >> 16),
                (byte) ((value & 0xFF00) >> 8),
                (byte) (value & 0xFF)
            }, 0, 4);
        }

        public void WriteUInt64(UInt64 value)
        {
            Write(new[]
            {
                (byte)((value & 0xFF00000000000000) >> 56),
                (byte)((value & 0xFF000000000000) >> 48),
                (byte)((value & 0xFF0000000000) >> 40),
                (byte)((value & 0xFF00000000) >> 32),
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 8);
        }

        public void WriteVariableInt(int value, out int length)
        {
            var internalValue = (uint) value;
            length = 0;
            while (true)
            {
                length++;
                if ((internalValue & 0xFFFFFF80u) == 0)
                {
                    WriteUInt8((byte) value);
                    break;
                }
                WriteUInt8((byte) (value & 0x7F | 0x80));
                value >>= 7;
            }
        }

        public void WriteVariableInt(int value)
        {
            int length;
            WriteVariableInt(value, out length);
        }


        #endregion Write

    }
}
