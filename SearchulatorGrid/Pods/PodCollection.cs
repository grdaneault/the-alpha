using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace SearchulatorGrid.Pods
{
    public class PodCollection : ObservableCollection<Pod>
    {
        private readonly List<Pod> _unsizedPods;
        
        public PodCollection(PodCollection other) : base(other)
        {
            
        }


        public PodCollection(IEnumerable<Pod> pods, int numberRows)
        {
            Pod.MaxRows = numberRows;

            var podList = pods as IList<Pod> ?? pods.ToList(); //Prevent multiple enumeration
            _unsizedPods = new List<Pod>();
            foreach (Pod p in podList)
            {
                p.ResetRowColSpans();
                _unsizedPods.Add(p);
            }


            List<Pod> sized = Fill(_unsizedPods.ToList(), 2);
            foreach (Pod p in sized)
            {
                Add(p);
            }
            Add(AdPod.CreateAd(AdPod.AdType.Normal));
        }

        private List<Pod> Fill(List<Pod> pods, int numCols)
        {
            var sized = new List<Pod>();
            bool first = true;
            while (pods.Count > 0)
            {
                var col = new PodGridConfiguration(Pod.MaxRows, numCols);

                while (pods.Count > 0 && col.Place(pods[0], pods[0].RowSpan, pods[0].ColSpan))
                {
                    pods.RemoveAt(0);
                }

                col.FillOut(false); //pods.Count == 0 && !first);
                first = false;
                sized.AddRange(col.GenerateFinalList());
            }


            return sized;
        }


        public void Resize(int numRows)
        {
            Pod.MaxRows = numRows;

            foreach (Pod p in _unsizedPods)
            {
                p.ResetRowColSpans();
            }

            List<Pod> sized = Fill(_unsizedPods.ToList(), 2);
            ClearItems();
            int i = 0;
            foreach (Pod p in sized)
            {
                InsertItem(i, p);
                i++;
            }
        }
    }
}