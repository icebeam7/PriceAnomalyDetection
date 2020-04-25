using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using PriceAnomalyDetection.Models;
using PriceAnomalyDetection.Services;

using SkiaSharp;
using Microcharts;
using Xamarin.Forms;

namespace PriceAnomalyDetection.ViewModels
{
    public class AnomalyDetectorViewModel : BaseViewModel
    {
        public Command FindAnomaliesCommand { get; set; }
        public Command DetectStatusCommand { get; set; }

        private int sensitivity;

        public int Sensitivity
        {
            get { return sensitivity; }
            set { sensitivity = value; OnPropertyChanged(); }
        }

        private PriceInfo priceInfo;

        public PriceInfo PriceInfo
        {
            get { return priceInfo; }
            set { priceInfo = value; OnPropertyChanged(); }
        }

        private PriceResult priceResult;

        public PriceResult PriceResult
        {
            get { return priceResult; }
            set { priceResult = value; OnPropertyChanged(); }
        }

        private PriceStatus priceStatus;

        public PriceStatus PriceStatus
        {
            get { return priceStatus; }
            set { priceStatus = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Series> priceDataSeries;

        public ObservableCollection<Series> PriceDataSeries
        {
            get { return priceDataSeries; }
            set { priceDataSeries = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Anomaly> priceAnomalies;

        public ObservableCollection<Anomaly> PriceAnomalies
        {
            get { return priceAnomalies; }
            set { priceAnomalies = value; OnPropertyChanged(); }
        }

        private Chart priceChart;

        public Chart PriceChart
        {
            get { return priceChart; }
            set { priceChart = value; OnPropertyChanged(); }
        }

        public AnomalyDetectorViewModel()
        {
            GetPriceData();
            CreateChart(anomalies: false);
            FindAnomaliesCommand = new Command(async () => await FindAnomalies());
            DetectStatusCommand = new Command(async () => await DetectStatus());
        }

        private void GetPriceData()
        {
            var priceSeries = PriceDataService.GetPriceDataSeries().Take(30).ToList();
            PriceDataSeries = new ObservableCollection<Series>(priceSeries);

            Sensitivity = 95;

            PriceInfo = new PriceInfo()
            {
                granularity = "daily",
                maxAnomalyRatio = 0.25,
                sensitivity = Sensitivity,
                series = priceSeries
            };
        }

        private void CreateChart(bool anomalies)
        {
            PriceChart = new LineChart()
            {
                LineMode = LineMode.Spline,
                LabelTextSize = 0
            };

            PriceChart.Entries = PriceInfo.series.Select(
                (v, index) => new Microcharts.Entry(v.value)
                {
                    Color = !anomalies
                                ? SKColors.Green
                                : !PriceAnomalies.Any(x => x.Timestamp.ToShortDateString() == v.timestamp.ToShortDateString())
                                    ? SKColors.Green
                                    : SKColors.Red,
                    Label = v.timestamp.ToShortDateString()
                });
        }

        private async Task FindAnomalies()
        {
            IsBusy = true;

            PriceInfo.sensitivity = Sensitivity;
            PriceAnomalies = new ObservableCollection<Anomaly>();
            PriceResult = await AnomalyDetectorService.DetectAnomalies(priceInfo);

            if (PriceResult != null)
            {
                for (int i = 0; i < PriceResult.IsAnomaly.Length; i++)
                {
                    if (PriceResult.IsAnomaly[i])
                    {
                        var priceData = PriceInfo.series[i];

                        var lowerBoundary = PriceResult.ExpectedValues[i] - PriceResult.LowerMargins[i];
                        var upperBoundary = PriceResult.ExpectedValues[i] + PriceResult.UpperMargins[i];

                        PriceAnomalies.Add(new Anomaly()
                        {
                            Value = priceData.value,
                            Timestamp = priceData.timestamp,
                            IsPositive = PriceResult.IsPositiveAnomaly[i],
                            Range = $"Range: [{lowerBoundary:N2}, {upperBoundary:N2}]"
                        });
                    }
                }

                CreateChart(anomalies: true);
            }

            IsBusy = false;
        }

        private async Task DetectStatus()
        {
            IsBusy = true;

            PriceInfo.sensitivity = Sensitivity;
            PriceAnomalies = new ObservableCollection<Anomaly>();
            PriceStatus = await AnomalyDetectorService.DetectStatus(priceInfo);

            if (PriceStatus != null)
            {
                if (PriceStatus.IsAnomaly)
                {
                    var priceData = PriceInfo.series.Last();

                    var lowerBoundary = PriceStatus.ExpectedValue - PriceStatus.LowerMargin;
                    var upperBoundary = PriceStatus.ExpectedValue + PriceStatus.UpperMargin;

                    PriceAnomalies.Add(new Anomaly()
                    {
                        Value = priceData.value,
                        Timestamp = priceData.timestamp,
                        IsPositive = PriceStatus.IsPositiveAnomaly,
                        Range = $"Range: [{lowerBoundary:N2}, {upperBoundary:N2}]"
                    });
                }

                CreateChart(anomalies: true);
            }

            IsBusy = false;
        }
    }
}
