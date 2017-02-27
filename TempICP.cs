using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICPClientLinq
{
    public class TempICP
    {
        public TempICP()
        {
            this.errorList = new List<string>();
            this.warningList = new List<string>();
            this.elements = new SortedList<string, decimal?>();
        }

        public int? sampleID { get; set; }
        public string barCode { get; set; }
        public string boxNumber { get; set; }
        public string duplicate { get; set; }
        public string code { get; set; }
        public DateTime anaDate { get; set; }
        public string comments { get; set; }

        public SortedList<string, decimal?> elements { get; set; }
        public List<string> errorList { get; set; }
        public List<string> warningList { get; set; }
    }

    /// <summary>
    /// Comparer class for TempICP objects
    /// </summary>
    public class CompareTempICP : IComparer<TempICP>
    {
        private string compareBy = "BarCode";

        public CompareTempICP(string cBy)
        {
            compareBy = cBy;
        }

        public int Compare(TempICP icp1, TempICP icp2)
        {
            int retVal = 0;

            switch (compareBy)
            {
                case "BarCode":
                    retVal = string.Compare(icp1.barCode, icp2.barCode);
                    break;
                case "AnaDate":
                    retVal = DateTime.Compare(icp1.anaDate, icp2.anaDate);
                    break;
                case "BoxNumber":
                    retVal = string.Compare(icp1.boxNumber, icp2.boxNumber);
                    break;
                case "Sample":
                    double diff = Convert.ToDouble(icp1.code) - Convert.ToDouble(icp2.code);
                    retVal = (diff == 0) ? string.Compare(icp1.duplicate, icp2.duplicate) :
                             (diff > 0) ? 1 : -1;
                    break;

                default:
                    retVal = 0;
                    break;
            }
            return retVal;
        }
    }

}
