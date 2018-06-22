using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    internal class IntArray
    {
        public static byte[] ToByteArray(int[] intArray)

        {
            byte[] data = new byte[intArray.Length * 4];

            for (int i = 0; i < intArray.Length; i++)

                Array.Copy(BitConverter.GetBytes(intArray[i]), 0, data, i * 4, 4);

            return data;
        }
    }
}