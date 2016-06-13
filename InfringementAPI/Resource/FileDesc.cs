using System;
using System.Collections.Generic;

namespace InfringementAPI.Resource
{
    public class FileDesc
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }

        public FileDesc(string n, string p, long s)
        {
            Name = n;
            Path = p;
            Size = s;
        }
    }
}