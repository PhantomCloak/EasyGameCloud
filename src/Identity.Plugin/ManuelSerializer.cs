using System;
using System.Text;

namespace Identity.Plugin
{
    public class ManuelSerializer
    {
        private readonly byte[] _buffer;

        private int ReadOffset { get; set; }
        private int WriteOffset { get; set; }

        public ManuelSerializer(byte[] existingBuffer)
        {
            _buffer = existingBuffer;
        }

        public ManuelSerializer()
        {
            _buffer = new byte[1500];
        }

        public unsafe void Write(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException();

            Write(str.Length);

            var byes = Encoding.UTF8.GetBytes(str);
            Buffer.BlockCopy(byes,0,_buffer,WriteOffset,byes.Length);
            WriteOffset += byes.Length;
        }

        public unsafe void Write(int value)
        {
            fixed (byte* ptr = &_buffer[WriteOffset])
            {
                *(int*) ptr = value;
            }

            WriteOffset += 4;
        }

        public unsafe void Write(byte[] value)
        {
            Write(value.Length);

            foreach (var t in value)
            {
                _buffer[WriteOffset] = t;
                WriteOffset += 1;
            }
        }

        public unsafe int ReadInt()
        {
            int val;
            fixed (byte* ptr = &_buffer[ReadOffset])
            {
                val = *(int*) ptr;
            }

            ReadOffset += 4;
            return val;
        }

        public string ReadString()
        {
            var len = ReadInt();
            var value = Encoding.UTF8.GetString(_buffer, ReadOffset, len);
            ReadOffset += len;
            return value;
        }

        public byte[] ReadBytes()
        {
            var len = ReadInt();

            var newBlock = new byte[len];

            System.Buffer.BlockCopy(_buffer, ReadOffset, newBlock, 0, len);

            ReadOffset += len;
            return newBlock;
        }

        public byte[] GetBytes()
        {
            var arr = new byte[WriteOffset];
            Buffer.BlockCopy(_buffer,0,arr,0,WriteOffset);
            return arr;
        }
    }
}