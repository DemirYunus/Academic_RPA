namespace RPA.GRASP
{
    public class TimeHorizon
    {
        public int Start { get; set; }
        public int End { get; set; }

        public TimeHorizon(int start, int end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"{Start}-{End}";
        }
    }

    public class ResourceScheduler
    {
        private const int HorizonStart = 0;
        private const int HorizonEnd = 1440;

        // Kaynakların boş zamanlarını (idle times) tutan koleksiyon
        private Dictionary<string, List<TimeHorizon>> _idleTimes;

        public ResourceScheduler(IEnumerable<string> resources)
        {
            _idleTimes = new Dictionary<string, List<TimeHorizon>>();

            foreach (var res in resources)
            {
                // Başlangıçta tüm ufuk (0-1440) boş kabul edilir
                _idleTimes[res] = new List<TimeHorizon> { new TimeHorizon(HorizonStart, HorizonEnd) };
            }
        }

        // Yeni operasyon ekleyen ve boş zamanları güncelleyen metot
        public void AddOperation(string resourceId, int opStart, int opEnd)
        {
            if (!_idleTimes.ContainsKey(resourceId))
            {
                throw new ArgumentException("Geçersiz kaynak kimliği.");
            }

            var currentIdleTimes = _idleTimes[resourceId];
            var newIdleTimes = new List<TimeHorizon>();

            foreach (var idle in currentIdleTimes)
            {
                // Eğer operasyon mevcut boşlukla kesişmiyorsa, boşluğu aynen koru
                if (opEnd <= idle.Start || opStart >= idle.End)
                {
                    newIdleTimes.Add(idle);
                }
                else
                {
                    // Kesişme durumu: Boşluğu parçala (öncesi ve sonrası olarak)
                    if (idle.Start < opStart)
                    {
                        newIdleTimes.Add(new TimeHorizon(idle.Start, opStart));
                    }
                    if (idle.End > opEnd)
                    {
                        newIdleTimes.Add(new TimeHorizon(opEnd, idle.End));
                    }
                }
            }

            // Güncellenmiş boşlukları zamana göre sıralayarak kaydet
            _idleTimes[resourceId] = newIdleTimes.OrderBy(x => x.Start).ToList();
        }

        // İstendiğinde bir kaynağın boş zamanlarına erişmek için kullanılan metot
        public List<TimeHorizon> GetIdleTimes(string resourceId)
        {
            if (_idleTimes.ContainsKey(resourceId))
            {
                return _idleTimes[resourceId];
            }
            return new List<TimeHorizon>();
        }
    }
}
