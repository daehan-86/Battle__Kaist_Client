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
        // Type: 이벤트 유형, Action<GameEvent>: 이벤트 핸들러 매핑

        static readonly Dictionary<Delegate, Action<GameEvent>> s_EventLookups =
            new Dictionary<Delegate, Action<GameEvent>>();
        // Delegate: 이벤트 발생 시, 이벤트 핸들러를 호출하는(등록하는) 메서드(델리게이트) or 이벤트 리스너, Action<GameEvent>: 호출당하는 이벤트 핸들러

        public static void AddListener<T>(Action<T> evt) where T : GameEvent // T타입은 GameEvent 클래스를 상속한 타입, T타입 이벤트 핸들러를 받아서 s_Events에 추가하는 메서드
        {
            if (!s_EventLookups.ContainsKey(evt))
            {
                Action<GameEvent> newAction = (e) => evt((T) e); // newAction은 Action<GameEvent>타입의 델리게이트, GameEvent타입을 받아서 T타입으로 변환 후 evt 호출
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