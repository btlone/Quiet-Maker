// Push button w/ debounce


using System;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PushButton {
	public sealed partial class MainPage : Page {
		public MainPage() {
			InitializeComponent();
			InitGPIO();
		}

		private void InitGPIO() {
			var gpio = GpioController.GetDefault();

			// error prompt
			if (gpio == null) {
				GpioStatus.Text = "There's no GPIO controller on this device";
				return -1;
			}

			buttonPin = gpio.OpenPin(BUTTON_PIN);
			ledPin = gpio.OpenPin(LED_PIN);

			// init LED to OFF by HIGH, cuz LED is wired in LOW config
			ledPin.Write(GpioPinValue.High);
			ledPin.SetDriveMode(GpioPinDriveMode.Output);

			// checking if input pull-up resistors are supported
			if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp)) {
				buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
			}
			else {
				buttonPin.SetDriveMode(GpioPinDriveMode.Input);
			}

			// setting debounce timeout
			buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

			// register for ValueChanged event
			// so buttonPin_ValueChanged()
			// is called when button is pressed
			buttonPin.ValueChanged += buttonPin_ValueChanged;

			GpioStatus.Text = "GPIO pins initialized correctly";
		}

		private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
			// toggling state of LED w/ every press
			if (e.Edge == GpioPinEdge.FallingEdge) {
				ledPinValue = (ledPinValue == GpioPinValue.Low) ? GpioPinValue.High : GpioPinValue.Low;
				ledPin.Write(ledPinValue);
			}

			// call UI updates on UI thread
			// cuz this handler gets invoked
			// on spearate thread
			var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
				if (e.Edge == GpioPinEdge.FallingEdge) {
					ledEllipse.Fill = (ledPinValue == GpioPinValue.Low) ? redBrush : grayBrush;
					GpioStatus.Text = "Button pressed";
				}
				else {
					GpioStatus.Text = "Button released";
				}
			});
		}

		private const int BUTTON_PIN = 5;
		private const int LED_PIN = 6;
		private GpioPin buttonPin;
		private GpioPin ledPin;
		private GpioPinValue ledPinValue = GpioPinValue.High;
		private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
		private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
	}
}
