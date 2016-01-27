using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeakPubSub
{
    class Program
    {
        static void Main(string[] args)
        {
            var publisher1 = new CustomPublisher("1");
            PubSubManager.Publish("publisher1", publisher1);
            var publisher2 = new CustomPublisher("2");
            PubSubManager.Publish("publisher2", publisher2);

            var subscriber11 = new CustomSubscriber("11");
            PubSubManager.Subscribe("publisher1", subscriber11);
            var subscriber12 = new CustomSubscriber("12");
            PubSubManager.Subscribe("publisher1", subscriber12);

            var subscriber21 = new CustomSubscriber("21");
            PubSubManager.Subscribe("publisher2", subscriber21);
            var subscriber22 = new CustomSubscriber("22");
            PubSubManager.Subscribe("publisher2", subscriber22);

            publisher1.Raise(new CustomEventArgs() { Data = "arg 1" });
            publisher2.Raise(new CustomEventArgs() { Data = "arg 2" });

            Console.WriteLine("Remove subscribers 11, 21");

            PubSubManager.Unsubscribe("publisher1", subscriber11);
            PubSubManager.Unsubscribe("publisher2", subscriber21);

            publisher1.Raise(new CustomEventArgs() { Data = "arg 1" });
            publisher2.Raise(new CustomEventArgs() { Data = "arg 2" });

            Console.WriteLine("Remove publisher 2");

            PubSubManager.Unpublish<CustomEventArgs>("publisher2");

            publisher1.Raise(new CustomEventArgs() { Data = "arg 1" });
            publisher2.Raise(new CustomEventArgs() { Data = "arg 2" });

            Console.ReadLine();
        }
    }

    internal sealed class CustomEventArgs : EventArgs
    {
        public string Data { get; set; }
    }

    internal sealed class CustomSubscriber : ISubscriber<CustomEventArgs>
    {
        private readonly string _text;

        public CustomSubscriber(string text)
        {
            _text = text;
        }
        public void Handler(Object sender, CustomEventArgs eventArgs)
        {
            var publisher = (CustomPublisher)sender;
            Console.WriteLine("publisher: {0}, subscriber: {1}, Data: {2}", publisher.Text, _text, eventArgs.Data);
        }
    }

    internal sealed class CustomPublisher : IPublisher<CustomEventArgs>
    {
        public CustomPublisher(string text)
        {
            Text = text;
        }

        public event EventHandler<CustomEventArgs> Send;

        public void Raise(CustomEventArgs eventArgs)
        {
            if (Send != null)
                Send(this, eventArgs);
        }

        public string Text { get; private set; }
    }
}
