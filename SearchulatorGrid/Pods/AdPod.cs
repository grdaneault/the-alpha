namespace SearchulatorGrid.Pods
{
    class AdPod : Pod
    {
        public string AdUnit { get; set; }
        public string ApplicationId
        {
            get { return "d25517cb-12d4-4699-8bdc-52040c712cab"; }
        } //030470f5-0d60-41f7-bf50-b1058d83dcbd production

        public enum  AdType
        {
            Normal,
            Snapped
        }
        public AdPod(ImageResult image, string adUnit) : base("Advertisement", image)
        {            
            AdUnit = adUnit;
            RowSpan = 2;
            ColSpan = 1;
        }

        public static AdPod CreateAd(AdType type)
        {
            ImageResult i;
            switch (type)
            {
                case AdType.Normal:
                    i = new ImageResult("lala", 300, 250);
                    //return new AdPod(i, "10054695");
                    return new AdPod(i, "10043055");
                    
                case AdType.Snapped:
                    i = new ImageResult("", 292, 60);
                    return new AdPod(i, "10050472");
            }

            return null;
        }
    }
}
