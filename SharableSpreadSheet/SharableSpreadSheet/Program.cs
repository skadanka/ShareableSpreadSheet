using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Threading;


namespace shareableSpreadSheet {
   public class SharableSpreadSheet
    {
        int numOfUsers;
        int nRows;
        int nCols;
        string[,] spreadSheet;


        Dictionary<int, RequestHandler> lockedRows;
        Dictionary<int, RequestHandler> lockedCols;

        Mutex accsesMutex;
        Mutex releaseMutex;


        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
            // construct a nRows*nCols spreadsheet
            this.spreadSheet = new string[nRows, nCols];
            this.lockedRows = new Dictionary<int, RequestHandler>();
            this.lockedCols = new Dictionary<int, RequestHandler>();
            this.accsesMutex = new Mutex();
            this.releaseMutex = new Mutex();
        }

        public void RequestAccessRowCol(int row, int col)
        {
            accsesMutex.WaitOne();
            accsesMutex.ReleaseMutex();
            Thread req1 = new Thread(() => RequestAccessRow(row));
            Thread req2 = new Thread(() => RequestAccessCol(col));

            req1.Start();
            req2.Start();
            req1.Join();
            req2.Join();

        }

        public void RequestAccessRowRow(int row1, int row2)
        {
            accsesMutex.WaitOne();
            accsesMutex.ReleaseMutex();
            Thread req1 = new Thread(() => RequestAccessRow(row2));
            Thread req2 = new Thread(() => RequestAccessRow(row1));

            req1.Start();
            req2.Start();
            req1.Join();
            req2.Join();
        }

        public void RequestAccessColCol(int col1, int col2)
        {
            accsesMutex.WaitOne();
            accsesMutex.ReleaseMutex();
            Thread req1 = new Thread(() => RequestAccessCol(col1));
            Thread req2 = new Thread(() => RequestAccessCol(col2));

            req1.Start();
            req2.Start();
            req1.Join();
            req2.Join();
        }

        public void ReleaseAccessRowCol(int row, int col)
        {
            ReleaseAccessRow(row);
            ReleaseAccessCol(col);
        }

        public void ReleaseAccessRowRow(int row1, int row2)
        {
            ReleaseAccessRow(row1);
            ReleaseAccessRow(row2);
        }

        public void ReleaseAccessColCol(int col1, int col2)
        {
            ReleaseAccessCol(col1);
            ReleaseAccessCol(col2);
        }

        public void RequestAccessRow(int row) {
            RequestHandler handler;
            if (lockedRows.TryGetValue(row, out handler))
            {
                handler.Request();
            }
            else
            {
                lockedRows.Add(row, handler = new RequestHandler());
                handler.Request();
            }
        }

        public void ReleaseAccessRow(int row)
        {
            RequestHandler handler;
            if(lockedRows.TryGetValue(row, out handler))
                handler.done();           
        }

        public void ReleaseAccessCol(int col)
        {
            RequestHandler handler;
            if (lockedCols.TryGetValue(col, out handler))
                handler.done();
        }

        public void RequestAccessCol(int col) {
            RequestHandler handler;
            if (lockedCols.TryGetValue(col, out handler))
            {
                handler.Request();
            }
            else
            {
                lockedCols.Add(col, handler = new RequestHandler());
                handler.Request();
            }
        }
        
        
        public String getCell(int row, int col)
        {
            // return the string at [row,col]
            // The logic is to wait for both request to be given, therefore maybe threads for each request will be needed
            // maybe access not needed and just return the requested cell will be responsive enough

            RequestAccessRow(row);
            RequestAccessCol(col);
            String str = spreadSheet[row, col];
            ReleaseAccessRow(row);
            ReleaseAccessCol(col);
            return str;
        }

        public void setCell(int row, int col, String str)
        {
            // set the string at [row,col]
            RequestAccessRow(row);
            RequestAccessCol(col);
            spreadSheet[row, col] = str;
            ReleaseAccessRow(row);
            ReleaseAccessCol(col);
        }

        public Tuple<int, int> searchString(String str)
        {
            int row = 0, col = 0;
            bool flag = false;
            // return first cell indexes that contains the string (search from first row to the last row)
            for (; row < nRows; row++)
            {
                RequestAccessRow(row);
                for (; col < nCols; col++)
                {
                    RequestAccessCol(col);
                    if (spreadSheet[row, col].CompareTo(str) == 0)
                    {
                        ReleaseAccessCol(col);
                        break;
                    }
                    ReleaseAccessCol(col);
                }
                ReleaseAccessRow(row);
                if (flag)
                    break;
            }
            Tuple<int, int> cell = new Tuple<int, int>(row, col);
            return cell;
        }

        public void exchangeRows(int row1, int row2)
        {
            // exchange the content of row1 and row2

        }

        public void exchangeCols(int col1, int col2)
        {
            // exchange the content of col1 and col2
        }

        public int searchInRow(int row, String str)
        {
            int col;
            // perform search in specific row
            RequestAccessRow(row);
            for (col = 0; col < nCols; col++)
            {
                if (spreadSheet[row, col].CompareTo(str) == 0)
                {
                    ReleaseAccessRow(row);
                    return col;
                }
            }
            ReleaseAccessRow(row);
            return -1;
        }

        public int searchInCol(int col, String str)
        {
            int row;
            // perform search in specific col
            RequestAccessCol(col);
            for(row = 0; row < nRows; row++)
            {
                if(spreadSheet[row, col].CompareTo(str) == 0)
                {
                    ReleaseAccessCol(col);
                    return row;
                }
            }
            ReleaseAccessCol(col);
            return row;
        }

        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
        {
            int row, col;
            // perform search within spesific range: [row1:row2,col1:col2] 
            //includes col1,col2,row1,row2
            for (row = row1; row < row2; row++)
                for (col = col1; col < col2; col++)
                    if (getCell(row, col).CompareTo(str) == 0)
                        return new Tuple<int, int>(row, col);
            return new Tuple<int, int>(-1, -1);
        }

        public void addRow(int row1)
        {
            //add a row after row1
            accsesMutex.WaitOne();
            // Resizing the array rows code
            accsesMutex.ReleaseMutex();
        }
        public void addCol(int col1)
        {
            //add a column after col1
            accsesMutex.WaitOne();
            // Resizing the array columns code
            accsesMutex.ReleaseMutex();
        }

        public Tuple<int, int>[] findAll(String str, bool caseSensitive)
        {
            // perform search and return all relevant cells according to caseSensitive param
            List<Tuple<int, int>> cell_tup_arr = new List<Tuple<int, int>>();
            String strcmp;
            for(int row = 0; row < nRows; row++)
                for(int col =0; col < nCols; col++)
                {
                    strcmp = getCell(row, col);
                    if (caseSensitive) {
                        if (strcmp.CompareTo(str) == 0)
                            cell_tup_arr.Add(new Tuple<int, int>(row, col));
                    }
                    else
                    {
                        if (String.Compare(strcmp, str, true) == 0)
                            cell_tup_arr.Add(new Tuple<int, int>(row, col));
                    }
                }
            return cell_tup_arr.ToArray();
        }

        public void setAll(String oldStr, String newStr, bool caseSensitive)
        {
            // replace all oldStr cells with the newStr str according to caseSensitive param
            Tuple<int, int>[] oldstr = findAll(oldStr, caseSensitive);
            foreach(Tuple<int, int> tup in oldstr)
                setCell(tup.Item1, tup.Item2, newStr);       
        }

        public Tuple<int, int> getSize()
        {
            // return the size of the spreadsheet in nRows, nCols
            return new Tuple<int, int>(nRows, nCols);
        }

        public void setConcurrentSearchLimit(int nUsers)
        {
            // this function aims to limit the number of users that can perform the search operations concurrently.
            // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
            // In this case additional search operations will wait for existing search to finish.
            // This function is used just in the creation
        }

        public void save(String fileName)
        {
            // save the spreadsheet to a file fileName.
            // you can decide the format you save the data. There are several options.
        }

        public void load(String fileName)
        {
            // load the spreadsheet from fileName
            // replace the data and size of the current spreadsheet with the loaded data
        }
    }
}