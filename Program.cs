using System;
using System.IO;

namespace thd2thp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                usage();
            }
            else
            {
                string headerFile = Path.GetFullPath(args[0]);
                string dataFile = Path.GetFullPath(args[1]);
                string outputFile = Path.GetFullPath(args[2]);
                
                byte[] headerBytes;
                long headerLength;
                int bytesRead;

                uint temp;

                using (FileStream fs = File.Open(headerFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    headerLength = fs.Length;
                                        
                    // read header from file
                    headerBytes = new byte[headerLength];
                    bytesRead = fs.Read(headerBytes, 0, (int)headerLength);

                    if (bytesRead != headerLength)
                    {
                        Console.WriteLine("Error reading header.  Aborting.");
                        return;
                    }                    
                    else
                    {
                        if (ReadUintBE(headerBytes, 0) != 0x54485000)
                        {
                            Console.WriteLine("WARNING: Invalid header, THP\\0 not found.");
                        }
                        
                        //----------------
                        // update header
                        //----------------

                        // update component data offset
                        //temp = ReadUintBE(headerBytes, 0x20);
                        //temp += (uint)headerLength;
                        //headerBytes = WriteUintBE(headerBytes, 0x20, temp);

                         // update first frame offset
                        temp = ReadUintBE(headerBytes, 0x28);
                        temp += (uint)headerLength;
                        headerBytes = WriteUintBE(headerBytes, 0x28, temp);

                        // update last frame offset
                        temp = ReadUintBE(headerBytes, 0x2C);
                        temp += (uint)headerLength;
                        headerBytes = WriteUintBE(headerBytes, 0x2C, temp);

                        // build thp file
                        AddHeaderToFile(headerBytes, dataFile, outputFile);
                    }
                
                }
                
            }
        }

        static void AddHeaderToFile(byte[] headerBytes, string sourceFile, string destinationFile)
        {
            int bytesRead;
            byte[] readBuffer = new byte[70000];

            using (FileStream destinationStream = File.Open(destinationFile, FileMode.CreateNew, FileAccess.Write))
            {
                // write header
                destinationStream.Write(headerBytes, 0, headerBytes.Length);

                // write the source file
                using (FileStream sourceStream = File.Open(sourceFile, FileMode.Open, FileAccess.Read))
                {
                    while ((bytesRead = sourceStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        destinationStream.Write(readBuffer, 0, bytesRead);
                    }
                }
            }
        }

        static uint ReadUintBE(byte[] inBytes, long offset)
        {
            byte[] val = new byte[4];

            Array.Copy(inBytes, offset, val, 0, 4);
            Array.Reverse(val);

            return BitConverter.ToUInt32(val, 0);
        }

        static byte[] WriteUintBE(byte[] buffer, int offset, uint value)
        {
            byte[] val = BitConverter.GetBytes(value);

            Array.Reverse(val);
            Array.Copy(val, 0, buffer, offset, 4);
            
            return buffer;
        }

        static void usage()
        {
            Console.WriteLine("thd2thp <thh header file> <thd data file> <output file>");
            Console.WriteLine("    thh - header file");
            Console.WriteLine("    thd - data file containing audio/video data");
            Console.WriteLine("    output file - path to output file");
            Console.WriteLine();
        }
    }
}
