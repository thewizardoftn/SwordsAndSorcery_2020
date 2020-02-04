using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class NewsRepository
    {
        private List<NewsType> _news;
        public List<NewsType> NewsList { get { return _news; } set { _news = value; } }
        public int UserID { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }

        public NewsRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
            DataTable dt = new DataTable();

            using (Data DC = new Data("conn", Page, Process))
            {

                _news = new List<NewsType>();

                string sSQL = "SELECT * FROM News order by NewsDate DESC";
                DC.AddCommand(CommandType.Text, sSQL);
                try
                {
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
                    string item = dr["NewsItem"].ToString().Replace("[", "<");
                    item = item.Replace("]", ">");
                    _news.Add(new NewsType
                    {
                        NewsItem = item,
                        NewsDate = Utils.ParseDateControlledReturn(dr["NewsDate"]),
                        ID = (int)Utils.ParseNumControlledReturn(dr["ID"])
                    });
                }
            }

        }
        public string InsertNews(string date, string content)
        {
            string sRet = "Success!";
                string sSQL = "INSERT INTO News (NewsDate, NewsItem) Values(@NewsDate, @NewsItem);";
            using (Data DC = new Data("conn", Page, Process))
            {

                DC.AddCommand(CommandType.Text, sSQL);
                DC.AttachParameterByValue("NewsDate", date);
                DC.AttachParameterByValue("NewsItem", content);

                try
                {

                    DC.ExecuteCommand();


                }
                catch (Exception ex)
                {
                    sRet = ex.Message;
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }
            }
            return sRet;
        }

        public List<NewsType> GetNews()
        {
            List<NewsType> news = new List<NewsType>();
            DataTable dt = new DataTable();
            string sSQL = "SELECT * FROM News order by NewsDate DESC";

            using (Data DC = new Data("conn", Page, Process))
            {
                DC.AddCommand(CommandType.Text, sSQL);
                try
                {
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

                try
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string item = dr["NewsItem"].ToString().Replace("[", "<");
                            item = item.Replace("]", ">");
                            news.Add(new NewsType
                            {
                                NewsItem = item,
                                NewsDate = Utils.ParseDateControlledReturn(dr["NewsDate"]),
                                ID = (int)Utils.ParseNumControlledReturn(dr["ID"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);
                }
            }
            return news;
        }
    }
}
