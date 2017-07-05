using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RateMatrix.xxx;
namespace RateMatrix
{
    
    public class KmeanClustering
    {
        hocaspnetEntities db = new hocaspnetEntities();
        List<DataPoint> _rawDataToCluster = new List<DataPoint>();
        //List<DataPoint> _rawDataToCluster = new List<DataPoint>();
        List<DataPoint> _clusters = new List<DataPoint>();
        public int _numberOfClusters = 10;
        public void InitilizeRawData()
        {
            if (_rawDataToCluster.Count == 0)
            {
                foreach (Customer customer in db.Customers)
                {
                    List<double> lstRate = new List<double>();
                    var rates = db.RateMatrices.Where(r => r.UserId == customer.Id).ToList();
                    foreach (xxx.RateMatrix item in rates)
                    {
                        lstRate.Add(double.Parse(item.Value.ToString()));
                    }
                    DataPoint dp = new DataPoint();
                    dp.UserId = customer.Id;
                    dp.RateList = lstRate;
                    _rawDataToCluster.Add(dp);
                }
            }
        }
        /// <summary>
        /// Hiển thị dữ liệu
        /// </summary>
        /// <param name="data"></param>
        /// <param name="decimals"></param>
        /// <param name="outPut"></param>
        public void ShowData(List<DataPoint> data, int decimals, TextBox outPut)
        {
            foreach (DataPoint dp in data)
            {
                outPut.Text += dp.ToString() + Environment.NewLine;
            }
        }
        /// <summary>
        /// Khởi tạo tâm
        /// </summary>
        public void InitializeCentroids()
        {
            Random random = new Random(_numberOfClusters);
            for (int i = 0; i < _numberOfClusters; ++i)
            {
                _rawDataToCluster[i].Cluster = i;
            }
            for (int i = _numberOfClusters; i < _rawDataToCluster.Count; i++)
            {
                _rawDataToCluster[i].Cluster = random.Next(0, _numberOfClusters);
            }
        }
        /// <summary>
        /// cập nhật tọa độ tâm
        /// </summary>
        /// <returns></returns>
        public bool UpdateDataPointMeans()
        {
            //nếu trống là nghỉ luôn
            if (EmptyCluster(_rawDataToCluster))
                return false;
            //nhóm các data poit theo nhóm random phía trên, mỗi cluster gồm nhiều datapoint
            var currentCluster = new List<DataPoint>();
            currentCluster = _clusters;
            var groupToComputeMeans = _rawDataToCluster.GroupBy(s => s.Cluster).OrderBy(s => s.Key);
            //khởi tạo lại số nhóm
            int clusterIndex = 0;
            //một item là 1 nhóm các datapoint có cluster thứ i
            foreach (var item in groupToComputeMeans)
            {
                //khởi tạo biến tạm để đấy
                List<double> temp = new List<double>();
                //biến giả tạm cho 1 thười điểm để tính tổng tất cả csac giá trị thời điểm đó
                double x = 0.0;
                //duyệt qua lần lượt cho đủ giá trị trong 1 ratelisst
                foreach (var dou in item.FirstOrDefault().RateList)
                {
                    x = 0.0;
                    int index = item.FirstOrDefault().RateList.IndexOf(dou);
                    foreach (var dataPoint in item)
                    {
                        x += dataPoint.RateList.ElementAt(index);
                    }
                    temp.Add(x / item.Count());
                }
                _clusters[clusterIndex].RateList = temp;
                clusterIndex++;
                x = 0.0;
            }
            return true;
        }
        /// <summary>
        /// kiểm tra xem có trống ko
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool EmptyCluster(List<DataPoint> data)
        {
            var emptyCluster =
            data.GroupBy(s => s.Cluster).OrderBy(s => s.Key).Select(g => new { Cluster = g.Key, Count = g.Count() });

            foreach (var item in emptyCluster)
            {
                if (item.Count == 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Công thứu Ơ Clits
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        public double ElucidanDistance(DataPoint dataPoint, DataPoint mean)
        {
            double _diffs = 0.0;
            foreach (var item in dataPoint.RateList)
            {
                int a = dataPoint.RateList.IndexOf(item);
                _diffs += Math.Pow(item-mean.RateList.ElementAt(a),2);
            }
            return Math.Sqrt(_diffs);
        }
        /// <summary>
        /// Cập nhật các con của 1 cụm
        /// </summary>
        /// <returns></returns>
        public bool UpdateClusterMembership()
        {
            bool changed = false;

            double[] distances = new double[_numberOfClusters];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _rawDataToCluster.Count; ++i)
            {

                for (int k = 0; k < _numberOfClusters; ++k)
                    distances[k] = ElucidanDistance(_rawDataToCluster[i], _clusters[k]);

                int newClusterId = MinIndex(distances);
                if (newClusterId != _rawDataToCluster[i].Cluster)
                {
                    changed = true;
                    _rawDataToCluster[i].Cluster = _rawDataToCluster[i].Cluster = newClusterId;
                    sb.AppendLine("Data Point >> UserID: " + _rawDataToCluster[i].UserId+ ",Rate: "+ _rawDataToCluster[i].RateList.Take(5) + " moved to Cluster # " + newClusterId);
                }
                else
                {
                    sb.AppendLine("No change.");
                }
                sb.AppendLine("------------------------------");
                Console.Write(sb.ToString());
            }
            if (EmptyCluster(_rawDataToCluster))
                return false;
            //if (changed)
            //{
            //    UpdateDataPointMeans();
            //}
            return changed;
        }
        /// <summary>
        /// xem cái nào gần nhất
        /// </summary>
        /// <param name="distances"></param>
        /// <returns></returns>
        public int MinIndex(double[] distances)
        {
            int _indexOfMin = 0;
            double _smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < _smallDist)
                {
                    _smallDist = distances[k];
                    _indexOfMin = k;
                }
            }
            return _indexOfMin;
        }
        /// <summary>
        /// Hàm chính để phân nhóm
        /// </summary>
        /// <param name="data"></param>
        /// <param name="numClusters"></param>
       
        /// <summary>
        /// thực thi phân nhóm
        /// </summary>
        public string ExcuteClustering()
        {
            InitilizeRawData();

            for (int i = 0; i < _numberOfClusters; i++)
            {
                _clusters.Add(new DataPoint() { Cluster = i });
            }

            Cluster(_rawDataToCluster, _numberOfClusters);
            StringBuilder sb = new StringBuilder();
            var group = _rawDataToCluster.GroupBy(s => s.Cluster).OrderBy(s => s.Key);
            foreach (var g in group)
            {
                sb.AppendLine("Cluster # " + g.Key + ":");
                foreach (var value in g)
                {
                    sb.Append(value.ToString());
                    sb.AppendLine();
                    var customer = db.Customers.Where(c => c.Id == value.UserId).FirstOrDefault();
                    //var rate = db.RateMatrices.Where(r => r.UserId == value.UserId).FirstOrDefault();
                    if (customer!=null)
                    {
                        customer.Cluster = value.Cluster;
                        //rate.Cluster = value.Cluster;
                        db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                        //db.Entry(rate).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                
                sb.AppendLine("------------------------------");
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.InnerException.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sb.ToString();
        }

        public void Cluster(List<DataPoint> data, int _numberOfClusters)
        {
            bool _changed = true;
            bool _success = true;
            InitializeCentroids();
            //3
            int maxIteration = data.First().RateList.Count-80;
            int _threshold = 0;
            //khi mà change == true tức là vẫn có thay đỏi thì còn chay. 
            //Chạy tới bao h change = false trức là ko thày đổi thì thôi
            while (_success && _changed && _threshold < maxIteration)
            {
                ++_threshold;
                _success = UpdateDataPointMeans();
                _changed = UpdateClusterMembership();
            }
        }
    }
}
