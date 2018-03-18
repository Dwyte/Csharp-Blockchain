using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Blockchain
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(BlockchainFunctions.SHA256(Console.ReadLine()));
            Console.ReadLine();
        }
    }

    class BlockchainFunctions
    {
        public static string SHA256(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static string Stringify(Object obj)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }
    }


    //-----Parts of the Blockchain
    public class Blockchain
    {
        public Blockchain()
        {

        }
    }

    public class Block
    {
        public Block()
        {

        }
    }

    public class Transaction
    {

    }


}
