using System;
using System.Reactive.Linq;
using Acr.DeviceInfo.Internals;
using Android.Content;
using Android.OS;


namespace Acr.DeviceInfo
{

    public class BatteryImpl : IBattery
    {
        readonly IDisposable chargeOb;
        readonly IDisposable statOb;


        public BatteryImpl() 
        {
            this.chargeOb = this
                .WhenPowerStatusChanged()
                .Subscribe(x => this.Status = x);

            this.chargeOb = this
                .WhenBatteryPercentageChanged()
                .Subscribe(x => this.Percentage = x);
            //this.WhenBatteryPercentageChanged().FirstOrDefault();
            //this.WhenPowerStatusChanged().FirstOrDefault();
        }


        public int Percentage { get; private set; }
        public PowerStatus Status { get; private set; }


        public IObservable<int> WhenBatteryPercentageChanged()
        {
            return AndroidObservables
                .WhenIntentReceived(Intent.ActionBatteryChanged)
                .Select(intent =>
                {
                    var level = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);
                    var scale = intent.GetIntExtra(BatteryManager.ExtraScale, -1);
                    var value = (int)Math.Floor(level * 100D / scale);
                    return value;
                });
        }


        public IObservable<PowerStatus> WhenPowerStatusChanged()
        {
            return AndroidObservables
                .WhenIntentReceived(Intent.ActionBatteryChanged)
                //.WhenIntentReceived(Intent.ActionPowerConnected, Intent.ActionPowerDisconnected)
                .Select(intent =>
                {
                    var status = (BatteryStatus)intent.GetIntExtra(BatteryManager.ExtraStatus, -1);
                    switch (status)
                    {
                        case BatteryStatus.Discharging:
                        case BatteryStatus.NotCharging:
                            return PowerStatus.Discharging;

                        case BatteryStatus.Charging:
                            return PowerStatus.Charging;

                        case BatteryStatus.Full:
                            return PowerStatus.Charged;

                        default:
                            return PowerStatus.Unknown;
                    }
                });
        }
    }
}