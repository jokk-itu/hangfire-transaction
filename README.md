# Hangfire Transaction

This repository implements seamless transaction handling between hangfire and a dbcontext from EntityFrameworkCore.
It makes sure the transaction is not upgraded to a distributed transaction by using TransactionScope and one scoped DbConnection being used by DbContext and BackgroundJobClient.

The system consists of one Api, which can be used to successfully commit a transaction, and one endpoint which fails and performs a rollback.
The system consists of one Worker, which acts as a Hangfire server.
The system consists of one job, which is a fire-and-forget job called "DummyJob".

You can invoke the endpoints through the api.http script.