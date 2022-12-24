# ShareableSpreadSheet

In This assignment we Implemented ShareablespreadSheet.
We choose to handle the shared resources problem with DeadLock avoidence using the Bank
Algorithem adujsted to our problem. The writers requestes treated as Clients, supporting polymorphism, each client has different resources requests.
for example SetCellClient hold two Lists - Row, Col Requests in each List, supporting scaleability for larger requestes potentionally. 
The Bank open at the creation of ShareableSpreadSheet, and writer requests enter the Bank.
A Thread handle the bank, and in while loop asking if the bank isSafe(), handling the current clients in bank, try allocating the resources, till al clients request handled, and recall the isSafe() method.
while isSafe() method run, clients enter the queue that will be handled in the next isSafe() call. 

![image](https://user-images.githubusercontent.com/62882347/209449356-5a019ee2-6c6b-43f9-a1d5-16cdb76f957c.png)

![image](https://user-images.githubusercontent.com/62882347/209449359-1df70842-3445-4a01-93df-266487e0f97f.png)


Each Clients hold request a Tuple were each <List<int>, List<int>> = < row, col> request for resoucres.  And after entering the bank will enter busy loop.
Handling   requestes:
SetCellClient – 
since its fast operation, the request won’t enter the queue, will enter busy   loop till the resources needed are accpeted. Defined with priority 1.
exchangeRowsClient, exchangeColsClient – 
In Client creation finish = false, running = true;
both requestes enter the bank and handled the same way, try to catch resources till approved.
In the spreadsheet using Petersons between rowExchange, colsExchange to avoid starvition of each 
requests.
Mutex inside each method to allow only one thread to enter each function, and Peterson, to do each opeartion in turns. On method exit finish = true, running = false.
The bank isSafe() working on current client queue till all finish = true.

## exchangeRows – 
With this structure of List<string>[], we swap the reference of both cells.
for example we swap row1 = 3, row2 = 5 references of List<string> we avoid      tradeoff and the Opertion done with O(1).  
exchangeCols - 
each Row is List<string> so we swap each Row[col1] with Row[col2] value operation time O(Row)
The common sloution was matrix[,], so the tradeoff is equal to the tradeoff given from this operation.

## AddRow, AddColumn -
We didn’t treated those requestes as clients, since the huge trade off to request all the resources they need, at worst case only requesting the resources would be O(n), to handle that problem
we implemented here Peterson’s algorithem aswell between the methods, to avoid starvition.
If its operation turn, we create new thread to send the bank into maintance mode.
maintance mode-
- lock isSafe() function, so all resources won’t be accesable to the clients in queue.
- we allocate the new needed resources in O(n) (List Insert) to col or row.
- go to busy wait till the maintance is over and the addRow, addCol don’t expanding 
  the spreadsheet aswell.


### AddRow -   
Creating new empty List<string> filled with nCols empty string node.
Resizing the array by 1, leaving the requested row to expand empty, then assigen the created empty list requested row. Avoided creating the whole list and copy all the lists content by keeping the list only reassigen to Row+1 array to hold all the previous lists.

### AddCol - 
A very expansive operation since we have to List.Insert(col) on nRows, worst time O(n^2)
since its array of Lists, the lists are not dependent, we accelrated the request with multithreading!
since lack of time, assigened const size of 4 threads to do the operation, the tradeoff come to light with the smaller spreadsheets, creating the threads is huge overhead, and one thread will do better job. On over hand assume for big enough nRows, it will be done with O(nRows/ 4).

AddRow and AddCol are costly operations and till done the queue of clients that isSafe() have to handle got long, and sometimes creating bottlenecks, but the program stabilizing after a while.

The motivation behind Bank algorithem
The naïve solution was to handle locks for cells/rows/columns, the huge tradeoff was holding many locks and mangning them, getting deadlocks and handle.
in addition mutex doesn’t support LIFO & FIFO, so all the requests given are not consistent with the arriving time.
The Bank hold queue of clients supporting FIFO, when isSafe() is called the current queue passed to the method, and new queue installed, therefore the Bank still receiving requests while handling the clients entered. By checking if can allocate the needed resources we avoid DeadLocks by avoiding allocating the needed resources to requests if not all needed resources available.
IsSafe()
The method handling the clients and satisfy their request if possiable.
Pseduo code:
1) Catching the maintance mutex // so expanding resources and handling clients won’t overlap
2) Catching the handlingClients Mutex so clients won’t enter
                         Assigen the queue to local variable processClients (type: List<client>)
                         Install new List at prePremitionClients 
                         Releasing the handlingClientsMutex
3) While processClient is not null:
        create failed queue // for clients the failed to finish the process on this run
        For each client in processClients:
If finished is false: // checking the client field, finished is true when client done the request
Add to failed queue the client 
	if Running is true;
		continue;
	check if can satisfy the request if true:
		allocate resources
	else continue
 else: free the allocated resources
processClients = failed // all the failed clients will try to finish in next iteartion
