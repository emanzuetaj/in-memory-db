using System;
using System.Collections;

namespace InMemoryDatabaseWithTransactions
{
    class Program
    {
        static void Main(string[] args)
        {
            // declare mainProcessor here to maintain object inside while loop instead of letting it create a new instance on each iteration
            var mainProcessor = new Processor(false);
            string mainStatement = "";
            // run loop as long as the mainStatement is not END which gets updated after loop run which is why it must be checked throughout loop as well
            while (mainStatement != null && mainStatement.ToUpper() != "END")
            {
                mainStatement = GetUserInput();
                if (mainStatement != null && mainStatement.ToUpper() != "END" && mainStatement.ToUpper() != "BEGIN")
                {
                    PerformAction(mainProcessor, mainStatement);
                }
                // this else-if is used for transactions
                else if(mainStatement != null && mainStatement.ToUpper() == "BEGIN")
                {
                    // enclose in using statement to automatically dispose object
                    using (var transactionProcessor = new Processor(true))
                    {
                        // get most recent version of the hashtable for transaction
                        transactionProcessor.data = new Hashtable(mainProcessor.data);
                        transactionProcessor.history.Add(new Hashtable(transactionProcessor.data));
                        var transactionStatement = "";
                        // run loop as long as the transaction is not committed, value is nullified, or everything is ended
                        while (ContinueTransaction(transactionStatement))
                        {
                            transactionStatement = GetUserInput();
                            if(ContinueTransaction(transactionStatement))
                            {
                                PerformAction(transactionProcessor, transactionStatement);
                            }
                        }
                        // get the changes into the main hashtable
                        if (transactionStatement != null && transactionStatement.ToUpper() == "COMMIT")
                        {
                            mainProcessor.data = new Hashtable(transactionProcessor.data);
                        }
                        // change mainStatement to END to stop loop - could have also used break
                        else if (transactionStatement != null && transactionStatement.ToUpper() == "END")
                        {
                            mainStatement = transactionStatement;
                        }
                    }
                }
            }
            // dispose main data processor
            mainProcessor.Dispose();
        }
        public static bool ContinueTransaction(string statement)
        {
            return statement != null && statement.ToUpper() != "COMMIT" && statement.ToUpper() != "END";
        }
        public static string GetUserInput()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }
        public static void PerformAction(Processor processor, string statement)
        {
            try
            {
                processor.SetStatement(statement);
                processor.ReadCommand();
                var result = processor.ProcessCommand();
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
