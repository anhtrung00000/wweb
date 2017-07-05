using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mommo.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Policy;
using RateMatrix.xxx;
using Newtonsoft.Json;

namespace RateMatrix
{
    public partial class MainForm : Form
    {
        hocaspnetEntities db = new hocaspnetEntities();
        public MainForm()
        {
            InitializeComponent();
            InitRateMatrix();
            //
            NCBestSellingForNC();
            TCBestSellingForTC();
            //
            TCPurchasedForTC();
            TCNotPurchasedForTC();
            //
            TCNewItemForTC();
            NCNewItemForNewCLient();
        }
        /// <summary>
        /// hàm khởi tạo các giá tri cho ma trận rate
        /// </summary>
        private void InitRateMatrix()
        {
            db.Database.ExecuteSqlCommand("Truncate table RateMatrix");
            int rowNum = db.Customers.Count();
            int columnNum = db.Products.Count();
            string[] colnames = new string[columnNum];

            float[,] rateArr = new float[rowNum, columnNum];
            int i = 0;
            foreach (Customer customer in db.Customers.ToList())
            {
                int j = 0;
                foreach (Product product in db.Products.ToList())
                {
                    if (i == 0)
                    {
                        colnames[j] = string.Format("-I{0}-", j);
                    }
                    int slt = 0;
                    int slg = 0;
                    var orderInfor = db.Orders.Where(o => o.CustomerId == customer.Id)
                        .Join(db.OrderDetails, o => o.Id, od => od.OrderId, (o, od) => new
                        {
                            o.Id,
                            o.CustomerId,
                            od.ProductId,
                            od.Quantity,
                            o.Amount
                        })
                       .Where(n => n.ProductId == product.Id);
                    if (orderInfor.Count() > 0)
                    {
                        slt = orderInfor.Count();
                        slg = orderInfor.Sum(o => o.Quantity);
                    }
                    rateArr[i, j] = (float)(slt + slg) / 2;
                    xxx.RateMatrix rate = new xxx.RateMatrix();
                    rate.UserId = customer.Id;
                    rate.ProductId = product.Id;
                    rate.Value = rateArr[i, j];
                    db.RateMatrices.Add(rate);
                    j++;
                }
                i++;
            }
            try
            {
                db.SaveChanges();
                dataGridView1.DataSource = new ArrayDataView(rateArr, colnames);
            }
            catch (Exception ex)
            {
                string a = ex.Message + "" + ex.InnerException;
            }

        }
        /// <summary>
        /// hiển thị phân nhóm khách hàng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCluster_Click(object sender, EventArgs e)
        {
            var kmean = new Kmean();
            kmean.Show();
        }

        /// <summary>
        /// hàm xuát ra gợi ý best selling cho khác hàng đã thực hiện giao dịc trong hệ thống.
        /// </summary>
        private void TCBestSellingForTC()
        {
            //khách ahgn đã có tài khoản trên hệ thống
            var tranditionalClient = db.Customers.Where(c => db.Orders.Select(o => o.CustomerId).Contains(c.Id));
            //lấy số lượng toàn hệ thống
            int totalQuantity = db.OrderDetails.Sum(o => o.Quantity);
            //các mặt hàng đã mua
            var purchase = db.OrderDetails.Select(o => new { o.ProductId, o.Quantity }).GroupBy(o => o.ProductId);
            //nhóm các mặt hàng đã mua
            var purchasedCategory = db.Products.Join(purchase,p=>p.Id,pu=>pu.Key,(p,pu)=>p).Select(p=>p.CategoryId).Distinct();
            //list các priority
            Dictionary<int, double> priorityDictionary = new Dictionary<int, double>();
            foreach (var item in purchase)
            {
                int total = item.Sum(o => o.Quantity);
                double rate = (double)total / totalQuantity;
                //mặt hàng tương ứng
                var product = db.Products.Find(item.Key);
                if (purchasedCategory.ToList().Contains(product.CategoryId))
                {
                    rate = rate*0.1;
                }
                priorityDictionary.Add(item.Key, rate);
            }
            //duyệt qua 1 lượt tất cả các khách hàng mới
            foreach (var customer in tranditionalClient)
            {
                var x = priorityDictionary.OrderByDescending(o => o.Value);
                customer.TCBestSellingForTC = JsonConvert.SerializeObject(x);
                db.Entry(customer).State = EntityState.Modified;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// hàm xuất ra gợi ý BS cho khác hàng chưa thực heien bất cứ giao dịch nào
        /// </summary>
        private void NCBestSellingForNC()
        {
            //lấy danh sách khách ahngf mới
            var newClients = db.Customers.Where(c=> !db.Orders.Select(o => o.CustomerId).Contains(c.Id));
            //lấy số lượng toàn hệ thống
            int totalQuantity = db.OrderDetails.Sum(o => o.Quantity);
            //các ặmt hàng đã bán
            var purchase = db.OrderDetails.Select(o=> new { o.ProductId,o.Quantity }).GroupBy(o => o.ProductId);
            //list các priority
            Dictionary<int,double> priorityDictionary = new Dictionary<int, double>();
            //duyệt qua tất cả các hàng đã bán
            foreach (var item in purchase)
            {
                int total = item.Sum(o => o.Quantity);
                double rate = (double) total/totalQuantity;
                priorityDictionary.Add(item.Key, rate);
            }
            //duyệt qua 1 lượt tất cả các khách hàng mới
            foreach (var customer in newClients)
            {
                var x = priorityDictionary.OrderByDescending(o => o.Value);
                customer.NCBestSellingForNC = JsonConvert.SerializeObject(x);
                db.Entry(customer).State = EntityState.Modified;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                    
            }
        }
        /// <summary>
        /// hàm xuát gợi ý hangf đã mua cho khác hàng truyền tống
        /// </summary>
        private void TCPurchasedForTC()
        {
            //tính toán tốc độ sử dụng hàng
            //danh sách khách ahngf truyền thống
            var tranditionalClient = db.Customers.Join(db.Orders,
                                                       c => c.Id,
                                                       o => o.CustomerId,
                                                       (c, o) =>
                                                       new
                                                       {
                                                           OrderID = o.Id,
                                                           o.CustomerId,
                                                           c.Fullname,
                                                           o.RequireDate
                                                       });
            //danh sahcs  các mặt hàng đã được mua
            var purchasedList = tranditionalClient.Join(db.OrderDetails,
                                                        t => t.OrderID,
                                                        od => od.OrderId,
                                                        (t, od) =>
                                                        new
                                                        {
                                                            t.OrderID,
                                                            t.CustomerId,
                                                            t.Fullname,
                                                            t.RequireDate,
                                                            od.ProductId,
                                                            od.Quantity
                                                        }).GroupBy(p => p.CustomerId).OrderBy(s => s.Key);

            //các mặt hàng đã mua của các khách hàng tương ứng.
            Dictionary<int, float> priorityList = new Dictionary<int, float>();
            string customerId = string.Empty;
            foreach (var purchased in purchasedList)
            {
                customerId = purchased.Key;
                var products = purchased.GroupBy(pu => pu.ProductId).OrderBy(s => s.Key);
                //tính toán ra các giá trị trung bình của cá cmawjt hàng đã mua
                foreach (var item in products)
                {
                    var productid = item.Key;
                    var minDate = item.Min(i => i.RequireDate);
                    var myDate = DateTime.Now;
                    var newDate = myDate.AddYears(-1);
                    if (minDate < newDate)
                    {
                        minDate = newDate;
                    }
                    var res = item.Where(i => i.RequireDate >= minDate).Sum(i => i.Quantity);
                    //giá trị trung bình trên 1 năm
                    float avg = (float)res / 365;
                    //lần mua gần đây nhất là
                    var lastOrder = item.OrderByDescending(i => i.RequireDate).First();
                    float priority = float.MinValue;
                    if (lastOrder.RequireDate >= minDate)
                    {
                        //thời gian mua
                        DateTime lastTime = lastOrder.RequireDate;
                        //số lượng sử dụng lý thuyết
                        int totalDay = (int)Math.Round(Math.Abs((DateTime.Now - lastTime).TotalDays), MidpointRounding.AwayFromZero);
                        float fakeQuantity = avg * totalDay;
                        //sô sluwonjg mua gần nhất
                        int lastQuantity = lastOrder.Quantity;
                        //độ chênh lệch giữa lý thuyệt và thực tế
                        float dateRate = Math.Abs(fakeQuantity - lastQuantity);
                        //lưu lại
                        priorityList.Add(productid, dateRate);
                    }
                }

                string pris = JsonConvert.SerializeObject(priorityList);
                var customer = db.Customers.Find(customerId);
                if (customer != null)
                {
                    customer.TCPurchasedForTC = pris;
                    db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                MessageBox.Show(ex.InnerException.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// hàm gợi ý hàng chưa mua cho TC
        /// </summary>
        private void TCNotPurchasedForTC()
        {
            //nhóm các khách hàng theo kmean
            var groups = db.Customers.GroupBy(c => c.Cluster).OrderBy(c => c.Key);
            //duyệt qua các nhóm khách hàng
            foreach (var group in groups)
            {
                //mảng rate của nhóm hiện tại
                var thisMatrix = group.Join(db.RateMatrices, g => g.Id, r => r.UserId, (g, r) => r);
                //mảng các sản phẩm được mua bởi nhóm
                var rateMatrices = thisMatrix as IList<xxx.RateMatrix> ?? thisMatrix.ToList();
                //các sản phẩm được mua bởi nhóm hiện tại
                var products = rateMatrices.Join(db.Products, r => r.ProductId, p => p.Id, (r, p) => r).Select(r => r.ProductId).Distinct();
                //khai báo 1 mảng 2 chiều để lưu giá trị rate
                double[,] rateMatrix = new double[group.Count(), rateMatrices.Count()];
                //list cac sanr pham chưa được mua.
                var notPurchased = new List<int>();
                var purchased = new List<int>();
                //duyệt qua tất cả các khách hàng trong từng group
                //khai báo biến i để load giá trị vào mảng 2 chiều
                int i = 0;
                foreach (var customer in group)
                {
                    int j = 0;
                    foreach (var product in products)
                    {
                        var value = db.RateMatrices.First(r => r.UserId == customer.Id && r.ProductId == product).Value;
                        if (value != null)
                        {
                            rateMatrix[i, j] = (double)value;
                            //nếu giá trị khác không tức là sanr phẩm đã được mua. nhét nó vào list các sản phẩm đã mua
                            if (value != 0)
                            {
                                if (product != null) purchased.Add((int)product);
                            }
                            else
                            {
                                //nếu nó bằng 0 tức là chưa hề có rate value, nhét nó vào list các sản phẩm chưa mua
                                if (product != null) notPurchased.Add((int)product);
                            }
                        }
                    }
                    //đã có được 2 list đã mua và chưua mua. tiến hành tính khoảng cách từu các item chưa mua tới các item đã mua.
                    Dictionary<int, double> compareDic = new Dictionary<int, double>();
                    foreach (var notItem in notPurchased.Distinct())
                    {
                        //các giá trị rate của hàng chưa được mua
                        var notValue = rateMatrices.Where(p => p.ProductId == notItem).Select(p => p.Value).ToList<Nullable<double>>();
                        double distance = 0.0;
                        foreach (var item in purchased.Distinct())
                        {
                            //các giá trị rate đã mua
                            var value = rateMatrices.Where(p => p.ProductId == item).Select(p => p.Value).ToList<Nullable<double>>();
                            //tính tổng khoảng các giữa 1 notItem và tất cả các sản phẩm đã bán
                            distance += ElucidanDistance(notValue, value);
                        }//kết thúc sẽ là tổng khoảng cáhc từ 1 item tới các nót item

                        compareDic.Add(notItem, distance);
                    }
                    var x = compareDic.OrderByDescending(c => c.Value);
                    customer.TCNotPurchasedForTC = JsonConvert.SerializeObject(x);
                    db.Entry(customer).State = EntityState.Modified;
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.InnerException.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// hàm đưa ra gợi ý cho khách hàng tryueefn thống
        /// </summary>
        private void TCNewItemForTC()
        {
            var myDate = DateTime.Now;
            var newDate = myDate.AddYears(-1);
            var newItems = db.Products.Where(p => !db.OrderDetails.Select(o => o.ProductId).Contains(p.Id)).Where(p=>p.ProductDate >= newDate).ToList();
           
            //cách mới
            //nhóm các khách hàng theo kmean
            var groups = db.Customers.GroupBy(c => c.Cluster).OrderBy(c => c.Key);
            //duyệt qua các nhóm khách hàng
            foreach (var group in groups)
            {
                ////luuw trữ các gợi ý
                Dictionary<int, int> priorityList = new Dictionary<int, int>();
                //mảng rate của nhóm hiện tại
                var thisMatrix = group.Join(db.RateMatrices, g => g.Id, r => r.UserId, (g, r) => r);
                //var thisMatrix = db.RateMatrices.Where(r=> )
                //mảng các sản phẩm được mua bởi nhóm
                var rateMatrices = thisMatrix as IList<xxx.RateMatrix> ?? thisMatrix.ToList();
                //các sản phẩm được mua bởi nhóm hiện tại
                var products = rateMatrices.Join(db.Products, r => r.ProductId, p => p.Id, (r, p) => r).Select(r => r.ProductId).Distinct();
                //khai báo 1 mảng 2 chiều để lưu giá trị rate
                double[,] rateMatrix = new double[group.Count(), products.Count()];
                //các mhóm sản phẩm đã được mua
                var purchased = new List<int>();
                //duyệt qua tất cả các khách hàng trong từng group
                //khai báo biến i để load giá trị vào mảng 2 chiều
                int i = 0;
                foreach (var customer in group)
                {
                    purchased.Clear();
                    priorityList.Clear();
                    int j = 0;
                    foreach (var product in products)
                    {
                        var value = db.RateMatrices.First(r => r.UserId == customer.Id && r.ProductId == product).Value;
                        if (value != null)
                        {
                            rateMatrix[i, j] = (double)value;
                            //nếu giá trị khác không tức là sanr phẩm đã được mua. nhét nó vào list các sản phẩm đã mua
                            if (value != 0)
                            {
                                var cate = db.Products.First(p => p.Id == product).CategoryId;
                                if (product != null)
                                    if (!purchased.Contains(cate))
                                    {
                                        purchased.Add((int)cate);
                                    }
                            }
                        }
                    }
                    //thu được các loại hàng được uwu tiên
                    if (newItems.Count > 0)
                    {
                        foreach (var item in newItems)
                        {
                            //các giá trị rate của hàng chưa được mua
                            if (purchased.Contains(item.CategoryId))
                            {
                                //1 được uuw tiên
                                priorityList.Add(item.Id, 1);
                            }
                            else
                            {
                                //0 là ko có gì
                                priorityList.Add(item.Id, 0);
                            }
                        }
                        var x = priorityList.OrderByDescending(c => c.Value);
                        customer.TCNewItemForTC = JsonConvert.SerializeObject(x);
                        db.Entry(customer).State = EntityState.Modified;
                    }
                    else
                    {
                        customer.TCNewItemForTC = "{}";
                        db.Entry(customer).State = EntityState.Modified;
                    }                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.InnerException.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// hàm đưa ra gợi ý new items cho khách hàng chưa mua mặt hàng nào - new clients
        /// </summary>
        private void NCNewItemForNewCLient()
        {
            //thời gian giới hạn là 12 tháng gần đây
            var myDate = DateTime.Now;
            var newDate = myDate.AddYears(-1);
            //hàng mới được lấy trên toàn hệ thống
            var newItems = db.Products.Where(p => !db.OrderDetails.
                                                                   Select(o => o.ProductId).
                                                                   Contains(p.Id))
                                      .Where(p => p.ProductDate >= newDate).ToList();
            //hiện thực ngay trong source web
        }
        #region công thức công củng

        public double ElucidanDistance(List<Nullable<double>> no, List<Nullable<double>> yes)
        {
            double _diffs = 0.0;
            foreach (var item in no)
            {
                int a = no.IndexOf(item);
                var d = item - yes.ElementAt(a);
                if (d != null) _diffs += Math.Pow((double)d, 2);
            }
            return Math.Sqrt(_diffs);
        }
        #endregion

    }
}
