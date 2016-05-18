using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Rover
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //running some task on different thread
        private BackgroundWorker _worker;
        private CoreDispatcher _dispatcher;

        //this is a flag to stop the robot when the application is closed.
        private bool _finish;

        //these are 2 variables to store the left and right distance
        private double distanceleft = 0;
        private double distanceright = 0;

        //variables to store average values of readings from ultrasonic sensors
        double xLeft = 0.0;
        double xRight = 0.0;

        //ultrasonic sensors
        UltrasonicDistanceSensor ultrasonicDistanceSensorLeft = null;
        UltrasonicDistanceSensor ultrasonicDistanceSensorRight = null;

        //this is the 1st starting point.
        public MainPage()
        {
            InitializeComponent();

            //you won't really need to touch these, these are just for the lifecycle of the application, and loading/unloading your logic and sensors when the application is ready/closed.
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        //when the UI is loaded
        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //dispatcher is another thread to update the UI, you will not need to touch this
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;



            //we would like the motor to be running on a background thread
            _worker = new BackgroundWorker();
            _worker.DoWork += DoWork;
            _worker.RunWorkerAsync();



        }

        //this only runs when the application is exit
        private void MainPage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //this flag will stop the robot from moving forward
            _finish = true;

        }


        /// <summary>
        /// For the entire challenge, it is pretty safe to say you only need to change the algorithm in this method
        /// If you would like to venture outside of this method, by all means :)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            //instantiate the Motor and Ultrasonic sensors. You will not need to change the GPIO pin numbers below. 
            //The numbers below are GPIO pins which the Raspberry Pi uses to control the motor and sensors
            var driver = new TwoMotorsDriver(new Motor(27, 22), new Motor(5, 6));

            ultrasonicDistanceSensorRight = new UltrasonicDistanceSensor(23, 24);
            ultrasonicDistanceSensorLeft = new UltrasonicDistanceSensor(21, 20);


            #region as a recommendation, you may wish to start editing the region from below

            //number of milliseconds to turn
            int turning = 200;

            //while (true)
            //{
            //    await Task.Delay(1000);
            //    double xLeft = await ultrasonicDistanceSensor.GetDistanceInCmAsync(1000);
            //    double xRight = await ultrasonicDistanceSensorright.GetDistanceInCmAsync(1000);
            //    await WriteLog(xLeft + " : " + xRight);
            //}

            driver.MoveForward();

            //get the readings from the ultrasonic sensors. you might want to optimize this as they are reading in "sequential"
            double x = await ultrasonicDistanceSensorLeft.GetDistanceInCmAsync(1000);
            double y = await ultrasonicDistanceSensorRight.GetDistanceInCmAsync(1000);

            //infinite looping
            while (true)
            {
                //too close to left
                if (x < 20)
                {
                    await driver.TurnRightAsync(turning);
                }
                //too close to right
                else if (y < 20)
                {
                    await driver.TurnLeftAsync(turning);
                }
                else
                {
                    driver.MoveForward();
                }

                //get the readings from the ultrasonic sensors. you might want to optimize this as they are reading in "sequential"
                x = await ultrasonicDistanceSensorLeft.GetDistanceInCmAsync(1000);
                y = await ultrasonicDistanceSensorRight.GetDistanceInCmAsync(1000);
             
                await WriteLog(x + " : " + y);
            }
                


            //avg();
        
            //while (true)
            //{
            //    await WriteLog(xLeft + " : " + xRight);
            //    if (xLeft < xRight)
            //    {
            //        double xRightNow = 0.0;
            //        //facing lhs
            //        while (xRight - xRightNow > 0)
            //        {
            //            if (xRightNow != 0)
            //                xRight = xRightNow;
            //            await driver.TurnRightAsync(turning);
            //            await Task.Delay(500);
            //            double temp = 0.0;
            //            while (temp == 0.0)
            //            {
            //                temp = await ultrasonicDistanceSensorright.GetDistanceInCmAsync(1000);
            //            }
            //            xRightNow = temp;
            //            await WriteLog(xRight + " : " + xRightNow);
            //        }
            //        //await driver.TurnLeftAsync(turning);
            //    }
            //    else
            //    {
            //        //facing rhs
            //        double xLeftNow = 0.0;
            //        //facing lhs
            //        while (xLeft - xLeftNow > 0)
            //        {
            //            if (xLeftNow != 0)
            //                xLeft = xLeftNow;
            //            await driver.TurnLeftAsync(turning);
            //            await Task.Delay(500);
            //            double temp = 0.0;
            //            while (temp == 0.0)
            //            {
            //                temp = await ultrasonicDistanceSensor.GetDistanceInCmAsync(1000);
            //            }
            //            xLeftNow = temp;
            //            await WriteLog(xLeft + " : " + xLeftNow);
            //        }
            //        //await driver.TurnRightAsync(turning);
            //    }

            //    driver.MoveForward();
            //    await Task.Delay(500);
            //    driver.Stop();
            //    await Task.Delay(3000);

            //    avg();
            //}

        }

        //currently unused method, this is useful when sensors give an outlier reading
        //these outlier readings are common for low cost sensors
        private async void avg()
        {
            int count = 5;
            xLeft = 0.0;
            for (int i = 0; i < count; i++)
            {
                double temp = 0.0;
                while (temp == 0.0)
                {
                    temp = await ultrasonicDistanceSensorLeft.GetDistanceInCmAsync(1000);
                    await Task.Delay(1000);
                }
                xLeft += temp;
            }
            xLeft = xLeft / count;

            xRight = 0.0;
            for (int i = 0; i < count; i++)
            {
                double temp = 0.0;
                while (temp == 0.0)
                {
                    temp = await ultrasonicDistanceSensorRight.GetDistanceInCmAsync(1000);
                    await Task.Delay(1000);
                }
                xRight += temp;
            }
            xRight = xRight / count;
            await WriteLog(xLeft + " : " + xRight);
        }

        //display log onto UI
        private async Task WriteLog(string text)
        {
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Log.Text = $"{text} | " + Log.Text
                    Log.Text = $"{text}  ";
                });
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

    }
}
