using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using CTSTestApplication;
namespace CTSTradeApp
{
    class Program
    {
        static List<Trade> ReadXMLFile()
        {
            var trades = new List<Trade>();
            var idCounter = 1;
            using (XmlReader reader = XmlReader.Create(@"D:\test\TradesList.xml"))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Trade")
                    {
                        try
                        {
                            var trade = new Trade();
                            reader.ReadToFollowing("Direction");
                            trade.Direction = reader.ReadElementContentAsString() == "S" ? Direction.Sell : Direction.Buy;
                            reader.ReadToFollowing("ISIN");
                            trade.Isin = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("Quantity");
                            trade.Quantity = int.Parse(reader.ReadElementContentAsString());
                            reader.ReadToFollowing("Price");
                            trade.Price = decimal.Parse(reader.ReadElementContentAsString().Replace('.', ','));
                            trade.Id = idCounter;
                            idCounter++;
                            trades.Add(trade);
                        }
                        catch (Exception ex)
                        {
                            // TODO: LOG exception
                        }
                    }
                }
            }

            return trades;
        }

        static void CreateXMLFile()
        {
            var tester = new Tester();
            tester.CreateTestFile(@"D:\test", 1000000);
        }

        static bool ProcessTransaction(string transactionName, Operation operation, IEnumerable<Trade> trades, IDataAdapter dataAdapter)
        {
            dataAdapter.BeginTransaction(transactionName);
            try
            {
                foreach (var trade in trades)
                {
                    switch (operation)
                    {
                        case Operation.Insert:
                            trade.Create(dataAdapter);
                            break;
                        case Operation.Delete:
                            trade.Delete(dataAdapter);
                            break;
                        case Operation.Update:
                            trade.Edit(dataAdapter);
                            break;
                        case Operation.Get:
                            trade.Load(dataAdapter);
                            break;
                    }
                }
                dataAdapter.CommitTransaction(transactionName);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: Log exception.
                dataAdapter.RollbackTransaction(transactionName);
                return false;
            }
        }

        static void Main(string[] args)
        {
            var dataAdapter = new DataAdapter(@"D:\test\");
            var trades = ReadXMLFile();
            ProcessTransaction("InsertAllDataTransaction", Operation.Insert, trades, dataAdapter);
            ProcessTransaction("InsertOneTradeTransaction", Operation.Insert, new Trade[] { trades[1000] }, dataAdapter);
            ProcessTransaction("DeleteTransaction1", Operation.Delete, new Trade[] { trades[1], trades[2] }, dataAdapter);
            ProcessTransaction("DeleteTransaction2", Operation.Delete, new Trade[] { trades[1000], trades[2000], trades[4000] }, dataAdapter);
            ProcessTransaction("EditSomeDataTransaction", Operation.Update, new Trade[] { trades[5], trades[6], trades[8] }, dataAdapter);

            var tradesToLoad = new Trade[] { new Trade(), new Trade(), new Trade() };
            ProcessTransaction("LoadTransaction", Operation.Get, tradesToLoad, dataAdapter);

            Console.WriteLine("DONE.");
        }
    }
}
