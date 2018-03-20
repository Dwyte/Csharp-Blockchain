using System;
using System.Collections.Generic;
using System.Text;
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

            coin.CreateTransaction(new Transaction("address1","address2", 50,1));
            coin.CreateTransaction(new Transaction("address1", "address2", 50, 2));
            coin.CreateTransaction(new Transaction("address1", "address2", 50, 3));
            coin.CreateTransaction(new Transaction("address1", "address2", 50, 4));
            coin.CreateTransaction(new Transaction("address1", "address2", 50, 5));
            coin.CreateTransaction(new Transaction("address1", "address2", 50, 6));
            coin.CreateTransaction(new Transaction("address1", "address2", 50, 7));


            coin.MineSelectedTransactions("address3");

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

        public static long Timestamp()
        {
            return long.Parse(DateTime.Now.ToString("yyyyMMddhhmmss"));
        }
    }


    //-----Parts of the Blockchain
    public class Blockchain
    {
        public List<Block> chain = new List<Block>();
        public List<Transaction> pendingTransactions = new List<Transaction>();
        public int difficulty;
        public int blockSize;

        public Blockchain()
        {
            difficulty = 2;
            blockSize = 8;
        }

        //-----Functions
        public void CreateGenesisBlock()
        {
            BlockHeader genesisBlockHeader = new BlockHeader(0, BlockchainFunctions.Timestamp(), "0000000000000000000000000000000000000000000000000000000000000000");
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

        public void MineSelectedTransactions(string minerAddress)
        {
            BlockHeader newBlockHeader = new BlockHeader(chain.ToArray().Length, BlockchainFunctions.Timestamp(), GetLatestBlock().hash);
            Block newBlock = new Block(newBlockHeader, SelectPendingTransactions(blockSize));
            newBlock.MineBlock(difficulty);
            chain.Add(newBlock);
        }

        public Transaction[] SelectPendingTransactions(int blockSize)
        {
            List<Transaction> selectedTransactions = new List<Transaction>();

            // Sort Pending Transactions according to the fee (descending)
            Array.Sort(pendingTransactions.ToArray(), delegate (Transaction tx1, Transaction tx2)
            {
                return tx1.fee.CompareTo(tx2.fee);
            });
            pendingTransactions.Reverse();
            pendingTransactions.Insert(0, new Transaction(null, "MinerAddress", 50));


            // Select transactions to be mined from the pending array
            for (int i = 0; i < blockSize; i++)
            {
                if (pendingTransactions.ToArray().Length == 0)
                    return selectedTransactions.ToArray();

                selectedTransactions.Add(pendingTransactions.ToArray()[0]);
                pendingTransactions.Remove(pendingTransactions.ToArray()[0]);
            }
            return selectedTransactions.ToArray();
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
            header.merkleRoot = GetMerkleRoot();
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
            }
        }

        public string GetMerkleRoot()
        {
            List<string> bottom = new List<string>();
            List<string> middle = new List<string>();
            List<string> top = new List<string>();
            string merkleRoot = "";
            string hashOfBottom = "";
            string hashOfMiddle = "";
            string hashOfTop = "";

            // Hash all transactions first
            for (int i = 0; i < transactionList.Length; i++)
            {
                bottom.Add(BlockchainFunctions.SHA256(BlockchainFunctions.Stringify(transactionList[i]))); //add the has to the bottom list
                //Console.WriteLine("-" + BlockchainFunctions.Stringify(bottom));
            }

            for (int i = 0; i < bottom.ToArray().Length; i++)
            {
                //Check if i == to the last element of the array and if the array is has odd number of elements
                if (i == bottom.ToArray().Length - 1 && (i+1)%2 != 0)
                {
                    hashOfBottom += bottom.ToArray()[i]+bottom.ToArray()[i];
                    middle.Add(BlockchainFunctions.SHA256(hashOfBottom));

                    Console.WriteLine("#1");

                    if (middle.ToArray().Length == 1)
                    {
                        merkleRoot = middle.ToArray()[i];
                        Console.WriteLine("#2");
                        return merkleRoot;
                    }
                }
                // Hash the hashes of the bottom list and put it in an array
                if (i > 0 && (i+1)%2 == 0)
                {
                    hashOfBottom += bottom.ToArray()[i - 1];
                    hashOfBottom += bottom.ToArray()[i];
                    middle.Add(BlockchainFunctions.SHA256(hashOfBottom));
                    hashOfBottom = "";
                }
            }


            for (int i = 0; i < middle.ToArray().Length; i++)
            {
                //Check if i == to the last element of the array and if the array is has odd number of elements
                if (i == middle.ToArray().Length - 1 && (i + 1) % 2 != 0)
                {
                    hashOfMiddle += middle.ToArray()[i] + middle.ToArray()[i];
                    top.Add(BlockchainFunctions.SHA256(hashOfMiddle));
                    Console.WriteLine("#3");

                    if (top.ToArray().Length == 1)
                    {
                        merkleRoot = top.ToArray()[i];
                        Console.WriteLine("#4");
                        return merkleRoot;
                    }

                }
                
                // Hash the hashes of the bottom list and put it in an array
                if (i > 0 && (i + 1) % 2 == 0)
                {
                    hashOfMiddle += middle.ToArray()[i - 1];
                    hashOfMiddle += middle.ToArray()[i];
                    top.Add(BlockchainFunctions.SHA256(hashOfBottom));
                    hashOfMiddle = "";
                }
            }


            for (int i = 0; i < top.ToArray().Length; i++)
            {
                //Check if i == to the last element of the array and if the array is has odd number of elements
                if (i == top.ToArray().Length - 1 && (i + 1) % 2 != 0)
                {
                    hashOfTop += top.ToArray()[i] + top.ToArray()[i];
                    merkleRoot = BlockchainFunctions.SHA256(hashOfTop);
                    Console.WriteLine("#5");
                    if (top.ToArray().Length == 1)
                    {
                        Console.WriteLine("#6");
                        return merkleRoot;
                    }
                }

                // Hash the hashes of the bottom list and put it in an array
                if (i > 0 && (i + 1) % 2 == 0)
                {
                    hashOfTop += middle.ToArray()[i - 1];
                    hashOfTop += middle.ToArray()[i];
                    merkleRoot = BlockchainFunctions.SHA256(hashOfTop);
                    Console.WriteLine("#7");
                    return merkleRoot;
                }
            }
            return merkleRoot;
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
        public int fee;

        public Transaction(string FromAddress, string ToAddress, int Amount, int Fee = 0)
        {
            fromAddress = FromAddress;
            toAddress = ToAddress;
            amount = Amount;
            fee = Fee;
        }
    }


}
