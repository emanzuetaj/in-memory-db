# In Memory Database with Transactions

This application allows you to save data and retrieve while also allowing transaction-like behavior.

# Commands
- SET [name] [value]: Set a variable [name] to the value [value]
- GET [name]: Print out the value stored under the variable [name]
- UNSET [name]: Unset the variable [name]
- COUNT [value]: Return the count of variables equal to [value]
- END: Exit the program

# Transaction Commands
- BEGIN: Open a transactional block
- ROLLBACK: Rollback all of the commands from the most recent transaction block
- COMMIT: Permanently store all of the operations from any presently open transactional blocks
