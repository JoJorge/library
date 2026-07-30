namespace FsmSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Top-level manager responsible for creating, registering, updating, and removing all Machine instances.
    /// </summary>
    public class FsmManager
    {
        /// <summary>
        /// 已註冊的 Machine 清單（按註冊順序排列）。
        /// </summary>
        private readonly List<MachineBase> _machines = new List<MachineBase>();

        /// <summary>
        /// 等待加入的 Machine 佇列。
        /// </summary>
        private readonly List<MachineBase> _pendingAdd = new List<MachineBase>();

        /// <summary>
        /// 等待移除的 Machine 佇列。
        /// </summary>
        private readonly List<MachineBase> _pendingRemove = new List<MachineBase>();

        /// <summary>
        /// 更新迴圈執行中旗標。
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// Creates a new Machine instance with the specified initial state, registers it, and returns the instance.
        /// </summary>
        /// <typeparam name="TMachine">The concrete Machine type to create.</typeparam>
        /// <param name="initialState">The initial state for the Machine. Must not be null.</param>
        /// <returns>The newly created Machine instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="initialState"/> is <c>null</c>.
        /// </exception>
        public TMachine CreateMachine<TMachine>(IState<TMachine> initialState)
            where TMachine : Machine<TMachine>
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            var machine = (TMachine)Activator.CreateInstance(
                typeof(TMachine),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[] { initialState },
                null);

            if (_isUpdating)
            {
                _pendingAdd.Add(machine);
            }
            else
            {
                _machines.Add(machine);
            }

            return machine;
        }

        /// <summary>
        /// Removes the specified Machine from this manager.
        /// If the Machine is Running, it will be stopped before removal.
        /// The Machine's status is set to Destroyed after removal.
        /// </summary>
        /// <typeparam name="TMachine">The concrete Machine type.</typeparam>
        /// <param name="machine">The Machine instance to remove.</param>
        public void RemoveMachine<TMachine>(TMachine machine)
            where TMachine : Machine<TMachine>
        {
            if (machine == null)
            {
                return;
            }

            if (_isUpdating)
            {
                _pendingRemove.Add(machine);
            }
            else
            {
                ProcessRemoval(machine);
            }
        }

        /// <summary>
        /// Updates all registered Running Machines in registration order.
        /// Exceptions from individual Machines are isolated and do not affect other Machines.
        /// </summary>
        public void Update()
        {
            _isUpdating = true;
            try
            {
                for (int i = 0; i < _machines.Count; i++)
                {
                    var machine = _machines[i];
                    if (machine.Status == MachineStatus.Running)
                    {
                        try
                        {
                            machine.Update();
                        }
                        catch (Exception)
                        {
                            // 故障隔離：記錄錯誤後繼續更新其餘 Machine。
                        }
                    }
                }
            }
            finally
            {
                _isUpdating = false;
                ApplyPendingChanges();
            }
        }

        /// <summary>
        /// Performs fixed-time step updates for all registered Running Machines in registration order.
        /// Exceptions from individual Machines are isolated and do not affect other Machines.
        /// </summary>
        public void FixedUpdate()
        {
            _isUpdating = true;
            try
            {
                for (int i = 0; i < _machines.Count; i++)
                {
                    var machine = _machines[i];
                    if (machine.Status == MachineStatus.Running)
                    {
                        try
                        {
                            machine.FixedUpdate();
                        }
                        catch (Exception)
                        {
                            // 故障隔離：記錄錯誤後繼續更新其餘 Machine。
                        }
                    }
                }
            }
            finally
            {
                _isUpdating = false;
                ApplyPendingChanges();
            }
        }

        /// <summary>
        /// Applies pending additions and removals after the update loop completes.
        /// </summary>
        private void ApplyPendingChanges()
        {
            if (_pendingAdd.Count > 0)
            {
                _machines.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            if (_pendingRemove.Count > 0)
            {
                for (int i = 0; i < _pendingRemove.Count; i++)
                {
                    ProcessRemoval(_pendingRemove[i]);
                }

                _pendingRemove.Clear();
            }
        }

        /// <summary>
        /// Processes the immediate removal of a Machine.
        /// If the Machine is Running, stops it first. Sets status to Destroyed and removes from the list.
        /// </summary>
        /// <param name="machine">The Machine to remove.</param>
        private void ProcessRemoval(MachineBase machine)
        {
            if (machine.Status == MachineStatus.Running)
            {
                machine.StopMachine();
            }

            machine.Status = MachineStatus.Destroyed;
            _machines.Remove(machine);
        }
    }
}
