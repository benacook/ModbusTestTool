using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    internal class ByteArray
    {
        public static int[] ToIntArray(byte[] byteArray)

        {
            int[] intArray = new int[byteArray.Length / 4];

            for (int i = 0; i < byteArray.Length; i += 4)

                intArray[i / 4] = BitConverter.ToInt32(byteArray, i);

            return intArray;
        }
    }
}