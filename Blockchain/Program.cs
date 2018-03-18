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
            Console.ReadLine();
            Blockchain coin = new Blockchain();
            coin.CreateGenesisBlock();

            coin.CreateTransaction(new Transaction("address1","address2", 50));
            coin.minePendingTransaction("address3");

            Console.WriteLine(BlockchainFunctions.Stringify(coin));
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
        public List<Block> chain = new List<Block>();
        public List<Transaction> pendingTransactions = new List<Transaction>();
        public int difficulty;

        public Blockchain()
        {
            difficulty = 4;
        }

        //-----Functions
        public void CreateGenesisBlock()
        {
            BlockHeader genesisBlockHeader = new BlockHeader(0, long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss")), "0000000000000000000000000000000000000000000000000000000000000000");
            Block genesisBlock = new Block(genesisBlockHeader, pendingTransactions.ToArray());
            genesisBlock.MineBlock(difficulty);
            chain.Add(genesisBlock);
        }

        public Block GetLatestBlock()
        {
            Block[] chainArray = chain.ToArray();
            return chainArray[chainArray.Length - 1];
        }

        public void CreateTransaction(Transaction tx)
        {
            pendingTransactions.Add(tx);
        }

        public void minePendingTransaction(string minerAddress)
        {
            pendingTransactions.Add(new Transaction(null, minerAddress, 50));
            BlockHeader newBlockHeader = new BlockHeader(chain.ToArray().Length, long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss")), GetLatestBlock().hash);
            Block newBlock = new Block(newBlockHeader, pendingTransactions.ToArray());
            newBlock.MineBlock(difficulty);
            chain.Add(newBlock);
            pendingTransactions.Clear();
        }
    }

    public class Block
    {
        public BlockHeader header;
        public Transaction[] transactionList;
        public string hash;

        public Block(BlockHeader Header, Transaction[] TransactionList)
        {
            header = Header;
            transactionList = TransactionList;
            hash = CalculateHash();
        }

        //-----Functions
        public string CalculateHash()
        {
            return BlockchainFunctions.SHA256(header.blockNumber + header.previousHash + header.merkleRoot + header.timestamp + header.target + header.nonce);
        }

        public void MineBlock(int difficulty)
        {
            string zeroes = "";
            for (int i = 0; i < difficulty; i++)
            {
                zeroes += "0";
            }

            while (hash.Substring(0, difficulty) != zeroes)
            {
                header.nonce++;
                hash = CalculateHash();
                Console.Write("\r Nonce: {0} Hash: {1}", header.nonce, hash);
            }
            Console.WriteLine("\n Proof of Work Added!, Block Mined.");
        }
    }

    public class BlockHeader
    {
        public int blockNumber;
        public string previousHash;
        public string merkleRoot;
        public long timestamp;
        public int target;
        public int nonce;


        public BlockHeader(int BlockNumber, long Timestamp, string PreviousHash = "")
        {
            blockNumber = BlockNumber;
            timestamp = Timestamp;
            previousHash = PreviousHash;
        }
    }

    public class Transaction
    {
        public string fromAddress;
        public string toAddress;
        public int amount;

        public Transaction(string FromAddress, string ToAddress, int Amount)
        {
            fromAddress = FromAddress;
            toAddress = ToAddress;
            amount = Amount;
        }
    }


}
