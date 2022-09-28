﻿using System.Collections.Generic;
using UnityEngine;
namespace JKFrame
{
    public class ConfigManager : ManagerBase<ConfigManager>
    {
        [SerializeField]
        private ConfigSetting configSetting;

        /// <summary>
        /// 获取configTypeName下的全部配置
        /// </summary>
        public Dictionary<int, ConfigBase> GetConfigs(string configTypeName)
        { 
            return configSetting.GetConfigs(configTypeName);
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T">具体的配置类型</typeparam>
        /// <param name="configTypeName">配置类型名称</param>
        /// <param name="id">id</param>
        public T GetConfig<T>(string configTypeName, int id = 0) where T : ConfigBase
        {
            return configSetting.GetConfig<T>(configTypeName, id);
        }
    }
}