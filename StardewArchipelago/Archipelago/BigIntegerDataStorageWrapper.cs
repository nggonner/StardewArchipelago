﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago
{
    public class BigIntegerDataStorageWrapper : IDataStorageWrapper<BigInteger>
    {
        private IMonitor _monitor;
        private ArchipelagoSession _session;

        public BigIntegerDataStorageWrapper(IMonitor monitor, ArchipelagoSession session)
        {
            _monitor = monitor;
            _session = session;
        }

        public void Set(Scope scope, string key, BigInteger value)
        {
            var token = JToken.FromObject(value);
            _session.DataStorage[scope, key] = token;
        }

        public BigInteger? Read(Scope scope, string key)
        {
            try
            {
                var value = _session.DataStorage[scope, key];
                value.Initialize(0);
                var bigIntegerValue = value.To<BigInteger>();
                return bigIntegerValue;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error Reading BigInteger from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}", LogLevel.Error);
                return null;
            }
        }

        public async Task<BigInteger?> ReadAsync(Scope scope, string key)
        {
            return await ReadAsync(scope, key, null);
        }

        public async Task<BigInteger?> ReadAsync(Scope scope, string key, Action<BigInteger> callback)
        {
            try
            {
                var value = _session.DataStorage[scope, key];
                value.Initialize(0);
                var bigIntegerValue = await value.GetAsync<BigInteger>();
                callback?.Invoke(bigIntegerValue);
                return bigIntegerValue;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error Reading BigInteger from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}", LogLevel.Error);
                return null;
            }
        }

        public bool Add(Scope scope, string key, long amount)
        {
            try
            {
                _session.DataStorage[scope, key] += amount;
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error adding {amount} to DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}", LogLevel.Error);
                return false;
            }
        }

        public bool Subtract(Scope scope, string key, long amount, bool dontGoBelowZero)
        {
            try
            {
                if (dontGoBelowZero)
                {
                    _session.DataStorage[scope, key] = (_session.DataStorage[scope, key] - amount) + Operation.Max(0);
                }
                else
                {
                    _session.DataStorage[scope, key] -= amount;
                }
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error subtracting {amount} from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}", LogLevel.Error);
                return false;
            }
        }

        public bool Multiply(Scope scope, string key, int multiple)
        {
            try
            {
                _session.DataStorage[scope, key] *= multiple;
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error multiplying by {multiple} in DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}", LogLevel.Error);
                return false;
            }
        }

        public bool DivideByTwo(Scope scope, string key)
        {
            try
            {
                _session.DataStorage[scope, key] += new OperationSpecification { OperationType = OperationType.RightShift };
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error dividing by 2 in DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}", LogLevel.Error);
                return false;
            }
        }
    }
}
