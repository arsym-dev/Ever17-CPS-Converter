using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace E17_CPS
{
    class CPS_format
    {
        [StructLayout(LayoutKind.Explicit)]
        struct CPSHeader
        {
            [FieldOffset(0x00)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string ID;

            [FieldOffset(0x04)]
            public Int32 CPSFileSize;

            [FieldOffset(0x08)]
            public Int16 Version;

            [FieldOffset(0x0A)]
            public Int16 CompressionType;

            [FieldOffset(0x0C)]
            public Int32 UnpackedFileSize;

            [FieldOffset(0x10)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string SubHead;
        }

        public static T ByteToType<T>(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        public static MemoryStream Compress(string filename, bool compress = true, string savepath = null)
        {
            FileStream fin_stream = new FileStream(filename, FileMode.Open);

            // Copy to a memory stream so that we can modify it as we like
            MemoryStream stream = new MemoryStream();
            fin_stream.CopyTo(stream);
            fin_stream.Close();
            fin_stream.Dispose();

            MemoryStream stream_out = Compress(stream, compress, savepath);
            stream.Dispose();

            return stream_out;
        }

        public static MemoryStream Compress(MemoryStream stream, bool compress = true, string savepath = null)
        {
            MemoryStream stream_compressed = new MemoryStream();

            stream.Position = 0;
            var prt_size = stream.Length;
            var cmp_size = prt_size + 0x14 + 4;

            /*
            * 0xC0 - Duplicate one byte from input  many times
            * 0x80 - Duplicate one byte from output many times
            * 0x40 - Copy a sequence of bytes from input many times
            * 0x00 - Copy input to output directly
            * 
            * 
            * 
            * Anything with 0x20 will simply extend the amount
            * of bytes to read/write
            * 
            * eg. (3F 01) 0011 1111   0000 0001
            * Total count: (3F & 1F) + 1 + (01 << 5)
            *              0001 1111 + 1 + 0010 0000
            *              0011 1111 + 1
            *              0100 0000
            */


            if (compress)
            {
                // Get the difference of bytes to find repeated sequences
                byte[] byte_stream = stream.ToArray();
                byte[] byte_difference = stream.ToArray();
                for (int i = 0; i < byte_difference.Length - 1; i++)
                    byte_difference[i] = (byte)(byte_stream[i + 1] - byte_stream[i]);

                byte_difference[byte_difference.Length - 1] = 1; // Set last byte as a difference

                bool repeated = false;
                int count = 0;
                for (int i = 0; i < byte_difference.Length; i++)
                {
                    if (byte_difference[i] == 0x00 && i != byte_difference.Length - 1)
                    {
                        if (!repeated) { repeated = true; count = 0; }
                        count++;
                    }
                    else
                    {
                        if (repeated)
                        {
                            // Write to stream
                            count--;
                            if (count > 0x1F)
                            {
                                byte opcode = 0xC0;
                                opcode += (byte)(count & 0x1F);
                                stream_compressed.WriteByte(opcode);
                                stream_compressed.WriteByte((byte)(count));
                            }

                            repeated = false;
                        }
                        else
                        {
                            // Copy to stream directly
                            stream_compressed.WriteByte(byte_stream[i]);
                        }
                    }
                }
            }
            else
            {
                stream.CopyTo(stream_compressed);
            }

            // Create final file
            MemoryStream stream_out = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream_out);

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(("CPS" + '\0').ToCharArray());
            writer.Write((Int32)cmp_size);
            if (compress)
            {
                writer.Write((Int16)0x0066); // Version
                writer.Write((Int16)0x0101); // Compression Type
            }
            else
            {
                writer.Write((Int16)0x0066); // Version
                writer.Write((Int16)0x0000); // Compression Type
            }
            writer.Write((Int32)prt_size);
            writer.Write("bmp\0".ToCharArray());
            writer.Write(stream_compressed.ToArray());
            writer.Write((Int32)0x07534682); //Decryption setting (ie. no decryption)



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

            stream_compressed.Dispose();

            stream_out.Seek(0, SeekOrigin.Begin);
            return stream_out;
        }


        public static bool Decompress(string filename)
        {
            FileStream fin_stream = new FileStream(filename, FileMode.Open);

            // Copy to a memory stream so that we can modify it as we like
            MemoryStream stream = new MemoryStream();
            fin_stream.CopyTo(stream);
            fin_stream.Close();
            fin_stream.Dispose();

            stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);
            BinaryWriter writer = new BinaryWriter(stream);

            CPSHeader cps_header = ByteToType<CPSHeader>(reader);

            if (cps_header.Version != 0x66 || !cps_header.ID.Equals("CPS"))
            {
                // Unkown file parameters
                MessageBox.Show("Unsupported CPS version");

                writer.Dispose();
                reader.Dispose();
                stream.Dispose();

                return false;
            }

            // =====
            // DECRYPTION

            // Read the last 4 bytes. Binary reader doesn't have a Seek function, so we move the underlying stream
            stream.Seek(cps_header.CPSFileSize - 4, SeekOrigin.Begin);
            Int32 f = reader.ReadInt32() - 0x07534682;

            if (f != 0)
            {
                stream.Seek(f, SeekOrigin.Begin);
                Int32 b = reader.ReadInt32() + f + 0x03786425;

                int offset = 0x10;
                while (offset <= cps_header.CPSFileSize - 4)
                {
                    if (offset != f)
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        int v = reader.ReadInt32();
                        stream.Seek(offset, SeekOrigin.Begin);
                        writer.Write(v - cps_header.CPSFileSize - b);
                    }

                    b = b * 0x41C64E6D + 0x9B06;
                    offset += 4;
                }
            }

            if (File.Exists(filename + ".decrypted"))
                File.Delete(filename + ".decrypted");
            FileStream fout_stream = new FileStream(filename + ".decrypted", FileMode.OpenOrCreate, FileAccess.Write);
            stream.WriteTo(fout_stream);
            fout_stream.Flush();
            fout_stream.Close();
            fout_stream.Dispose();


            // =========
            // UNPACK

            stream.Seek(cps_header.CPSFileSize - 4, SeekOrigin.Begin);
            writer.Write((int)0);

            byte[] input = stream.ToArray();
            byte[] output = new byte[cps_header.UnpackedFileSize + 0x10];
            int input_ptr = 0x13;
            int output_ptr = 0;

            if ((cps_header.CompressionType & 0x01) > 0)
            {
                int w = 0;
                Int32 z = 0, x1 = 0, y = 0, x2 = 0;

                bool done = false;

                /*
                 * 0xC0 - Duplicate one byte from input many times
                 * 0x80 - Duplicate one byte from output (seek prev offset) many times
                 * 0x40 - Copy a sequence of bytes from input many times
                 * 0x00 - Copy input to output directly
                 * 
                 * 
                 * 
                 * Anything with 0x20 will simply extend the amount
                 * of bytes to read/write
                 * 
                 * eg. (3F 01) 0011 1111   0000 0001
                 * Total count: (3F & 1F) + 1 + (01 << 5)
                 *              0001 1111 + 1 + 0010 0000
                 *              0011 1111 + 1
                 *              0100 0000
                 */

                while (w <= cps_header.UnpackedFileSize && !done)
                {
                    byte c = input[input_ptr];
                    input_ptr++;

                    if ((c & 0x80) > 0)
                    {
                        if ((c & 0x40) > 0)
                        {
                            z = c & 0x1F + 2;

                            if ((c & 0x20) > 0)
                            {
                                z += input[input_ptr] << 5;
                                input_ptr++;
                            }

                            c = input[input_ptr];
                            input_ptr++;
                            x1 = 0;

                            while (x1 < z)
                            {
                                if (w <= cps_header.UnpackedFileSize)
                                {
                                    output[output_ptr] = c;
                                    output_ptr++;
                                    w++;
                                    x1++;
                                }
                                else
                                {
                                    done = true;
                                    break;
                                }
                            }
                        }

                        else
                        {
                            int prev_pos = output_ptr;
                            prev_pos -= ((c & 0x03) << 8) + input[input_ptr] + 1;
                            input_ptr++;
                            z = ((c >> 2) & 0x0F) + 2;

                            x1 = 0;
                            while (x1 < z)
                            {
                                if (w <= cps_header.UnpackedFileSize)
                                {
                                    output[output_ptr] = output[prev_pos];

                                    output_ptr++;
                                    w++;
                                    x1++;
                                }
                                else
                                {
                                    done = true;
                                    break;
                                }
                            }
                        }
                    }

                    else if ((c & 0x40) > 0)
                    {
                        x2 = 0;
                        z = (c & 0x3F) + 2;
                        y = output[output_ptr] + 1;
                        output_ptr++;

                        while (x2 < y)
                        {
                            x1 = 0;
                            while (x1 < z)
                            {
                                if (w <= cps_header.UnpackedFileSize)
                                {
                                    output[output_ptr] = input[input_ptr + x1];

                                    output_ptr++;
                                    w++;
                                    x1++;
                                }
                                else
                                {
                                    done = true;
                                    break;
                                }
                            }

                            x2++;
                        }

                        input_ptr += z;
                    }

                    else
                    {
                        z = (c & 0x1F) + 1;
                        if ((c & 0x20) > 0)
                        {
                            z += input[input_ptr] << 5;
                            input_ptr++;
                        }

                        x1 = 0;

                        while (x1 < z)
                        {
                            if (w <= cps_header.UnpackedFileSize)
                            {
                                output[output_ptr] = input[input_ptr];

                                input_ptr++;
                                output_ptr++;
                                w++;
                                x1++;
                            }
                            else
                            {
                                done = true;
                                break;
                            }
                        }
                    }
                }
            }






            else if ((cps_header.CompressionType & 0x02) > 0)
            {
                MessageBox.Show("Unsupported unpacking type 2");

                writer.Dispose();
                reader.Dispose();
                stream.Dispose();

                return false;
            }
            else
            {
                // No unpacking needed
                output = stream.ToArray();
            }


            // Write to file
            if (File.Exists(filename + ".unpacked"))
                File.Delete(filename + ".unpacked");

            FileStream funp_stream = new FileStream(filename + ".unpacked", FileMode.OpenOrCreate, FileAccess.Write);
            funp_stream.Write(output, 0, output.Length);
            funp_stream.Flush();
            funp_stream.Close();
            funp_stream.Dispose();


            // =========
            // CLEANUP

            writer.Dispose();
            reader.Dispose();
            stream.Dispose();

            return true;
        }
    }
}
