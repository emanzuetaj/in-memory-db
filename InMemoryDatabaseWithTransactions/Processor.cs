using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDatabaseWithTransactions
{
    class Processor : IDisposable
    {
        private readonly string[] supportedCommands = { "SET", "GET", "UNSET", "COUNT", "ROLLBACK"};
        public Hashtable data;
        private string command;
        string statement;
        string[] statementParts;
        bool disposed = false;
        bool transaction = false;
        public List<Hashtable> history;
        SafeHandle handle;

        public Processor(bool isATransaction)
        {
            transaction = isATransaction;
            handle = new SafeFileHandle(IntPtr.Zero, true);
            // create new hashtable for each instance
            data = new Hashtable();
            if(transaction)
            {
                // different commands supported in a transaction
                supportedCommands = new string[] { "SET", "GET", "UNSET", "COUNT", "ROLLBACK", "BEGIN" };
                history = new List<Hashtable>();
            }
        }
        public void SetStatement(string newStatement)
        {
            statement = newStatement;
            // make sure we did not get an empty input
            if (string.IsNullOrEmpty(statement))
            {
                throw new Exception("Statement cannot be empty");
            }
            // set array to handle each part of statement separately
            statementParts = statement.Split(' ');
        }
        public void ReadCommand()
        {
            // ToUpper() for lowercase/uppercase compatibility in command
            command = statementParts[0].ToUpper();
            if (!supportedCommands.Contains(command))
            {
                throw new Exception("Command is not supported, supported commands are " + string.Join(",", supportedCommands) + ".");
            }
            // check if statement length is valid for command
            if ((command == "SET" && statementParts.Length != 3) || ((command == "COUNT" || command == "UNSET" || command == "GET") && statementParts.Length != 2) || ((command == "ROLLBACK" || command == "BEGIN") && statementParts.Length != 1))
            {
                throw new Exception("Statement length is not appropriate for command: " + command);
            }
        }
        public string ProcessCommand()
        {
            string result = "";
            // statementParts[1] is the variable name while statementParts[2] is the value it is being set or updated to
            switch (command.ToString())
            {
                case "SET":
                    if (data.ContainsKey(statementParts[1]))
                    {
                        data[statementParts[1]] = statementParts[2];
                    }
                    else
                    {
                        data.Add(statementParts[1], statementParts[2]);
                    }
                    break;
                case "GET":
                    if (data.ContainsKey(statementParts[1]))
                    {
                        result = data[statementParts[1]].ToString();
                    }
                    else
                    {
                        result = "NULL";
                    }
                    break;
                case "UNSET":
                    if (data.ContainsKey(statementParts[1]))
                    {
                        data.Remove(statementParts[1]);
                    }
                    break;
                case "COUNT":
                    int count = 0;
                    foreach (DictionaryEntry pair in data)
                    {
                        if (pair.Value.ToString() == statementParts[1])
                        {
                            count++;
                        }
                    }
                    result = count.ToString();
                    break;
                case "ROLLBACK":
                    result = Rollback();
                    break;
                case "BEGIN":
                    // add current data to a list in case of rollback
                    history.Add(new Hashtable(data));
                    break;
                default:
                    result = "This command is not supported.";
                    break;
            }
            return result;
        }
        public string Rollback()
        {
            if (!transaction)
            {
                return "INVALID ROLLBACK";
            } else
            {
                data = history.Last();
                history.RemoveAt(history.Count - 1);
                return "";
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }
            disposed = true;
        }
    }
}
