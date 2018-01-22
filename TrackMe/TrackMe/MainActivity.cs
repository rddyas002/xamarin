using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using System.Text;
using System.Globalization;
using static Android.Views.View;
using Android.Views;
using System.Net.Mqtt;
using System.Reactive.Linq;

namespace TrackMe
{
    [Activity(Label = "TrackMe", MainLauncher = true)]
    public class MainActivity : Activity, ILocationListener
    {
        static readonly string TAG = "X:" + typeof(Activity).Name;
        Location _currentLocation;
        LocationManager _locationManager;
        string _locationProvider;
        TextView _locationText;
        static CultureInfo ci = new CultureInfo("en-GB", true);

        public void OnLocationChanged(Location location)
        {
            StringBuilder jsonPacket = new StringBuilder();
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                jsonPacket.Append("{\"id\":\"S6\",");
                jsonPacket.Append("\"position\":[");
                jsonPacket.AppendFormat(ci, "{0},{1},{2}],", _currentLocation.Latitude, _currentLocation.Longitude, _currentLocation.Altitude);
                jsonPacket.AppendFormat(ci, "\"accuracy\":{0},", _currentLocation.Accuracy);
                jsonPacket.AppendFormat(ci, "\"provider\":{0}}}", _currentLocation.Provider);
                _locationText.Text = jsonPacket.ToString();
            }
        }

        public void OnProviderDisabled(string provider)
        {
            
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            _locationText = FindViewById<TextView>(Resource.Id.locationTextView);
            _locationText.Text = "Waiting for GPS";

            InitializeLocationManager();

            Button connectButton = FindViewById<Button>(Resource.Id.connectWebSocket);
            //Assign The Event To Button
            connectButton.Click += delegate {

                //Call Your Method When User Clicks The Button
                btnConnectClicked();
            };
        }

        public async void btnConnectClicked()
        {
            Toast.MakeText(this, "Connect clicked", ToastLength.Long).Show();
            var configuration = new MqttConfiguration { Port = 1883 };
            var client = await MqttClient.CreateAsync("tcp://104.196.195.27", configuration);
            await client.ConnectAsync(new MqttClientCredentials("testClient", "yashren", "mqtt"));
            await client.SubscribeAsync("test", MqttQualityOfService.AtMostOnce);
/*
 *          client.MessageStream.Subscribe(msg =>
            {
                //All the messages from the Broker to any subscribed topic will get here
                //The MessageStream is an Rx Observable, so you can filter the messages by topic with Linq to Rx
                //The message object has Topic and Payload properties. The Payload is a byte[] that you need to deserialize 
                //depending on the type of the message
                Console.WriteLine($"Message received in topic {msg.Topic}");
            });
            */
            var message = new MqttApplicationMessage("test", Encoding.UTF8.GetBytes("Test String  Message"));
            await client.PublishAsync(message, MqttQualityOfService.AtLeastOnce);
        }

        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }
    }
}

