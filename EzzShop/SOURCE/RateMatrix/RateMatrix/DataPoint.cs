using System.Collections.Generic;
using System.Text;

namespace RateMatrix
{
    public class DataPoint
    {
        public string UserId { get; set; }
        public List<double> RateList { get; set; }
        public int Cluster { get; set; }
        public DataPoint()
        {

        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in RateList)
            {
                sb.Append(item + ", ");
            }
            return "UserID: " + UserId+", " +sb.ToString();
        }
    }
}