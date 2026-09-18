using System;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// 基於 PlayerPrefs 的綁定儲存實作，作為 <see cref="IBindingStorage"/> 的預設實作。
    /// </summary>
    public class PlayerPrefsBindingStorage : IBindingStorage
    {
        private const string KeyPrefix = "InputBindings_";

        /// <inheritdoc/>
        public bool Save(InputDeviceType deviceType, string json)
        {
            try
            {
                string key = this.GetKey(deviceType);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public string Load(InputDeviceType deviceType)
        {
            string key = this.GetKey(deviceType);
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
        }

        /// <inheritdoc/>
        public bool Exists(InputDeviceType deviceType)
        {
            return PlayerPrefs.HasKey(this.GetKey(deviceType));
        }

        private string GetKey(InputDeviceType deviceType)
        {
            return KeyPrefix + deviceType.ToString();
        }
    }
}
