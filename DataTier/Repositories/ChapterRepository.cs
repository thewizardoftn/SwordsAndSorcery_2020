using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class ChapterRepository
    {
        private List<ChapterType> books;
        public List<ChapterType> Books { get { return books; } set { value = books; } }
        public string Page { get; set; }
        public string Process { get; set; }


        public ChapterRepository(string BookName, int UserID, string page, string process)
        {
            Page = page;
            Process = process;
            books = new List<ChapterType>();

            DataTable dt = new DataTable();
            string sSQL = "Select * FROM dbo.Books WHERE BookName = @BookName Order By displayorder";

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("BookName", BookName);
                    dt = DC.ExecuteCommandForDT();
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }
            }
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string item = dr["Chapter"].ToString().Replace("[", "<");
                    item = item.Replace("]", ">");
                    books.Add(new ChapterType
                    {
                        Chapter = item,
                        BookName = dr["BookName"].ToString(),
                        ChapterName = dr["ChapterName"].ToString(),
                        Title = dr["Title"].ToString(),
                        DisplayOrder = (int)Utils.ParseNumControlledReturn(dr["DisplayOrder"])
                    });
                }
            }
        }
    }
}
