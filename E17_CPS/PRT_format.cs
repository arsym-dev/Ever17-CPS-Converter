using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace E17_CPS
{
    class PRT_format
    {
        [StructLayout(LayoutKind.Explicit)]
        struct PRTHeader
        {
            [FieldOffset(0x00)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string ID;

            [FieldOffset(0x04)]
            public Int16 Version;

            [FieldOffset(0x06)]
            public Int16 ColorDepth;

            [FieldOffset(0x08)]
            public Int16 PalletteOffset;

            [FieldOffset(0x0A)]
            public Int16 DataOffset;

            [FieldOffset(0x0C)]
            public Int16 Width;

            [FieldOffset(0x0E)]
            public Int16 Height;

            [FieldOffset(0x10)]
            public Int32 Alpha;

            [FieldOffset(0x14)]
            public Int32 Base1Offset;

            [FieldOffset(0x18)]
            public Int32 u2;

            [FieldOffset(0x1C)]
            public Int32 Width2;

            [FieldOffset(0x20)]
            public Int32 Height2;
        }

        public static MemoryStream Convert(string filename, bool compress = true, string savepath = null)
        {
            FileStream fin_stream = new FileStream(filename, FileMode.Open);

            // Copy to a memory stream so that we can modify it as we like
            MemoryStream stream = new MemoryStream();
            fin_stream.CopyTo(stream);
            fin_stream.Close();
            fin_stream.Dispose();

            MemoryStream stream_out = Convert(stream, compress, savepath);
            stream.Dispose();

            return stream_out;
        }

        public static MemoryStream Convert(MemoryStream stream, bool compress = true, string savepath = null)
        {
            // Read the BMP data
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0x0A, SeekOrigin.Begin);
            Int32 data_offset = reader.ReadInt32();
            Int32 header_size = reader.ReadInt32(); //Assumed to be 40 (ie. Windows format, not OS/2)
            Int32 width  = reader.ReadInt32();
            Int32 height = reader.ReadInt32();
            Int16 one = reader.ReadInt16();
            Int16 depth = reader.ReadInt16();

            // 32 bit data is represented as 24 bit data + a giant block of alpha data at the end
            stream.Position = 0x36;
            byte[] data_block = null;
            byte[] alpha_block = null;
            if (depth == 32)
            {
                data_block = new byte[width * height * 3];
                alpha_block = new byte[width * height];

                // Read three color bytes and one alpha byte
                int i = 0;
                while (stream.Position < stream.Length)
                {
                    data_block[i*3  ] = (byte)stream.ReadByte();
                    data_block[i*3+1] = (byte)stream.ReadByte();
                    data_block[i*3+2] = (byte)stream.ReadByte();
                    alpha_block[i] = (byte)stream.ReadByte();
                    i++;
                }

                // If all the alpha bytes are 0xFF, change the depth to 24 bits
                bool all_opaque = true;
                for (i = 0; i<alpha_block.Length; i++)
                {
                    if (alpha_block[i] != 0xff)
                    {
                        all_opaque = false;
                        break;
                    }
                }

                if (all_opaque)
                    depth = 24;
            }
            else if (depth == 24)
            {
                // Copy BMP data directly
                data_block = new byte[width * height * 3];

                int i = 0;
                while (stream.Position < stream.Length)
                {
                    data_block[i] = (byte)stream.ReadByte();
                    i++;
                }
            }
            else if (depth == 8)
            {
                // Copy BMP data directly, including pallete data
                data_block = new byte[stream.Length - stream.Position];

                int i = 0;
                while (stream.Position < stream.Length)
                {
                    data_block[i] = (byte)stream.ReadByte();
                    i++;
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            // Write the PRT data
            MemoryStream stream_out = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream_out);

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(("PRT" + '\0').ToCharArray());
            writer.Write((Int16) 0x0066); // Version
            writer.Write((Int16) depth);
            writer.Write((Int16) 0x24); //PalletteOffset. Should be 0x24 assuming a windows format bitmap with no compression
            writer.Write((Int16) (data_offset + 0x24-0x36)); // Data offset
            writer.Write((Int16) width);
            writer.Write((Int16) height);
            if (depth == 32)
                writer.Write((Int32)1); // Alpha enabled
            else
                writer.Write((Int32)0); // No alpha
            writer.Write((Int32)0); // Base L offset
            writer.Write((Int32)0); // U2
            writer.Write((Int32)0); // Width2
            writer.Write((Int32)0); // Height2
            writer.Write(data_block);
            if (depth == 32)
                writer.Write(alpha_block);
            /*
            // Copy BMP data to the PRT file
            stream.Position = 0x36;
            while (stream.Position < stream.Length)
                stream_out.WriteByte((byte) stream.ReadByte());
            */

            if (savepath != null)
            {
                if (File.Exists(savepath))
                    File.Delete(savepath);

                FileStream fout_stream = new FileStream(savepath, FileMode.OpenOrCreate, FileAccess.Write);
                stream_out.Seek(0, SeekOrigin.Begin);
                stream_out.CopyTo(fout_stream);
                fout_stream.Flush();
                fout_stream.Close();
                fout_stream.Dispose();
            }

            stream_out.Seek(0, SeekOrigin.Begin);
            return stream_out;
        }
    }
}
