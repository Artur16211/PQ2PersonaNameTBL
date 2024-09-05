using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amicitia.IO.Binary;

namespace PQ2NameTableEditor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Usage:\nDrag and Drop a nametable.tbl file into the program's exe to convert to txt\nDrag and Drop a converted txt to convert back to nametable.tbl\nPress any key to exit...");
                Console.ReadKey();
            }
            else
            {
                FileInfo arg0 = new FileInfo(args[0]);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                if (arg0.Extension.ToLower() == ".tbl")
                {
                    Console.WriteLine($"Attempting to convert {arg0.Name}");

                    using (BinaryObjectReader NAMETBLFile = new BinaryObjectReader(args[0], Endianness.Little, Encoding.GetEncoding("shift-jis")))
                    {
                        List<String> NameTBLStrings = new List<String>();
                        List<UInt16> StringPointers = new List<UInt16>();

                        int numOfPointers = NAMETBLFile.ReadUInt16();

                        for (int j = 0; j < numOfPointers; j++)
                        {
                            StringPointers.Add(NAMETBLFile.ReadUInt16());
                        }

                        long basePos = NAMETBLFile.Position;

                        for (int j = 0; j < numOfPointers - 1; j++)
                        {
                            NAMETBLFile.Seek(basePos + StringPointers[j], SeekOrigin.Begin);

                            var targetString = NAMETBLFile.ReadString(StringBinaryFormat.NullTerminated);
                            NameTBLStrings.Add(targetString);
                        }

                        NameTBLStrings.Add("NotExist"); // this doesn't exist in tbl, last pointer goes past EoF

                        var savePath = arg0.FullName.Replace(arg0.Extension, ".txt");
                        File.WriteAllLines(savePath, NameTBLStrings, Encoding.GetEncoding("shift-jis"));
                        Console.WriteLine("Saving " + arg0.FullName.Replace(arg0.Extension, ".txt"));
                    }
                }
                else if (arg0.Extension.ToLower() == ".txt")
                {
                    var savePath = arg0.FullName.Replace(arg0.Extension, ".tbl");

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    using (BinaryObjectWriter NAMETBLFile = new BinaryObjectWriter(savePath, Endianness.Little, Encoding.GetEncoding("shift-jis")))
                    {
                        string[] NameTBLStrings = Array.Empty<string>();

                        Console.WriteLine("Reading txt file " + arg0.Name);

                        NameTBLStrings = File.ReadAllLines(arg0.FullName, Encoding.GetEncoding("shift-jis"));

                        NAMETBLFile.WriteUInt16((ushort)NameTBLStrings.Length);

                        List<long> StringPointers = new List<long>();

                        for (int j = 0; j < NameTBLStrings.Length; j++)
                        {
                            NAMETBLFile.WriteUInt16(0); // write dummy pointers
                        }

                        long basePos = NAMETBLFile.Position; // save position before strings

                        NAMETBLFile.WriteString(StringBinaryFormat.NullTerminated, "BLANK"); // original behavior

                        // Write Strings
                        for (int j = 0; j < NameTBLStrings.Length; j++)
                        {
                            StringPointers.Add(NAMETBLFile.Position - basePos);
                            NAMETBLFile.WriteString(StringBinaryFormat.NullTerminated, NameTBLStrings[j]);
                        }

                        NAMETBLFile.Seek(2, SeekOrigin.Begin); // seek back to write Pointers
                        for (int j = 0; j < NameTBLStrings.Length; j++)
                        {
                            NAMETBLFile.WriteUInt16((ushort)StringPointers[j]);
                        }
                    }
                }
                else Console.WriteLine("https://youtu.be/lMEt3RdqB9Y");
            }
        }
    }
}
