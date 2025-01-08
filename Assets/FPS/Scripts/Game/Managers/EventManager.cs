using System;
using System.Collections.Generic;

namespace Unity.FPS.Game
{
    public class GameEvent
    {
    }

    // A simple Event System that can be used for remote systems communication
    public static class EventManager
    {
        static readonly Dictionary<Type, Action<GameEvent>> s_Events = new Dictionary<Type, Action<GameEvent>>();
        // Type: �̺�Ʈ ����, Action<GameEvent>: �̺�Ʈ �ڵ鷯 ����

        static readonly Dictionary<Delegate, Action<GameEvent>> s_EventLookups =
            new Dictionary<Delegate, Action<GameEvent>>();
        // Delegate: �̺�Ʈ �߻� ��, �̺�Ʈ �ڵ鷯�� ȣ���ϴ�(����ϴ�) �޼���(��������Ʈ) or �̺�Ʈ ������, Action<GameEvent>: ȣ����ϴ� �̺�Ʈ �ڵ鷯

        public static void AddListener<T>(Action<T> evt) where T : GameEvent // TŸ���� GameEvent Ŭ������ ����� Ÿ��, TŸ�� �̺�Ʈ �ڵ鷯�� �޾Ƽ� s_Events�� �߰��ϴ� �޼���
        {
            if (!s_EventLookups.ContainsKey(evt))
            {
                Action<GameEvent> newAction = (e) => evt((T) e); // newAction�� Action<GameEvent>Ÿ���� ��������Ʈ, GameEventŸ���� �޾Ƽ� TŸ������ ��ȯ �� evt ȣ��
                s_EventLookups[evt] = newAction;

                if (s_Events.TryGetValue(typeof(T), out Action<GameEvent> internalAction))
                    s_Events[typeof(T)] = internalAction += newAction;
                else
                    s_Events[typeof(T)] = newAction;
            }
        }

        public static void RemoveListener<T>(Action<T> evt) where T : GameEvent
        {
            if (s_EventLookups.TryGetValue(evt, out var action))
            {
                if (s_Events.TryGetValue(typeof(T), out var tempAction))
                {
                    tempAction -= action;
                    if (tempAction == null)
                        s_Events.Remove(typeof(T));
                    else
                        s_Events[typeof(T)] = tempAction;
                }

                s_EventLookups.Remove(evt);
            }
        }

        public static void Broadcast(GameEvent evt)
        {
            if (s_Events.TryGetValue(evt.GetType(), out var action))
                action.Invoke(evt);
        }

        public static void Clear()
        {
            s_Events.Clear();
            s_EventLookups.Clear();
        }
    }
}