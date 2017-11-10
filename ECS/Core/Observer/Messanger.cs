using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public class Messanger {

        public List<IMessageReceiver> receiverList = new List<IMessageReceiver>();

        public void Register(IMessageReceiver receiver) {
            receiverList.Add(receiver);
        }

        public void Notify() {
            foreach (var receiver in receiverList) {
                receiver.Notify();
            }
        }
    }

    public class Messanger<TMessage> {

        public List<IMessageReceiver<TMessage>> receiverList = new List<IMessageReceiver<TMessage>>();

        public void Register(IMessageReceiver<TMessage> receiver) {
            receiverList.Add(receiver);
        }

        public void Notify(TMessage message) {
            foreach (var receiver in receiverList) {
                receiver.Notify(message);
            }
        }
    }
}